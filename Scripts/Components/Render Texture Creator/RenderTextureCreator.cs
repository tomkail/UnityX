using System;
using UnityEditor;
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


    // public static Vector2Int screenSize => new Vector2Int(Screen.width, Screen.height);
    public static Vector2Int screenSize => new(screenWidth, screenHeight);
    // ARGH I hate this. It's necessary because screen/display don't return the values for game view in some editor contexts (using inspector windows, for example)
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

    void Awake() {
	    _renderTexture = null;
    }

    protected virtual void OnValidate () {
        // ReleaseRenderTexture();
        RefreshRenderTexture();
    }
    
    public void RefreshRenderTexture () {
	    Vector2Int targetSize = calculatedTextureSize;
	    if (targetSize.x <= 0 || targetSize.y <= 0) {
		    Debug.LogWarning($"{GetType().Name}: Target size is {targetSize}, so not creating RenderTexture.", this);
		    return;
	    }
	    
        if(_renderTexture == null) {
            _renderTexture = new RenderTexture (targetSize.x, targetSize.y, (int)renderTextureDepth, renderTextureFormat, renderTextureReadWrite) {
	            name = $"RenderTextureCreator {transform.HierarchyPath()}",
	            enableRandomWrite = enableRandomWrite,
	            filterMode = filterMode,
	            antiAliasing = (int)antiAliasing,
	            hideFlags = HideFlags.HideAndDontSave
            };
            if(OnCreateRenderTexture != null) OnCreateRenderTexture(_renderTexture);
        } else {
	        var textureRequiresChange = 
		        _renderTexture != null && 
		        (_renderTexture.width != targetSize.x || 
		         _renderTexture.height != targetSize.y || 
		         _renderTexture.depth != (int)renderTextureDepth || 
		         _renderTexture.format != renderTextureFormat || 
		         _renderTexture.enableRandomWrite != enableRandomWrite || 
		         _renderTexture.filterMode != filterMode ||
		         _renderTexture.antiAliasing != (int)antiAliasing
		        );
	    
	        if(textureRequiresChange) {
		        ReleaseRenderTexture();
		        _renderTexture.width = targetSize.x;
		        _renderTexture.height = targetSize.y;
		        _renderTexture.depth = (int)renderTextureDepth;
		        _renderTexture.format = renderTextureFormat;
		        _renderTexture.enableRandomWrite = enableRandomWrite;
		        _renderTexture.filterMode = filterMode;
		        _renderTexture.antiAliasing = (int)antiAliasing;
		        _renderTexture.Create();
		        if(OnCreateRenderTexture != null) OnCreateRenderTexture(_renderTexture);
	        }
        }
        if (_renderTexture.depth != (int) renderTextureDepth) {
	        Debug.LogWarning($"{GetType().Name}: Depth {(int)renderTextureDepth} appears not to be supported. You should change this so that the RenderTexture doesn't change each frame.", this);
        }
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