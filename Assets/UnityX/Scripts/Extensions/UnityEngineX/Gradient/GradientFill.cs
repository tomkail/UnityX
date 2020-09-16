using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GradientFill {
	public static Color[] Create (GradientX.GradientType gradientType, Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height) {
		switch (gradientType){
		    case GradientX.GradientType.Linear:
			return Linear(gradient, startPosition, endPosition, width, height);
		    case GradientX.GradientType.Radial:
			return Radial(gradient, startPosition, endPosition, width, height);
			case GradientX.GradientType.Conical:
			return Conical(gradient, startPosition, endPosition, width, height);
			case GradientX.GradientType.Reflected:
			return Reflected(gradient, startPosition, endPosition, width, height);
			default:
			return Conical(gradient, startPosition, endPosition, width, height);
		}
	}

	public static Color[] Linear(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
		int numPixels = width * height;
		Color[] pixels = new Color[numPixels];
		float widthReciprocal = 1f/MathX.Clamp1Infinity(width-1);
		float heightReciprocal = 1f/MathX.Clamp1Infinity(height-1);
		
		for(int y = 0; y < height; y++){
			for(int x = 0; x < width; x++){
				Vector2 point = new Vector2(x * widthReciprocal, y * heightReciprocal);
				float distance = Vector2.Dot(point - endPosition, startPosition - endPosition) / ((endPosition-startPosition).sqrMagnitude);
				pixels[y * width + x] = gradient.Evaluate(distance);
			}
		}

		return pixels;
	}

	public static Color[] Radial(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
		int numPixels = width * height;
		Color[] pixels = new Color[numPixels];
		float widthReciprocal = 1f/MathX.Clamp1Infinity(width-1);
		float heightReciprocal = 1f/MathX.Clamp1Infinity(height-1);
		float length = Vector2.Distance(startPosition, endPosition);

		for(int y = 0; y < height; y++){
			for(int x = 0; x < width; x++){
				float tmpRadius = Vector2.Distance(new Vector2(x * widthReciprocal, y * heightReciprocal), startPosition);
				pixels[y * width + x] = gradient.Evaluate(tmpRadius / length);
			}
		}

		return pixels;
	}

	public static Color[] Conical(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
		int numPixels = width * height;
		Color[] pixels = new Color[numPixels];
		float widthReciprocal = 1f/MathX.Clamp1Infinity(width-1);
		float heightReciprocal = 1f/MathX.Clamp1Infinity(height-1);
		float degrees = Vector2X.DegreesBetween(startPosition, endPosition);
		
		for(int y = 0; y < height; y++){
			for(int x = 0; x < width; x++){
				float a = Mathf.Atan2(y * heightReciprocal - startPosition.y, x * widthReciprocal - startPosition.x);
				a += (degrees+180) * Mathf.Deg2Rad;
				a /= (Mathf.PI * 2);
				a+=0.5f;
				a = Mathf.Repeat(a,1f);
				pixels[y * width + x] = gradient.Evaluate(a);
			}
		}

		return pixels;
	}

	public static Color[] Reflected(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
		int numPixels = width * height;
		Color[] pixels = new Color[numPixels];
		float widthReciprocal = 1f/MathX.Clamp1Infinity(width-1);
		float heightReciprocal = 1f/MathX.Clamp1Infinity(height-1);
		
		for(int y = 0; y < height; y++){
			for(int x = 0; x < width; x++){
				Vector2 point = new Vector2(x * widthReciprocal, y * heightReciprocal);
				float distance = NormalizedDistance(startPosition, endPosition, point);
				pixels[y * width + x] = gradient.Evaluate(distance);
			}
		}

		return pixels;
	}

	/// <summary>
	/// Returns a normalized value between 0 and 1 for the distance on a line defined by a,b
	/// </summary>
	/// <param name="a">Start point of line.</param>
	/// <param name="b">End point of line.</param>
	/// <param name="value">Point on line to find.</param>
	public static float NormalizedDistance(Vector2 a, Vector2 b, Vector2 point) {
	   return (Vector2.Dot(point - a, b - a) / ((a-b).sqrMagnitude)).Abs();
	}
}