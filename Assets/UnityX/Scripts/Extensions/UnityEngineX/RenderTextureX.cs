using UnityEngine;
public static class RenderTextureX {
	
	/// <summary>
	/// Get the contents of a RenderTexture as a color array.
	/// See http://docs.unity3d.com/ScriptReference/RenderTexture-active.html.
	/// </summary>
	/// <returns>The render texture pixels.</returns>
	/// <param name="rt">Rt.</param>
	static public Color[] GetPixels (this RenderTexture rt) {
		Texture2D texture = new Texture2D(rt.width, rt.height);
		
		// Store active render texture
		RenderTexture lastActiveRT = RenderTexture.active;
		
		// Set the supplied RenderTexture as the active one
		RenderTexture.active = rt;
		
		// Create a new Texture2D and read the RenderTexture image into it
		texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
		Color[] pixels = texture.GetPixels();
		
		// Restore previously active render texture
		RenderTexture.active = lastActiveRT;
		return pixels;
	}
}