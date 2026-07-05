using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityX.Colors;

public static class ColorX {
	public static Color orange => new(1f, 0.3f, 0f, 1f);

	public static Color pink => new(1f, 0f, 0.6f, 1f);

	public static Color MoveTowards (Color current, Color target, float maxDelta) {
		return new Color(
			Mathf.MoveTowards(current.r, target.r, maxDelta),
			Mathf.MoveTowards(current.g, target.g, maxDelta),
			Mathf.MoveTowards(current.b, target.b, maxDelta),
			Mathf.MoveTowards(current.a, target.a, maxDelta)
		);
	}

	public static Color SmoothDamp (Color current, Color target, ref Color currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
		return new Color(
			Mathf.SmoothDamp(current.r, target.r, ref currentVelocity.r, smoothTime, maxSpeed, deltaTime),
			Mathf.SmoothDamp(current.g, target.g, ref currentVelocity.g, smoothTime, maxSpeed, deltaTime),
			Mathf.SmoothDamp(current.b, target.b, ref currentVelocity.b, smoothTime, maxSpeed, deltaTime),
			Mathf.SmoothDamp(current.a, target.a, ref currentVelocity.a, smoothTime, maxSpeed, deltaTime)
		);
	}

	public static string ComponentToHex(float channel, bool toUpper = false) {
		channel = Mathf.Clamp(channel, 0f, 1f);
		int intValue = (int)(channel * 255);
		return intValue.ToString("X2");
	}

	public static string ToHex(this Color32 color, bool alpha = true, bool toUpper = false) {
		if(toUpper) {
			return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + (alpha?color.a.ToString("X2"):"");
		} else {
			return color.r.ToString("x2") + color.g.ToString("x2") + color.b.ToString("x2") + (alpha?color.a.ToString("x2"):"");
		}
	}

	public static string ToHexCode(this Color32 color, bool alpha = true, bool toUpper = false) {
		return "#"+ToHex(color, alpha, toUpper);
	}
	 
	public static Color HexToColor(string hex) {
		DebugX.Assert(hex.Length == 6 || hex.Length == 8, "Hex string is not valid");
		byte r = byte.Parse(hex.Substring(0,2), NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), NumberStyles.HexNumber);
		if(hex.Length == 8) {
			byte a = byte.Parse(hex.Substring(6,2), NumberStyles.HexNumber);
			return new Color32(r, g, b, a);
		} else {
			return new Color32(r, g, b, 255);
		}
	}

	// Convenience helper: a uniform random RGB (opaque). NOT equivalent to Random.ColorHSV, which samples in HSV space.
	public static Color RandomRGB() {
        return new Color(Random.value, Random.value, Random.value);
    }

	public static Color WithAlpha (this Color c, float alpha) {
		return new Color(c.r,c.g,c.b,alpha);
	}
	public static Color WithMultipliedAlpha (this Color c, float alpha) {
		return new Color(c.r,c.g,c.b,c.a*alpha);
	}

	public static Color ToGrayscaleColor (float gray) {
		return new Color(gray,gray,gray);
	}

	// Convenience helper: a named wrapper that returns Color.grayscale expanded back into a (gray,gray,gray) Color.
	public static Color Grayscale (Color color) {
		float gray = color.grayscale;
		return new Color(gray,gray,gray);
	}
	
	public static Color Average(this IList<Color> _colors){
		if(_colors.Count == 0) { Debug.LogError("Array length is 0!"); return Color.clear; } // else the divides below produce NaN
		Color color = Color.clear;
		for(int i = 0; i < _colors.Count; i++){
			color += _colors[i];
		}
		color.r /= _colors.Count;
		color.g /= _colors.Count;
		color.b /= _colors.Count;
		color.a /= _colors.Count;
		return color;
	}

	public static float[] ColorArrayToGrayscaleFloatArray(Color[] _colors){
		float[] floatArray = new float[_colors.Length];
		for(int i = 0; i < _colors.Length; i++)
			floatArray[i] = _colors[i].grayscale;
		return floatArray;
	}
	
	public static float[] ColorArrayToAlphaFloatArray(Color[] _colors){
		float[] floatArray = new float[_colors.Length];
		for(int i = 0; i < _colors.Length; i++)
			floatArray[i] = _colors[i].a;
		return floatArray;
	}

	public static Color[] GrayscaleFloatArrayToColorArray(float[] _floats){
		Color[] colorArray = new Color[_floats.Length];
		for(int i = 0; i < _floats.Length; i++)
			colorArray[i] = ToGrayscaleColor(_floats[i]);
		return colorArray;
	}

	public static Color[] ToGrayscale(Color[] _colors){
		Color[] colorArray = new Color[_colors.Length];
		for(int i = 0; i < _colors.Length; i++){
			colorArray[i] = Grayscale(_colors[i]);
		}
		return colorArray;
	}
	
	public static Color HueShift (this Color color, float amount) {
		HSLColor hslColor = color;
		hslColor.h += amount;
		return hslColor;
	}
	
	public static Color Saturate (this Color color, float amount) {
		HSLColor hslColor = color;
		hslColor.s += amount;
		return hslColor;
	}
	
	public static Color Lighten (this Color color, float amount) {
		HSLColor hslColor = color;
		hslColor.l += amount;
		return hslColor;
	}
	
	public static Color Darken (this Color color, float amount) {
		HSLColor hslColor = color;
		hslColor.l -= amount;
		return hslColor;
	}
	
	public static Color WithLightness (this Color color, float amount) {
		HSLColor hslColor = color;
		hslColor.l = amount;
		return hslColor;
	}
	

}