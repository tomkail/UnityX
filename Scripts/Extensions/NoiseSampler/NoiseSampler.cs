using System.Collections;
using UnityEngine;

[System.Serializable]
public class NoiseSampler {
	public Vector3 position;
	public NoiseSamplerProperties properties = NoiseSamplerProperties.standard;

	public static NoiseSample SampleAtPosition (Vector3 position, NoiseSamplerProperties properties) {
		return Noise.Sum(Noise.Perlin3D, position, properties.frequency, properties.octaves, properties.lacunarity, properties.persistence);
	}

	public NoiseSample SampleAtPosition (Vector3 position) {
		return SampleAtPosition(position, properties);
	}

	public NoiseSample Sample () {
		return SampleAtPosition(position);
	}
}