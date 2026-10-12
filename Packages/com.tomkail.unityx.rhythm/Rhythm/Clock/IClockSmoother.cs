namespace UnityX.Rhythm {
	// Turns the per-buffer dspTime staircase into a clock that advances every frame
	public interface IClockSmoother {
		// Call once per frame. Returns the smoothed dsp time.
		double Update(double realtime, double dspTime, double bufferDuration);
		void Reset();
		// True while dspTime isn't advancing (paused, backgrounded, device lost)
		bool IsStalled { get; }
		// How far the returned clock jumped on the last Update because the audio clock restarted on a new timeline
		// (e.g. the audio device was reset). 0 normally. Clocks shift their anchors by this so song time carries on.
		double LastDiscontinuity { get; }
	}
}
