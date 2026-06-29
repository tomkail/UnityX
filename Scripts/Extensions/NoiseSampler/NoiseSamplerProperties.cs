using System;

// How fractal (multi-octave) noise is normalized. Octaves are statistically independent, so the choice trades off
// bounded range against constant perceived strength as octave count changes. Sum is the default (kept from canonical
// fBm); the integer values are mirrored in NoiseVectorField.compute, so don't reorder them.
public enum NoiseNormalization {
	// Divide by the sum of octave amplitudes (canonical fBm). The peak range is fixed regardless of octave count, but
	// because uncorrelated octaves rarely peak together the *typical* magnitude falls as octaves increase.
	Sum = 0,
	// Divide by the RMS of the octave amplitudes (sqrt of the sum of squares). Holds typical field strength roughly
	// constant across octave counts — octaves add detail without changing loudness. Peaks can exceed Sum's range.
	RootMeanSquare = 1,
	// No normalization: octave count and persistence also act as gain, so adding octaves makes the field stronger.
	None = 2,
}

[Serializable]
public struct NoiseSamplerProperties : IEquatable<NoiseSamplerProperties> {

	public static NoiseSamplerProperties standard => new(0.1f, 1, 2, 0.5f);

	public float frequency;
	// Fractional: the integer part is summed at full amplitude, the fractional part fades in the next octave.
	public float octaves;
	public float lacunarity;
	public float persistence;
	public NoiseNormalization normalization;

	public NoiseSamplerProperties (float _frequency) {
		frequency = _frequency;
		octaves = 1f;
		lacunarity = 2f;
		persistence = 0.5f;
		normalization = NoiseNormalization.Sum;
	}
	public NoiseSamplerProperties (float _frequency, float _octaves, float _lacunarity, float _persistence, NoiseNormalization _normalization = NoiseNormalization.Sum) {
		frequency = _frequency;
		octaves = _octaves;
		lacunarity = _lacunarity;
		persistence = _persistence;
		normalization = _normalization;
	}

	public bool Equals (NoiseSamplerProperties other) =>
		frequency == other.frequency && octaves == other.octaves && lacunarity == other.lacunarity && persistence == other.persistence && normalization == other.normalization;
	public override bool Equals (object obj) => obj is NoiseSamplerProperties other && Equals(other);
	public override int GetHashCode () => HashCode.Combine(frequency, octaves, lacunarity, persistence, normalization);
	public static bool operator == (NoiseSamplerProperties a, NoiseSamplerProperties b) => a.Equals(b);
	public static bool operator != (NoiseSamplerProperties a, NoiseSamplerProperties b) => !a.Equals(b);
}