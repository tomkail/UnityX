using UnityEngine;

namespace UnityX.Colors {
	// Photoshop-style colour blend modes in their own portable module, so consumers (e.g. the Tween system's
	// ColorTween) can reference blending without pulling in all of ColorX.
	// HSLColor (used by the HSL-based modes) lives alongside this in the same module.
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

	public static class ColorBlend {
		public static Color Blend(Color color1, Color color2, float lerp, BlendMode blendMode){
			if(lerp == 0) return color1;
			else if(lerp == 1) return color2;
			switch (blendMode) {
				case BlendMode.Normal:
					return Color.Lerp(color1, color2, lerp);
				case BlendMode.Additive:
					return BlendAdditive(color1, color2, lerp);
				case BlendMode.Multiply:
					return BlendMultiply(color1, color2, lerp);
				case BlendMode.Screen:
					return BlendScreen(color1, color2);
				case BlendMode.Overlay:
					return BlendOverlay(color1, color2);
				case BlendMode.Darken:
					return BlendDarken(color1, color2);
				case BlendMode.Lighten:
					return BlendLighten(color1, color2);
				case BlendMode.Difference:
					return BlendDifference(color1, color2);
				case BlendMode.Hue:
					return BlendHue(color1, color2);
				case BlendMode.Saturation:
					return BlendSaturation(color1, color2);
				case BlendMode.Color:
					return BlendColor(color1, color2);
				case BlendMode.Luminosity:
					return BlendLuminosity(color1, color2);
				default:
					Debug.LogError("ColorBlend.BlendMode "+blendMode+" not recognized");
					return Color.Lerp(color1, color2, lerp);
			}
		}

		// Convenience helper: a weighted add, NOT just color1 + color2 (the lerp param scales color2's contribution).
		public static Color BlendAdditive(Color color1, Color color2, float lerp = 1f){
			return new Color(color1.r + color2.r * lerp, color1.g + color2.g * lerp, color1.b + color2.b * lerp, color1.a + color2.a * lerp);
		}

		// Convenience helper: a weighted multiply, NOT just color1 * color2 (the lerp param scales the product per channel).
		public static Color BlendMultiply(Color color1, Color color2, float lerp = 1f){
			return new Color(color1.r * color2.r * lerp, color1.g * color2.g * lerp, color1.b * color2.b * lerp, color1.a * color2.a * lerp);
		}

		public static Color BlendScreen(Color color1, Color color2){
			//outputColor = new Color((1 - ((1 - color1.r) * (1 - color2.r))), (1 - ((1 - color1.g) * (1 - color2.g))), (1 - ((1 - color1.b) * (1 - color2.b))), (1 - ((1 - color1.a) * (1 - color2.a))));
			return new Color(color1.r + color2.r - (color1.r * color2.r), color1.g + color2.g - (color1.g * color2.g), color1.b + color2.b - (color1.b * color2.b), color1.a + color2.a - (color1.a * color2.a));
		}

		public static Color BlendOverlay(Color color1, Color color2){
			// Standard per-channel overlay: base < 0.5 → 2·base·blend, else → 1 − 2·(1−base)(1−blend).
			return new Color(Overlay(color1.r, color2.r), Overlay(color1.g, color2.g), Overlay(color1.b, color2.b), Overlay(color1.a, color2.a));
		}

		static float Overlay(float b, float t) => b < 0.5f ? 2f * b * t : 1f - 2f * (1f - b) * (1f - t);

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
			HSLColor hslColor1 = color1;
			HSLColor hslColor2 = color2;
			HSLColor hslColor = new HSLColor(hslColor2.h, hslColor1.s, hslColor1.l);
			return hslColor;
		}

		//Changes the saturation of the lower layer to the saturation of the upper layer
		public static Color BlendSaturation(Color color1, Color color2){
			HSLColor hslColor1 = color1;
			HSLColor hslColor2 = color2;
			HSLColor hslColor = new HSLColor(hslColor1.h, hslColor2.s, hslColor1.l);
			return hslColor;
		}

		public static Color BlendColor(Color color1, Color color2){
			HSLColor hslColor1 = color1;
			HSLColor hslColor2 = color2;
			HSLColor hslColor = new HSLColor(hslColor2.h, hslColor2.s, hslColor1.l);
			return hslColor;
			//Color changes the hue and saturation of the lower layer to the hue and saturation of the upper layer
		}

		public static Color BlendLuminosity(Color color1, Color color2){
			HSLColor hslColor1 = color1;
			HSLColor hslColor2 = color2;
			HSLColor hslColor = new HSLColor(hslColor1.h, hslColor1.s, hslColor2.l);
			return hslColor;
			//Changes the luminosity of the lower layer to the luminosity of the upper layer
		}
	}
}
