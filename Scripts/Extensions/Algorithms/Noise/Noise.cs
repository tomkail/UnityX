using UnityEngine;

public delegate NoiseSample NoiseMethod (Vector3 point, float frequency);

public enum NoiseMethodType {
	Value,
	Perlin
}

public static class Noise {

	public static NoiseMethod[] valueMethods = {
		Value1D,
		Value2D,
		Value3D
	};

	public static NoiseMethod[] perlinMethods = {
		Perlin1D,
		Perlin2D,
		Perlin3D
	};

	public static NoiseMethod[][] methods = {
		valueMethods,
		perlinMethods
	};

	private static int[] hash = {
		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		 57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		 74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		 60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		 65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		 52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		 81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,

		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		 57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		 74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		 60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		 65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		 52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		 81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
	};

	private const int hashMask = 255;

	private static float[] gradients1D = {
		1f, -1f
	};

	private const int gradientsMask1D = 1;
	
	private static Vector2[] gradients2D = {
		new( 1f, 0f),
		new(-1f, 0f),
		new( 0f, 1f),
		new( 0f,-1f),
		new Vector2( 1f, 1f).normalized,
		new Vector2(-1f, 1f).normalized,
		new Vector2( 1f,-1f).normalized,
		new Vector2(-1f,-1f).normalized
	};
	
	private const int gradientsMask2D = 7;

	private static Vector3[] gradients3D = {
		new( 1f, 1f, 0f),
		new(-1f, 1f, 0f),
		new( 1f,-1f, 0f),
		new(-1f,-1f, 0f),
		new( 1f, 0f, 1f),
		new(-1f, 0f, 1f),
		new( 1f, 0f,-1f),
		new(-1f, 0f,-1f),
		new( 0f, 1f, 1f),
		new( 0f,-1f, 1f),
		new( 0f, 1f,-1f),
		new( 0f,-1f,-1f),
		
		new( 1f, 1f, 0f),
		new(-1f, 1f, 0f),
		new( 0f,-1f, 1f),
		new( 0f,-1f,-1f)
	};
	
	private const int gradientsMask3D = 15;

	private static float Dot (Vector2 g, float x, float y) {
		return g.x * x + g.y * y;
	}

	private static float Dot (Vector3 g, float x, float y, float z) {
		return g.x * x + g.y * y + g.z * z;
	}
	
	private static float Smooth (float t) {
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	private static float SmoothDerivative (float t) {
		return 30f * t * t * (t * (t - 2f) + 1f);
	}

	private static float sqr2 = Mathf.Sqrt(2f);

	public static NoiseSample Value1D (Vector3 point, float frequency) {
		point *= frequency;
		int i0 = Mathf.FloorToInt(point.x);
		float t = point.x - i0;
		i0 &= hashMask;
		int i1 = i0 + 1;

		int h0 = hash[i0];
		int h1 = hash[i1];

		float dt = SmoothDerivative(t);
		t = Smooth(t);

		float a = h0;
		float b = h1 - h0;

		NoiseSample sample;
		sample.value = a + b * t;
		sample.derivative.x = b * dt;
		sample.derivative.y = 0f;
		sample.derivative.z = 0f;
		sample.derivative *= frequency;
		return sample * (1f / hashMask);
	}

	public static NoiseSample Value2D (Vector3 point, float frequency) {
		point *= frequency;
		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);
		float tx = point.x - ix0;
		float ty = point.y - iy0;
		ix0 &= hashMask;
		iy0 &= hashMask;
		int ix1 = ix0 + 1;
		int iy1 = iy0 + 1;

		int h0 = hash[ix0];
		int h1 = hash[ix1];
		int h00 = hash[h0 + iy0];
		int h10 = hash[h1 + iy0];
		int h01 = hash[h0 + iy1];
		int h11 = hash[h1 + iy1];

		float dtx = SmoothDerivative(tx);
		float dty = SmoothDerivative(ty);
		tx = Smooth(tx);
		ty = Smooth(ty);

		float a = h00;
		float b = h10 - h00;
		float c = h01 - h00;
		float d = h11 - h01 - h10 + h00;

		NoiseSample sample;
		sample.value = a + b * tx + (c + d * tx) * ty;
		sample.derivative.x = (b + d * ty) * dtx;
		sample.derivative.y = (c + d * tx) * dty;
		sample.derivative.z = 0f;
		sample.derivative *= frequency;
		return sample * (1f / hashMask);
	}

	public static NoiseSample Value3D (Vector3 point, float frequency) {
		point *= frequency;
		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);
		int iz0 = Mathf.FloorToInt(point.z);
		float tx = point.x - ix0;
		float ty = point.y - iy0;
		float tz = point.z - iz0;
		ix0 &= hashMask;
		iy0 &= hashMask;
		iz0 &= hashMask;
		int ix1 = ix0 + 1;
		int iy1 = iy0 + 1;
		int iz1 = iz0 + 1;

		int h0 = hash[ix0];
		int h1 = hash[ix1];
		int h00 = hash[h0 + iy0];
		int h10 = hash[h1 + iy0];
		int h01 = hash[h0 + iy1];
		int h11 = hash[h1 + iy1];
		int h000 = hash[h00 + iz0];
		int h100 = hash[h10 + iz0];
		int h010 = hash[h01 + iz0];
		int h110 = hash[h11 + iz0];
		int h001 = hash[h00 + iz1];
		int h101 = hash[h10 + iz1];
		int h011 = hash[h01 + iz1];
		int h111 = hash[h11 + iz1];

		float dtx = SmoothDerivative(tx);
		float dty = SmoothDerivative(ty);
		float dtz = SmoothDerivative(tz);
		tx = Smooth(tx);
		ty = Smooth(ty);
		tz = Smooth(tz);

		float a = h000;
		float b = h100 - h000;
		float c = h010 - h000;
		float d = h001 - h000;
		float e = h110 - h010 - h100 + h000;
		float f = h101 - h001 - h100 + h000;
		float g = h011 - h001 - h010 + h000;
		float h = h111 - h011 - h101 + h001 - h110 + h010 + h100 - h000;
		
		NoiseSample sample;
		sample.value = a + b * tx + (c + e * tx) * ty + (d + f * tx + (g + h * tx) * ty) * tz;
		sample.derivative.x = (b + e * ty + (f + h * ty) * tz) * dtx;
		sample.derivative.y = (c + e * tx + (g + h * tx) * tz) * dty;
		sample.derivative.z = (d + f * tx + (g + h * tx) * ty) * dtz;
		sample.derivative *= frequency;
		return sample * (1f / hashMask);
	}

	public static NoiseSample Perlin1D (Vector3 point, float frequency) {
		point *= frequency;
		int i0 = Mathf.FloorToInt(point.x);
		float t0 = point.x - i0;
		float t1 = t0 - 1f;
		i0 &= hashMask;
		int i1 = i0 + 1;
		
		float g0 = gradients1D[hash[i0] & gradientsMask1D];
		float g1 = gradients1D[hash[i1] & gradientsMask1D];

		float v0 = g0 * t0;
		float v1 = g1 * t1;

		float dt = SmoothDerivative(t0);
		float t = Smooth(t0);

		float a = v0;
		float b = v1 - v0;

		float da = g0;
		float db = g1 - g0;

		NoiseSample sample;
		sample.value = a + b * t;
		sample.derivative.x = da + db * t + b * dt;
		sample.derivative.y = 0f;
		sample.derivative.z = 0f;
		sample.derivative *= frequency;
		return sample * 2f;
	}
	
	public static NoiseSample Perlin2D (Vector3 point, float frequency) {
		point *= frequency;
		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);
		float tx0 = point.x - ix0;
		float ty0 = point.y - iy0;
		float tx1 = tx0 - 1f;
		float ty1 = ty0 - 1f;
		ix0 &= hashMask;
		iy0 &= hashMask;
		int ix1 = ix0 + 1;
		int iy1 = iy0 + 1;
		
		int h0 = hash[ix0];
		int h1 = hash[ix1];
		Vector2 g00 = gradients2D[hash[h0 + iy0] & gradientsMask2D];
		Vector2 g10 = gradients2D[hash[h1 + iy0] & gradientsMask2D];
		Vector2 g01 = gradients2D[hash[h0 + iy1] & gradientsMask2D];
		Vector2 g11 = gradients2D[hash[h1 + iy1] & gradientsMask2D];

		float v00 = Dot(g00, tx0, ty0);
		float v10 = Dot(g10, tx1, ty0);
		float v01 = Dot(g01, tx0, ty1);
		float v11 = Dot(g11, tx1, ty1);

		float dtx = SmoothDerivative(tx0);
		float dty = SmoothDerivative(ty0);
		float tx = Smooth(tx0);
		float ty = Smooth(ty0);

		float a = v00;
		float b = v10 - v00;
		float c = v01 - v00;
		float d = v11 - v01 - v10 + v00;

		Vector2 da = g00;
		Vector2 db = g10 - g00;
		Vector2 dc = g01 - g00;
		Vector2 dd = g11 - g01 - g10 + g00;

		NoiseSample sample;
		sample.value = a + b * tx + (c + d * tx) * ty;
		sample.derivative = da + db * tx + (dc + dd * tx) * ty;
		sample.derivative.x += (b + d * ty) * dtx;
		sample.derivative.y += (c + d * tx) * dty;
		sample.derivative.z = 0f;
		sample.derivative *= frequency;
		return sample * sqr2;
	}
	
	public static NoiseSample Perlin3D (Vector3 point, float frequency) {
		point *= frequency;
		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);
		int iz0 = Mathf.FloorToInt(point.z);
		float tx0 = point.x - ix0;
		float ty0 = point.y - iy0;
		float tz0 = point.z - iz0;
		float tx1 = tx0 - 1f;
		float ty1 = ty0 - 1f;
		float tz1 = tz0 - 1f;
		ix0 &= hashMask;
		iy0 &= hashMask;
		iz0 &= hashMask;
		int ix1 = ix0 + 1;
		int iy1 = iy0 + 1;
		int iz1 = iz0 + 1;
		
		int h0 = hash[ix0];
		int h1 = hash[ix1];
		int h00 = hash[h0 + iy0];
		int h10 = hash[h1 + iy0];
		int h01 = hash[h0 + iy1];
		int h11 = hash[h1 + iy1];
		Vector3 g000 = gradients3D[hash[h00 + iz0] & gradientsMask3D];
		Vector3 g100 = gradients3D[hash[h10 + iz0] & gradientsMask3D];
		Vector3 g010 = gradients3D[hash[h01 + iz0] & gradientsMask3D];
		Vector3 g110 = gradients3D[hash[h11 + iz0] & gradientsMask3D];
		Vector3 g001 = gradients3D[hash[h00 + iz1] & gradientsMask3D];
		Vector3 g101 = gradients3D[hash[h10 + iz1] & gradientsMask3D];
		Vector3 g011 = gradients3D[hash[h01 + iz1] & gradientsMask3D];
		Vector3 g111 = gradients3D[hash[h11 + iz1] & gradientsMask3D];

		float v000 = Dot(g000, tx0, ty0, tz0);
		float v100 = Dot(g100, tx1, ty0, tz0);
		float v010 = Dot(g010, tx0, ty1, tz0);
		float v110 = Dot(g110, tx1, ty1, tz0);
		float v001 = Dot(g001, tx0, ty0, tz1);
		float v101 = Dot(g101, tx1, ty0, tz1);
		float v011 = Dot(g011, tx0, ty1, tz1);
		float v111 = Dot(g111, tx1, ty1, tz1);

		float dtx = SmoothDerivative(tx0);
		float dty = SmoothDerivative(ty0);
		float dtz = SmoothDerivative(tz0);
		float tx = Smooth(tx0);
		float ty = Smooth(ty0);
		float tz = Smooth(tz0);

		float a = v000;
		float b = v100 - v000;
		float c = v010 - v000;
		float d = v001 - v000;
		float e = v110 - v010 - v100 + v000;
		float f = v101 - v001 - v100 + v000;
		float g = v011 - v001 - v010 + v000;
		float h = v111 - v011 - v101 + v001 - v110 + v010 + v100 - v000;

		Vector3 da = g000;
		Vector3 db = g100 - g000;
		Vector3 dc = g010 - g000;
		Vector3 dd = g001 - g000;
		Vector3 de = g110 - g010 - g100 + g000;
		Vector3 df = g101 - g001 - g100 + g000;
		Vector3 dg = g011 - g001 - g010 + g000;
		Vector3 dh = g111 - g011 - g101 + g001 - g110 + g010 + g100 - g000;

		NoiseSample sample;
		sample.value = a + b * tx + (c + e * tx) * ty + (d + f * tx + (g + h * tx) * ty) * tz;
		sample.derivative = da + db * tx + (dc + de * tx) * ty + (dd + df * tx + (dg + dh * tx) * ty) * tz;
		sample.derivative.x += (b + e * ty + (f + h * ty) * tz) * dtx;
		sample.derivative.y += (c + e * tx + (g + h * tx) * tz) * dty;
		sample.derivative.z += (d + f * tx + (g + h * tx) * ty) * dtz;
		sample.derivative *= frequency;
		return sample;
	}

	// Returns a value between -0.5 and 0.5.
	public static NoiseSample Sum (NoiseMethod method, Vector3 point, float frequency, int octaves, float lacunarity = 2, float persistence = 0.5f) {
		NoiseSample sum = new NoiseSample();
		float amplitude = 1f;
		float maxValue = 0;
		for (int o = 0; o < octaves; o++) {
			sum += method(point, frequency) * amplitude;
			maxValue += amplitude;

			amplitude *= persistence;
			frequency *= lacunarity;
		}
		return sum * (1f / maxValue);
	}
}