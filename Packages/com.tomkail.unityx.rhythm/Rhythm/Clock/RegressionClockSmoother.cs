using System.Collections.Generic;

namespace UnityX.Rhythm {
	// Fits a line to the last few frames' (realtime, dspTime) and reads it at the current realtime.
	// The approach from https://rhythmquestgame.com/devlog/04.html. Kept for comparison: re-estimating the slope every
	// frame makes it about 5x more jittery than OffsetTrackingClockSmoother.
	public sealed class RegressionClockSmoother : IClockSmoother {
		public int sampleCount = 16;
		readonly Queue<(double x, double y)> samples = new();
		double lastEstimate = double.NegativeInfinity;
		double? lastDspTime;
		double lastDspChangeRealtime;

		public bool IsStalled { get; private set; }

		public void Reset() { samples.Clear(); lastEstimate = double.NegativeInfinity; lastDspTime = null; IsStalled = false; }

		public double Update(double realtime, double dspTime, double bufferDuration) {
			if (lastDspTime != dspTime) { lastDspTime = dspTime; lastDspChangeRealtime = realtime; }
			IsStalled = realtime - lastDspChangeRealtime > 3 * bufferDuration;
			samples.Enqueue((realtime, dspTime));
			while (samples.Count > sampleCount) samples.Dequeue();

			double sx = 0, sy = 0, sxx = 0, sxy = 0;
			foreach (var (x, y) in samples) { sx += x; sy += y; sxx += x * x; sxy += x * y; }
			var n = samples.Count;
			var ssx = sxx - sx * sx / n;
			// With fewer than two distinct samples there's no line to fit
			var prediction = ssx <= 1e-12 ? dspTime : sy / n + (sxy - sx * sy / n) / ssx * (realtime - sx / n);
			if (prediction > lastEstimate) lastEstimate = prediction;
			return lastEstimate;
		}
	}
}
