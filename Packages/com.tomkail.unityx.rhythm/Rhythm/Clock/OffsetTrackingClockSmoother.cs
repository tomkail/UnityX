using System;
using System.Collections.Generic;

namespace UnityX.Rhythm {
	// Turns AudioSettings.dspTime, which only advances once per audio buffer, into a clock that advances smoothly every frame.
	// The audio clock runs at the same rate as real time (measured drift is around 1ppm), so this only estimates the offset
	// between them: the average of (dspTime - realtime) over a window. The estimate is realtime + offset, with the offset
	// slewed slowly so the clock never jumps, snapped when it's badly wrong (seeks, device changes), and held while the
	// audio clock isn't advancing. It never goes backwards.
	public sealed class OffsetTrackingClockSmoother : IClockSmoother {
		// How much history the offset averages over, in seconds of real time
		public double window = 1;
		// The fastest the offset may change once settled, in seconds per second. Keeps the clock within 1% of real time.
		public double maxSlewRate = 0.01;
		// The faster limit used while the window refills after a start, snap or stall
		public double settlingSlewRate = 0.1;
		// If the offset is further than this from its target, jump straight to it
		public double snapThreshold = 0.05;
		// The audio clock counts as stalled once it hasn't advanced for this many buffers
		public double stallBuffers = 3;

		readonly Queue<(double realtime, double offset)> samples = new();
		double sampleSum;
		double? offset;
		double settledAtRealtime;
		double lastRealtime;
		double lastEstimate = double.NegativeInfinity;
		double? lastDspTime;
		double lastDspChangeRealtime;

		public bool IsStalled { get; private set; }
		public double LastDiscontinuity { get; private set; }
		public double Offset => offset ?? 0;

		public void Reset() {
			samples.Clear(); sampleSum = 0; offset = null;
			lastEstimate = double.NegativeInfinity; lastDspTime = null; IsStalled = false; LastDiscontinuity = 0;
		}

		public double Update(double realtime, double dspTime, double bufferDuration) {
			// dspTime never goes backwards on its own; when it does, the audio device restarted on a new timeline.
			// Start again on that timeline rather than holding the old value until dspTime catches up.
			if (lastDspTime.HasValue && dspTime < lastDspTime.Value) {
				var previousEstimate = lastEstimate;
				Reset();
				var restarted = Update(realtime, dspTime, bufferDuration);
				LastDiscontinuity = restarted - previousEstimate;
				return restarted;
			}
			LastDiscontinuity = 0;
			if (lastDspTime != dspTime) { lastDspTime = dspTime; lastDspChangeRealtime = realtime; }
			IsStalled = realtime - lastDspChangeRealtime > stallBuffers * bufferDuration;
			if (IsStalled) {
				samples.Clear(); sampleSum = 0; offset = null; lastRealtime = realtime;
				return double.IsNegativeInfinity(lastEstimate) ? dspTime : lastEstimate;
			}
			var sampleOffset = dspTime - realtime;
			samples.Enqueue((realtime, sampleOffset));
			sampleSum += sampleOffset;
			while (samples.Count > 1 && samples.Peek().realtime < realtime - window) sampleSum -= samples.Dequeue().offset;
			var targetOffset = sampleSum / samples.Count;
			if (offset == null) {
				// One sample is a rough start; the settling slew pulls it to the window average
				offset = sampleOffset;
				settledAtRealtime = realtime + window;
			} else if (Math.Abs(targetOffset - offset.Value) > snapThreshold) {
				offset = targetOffset;
				settledAtRealtime = realtime + window;
			} else {
				var slewRate = realtime < settledAtRealtime ? settlingSlewRate : maxSlewRate;
				var maxStep = slewRate * Math.Max(0, realtime - lastRealtime);
				offset += Math.Clamp(targetOffset - offset.Value, -maxStep, maxStep);
			}
			lastRealtime = realtime;
			// The true clock is never more than one buffer past dspTime, so don't run further ahead (e.g. at the start of a stall)
			var estimate = Math.Min(realtime + offset.Value, dspTime + bufferDuration);
			lastEstimate = Math.Max(estimate, lastEstimate);
			return lastEstimate;
		}
	}
}
