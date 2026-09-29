using System;
using UnityX.Noises;

namespace UnityX.NoiseSampler {
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
}
