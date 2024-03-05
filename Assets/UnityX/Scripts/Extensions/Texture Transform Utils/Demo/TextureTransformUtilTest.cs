using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;

public class TextureTransformUtilTest : MonoBehaviour {
    [PreviewTexture(200)]
    public Texture2D tex;
    [PreviewTexture(200)]
    public Texture2D rotatedTex;
    [PreviewTexture(200)]
    public Texture2D rotatedTex2;
    [PreviewTexture(200)]
    public Texture2D scaledTex;
    
    [Space]
    public TextureTransformUtil.ImageOrientation orientation;
    [Button]
    public void Test(int rotation, bool flip) {
        TextureTransformUtil.ApplyImageOrientation(tex, TextureTransformUtil.GetOrientation(rotation, flip));
    }
    
    [Button]
    public void RotationTest() {
        rotatedTex = TextureTransformUtil.CopyWithImageOrientation(tex, orientation);
        rotatedTex2 = TextureTransformUtil.CopyWithImageOrientationViaGraphics(tex, orientation);
    }
    [Button]
    public void ScaleTest() {
        rotatedTex = TextureTransformUtil.CopyWithSizeAndImageOrientation(tex, new Vector2Int(Mathf.RoundToInt(tex.width*0.3f), Mathf.RoundToInt(tex.height*0.3f)), orientation);
        rotatedTex2 = TextureTransformUtil.CopyWithSizeAndImageOrientation2(tex, new Vector2Int(Mathf.RoundToInt(tex.width*0.3f), Mathf.RoundToInt(tex.height*0.3f)), orientation);
    }

    public string assetLoadPath;
    [PreviewTexture] public Texture2D loadedTex;
    MediaLoadManager.TextureLoadRequest loadRequest;
    [Button]
    public void LoadTest() {
        if(loadRequest is {released: false}) loadRequest.Dispose();
        MediaLoadManager.Instance.GetOrLoadTexture(assetLoadPath, OnCompleteDownload, ref loadRequest);
    }
    void OnCompleteDownload(Texture2D obj) {
        loadedTex = obj;
    }
}
