using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public static class GradientX {
	
	public enum GradientType {
		Radial,
		Linear,
		Conical,
		Reflected
	}

	public static Gradient blackToWhite {
		get {
			return GradientX.Create(Color.black, Color.white);
		}
	}
	public static Gradient whiteToBlack {
		get {
			return GradientX.Create(Color.white, Color.black);
		}
	}
	
	public static Gradient blackToClear {
		get {
			return GradientX.Create(Color.black, Color.black.WithAlpha(0));
		}
	}
	public static Gradient whiteToClear {
		get {
			return GradientX.Create(Color.white, Color.white.WithAlpha(0));
		}
	}
	
	public static Gradient clearToBlack {
		get {
			return GradientX.Create(Color.black.WithAlpha(0), Color.black);
		}
	}

	public static Gradient clearToWhite {
		get {
			return GradientX.Create(Color.white.WithAlpha(0), Color.white);
		}
	}

	public static Gradient Reverse( this Gradient g ){
		GradientColorKey[] colorKeys = g.colorKeys;
		GradientAlphaKey[] alphaKeys = g.alphaKeys;
		
		for(int i = 0; i < colorKeys.Length; i++) {
			colorKeys[i].time = 1f-colorKeys[i].time;
		}
		for(int i = 0; i < alphaKeys.Length; i++) {
			alphaKeys[i].time = 1f-alphaKeys[i].time;
		}
		return GradientX.Create(colorKeys, alphaKeys);
	}

	public static Color Random( this Gradient g ){
		return g.Evaluate(UnityEngine.Random.value);
	}

	public static Gradient Create(GradientColorKey[] colorKeys, GradientAlphaKey[] alphaKeys = null) {
		Gradient gradient = new Gradient();
		if (alphaKeys.IsNullOrEmpty()) {
			alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey (1, 0) };
		}
		gradient.SetKeys(colorKeys, alphaKeys);
		return gradient;
	}
	
	public static Gradient Create(Color color, float time = 0f){
		Gradient gradient = new Gradient();
		GradientColorKey[] colorKeys = new GradientColorKey[1];
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[1];
		
		colorKeys[0].color = color;
		colorKeys[0].time = time;
		alphaKeys[0].alpha = color.a;
		alphaKeys[0].time = time;
		
		gradient.SetKeys(colorKeys, alphaKeys);
		
		return gradient;
	}
	
	public static Gradient Create(Color startColor, Color stopColor, float start = 0f, float stop = 1f){
		Gradient gradient = new Gradient();
		GradientColorKey[] colorKeys = new GradientColorKey[2];
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

		colorKeys[0].color = startColor;
		colorKeys[0].time = start;
		alphaKeys[0].alpha = startColor.a;
		alphaKeys[0].time = start;

		colorKeys[1].color = stopColor;
		colorKeys[1].time = stop;
		alphaKeys[1].alpha = stopColor.a;
		alphaKeys[1].time = stop;

		gradient.SetKeys(colorKeys, alphaKeys);

		return gradient;
	}

	public static void AddColorToGradientAtTime(ref Gradient gradient, Color color, float time, bool forceIfMaxed = true){
		List<GradientColorKey> colorKeys = gradient.colorKeys.ToList();

		if(colorKeys.Count < 8){
			bool found = false;
			for(int i = 0; i < colorKeys.Count; i++){
				if(MathX.NearlyEqual(colorKeys[i].time, time)){
					colorKeys[i] = new GradientColorKey(color, time);
					found = true;
					break;
				} else if(colorKeys[i].time >= time){
					colorKeys.Insert(i, new GradientColorKey(color, time));
					found = true;
					break;
				}
			}
			if(!found){
				//time makes this the last keyframe
				colorKeys.Add(new GradientColorKey(color, time));
			}
		} else {
			if(forceIfMaxed){
				int closestPoint = 0;
				for(int i = 0; i < colorKeys.Count; i++){
					if(Mathf.Abs(colorKeys[i].time-time) < Mathf.Abs(colorKeys[closestPoint].time-time)) closestPoint = i;
				}
				colorKeys[closestPoint] = new GradientColorKey(color, time);
			}
		}

		gradient.colorKeys = colorKeys.ToArray();
	}

	public static void AddAlphaToGradientAtTime(ref Gradient gradient, float alpha, float time, bool forceIfMaxed = true){
		List<GradientAlphaKey> alphaKeys = gradient.alphaKeys.ToList();

		if(alphaKeys.Count < 8){
			bool found = false;
			for(int i = 0; i < alphaKeys.Count; i++){
				if(MathX.NearlyEqual(alphaKeys[i].time, time)){
					alphaKeys[i] = new GradientAlphaKey(alpha, time);
					found = true;
					break;
				} else if(alphaKeys[i].time >= time){
					alphaKeys.Insert(i, new GradientAlphaKey(alpha, time));
					found = true;
					break;
				}
			}
			if(!found){
				alphaKeys.Add(new GradientAlphaKey(alpha, time));
			}
		} else {
			if(forceIfMaxed){
				int closestPoint = 0;
				for(int i = 0; i < alphaKeys.Count; i++){
					if(Mathf.Abs(alphaKeys[i].time-time) < Mathf.Abs(alphaKeys[closestPoint].time-time))closestPoint = i;
				}
				alphaKeys[closestPoint] = new GradientAlphaKey(alpha, time);
			}
		}

		gradient.alphaKeys = alphaKeys.ToArray();
	}


	private static float[] GetGradientKeyPositions(int numKeys = 8){
		numKeys = (int)Mathf.Clamp(numKeys, 1,9);
		float[] keyTimes = new float[numKeys];
		for(int i = 0; i < numKeys; i++){
			keyTimes[i] = i / (float)(numKeys-1);
		}
		return keyTimes;
	}
	
	public static Gradient Lerp(Gradient from, Gradient to, float t, int numKeys = 8){
		return GradientX.Lerp(from, to, t, GradientX.GetGradientKeyPositions(numKeys));
	}
	
	public static Gradient Lerp(Gradient from, Gradient to, float t, float[] keyTimes){
		Gradient gradient = new Gradient();
		GradientColorKey[] colorKeys = new GradientColorKey[keyTimes.Length];
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[keyTimes.Length];
		
		Color fromColor, toColor;
		for(int i = 0; i < keyTimes.Length; i++){
			fromColor = from.Evaluate(keyTimes[i]);
			toColor = to.Evaluate(keyTimes[i]);
			colorKeys[i].color = Color.Lerp (fromColor, toColor, t);
			colorKeys[i].time = keyTimes[i];
			alphaKeys[i].alpha = Mathf.Lerp (fromColor.a, toColor.a, t);
			alphaKeys[i].time = keyTimes[i];
		}
		
		gradient.SetKeys(colorKeys, alphaKeys);
		return gradient;
	}
	
	
	public static Gradient Add(Gradient from, Gradient to, int numKeys = 8){
		return GradientX.Add(from, to, GradientX.GetGradientKeyPositions(numKeys));
	}
	
	public static Gradient Add(Gradient from, Gradient to, float[] keyTimes){
		Gradient gradient = new Gradient();
		GradientColorKey[] colorKeys = new GradientColorKey[keyTimes.Length];
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[keyTimes.Length];
		
		Color fromColor, toColor;
		for(int i = 0; i < keyTimes.Length; i++){
			fromColor = from.Evaluate(keyTimes[i]);
			toColor = to.Evaluate(keyTimes[i]);
			colorKeys[i].color = fromColor + toColor;
			colorKeys[i].time = keyTimes[i];
			alphaKeys[i].alpha = fromColor.a + toColor.a;
			alphaKeys[i].time = keyTimes[i];
		}
		
		gradient.SetKeys(colorKeys, alphaKeys);
		return gradient;
	}
	
	
	public static Gradient Subtract(Gradient from, Gradient to, int numKeys = 8){
		return GradientX.Subtract(from, to, GradientX.GetGradientKeyPositions(numKeys));
	}
	
	public static Gradient Subtract(Gradient from, Gradient to, float[] keyTimes){
		Gradient gradient = new Gradient();
		GradientColorKey[] colorKeys = new GradientColorKey[keyTimes.Length];
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[keyTimes.Length];
		
		Color fromColor, toColor;
		for(int i = 0; i < keyTimes.Length; i++){
			fromColor = from.Evaluate(keyTimes[i]);
			toColor = to.Evaluate(keyTimes[i]);
			colorKeys[i].color = fromColor - toColor;
			colorKeys[i].time = keyTimes[i];
			alphaKeys[i].alpha = fromColor.a - toColor.a;
			alphaKeys[i].time = keyTimes[i];
		}
		
		gradient.SetKeys(colorKeys, alphaKeys);
		return gradient;
	}
}

