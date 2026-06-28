using System;

[Serializable]
public struct NoiseSamplerProperties : IEquatable<NoiseSamplerProperties> {

	public static NoiseSamplerProperties standard => new(0.1f, 1, 2, 0.5f);

	public float frequency;
	// Fractional: the integer part is summed at full amplitude, the fractional part fades in the next octave.
	public float octaves;
	public float lacunarity;
	public float persistence;

	public NoiseSamplerProperties (float _frequency) {
		frequency = _frequency;
		octaves = 1f;
		lacunarity = 2f;
		persistence = 0.5f;
	}
	public NoiseSamplerProperties (float _frequency, float _octaves, float _lacunarity, float _persistence) {
		frequency = _frequency;
		octaves = _octaves;
		lacunarity = _lacunarity;
		persistence = _persistence;
	}

	public bool Equals (NoiseSamplerProperties other) =>
		frequency == other.frequency && octaves == other.octaves && lacunarity == other.lacunarity && persistence == other.persistence;
	public override bool Equals (object obj) => obj is NoiseSamplerProperties other && Equals(other);
	public override int GetHashCode () => HashCode.Combine(frequency, octaves, lacunarity, persistence);
	public static bool operator == (NoiseSamplerProperties a, NoiseSamplerProperties b) => a.Equals(b);
	public static bool operator != (NoiseSamplerProperties a, NoiseSamplerProperties b) => !a.Equals(b);
}