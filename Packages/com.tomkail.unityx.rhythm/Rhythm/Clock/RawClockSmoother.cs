namespace UnityX.Rhythm {
	// No smoothing: the clock steps once per audio buffer
	public sealed class RawClockSmoother : IClockSmoother {
		double? lastDspTime;
		double lastDspChangeRealtime;
		public bool IsStalled { get; private set; }
		public double LastDiscontinuity { get; private set; }
		public void Reset() { lastDspTime = null; IsStalled = false; LastDiscontinuity = 0; }
		public double Update(double realtime, double dspTime, double bufferDuration) {
			// dspTime never goes backwards on its own; when it does, the audio device restarted on a new timeline
			LastDiscontinuity = lastDspTime.HasValue && dspTime < lastDspTime.Value ? dspTime - lastDspTime.Value : 0;
			if (lastDspTime != dspTime) { lastDspTime = dspTime; lastDspChangeRealtime = realtime; }
			IsStalled = realtime - lastDspChangeRealtime > 3 * bufferDuration;
			return dspTime;
		}
	}
}
