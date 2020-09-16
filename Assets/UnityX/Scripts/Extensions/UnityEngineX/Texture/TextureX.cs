using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityX.Geometry;

public static class TextureX {

	public enum TextureBlendMode {
		Normal,
		Additive,
		Multiply,
		Screen,
		Overlay,
		Darken,
		Lighten,
		Difference,
		Hue,
		Saturation,
		Color,
		Luminosity,
		Overwrite,
		Ignore
	}

	/// <summary>
	/// Clone the specified texToClone.
	/// </summary>
	/// <param name="texToClone">Tex to clone.</param>
	public static Texture2D Clone (Texture2D texToClone) {
		return MonoBehaviour.Instantiate(texToClone) as Texture2D;
	}
	
	/// <summary>
	/// Offsets the texture as if tiled using a material.
	/// </summary>
	/// <param name="texToClone">Tex to clone.</param>
	public static Texture2D Offset (Texture2D texToOffset, int offsetX, int offsetY) {
//		if(texToOffset == null) 
//			return null;
		Color[] pixels = texToOffset.GetPixels();
		Color[] newPixels = new Color[pixels.Length];
		offsetX = (int)Mathf.Repeat(offsetX, texToOffset.width);
		offsetY = (int)Mathf.Repeat(offsetY, texToOffset.height);
		for(int y = 0; y < texToOffset.height; y++) {
			for(int x = 0; x < texToOffset.width; x++) {
				int index = (y * texToOffset.width) + x;
				int offsetIndex = ((y + offsetY) * texToOffset.width) + (x + offsetX);
				if(offsetIndex >= pixels.Length) {
					offsetIndex = (pixels.Length) - (offsetIndex - (pixels.Length - 1));
				} else if(offsetIndex < 0) {
					offsetIndex = offsetIndex - (pixels.Length - 1);
				}
				newPixels[index] = pixels.GetRepeating(offsetIndex);
			}
		}
		return TextureX.Create (texToOffset.width, texToOffset.height, newPixels, texToOffset.filterMode, texToOffset.format);
	}
	
	/// <summary>
	/// Flips the texture horizontally
	/// </summary>
	/// <param name="texToClone">Tex to clone.</param>
	public static Texture2D FlipHorizontal (Texture2D texToFlip) {
		Color[] pixels = texToFlip.GetPixels();
		Color[] newPixels = new Color[pixels.Length];
		for(int y = 0; y < texToFlip.height; y++) {
			for(int x = 0; x < texToFlip.width; x++) {
				int i = (y * texToFlip.width) + x;
				int invertedi = (y * texToFlip.width) + ((texToFlip.width - 1) - x);
				newPixels[i] = pixels[invertedi];
			}
		}
		return TextureX.Create (texToFlip.width, texToFlip.height, newPixels, texToFlip.filterMode, texToFlip.format);
	}
	
	/// <summary>
	/// Flips the texture vertically
	/// </summary>
	/// <param name="texToClone">Tex to clone.</param>
	public static Texture2D FlipVertical (Texture2D texToFlip) {
		Color[] pixels = texToFlip.GetPixels();
		Color[] newPixels = new Color[pixels.Length];
		for(int y = 0; y < texToFlip.height; y++) {
			for(int x = 0; x < texToFlip.width; x++) {
				int i = (y * texToFlip.width) + x;
				int invertedi = (((texToFlip.height - 1) - y) * texToFlip.width) + x;
				newPixels[i] = pixels[invertedi];
			}
		}
		return TextureX.Create (texToFlip.width, texToFlip.height, newPixels, texToFlip.filterMode, texToFlip.format);
	}
	
	/// <summary>
	/// Flips the texture vertically
	/// </summary>
	/// <param name="texToClone">Tex to clone.</param>
	public static Texture2D FlipHorizontalAndVertical (Texture2D texToFlip) {
		Color[] pixels = texToFlip.GetPixels();
		Color[] newPixels = new Color[pixels.Length];
		for(int i = 0; i < pixels.Length; i++) {
			int invertedi = (pixels.Length - 1) - i;
			newPixels[i] = pixels[invertedi];
		}
		return TextureX.Create (new Point(texToFlip.width, texToFlip.height), newPixels, texToFlip.filterMode, texToFlip.format);
	}
	
	/// <summary>
	/// Creates a new texture from the pixels of another.
	/// </summary>
	/// <returns>The new Texture2D.</returns>
	/// <param name="texToClone">Tex to clone.</param>
	/// <param name="filterMode">Filter mode.</param>
	/// <param name="textureFormat">Texture format.</param>
	public static Texture2D CopyPixels (Texture2D texToClone, FilterMode filterMode = FilterMode.Point, TextureFormat textureFormat = TextureFormat.ARGB32) {
		return TextureX.Create (texToClone.width, texToClone.height, texToClone.GetPixels(0,0,texToClone.width,texToClone.height), filterMode, textureFormat);
	}
	
	/// <summary>
	/// Creates a new texture from the pixels of another.
	/// </summary>
	/// <returns>The new Texture2D.</returns>
	/// <param name="texToClone">Tex to clone.</param>
	/// <param name="filterMode">Filter mode.</param>
	/// <param name="textureFormat">Texture format.</param>
	public static Texture2D CopyPixels (Texture2D texToClone, Rect rect, FilterMode filterMode = FilterMode.Point, TextureFormat textureFormat = TextureFormat.ARGB32) {
		return TextureX.Create (texToClone.width, texToClone.height, texToClone.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height), filterMode, textureFormat);
	}
	
	/// <summary>
	/// Creates a 1x1 texture with specified color, filterMode and textureFormat.
	/// </summary>
	/// <param name="_color">_color.</param>
	/// <param name="filterMode">Filter mode.</param>
	/// <param name="textureFormat">Texture format.</param>
	public static Texture2D Create(Color _color, FilterMode filterMode = FilterMode.Point, TextureFormat textureFormat = TextureFormat.ARGB32){
		Texture2D tmpTexture = new Texture2D(1,1, textureFormat, false);
		tmpTexture.SetPixel(0, 0, _color);
		tmpTexture.filterMode = filterMode;
		tmpTexture.wrapMode = TextureWrapMode.Clamp;
		return tmpTexture;
	}

	public static Texture2D Create(int width, int height, Color _color, FilterMode filterMode = FilterMode.Point, TextureFormat textureFormat = TextureFormat.ARGB32){
		Color[] colors = new Color[width * height];
		colors.Fill(_color);
		return Create(width, height, colors, filterMode);
	}
	public static Texture2D Create(Point _size, Color _color, FilterMode filterMode = FilterMode.Point, TextureFormat textureFormat = TextureFormat.ARGB32){
		return Create (_size.x, _size.y, _color, filterMode, textureFormat);
	}
	
	public static Texture2D Create(int width, int height, Color[] _array, FilterMode filterMode = FilterMode.Point, TextureFormat textureFormat = TextureFormat.ARGB32){
		if(width * height != _array.Length) {
			MonoBehaviour.print("Cannot create color texture from color array because Size is ("+width+", "+height+") with area "+(width * height)+" and array size is "+_array.Length);
		}
		Texture2D tmpTexture = new Texture2D(width, height, textureFormat, false);
		tmpTexture.SetPixels(_array);
		tmpTexture.filterMode = filterMode;
		tmpTexture.wrapMode = TextureWrapMode.Clamp;
		return tmpTexture;
	}
	public static Texture2D Create(Point _size, Color[] _array, FilterMode filterMode = FilterMode.Point, TextureFormat textureFormat = TextureFormat.ARGB32){
		return Create(_size.x, _size.y, _array, filterMode, textureFormat);
	}
}