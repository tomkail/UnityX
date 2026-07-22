namespace UnityX.Noises {
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
}
