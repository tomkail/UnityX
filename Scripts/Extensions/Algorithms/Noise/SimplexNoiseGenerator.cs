using UnityEngine;

public class SimplexNoiseGenerator {
	
	// Shared setup for Generate/GenerateRepeating. Computes the loop-invariant constants that are
	// identical between the two methods (only the per-sample noise coordinate math differs).
	static void GetSetup (Vector2Int size, float scale, float contrast, float height, out int mapArrayLength, out float _contrast, out float oneMinusContrastReciprocal, out float halfContrast, out float contrastRelativeHeightModifier, out Vector2 scaledSizeReciprocal, out float sizeXReciprocal) {
		mapArrayLength = size.x*size.y;
		_contrast = (contrast-0.5f) * 2;
		// Local fix: guard against contrast == 1 (oneMinusContrast == 0) causing a divide-by-zero.
		float oneMinusContrast = Mathf.Max(1f - _contrast, 1e-6f);
		oneMinusContrastReciprocal = 1f / oneMinusContrast;
		halfContrast = _contrast * 0.5f;
		contrastRelativeHeightModifier = height / oneMinusContrast;
		scaledSizeReciprocal = new Vector2(1f/(size.x * scale), 1f/(size.y * scale));
		sizeXReciprocal = 1f/size.x;
	}

	public static float[] Generate (Vector2Int size, Vector3 position, float scale, float offset, float contrast, float height, bool clamp = false) {
		GetSetup(size, scale, contrast, height, out int mapArrayLength, out float _contrast, out float oneMinusContrastReciprocal, out float halfContrast, out float contrastRelativeHeightModifier, out Vector2 scaledSizeReciprocal, out float sizeXReciprocal);
		float[] map = new float[mapArrayLength];
		float xCoord, yCoord, sample;

		for(int i = 0; i < mapArrayLength; i++){
			xCoord = (-position.x + i%size.x) * scaledSizeReciprocal.x;
			yCoord = (-position.y + Mathf.Floor(i*sizeXReciprocal)) * scaledSizeReciprocal.y;
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
	
	public static float[] GenerateRepeating (Vector2Int size, Vector3 position, float scale, float offset, float contrast, float height, bool clamp = false) {
		GetSetup(size, scale, contrast, height, out int mapArrayLength, out float _contrast, out float oneMinusContrastReciprocal, out float halfContrast, out float contrastRelativeHeightModifier, out Vector2 scaledSizeReciprocal, out float sizeXReciprocal);
		float[] map = new float[mapArrayLength];
		float xCoord, yCoord, sample;

		float radius = Mathf.Min(size.x, size.y);
		for(int i = 0; i < mapArrayLength; i++){
			xCoord = (-position.x + i%size.x) * scaledSizeReciprocal.x;
			yCoord = (-position.y + Mathf.Floor(i*sizeXReciprocal)) * scaledSizeReciprocal.y;
			
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
