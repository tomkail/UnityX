using UnityEngine;
using UnityEngine.Experimental.Rendering;

// Basic matrix transforms for textures - scale, flip, rotate, transpose, etc.
// Uses a material by default, but can also use Graphics.DrawTexture; I assume the latter is faster but it doesn't deal with 0 alpha so well for some reason?
public static class TextureTransformUtil {
	public enum ImageOrientation { Unknown = -1, Normal = 0, Rotate90 = 1, Rotate180 = 2, Rotate270 = 3, FlipHorizontal = 4, Transpose = 5, FlipVertical = 6, Transverse = 7 }
    
	public static ImageOrientation GetOrientation(int rotation, bool flipHorizontal) {
		var correctedRotation = rotation%360;
		if (correctedRotation < 0) correctedRotation += 360;
        
		if (correctedRotation == 0 && !flipHorizontal) return ImageOrientation.Normal;
		if (correctedRotation == 90 && !flipHorizontal) return ImageOrientation.Rotate90;
		if (correctedRotation == 180 && !flipHorizontal) return ImageOrientation.Rotate180;
		if (correctedRotation == 270 && !flipHorizontal) return ImageOrientation.Rotate270;
		if (correctedRotation == 0 && flipHorizontal) return ImageOrientation.FlipHorizontal;
		if (correctedRotation == 90 && flipHorizontal) return ImageOrientation.Transpose;
		if (correctedRotation == 180 && flipHorizontal) return ImageOrientation.FlipVertical;
		if (correctedRotation == 270 && flipHorizontal) return ImageOrientation.Transverse;
		Debug.LogWarning("GetOrientation rotation must be a multiple of 90");
		return ImageOrientation.Unknown;
	}
    
	public static Vector2Int GetRotatedSize(Vector2Int size, ImageOrientation orientation) {
		var swapWidthAndHeight = orientation is ImageOrientation.Rotate90 or ImageOrientation.Rotate270 or ImageOrientation.Transpose or ImageOrientation.Transverse;
		if (swapWidthAndHeight) return new Vector2Int(size.y, size.x);
		else return size;
	}
	
	
    static Material _imageOrientationProcessingMaterial;
    static Material imageOrientationProcessingMaterial {
        get {
            if(_imageOrientationProcessingMaterial == null) _imageOrientationProcessingMaterial = new Material(Shader.Find("TextureProcessor/ApplyImageOrientation"));
            return _imageOrientationProcessingMaterial;
        }
    }

    public static Texture2D CopyWithImageOrientation(Texture2D inputTexture, ImageOrientation orientation) {
	    return CopyWithSizeAndImageOrientation(inputTexture, new Vector2Int(inputTexture.width, inputTexture.height), orientation);
    }

    public static Texture2D CopyWithSizeAndImageOrientation(Texture inputTexture, Vector2Int newSize, ImageOrientation orientation) {
        var tempRT = GetTemporaryRTWithNewSizeAndImageOrientation(inputTexture, newSize, orientation);
        var resultTexture = GetReadableTexture(tempRT);
        RenderTexture.ReleaseTemporary(tempRT);
        return resultTexture;
    }
    
    public static Texture2D CopyWithSizeAndImageOrientation2(Texture inputTexture, Vector2Int newSize, ImageOrientation orientation) {
	    var tempRT = GetTemporaryRTWithNewSizeAndImageOrientationViaGraphics(inputTexture, newSize, orientation);
	    var resultTexture = GetReadableTexture(tempRT);
	    RenderTexture.ReleaseTemporary(tempRT);
	    return resultTexture;
    }
    
    public static void ApplyImageOrientation(Texture2D rawTexture, ImageOrientation orientation) {
        ApplySizeAndImageOrientation(rawTexture, new Vector2Int(rawTexture.width, rawTexture.height), orientation);
    }
    public static void ApplySizeAndImageOrientation(Texture2D inputTexture, Vector2Int newSize, ImageOrientation orientation) {
        var tempRT = GetTemporaryRTWithNewSizeAndImageOrientation(inputTexture, newSize, orientation);
        if(inputTexture.width != tempRT.width) inputTexture.Reinitialize(tempRT.width, tempRT.height, inputTexture.graphicsFormat, false);
        GetReadableTexture(tempRT, inputTexture);
        RenderTexture.ReleaseTemporary(tempRT);
    }
    

    public static RenderTexture GetTemporaryRTWithImageOrientation(Texture inputTexture, ImageOrientation orientation) {
	    return GetTemporaryRTWithNewSizeAndImageOrientation(inputTexture, new Vector2Int(inputTexture.width, inputTexture.height), orientation);
    }
    public static RenderTexture GetTemporaryRTWithNewSizeAndImageOrientation(Texture inputTexture, Vector2Int unrotatedRenderTextureSize, ImageOrientation orientation) {
	    var size = GetRotatedSize(new Vector2Int(unrotatedRenderTextureSize.x, unrotatedRenderTextureSize.y), orientation);
	    var tempRT = RenderTexture.GetTemporary(new RenderTextureDescriptor(size.x, size.y, SystemInfo.GetCompatibleFormat(inputTexture.graphicsFormat, GraphicsFormatUsage.Render), 0));
	    imageOrientationProcessingMaterial.SetInt("_Orientation", (int)orientation);
	    Graphics.Blit(inputTexture, tempRT, imageOrientationProcessingMaterial);
	    return tempRT;
    }
    
    
    public static void ApplyImageOrientationViaGraphics(Texture2D inputTexture, ImageOrientation orientation) {
	    var tempRT = GetTemporaryRTWithNewSizeAndImageOrientationViaGraphics(inputTexture, new Vector2Int(inputTexture.width, inputTexture.height), orientation);
	    if(inputTexture.width != tempRT.width) inputTexture.Reinitialize(tempRT.width, tempRT.height, inputTexture.graphicsFormat, false);
	    GetReadableTexture(tempRT, inputTexture);
	    RenderTexture.ReleaseTemporary(tempRT);
    }
    public static Texture2D CopyWithImageOrientationViaGraphics(Texture inputTexture, ImageOrientation orientation) {
	    var tempRT = GetTemporaryRTWithNewSizeAndImageOrientationViaGraphics(inputTexture, new Vector2Int(inputTexture.width, inputTexture.height), orientation);
	    var resultTexture = GetReadableTexture(tempRT);
	    RenderTexture.ReleaseTemporary(tempRT);
	    return resultTexture;
    }
    
    public static RenderTexture GetTemporaryRTWithNewSizeAndImageOrientationViaGraphics(Texture inputTexture, Vector2Int newSize, ImageOrientation orientation) {
	    var size = GetRotatedSize(new Vector2Int(newSize.x, newSize.y), orientation); 
	    
	    RenderTextureDescriptor descriptor = new RenderTextureDescriptor(size.x, size.y, SystemInfo.GetCompatibleFormat(inputTexture.graphicsFormat, GraphicsFormatUsage.Render), 0);
	    var tempRT = RenderTexture.GetTemporary(descriptor);
	    
		Matrix4x4 m = Matrix4x4.identity;
		switch (orientation)
		{
			case ImageOrientation.Unknown:
			case ImageOrientation.Normal:
				m = Matrix4x4.TRS(new Vector3(0, inputTexture.height, 0f), Quaternion.Euler(0f, 0f, 0f), new Vector3(1,-1,1));
				break;
			case ImageOrientation.Rotate90:
				m = Matrix4x4.TRS(new Vector3(inputTexture.height, inputTexture.width, 0f), Quaternion.Euler(0f, 0f, 90f), new Vector3(-1,1,1));
				break;
			case ImageOrientation.Rotate180:
				m = Matrix4x4.TRS(new Vector3(inputTexture.width, 0, 0f), Quaternion.Euler(0f, 0f, 0f), new Vector3(-1,1,1));
				break;
			case ImageOrientation.Rotate270:
				m = Matrix4x4.TRS(new Vector3(0, 0, 0f), Quaternion.Euler(0f, 0f, -90f), new Vector3(-1,1,1));
				break;
			case ImageOrientation.FlipHorizontal:
				m = Matrix4x4.TRS(new Vector3(inputTexture.width, inputTexture.height, 0f), Quaternion.Euler(0f, 0f, 180), Vector3.one);
				break;
			case ImageOrientation.Transpose:
				m = Matrix4x4.TRS(new Vector3(0, inputTexture.height, 0f), Quaternion.Euler(0f, 0f, -90), new Vector3(inputTexture.height/(float)inputTexture.width, inputTexture.width/(float)inputTexture.height, 1));
				break;
			case ImageOrientation.FlipVertical:
				break;
			case ImageOrientation.Transverse:
				m = Matrix4x4.TRS(new Vector3(inputTexture.width, 0, 0f), Quaternion.Euler(0f, 0f, 90f), new Vector3(inputTexture.height/(float)inputTexture.width, inputTexture.width/(float)inputTexture.height, 1));
				break;
		}

		// Blit can't flip unless using a material, so we use Graphics.DrawTexture instead
		GL.InvalidateState();
		RenderTexture prevRT = RenderTexture.active;
		RenderTexture.active = tempRT;
		GL.Clear(false, true, Color.clear);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, inputTexture.width, 0f, inputTexture.height);
		Rect sourceRect = new Rect(0f, 0f, 1f, 1f);
		Rect destRect = new Rect(0f, 0, inputTexture.width, inputTexture.height);
		GL.MultMatrix(m);

		Graphics.DrawTexture(destRect, inputTexture, sourceRect, 0, 0, 0, 0);
		GL.PopMatrix();
		GL.InvalidateState();
		RenderTexture.active = prevRT;
	    return tempRT;
    }

	// Converts a non-readable texture to a readable Texture2D.
	// "targetTexture" can be null or you can pass in an existing texture.
	// Remember to Destroy() the returned texture after finished with it
	static Texture2D GetReadableTexture(RenderTexture inputTexture, Texture2D targetTexture = null) {
		if (targetTexture == null) targetTexture = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, false);

		RenderTexture prevRT = RenderTexture.active;
		RenderTexture.active = inputTexture;
		targetTexture.ReadPixels(new Rect(0f, 0f, inputTexture.width, inputTexture.height), 0, 0, false);
		RenderTexture.active = prevRT;
		targetTexture.Apply(false, false);
		
		return targetTexture;
	}
}
