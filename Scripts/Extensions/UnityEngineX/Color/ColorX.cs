using UnityEngine;
using System.Collections.Generic;

public static class ColorX {
	public static Color orange {
		get {
			return new Color(1f, 0.3f, 0f, 1f);
		}
	}

	public static Color pink {
		get {
			return new Color(1f, 0f, 0.6f, 1f);
		}
	}
	
	public enum BlendMode {
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
		Luminosity
	}

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
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		if(hex.Length == 8) {
			byte a = byte.Parse(hex.Substring(6,2), System.Globalization.NumberStyles.HexNumber);
			return new Color32(r, g, b, a);
		} else {
			return new Color32(r, g, b, 255);
		}
	}

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

	public static Color Grayscale (Color color) {
		float gray = color.grayscale;
		return new Color(gray,gray,gray);
	}
	
	public static Color Average(this IList<Color> _colors){
		if(_colors.Count == 0)Debug.LogError("Array length is 0!");
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
			colorArray[i] = ColorX.ToGrayscaleColor(_floats[i]);
		return colorArray;
	}

	public static Color[] ToGrayscale(Color[] _colors){
		Color[] colorArray = new Color[_colors.Length];
		for(int i = 0; i < _colors.Length; i++){
			colorArray[i] = ColorX.Grayscale(_colors[i]);
		}
		return colorArray;
	}
	
	public static Color HueShift (this Color color, float amount) {
		HSLColor hslColor = (HSLColor)color;
		hslColor.h += amount;
		return hslColor;
	}
	
	public static Color Saturate (this Color color, float amount) {
		HSLColor hslColor = (HSLColor)color;
		hslColor.s += amount;
		return hslColor;
	}
	
	public static Color Lighten (this Color color, float amount) {
		HSLColor hslColor = (HSLColor)color;
		hslColor.l += amount;
		return hslColor;
	}
	
	public static Color Darken (this Color color, float amount) {
		HSLColor hslColor = (HSLColor)color;
		hslColor.l -= amount;
		return hslColor;
	}
	
	public static Color WithLightness (this Color color, float amount) {
		HSLColor hslColor = (HSLColor)color;
		hslColor.l = amount;
		return hslColor;
	}
	
	public static Color Blend(Color color1, Color color2, float lerp, BlendMode blendMode){
		if(lerp == 0) return color1;
		else if(lerp == 1) return color2;
        switch (blendMode) {
			case BlendMode.Normal:
				return Color.Lerp(color1, color2, lerp);
			case BlendMode.Additive:
				return ColorX.BlendAdditive(color1, color2, lerp);
			case BlendMode.Multiply:
				return ColorX.BlendMultiply(color1, color2, lerp);
			case BlendMode.Screen:
				return ColorX.BlendScreen(color1, color2);
			case BlendMode.Overlay:
				return ColorX.BlendOverlay(color1, color2);
			case BlendMode.Darken:
				return ColorX.BlendDarken(color1, color2);
			case BlendMode.Lighten:
				return ColorX.BlendLighten(color1, color2);
			case BlendMode.Difference:
				return ColorX.BlendDifference(color1, color2);
			case BlendMode.Hue:
				return ColorX.BlendHue(color1, color2);
			case BlendMode.Saturation:
				return ColorX.BlendSaturation(color1, color2);
			case BlendMode.Color:
				return ColorX.BlendColor(color1, color2);
			case BlendMode.Luminosity:
				return ColorX.BlendLuminosity(color1, color2);
			default:
				Debug.LogError("ColorX.BlendMode "+blendMode+" not recognized");
				return Color.Lerp(color1, color2, lerp);		
		}
	}
	
	public static Color BlendAdditive(Color color1, Color color2, float lerp = 1f){
		return new Color(color1.r + color2.r * lerp, color1.g + color2.g * lerp, color1.b + color2.b * lerp, color1.a + color2.a * lerp);
	}

	public static Color BlendMultiply(Color color1, Color color2, float lerp = 1f){
		return new Color(color1.r * color2.r * lerp, color1.g * color2.g * lerp, color1.b * color2.b * lerp, color1.a * color2.a * lerp);
	}

	public static Color BlendScreen(Color color1, Color color2){
		//outputColor = new Color((1 - ((1 - color1.r) * (1 - color2.r))), (1 - ((1 - color1.g) * (1 - color2.g))), (1 - ((1 - color1.b) * (1 - color2.b))), (1 - ((1 - color1.a) * (1 - color2.a))));
		return new Color(color1.r + color2.r - (color1.r * color2.r), color1.g + color2.g - (color1.g * color2.g), color1.b + color2.b - (color1.b * color2.b), color1.a + color2.a - (color1.a * color2.a));
	}

	public static Color BlendOverlay(Color color1, Color color2){
		/*if(heightmapPixel<0.5f){
			grayscale = Mathf.Lerp(grayscale, grayscale * (heightmapPixel * 2), _heightmaps[i].strength);
		} else {
			grayscale = (heightmapPixel * ((1-grayscale)*2)) + (grayscale - (1-grayscale));
		}*/
		return new Color(color2.r, color2.g, color2.b, color2.a);
	}

	public static Color BlendLighten(Color color1, Color color2){
		return new Color(Mathf.Max(color1.r, color2.r), Mathf.Max(color1.g, color2.g), Mathf.Max(color1.b, color2.b), Mathf.Max(color1.a, color2.a));
	}

	public static Color BlendDarken(Color color1, Color color2){
		return new Color(Mathf.Min(color1.r, color2.r), Mathf.Min(color1.g, color2.g), Mathf.Min(color1.b, color2.b), Mathf.Min(color1.a, color2.a));
	}

	public static Color BlendDifference(Color color1, Color color2){
		Color lighter;
		Color darker;

		if(((HSLColor)color1).l > ((HSLColor)color2).l){
			lighter = color1;
			darker = color2;
		} else {
			darker = color1;
			lighter = color2;
		}

		return new Color(lighter.r - darker.r, lighter.g - darker.g, lighter.b - darker.b, 1);
	}

	//Changes the hue of the lower layer to the hue of the upper layer
	public static Color BlendHue(Color color1, Color color2){
		HSLColor hslColor1 = (HSLColor)color1;
		HSLColor hslColor2 = (HSLColor)color2;
		HSLColor hslColor = new HSLColor(hslColor2.h, hslColor1.s, hslColor1.l);
		return (Color)hslColor;
	}

	//Changes the saturation of the lower layer to the saturation of the upper layer
	public static Color BlendSaturation(Color color1, Color color2){
		HSLColor hslColor1 = (HSLColor)color1;
		HSLColor hslColor2 = (HSLColor)color2;
		HSLColor hslColor = new HSLColor(hslColor1.h, hslColor2.s, hslColor1.l);
		return (Color)hslColor;
	}

	public static Color BlendColor(Color color1, Color color2){
		HSLColor hslColor1 = (HSLColor)color1;
		HSLColor hslColor2 = (HSLColor)color2;
		HSLColor hslColor = new HSLColor(hslColor2.h, hslColor2.s, hslColor1.l);
		return (Color)hslColor;
		//Color changes the hue and saturation of the lower layer to the hue and saturation of the upper layer
	}

	public static Color BlendLuminosity(Color color1, Color color2){
		HSLColor hslColor1 = (HSLColor)color1;
		HSLColor hslColor2 = (HSLColor)color2;
		HSLColor hslColor = new HSLColor(hslColor1.h, hslColor1.s, hslColor2.l);
		return (Color)hslColor;
		//Changes the luminosity of the lower layer to the luminosity of the upper layer
	}
}