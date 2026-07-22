using UnityEngine;

public static class RenderTextureX {
	
    /// <summary>
    /// Get the contents of a RenderTexture as a Texture2D.
    /// See http://docs.unity3d.com/ScriptReference/RenderTexture-active.html.
    /// </summary>
    /// <returns>The render texture pixels.</returns>
    /// <param name="rt">Rt.</param>
    public static Texture2D CreateTexture (this RenderTexture rt, bool apply = false) {
        return CreateTexture(rt, rt.width, rt.height, apply);
    }

    public static Texture2D CreateTexture (this RenderTexture rt, int width, int height, bool apply = false) {
        Texture2D texture = new Texture2D(width, height);
		
        // Store active render texture
        RenderTexture lastActiveRT = RenderTexture.active;
		
        // Set the supplied RenderTexture as the active one
        RenderTexture.active = rt;
		
        // Create a new Texture2D and read the RenderTexture image into it
        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        if(apply) texture.Apply();
		
        // Restore previously active render texture
        RenderTexture.active = lastActiveRT;
        return texture;
    }
	
    public static bool RenderTextureDescriptorsMatch(RenderTextureDescriptor descriptorA, RenderTextureDescriptor descriptorB) {
        if (descriptorA.depthBufferBits != descriptorB.depthBufferBits) return false;
        if (descriptorA.width != descriptorB.width) return false;
        if (descriptorA.height != descriptorB.height) return false;
        if (descriptorA.depthStencilFormat != descriptorB.depthStencilFormat) return false;
        if (descriptorA.enableRandomWrite != descriptorB.enableRandomWrite) return false;
        if (descriptorA.colorFormat != descriptorB.colorFormat) return false;
        if (descriptorA.dimension != descriptorB.dimension) return false;
        return true;
    }
}