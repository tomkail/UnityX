using UnityEngine;
using System;

public static class TextureX {
	public static byte[] GetTextureBytesUsingFormatFromPath (Texture2D texture, string path, int jpegQuality = 75) {
		if(texture == null) {
			Debug.LogError("GetTextureBytesUsingFormatFromPath: Texture is null! "+path);
			return null;
		}
		byte[] textureBytes = null;
		var extension = System.IO.Path.GetExtension(path);
		if(string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)) textureBytes = texture.EncodeToJPG(jpegQuality);
		else if(string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)) textureBytes = texture.EncodeToPNG();
		else if(string.Equals(extension, ".tga", StringComparison.OrdinalIgnoreCase)) textureBytes = texture.EncodeToTGA();
		else if(string.Equals(extension, ".exr", StringComparison.OrdinalIgnoreCase)) textureBytes = texture.EncodeToEXR();
		else {
			Debug.LogError("GetTextureBytesUsingFormatFromPath: Unhandled format for texture! "+path);
		}
		return textureBytes;
	}

    /// <summary>
    /// Returns a scaled copy of given texture.
    /// </summary>
    /// <param name="tex">Source texure to scale</param>
    /// <param name="width">Destination texture width</param>
    /// <param name="height">Destination texture height</param>
    /// <param name="mode">Filtering mode</param>
    public static Texture2D CopyWithSizeScaled(this Texture src, int width, int height) {
        Rect texR = new Rect(0,0,width,height);
        GPUScale(src,width,height);
        
        //Get rendered data back to a new texture
        Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
        result.Reinitialize(width, height);
        result.ReadPixels(texR,0,0,true);
        return result;                 
    }
    
    /// <summary>
    /// Scales the texture data of the given texture.
    /// </summary>
    /// <param name="tex">Texure to scale</param>
    /// <param name="width">New width</param>
    /// <param name="height">New height</param>
    /// <param name="mode">Filtering mode</param>
    public static void ResizeScaled(this Texture2D tex, int width, int height) {
        Rect texR = new Rect(0,0,width,height);
        GPUScale(tex,width,height);
        
        // Update new texture
        tex.Reinitialize(width, height);
        tex.ReadPixels(texR,0,0,true);
        tex.Apply(true);
    }
    
    static void GPUScale(Texture src, int width, int height, int depth = 0) {
	    //We need the source texture in VRAM because we render with it
	    // src.filterMode = fmode;
	    // src.Apply(true);

	    //Using RTT for best quality and performance
	    RenderTexture rtt = RenderTexture.GetTemporary(width, height, depth);

	    //Set the RTT in order to render to it
	    Graphics.SetRenderTarget(rtt);

	    
	    //Setup 2D matrix in range 0..1, so nobody needs to care about sizes
	    GL.LoadPixelMatrix(0,1,1,0);

	    //Then clear & draw the texture to fill the entire RTT.
	    GL.Clear(true,true,new Color(0,0,0,0));
	    Graphics.DrawTexture(new Rect(0,0,1,1),src);

	    //Release the RenderTexture when it is no longer needed
	    RenderTexture.ReleaseTemporary(rtt);
    }
}