using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityX.Rhythm.Tests {
	// Replays a recorded audio clock through a smoother at a given frame rate and measures the result against the
	// true clock: the line through the trace's samples (the audio clock runs at a constant rate).
	public static class ClockTraceReplay {
		public struct Result {
			public double jitterRms;
			public double worstError;
			public int backwardsSteps;
		}

		public static Result Run(ClockTrace trace, IClockSmoother smoother, double fps, int seed = 1, double settleTime = 1.5) {
			var (slope, intercept) = FitLine(trace);
			var random = new Random(seed);
			var errors = new List<double>();
			var last = double.NegativeInfinity;
			var backwards = 0;
			for (var t = trace.StartRealtime; t < trace.EndRealtime; t += 1 / fps + (random.NextDouble() - 0.5) * 0.004) {
				var estimate = smoother.Update(t, trace.DspTimeAt(t), trace.bufferDuration);
				if (estimate < last) backwards++;
				last = estimate;
				if (t > trace.StartRealtime + settleTime) errors.Add(estimate - (intercept + slope * t));
			}
			var mean = errors.Average();
			return new Result {
				jitterRms = Math.Sqrt(errors.Average(e => (e - mean) * (e - mean))),
				worstError = errors.Max(e => Math.Abs(e - mean)),
				backwardsSteps = backwards
			};
		}

		public static (double slope, double intercept) FitLine(ClockTrace trace) {
			var mx = trace.samples.Average(s => s.realtime);
			var my = trace.samples.Average(s => s.dspTime);
			var slope = trace.samples.Sum(s => (s.realtime - mx) * (s.dspTime - my)) / trace.samples.Sum(s => (s.realtime - mx) * (s.realtime - mx));
			return (slope, my - slope * mx);
		}
	}
}
