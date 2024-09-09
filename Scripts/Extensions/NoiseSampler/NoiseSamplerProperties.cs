using System;

[Serializable]
public struct NoiseSamplerProperties {

	public static NoiseSamplerProperties standard => new(0.1f, 1, 2, 0.5f);

	public float frequency;
	public int octaves;
	public float lacunarity;
	public float persistence;

	public NoiseSamplerProperties (float _frequency) {
		frequency = _frequency;
		octaves = 1;
		lacunarity = 2f;
		persistence = 0.5f;
	}
	public NoiseSamplerProperties (float _frequency, int _octaves, float _lacunarity, float _persistence) {
		frequency = _frequency;
		octaves = _octaves;
		lacunarity = _lacunarity;
		persistence = _persistence;
	}
}