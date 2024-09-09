using UnityEngine;
using System.Collections;

public static class SpriteX {
	
	/// <summary>
	/// Returns the pivot in the coordinate space (0,0) to (1,1), where (0.5,0.5) is the center.
	/// </summary>
	/// <returns>The local pivot.</returns>
	/// <param name="sprite">Sprite.</param>
	public static Vector2 GetLocalPivot (this Sprite sprite) {
		return new Vector2(sprite.pivot.x/sprite.rect.width, sprite.pivot.y/sprite.rect.height);
	}
	
	/// <summary>
	/// Creates a 1x1 colored sprite.
	/// </summary>
	/// <returns>The colored sprite.</returns>
	/// <param name="newColor">New color.</param>
	/// <param name="pixelsPerUnit">Pixels per unit.</param>
	public static Sprite CreateColoredSprite (Color newColor, float pixelsPerUnit) {
		var texture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
		Color[] pixels = new Color[texture.width * texture.height].Fill(newColor);
		texture.SetPixels(pixels);
		texture.Apply();		
		
		Sprite newSprite = Sprite.Create(texture, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), pixelsPerUnit);
		return newSprite;
	}
}
