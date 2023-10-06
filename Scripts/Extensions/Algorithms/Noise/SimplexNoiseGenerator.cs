using UnityEngine;
using System.Collections;
using UnityX.Geometry;

public class SimplexNoiseGenerator {
	
	public static float[] Generate (Point size, Vector3 position, float scale, float offset, float contrast, float height, bool clamp = false) {
		int mapArrayLength = size.area;
		float[] map = new float[mapArrayLength];
		float xCoord, yCoord, sample;
		float _contrast = (contrast-0.5f) * 2;
		float oneMinusContrast = 1f - _contrast;
		float oneMinusContrastReciprocal = 1f / oneMinusContrast;
		float halfContrast = _contrast * 0.5f;
		float contrastRelativeHeightModifier = height / oneMinusContrast;
		Vector2 scaledSizeReciprocal = new Vector2(1f/(size.x * scale), 1f/(size.y * scale));
		float sizeXReciprocal = 1f/size.x;
		
		for(int i = 0; i < mapArrayLength; i++){
			xCoord = (-position.x + i%size.x) * scaledSizeReciprocal.x;
			yCoord = (-position.y + Mathf.Floor((float)i*sizeXReciprocal)) * scaledSizeReciprocal.y;
			sample = (SimplexNoise.Noise(xCoord, yCoord, position.z) * 0.5f) + 0.5f;

			sample += offset;// * -contrast;
			if(_contrast < 0f) {
				sample = Mathf.Lerp(sample, 0.5f, -_contrast);
			} else {
				sample -= halfContrast;
				sample *= oneMinusContrastReciprocal;
			}
			//sample += height;// * (1f/(1f-((contrast * 2) - 1f)));
			sample += contrastRelativeHeightModifier;
			if(clamp)
				sample = Mathf.Clamp01(sample);
			map[i] = sample;
		}
		
		return map;
	}
	
	public static float[] GenerateRepeating (Point size, Vector3 position, float scale, float offset, float contrast, float height, bool clamp = false) {
		int mapArrayLength = size.area;
		float[] map = new float[mapArrayLength];
		float xCoord, yCoord, sample;
		float _contrast = (contrast-0.5f) * 2;
		float oneMinusContrast = 1f - _contrast;
		float oneMinusContrastReciprocal = 1f / oneMinusContrast;
		float halfContrast = _contrast * 0.5f;
		float contrastRelativeHeightModifier = height / oneMinusContrast;
		Vector2 scaledSizeReciprocal = new Vector2(1f/(size.x * scale), 1f/(size.y * scale));
		float sizeXReciprocal = 1f/size.x;
		
		float radius = Mathf.Min(size.x, size.y);
		for(int i = 0; i < mapArrayLength; i++){
			xCoord = (-position.x + i%size.x) * scaledSizeReciprocal.x;
			yCoord = (-position.y + Mathf.Floor((float)i*sizeXReciprocal)) * scaledSizeReciprocal.y;
			
//			xCoord = (i%size.x) * scaledSizeReciprocal.x;
//			yCoord = Mathf.Floor((float)i*sizeXReciprocal);
			
//			xCoord = i%size.x;
//			yCoord = i/size.x<<0;
//			xCoord = xCoord/size.x;
//			yCoord = yCoord/size.y;
			
			float fRdx = xCoord * 2*Mathf.PI;
			float fRdy = yCoord * 2*Mathf.PI;
			float a = (radius/size.x) * Mathf.Sin(fRdx);
			float b = (radius/size.x) * Mathf.Cos(fRdx);
			float c = (radius/size.y) * Mathf.Sin(fRdy);
			float d = (radius/size.y) * Mathf.Cos(fRdy);
			sample = (SimplexNoise.Noise(a, b, c, d) * 0.5f) + 0.5f;
			
			sample += offset;// * -contrast;
			if(_contrast < 0f) {
				sample = Mathf.Lerp(sample, 0.5f, -_contrast);
			} else {
				sample -= halfContrast;
				sample *= oneMinusContrastReciprocal;
			}
			//sample += height;// * (1f/(1f-((contrast * 2) - 1f)));
			sample += contrastRelativeHeightModifier;
			if(clamp)
				sample = Mathf.Clamp01(sample);
			map[i] = sample;
		}
		
		return map;
	}
}
