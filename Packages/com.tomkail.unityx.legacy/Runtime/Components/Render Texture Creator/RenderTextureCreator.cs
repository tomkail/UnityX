using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class RenderTextureCreator : MonoBehaviour {
    [SerializeField] RenderTexture _renderTexture;
    public RenderTexture renderTexture => _renderTexture;

    public enum RenderTextureDepth {
        _0 = 0, 
        _16 = 16, 
        _24 = 24, 
        _32 = 32
    }
    public enum RenderTextureAntiAliasing {
	    _1 = 1, 
	    _2 = 2, 
	    _4 = 4, 
	    _8 = 8
    }
    public bool fullScreen;
    public Vector2Int renderTextureSize = new(512, 512);
    public FilterMode filterMode = FilterMode.Bilinear;
    public RenderTextureDepth renderTextureDepth = RenderTextureDepth._0;
    public RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;
    public RenderTextureReadWrite renderTextureReadWrite = RenderTextureReadWrite.Default;
    public bool enableRandomWrite;
    public RenderTextureAntiAliasing antiAliasing = RenderTextureAntiAliasing._1;
    public Vector2Int calculatedTextureSize => fullScreen ? screenSize : renderTextureSize;


    public static Vector2Int screenSize => new(screenWidth, screenHeight);
    // Screen/Display don't report the Game view size in some editor contexts (e.g. from inspector windows),
    // so in-editor we read UnityStats.screenRes. NOTE: this reports the actual rendered backbuffer resolution.
    // Handles.GetMainGameViewSize() returns the *logical* view size instead, which differs under the Game
    // view Scale slider / Low Resolution Aspect Ratios — not a guaranteed drop-in, so kept as-is.
	static int screenWidth {
		get {
			#if UNITY_EDITOR
			var res = UnityStats.screenRes.Split('x');
			return int.Parse(res[0]);
			#else
			// Consider adding target displays, then replace with this.
			// Display.displays[0].renderingWidth
			return Screen.width;
			#endif
		}
	}
	static int screenHeight {
		get {
			#if UNITY_EDITOR
			var res = UnityStats.screenRes.Split('x');
			return int.Parse(res[1]);
			#else
			// Consider adding target displays, then replace with this.
			// Display.displays[0].renderingHeight
			return Screen.height;
			#endif
		}
	}
    
    public Action<RenderTexture> OnCreateRenderTexture;

    // Callers like BackgroundShapeBlur refresh every frame, so a zero size would otherwise warn every frame.
    bool warnedAboutSize;
    // Not serialized, so a domain reload recreates the texture once
    Settings? createdWith;
    bool warnedAboutDepth;

    void Awake() {
	    _renderTexture = null;
    }

    protected virtual void OnValidate () {
        RefreshRenderTexture();
    }
    
    public void RefreshRenderTexture () {
	    Vector2Int targetSize = calculatedTextureSize;
	    if (targetSize.x <= 0 || targetSize.y <= 0) {
		    if (!warnedAboutSize) {
			    warnedAboutSize = true;
			    Debug.LogWarning($"{GetType().Name}: Target size is {targetSize}, so not creating RenderTexture.", this);
		    }
		    return;
	    }
	    warnedAboutSize = false;

	    var requested = new Settings(targetSize, (int)renderTextureDepth, renderTextureFormat, renderTextureReadWrite, enableRandomWrite, filterMode, (int)antiAliasing);
	    // Compared with what was asked for last time rather than the texture's properties, because the platform
	    // may substitute a setting (Metal gives a 32-bit depth buffer for 24) and that would recreate it every refresh.
	    bool settingsChanged = createdWith == null || !createdWith.Value.Equals(requested);
	    if (_renderTexture != null && !settingsChanged && _renderTexture.IsCreated()) return;

	    // The read/write mode can only be set in the constructor; anything else is changed in place so references to the texture stay valid
	    if (_renderTexture == null || createdWith == null || createdWith.Value.readWrite != requested.readWrite) {
		    DestroyRenderTexture();
		    _renderTexture = new RenderTexture(targetSize.x, targetSize.y, requested.depth, renderTextureFormat, renderTextureReadWrite) {
			    name = $"RenderTextureCreator {transform.HierarchyPath()}",
			    enableRandomWrite = enableRandomWrite,
			    filterMode = filterMode,
			    antiAliasing = requested.antiAliasing,
			    hideFlags = HideFlags.HideAndDontSave
		    };
	    } else if (settingsChanged) {
		    ReleaseRenderTexture();
		    _renderTexture.width = targetSize.x;
		    _renderTexture.height = targetSize.y;
		    _renderTexture.depth = requested.depth;
		    _renderTexture.format = renderTextureFormat;
		    _renderTexture.enableRandomWrite = enableRandomWrite;
		    _renderTexture.filterMode = filterMode;
		    _renderTexture.antiAliasing = requested.antiAliasing;
	    }
	    if (settingsChanged) {
		    createdWith = requested;
		    warnedAboutDepth = false;
	    }
	    _renderTexture.Create();
	    if (!warnedAboutDepth && _renderTexture.depth != requested.depth) {
		    warnedAboutDepth = true;
		    Debug.LogWarning($"{GetType().Name}: Depth {requested.depth} isn't supported here; got {_renderTexture.depth}. Keeping it.", this);
	    }
	    if(OnCreateRenderTexture != null) OnCreateRenderTexture(_renderTexture);
    }

    readonly struct Settings : IEquatable<Settings> {
	    public readonly Vector2Int size;
	    public readonly int depth;
	    public readonly RenderTextureFormat format;
	    public readonly RenderTextureReadWrite readWrite;
	    public readonly bool enableRandomWrite;
	    public readonly FilterMode filterMode;
	    public readonly int antiAliasing;

	    public Settings(Vector2Int size, int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite, bool enableRandomWrite, FilterMode filterMode, int antiAliasing) {
		    this.size = size;
		    this.depth = depth;
		    this.format = format;
		    this.readWrite = readWrite;
		    this.enableRandomWrite = enableRandomWrite;
		    this.filterMode = filterMode;
		    this.antiAliasing = antiAliasing;
	    }

	    public bool Equals(Settings other) =>
		    size == other.size && depth == other.depth && format == other.format && readWrite == other.readWrite &&
		    enableRandomWrite == other.enableRandomWrite && filterMode == other.filterMode && antiAliasing == other.antiAliasing;
    }

    public void ReleaseRenderTexture () {
        if(_renderTexture == null) return;
        if(RenderTexture.active == _renderTexture) RenderTexture.active = null;
        _renderTexture.Release();
    }

    public void DestroyRenderTexture() {
        if(_renderTexture == null) return;
        if(RenderTexture.active == _renderTexture) RenderTexture.active = null;
        if(Application.isPlaying) Destroy(_renderTexture);
        else DestroyImmediate(_renderTexture);
        _renderTexture = null;
    }
}