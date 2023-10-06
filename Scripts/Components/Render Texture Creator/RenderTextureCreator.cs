using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureCreator : MonoBehaviour {
    [SerializeField]
    RenderTexture _renderTexture;
    public RenderTexture renderTexture => _renderTexture;

    public enum RenderTextureDepth {
        _0 = 0, 
        _16 = 16, 
        _24 = 24, 
        _32 = 32
    }
    public bool fullScreen = false;
    public Vector2Int renderTextureSize = new Vector2Int(512, 512);
    public RenderTextureDepth renderTextureDepth = RenderTextureDepth._24;
    public RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;
    public RenderTextureReadWrite renderTextureReadWrite = RenderTextureReadWrite.Default;
    public bool enableRandomWrite = false;
    public Vector2Int calculatedTextureSize => fullScreen ? screenSize : renderTextureSize;


    // public static Vector2Int screenSize => new Vector2Int(Screen.width, Screen.height);
    public static Vector2Int screenSize => new Vector2Int(screenWidth, screenHeight);
    // ARGH I hate this. It's necessary because screen/display don't return the values for game view in some editor contexts (using inspector windows, for example)
	static int screenWidth {
		get {
			#if UNITY_EDITOR
			var res = UnityEditor.UnityStats.screenRes.Split('x');
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
			var res = UnityEditor.UnityStats.screenRes.Split('x');
			return int.Parse(res[1]);
			#else
			// Consider adding target displays, then replace with this.
			// Display.displays[0].renderingHeight
			return Screen.height;
			#endif
		}
	}
    
    public System.Action<RenderTexture> OnCreateRenderTexture;

    void OnValidate () {
        // ReleaseRenderTexture();
        RefreshRenderTexture();
    }
    
    public void RefreshRenderTexture () {
	    Vector2Int targetSize = calculatedTextureSize;
        if(_renderTexture != null && (_renderTexture.width != targetSize.x || _renderTexture.height != targetSize.y || _renderTexture.depth != (int)renderTextureDepth || _renderTexture.format != renderTextureFormat || _renderTexture.enableRandomWrite != enableRandomWrite)) {
            ReleaseRenderTexture();
            _renderTexture.width = targetSize.x;
            _renderTexture.height = targetSize.y;
            _renderTexture.depth = (int)renderTextureDepth;
            _renderTexture.format = renderTextureFormat;
            _renderTexture.enableRandomWrite = enableRandomWrite;
            _renderTexture.Create();
        }
        if(_renderTexture == null && targetSize.x > 0 && targetSize.y > 0) {
            _renderTexture = new RenderTexture (targetSize.x, targetSize.y, (int)renderTextureDepth, renderTextureFormat, renderTextureReadWrite) {
	            name = $"RenderTextureCreator {transform.HierarchyPath()}",
	            enableRandomWrite = enableRandomWrite,
	            filterMode = FilterMode.Bilinear,
	            hideFlags = HideFlags.HideAndDontSave
            };
            if(OnCreateRenderTexture != null) OnCreateRenderTexture(_renderTexture);
        }
    }

    void ReleaseRenderTexture () {
        if(_renderTexture == null) return;
        _renderTexture.Release();
    }

    void DestroyRenderTexture() {
        if(_renderTexture == null) return;
        if(Application.isPlaying) Destroy(_renderTexture);
        else DestroyImmediate(_renderTexture);
        _renderTexture = null;
    }
}