using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureCreator : MonoBehaviour {
    [SerializeField]
    RenderTexture rt;
    public RenderTexture renderTexture {
        get {
            return rt;
        }
    }
    
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
        Vector2Int targetSize = fullScreen ? screenSize : renderTextureSize;
        if(rt != null && (rt.width != targetSize.x || rt.height != targetSize.y || rt.depth != (int)renderTextureDepth || rt.format != renderTextureFormat)) {
            ReleaseRenderTexture();
        }
        if(rt == null && targetSize.x > 0 && targetSize.y > 0) {
            rt = new RenderTexture (targetSize.x, targetSize.y, (int)renderTextureDepth, renderTextureFormat, renderTextureReadWrite);
            rt.filterMode = FilterMode.Bilinear;
            rt.hideFlags = HideFlags.HideAndDontSave;
            rt.enableRandomWrite = enableRandomWrite;
            if(OnCreateRenderTexture != null) OnCreateRenderTexture(rt);
        }
    }

    public void ReleaseRenderTexture () {
        if(rt == null) return;
        rt.Release();
        rt = null;
    }
}