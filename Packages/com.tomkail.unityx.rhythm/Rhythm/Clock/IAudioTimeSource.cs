namespace UnityX.Rhythm {
	// Where clocks read time from. UnityAudioTimeSource reads Unity; tests use a fake.
	public interface IAudioTimeSource {
		// Real time in seconds on the same timeline as input event timestamps (Time.realtimeSinceStartupAsDouble)
		double Realtime { get; }
		// AudioSettings.dspTime
		double DspTime { get; }
		// Length of one audio buffer in seconds
		double BufferDuration { get; }
	}
}
