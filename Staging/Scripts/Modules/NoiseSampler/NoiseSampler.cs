using System;
using UnityEngine;
using UnityX.Noises;

namespace UnityX.NoiseSampler {
	[Serializable]
	public class NoiseSampler {
		public Vector3 position;
		public NoiseSamplerProperties properties = NoiseSamplerProperties.standard;

		public static NoiseSample SampleAtPosition (Vector3 position, NoiseSamplerProperties properties) {
			return Noise.Sum(Noise.Perlin3D, position, properties.frequency, properties.octaves, properties.lacunarity, properties.persistence, properties.normalization);
		}

		public NoiseSample SampleAtPosition (Vector3 position) {
			return SampleAtPosition(position, properties);
		}

		public NoiseSample Sample () {
			return SampleAtPosition(position);
		}
	}
}
