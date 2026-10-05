namespace UnityX.Rhythm {
	// No smoothing: the clock steps once per audio buffer
	public sealed class RawClockSmoother : IClockSmoother {
		double? lastDspTime;
		double lastDspChangeRealtime;
		public bool IsStalled { get; private set; }
		public void Reset() { lastDspTime = null; IsStalled = false; }
		public double Update(double realtime, double dspTime, double bufferDuration) {
			if (lastDspTime != dspTime) { lastDspTime = dspTime; lastDspChangeRealtime = realtime; }
			IsStalled = realtime - lastDspChangeRealtime > 3 * bufferDuration;
			return dspTime;
		}
	}
}
