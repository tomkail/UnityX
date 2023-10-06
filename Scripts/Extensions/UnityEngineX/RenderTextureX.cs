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
}