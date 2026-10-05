using NUnit.Framework;

namespace UnityX.Rhythm.Tests {
	public class SmootherTests {
		static ClockTrace MacTrace => Fixtures.LoadTrace("mac-44100-1024.txt");

		[TestCase(30, 1.5, 4.0)]
		[TestCase(60, 1.0, 2.0)]
		[TestCase(144, 1.0, 2.0)]
		[TestCase(240, 1.0, 2.0)]
		public void OffsetTrackingIsSmoothOnRecordedClock(double fps, double maxJitterMs, double maxWorstMs) {
			var result = ClockTraceReplay.Run(MacTrace, new OffsetTrackingClockSmoother(), fps);
			Assert.Less(result.jitterRms * 1000, maxJitterMs);
			Assert.Less(result.worstError * 1000, maxWorstMs);
			Assert.AreEqual(0, result.backwardsSteps);
		}

		[Test]
		public void OffsetTrackingBeatsRegressionAndRaw() {
			var raw = ClockTraceReplay.Run(MacTrace, new RawClockSmoother(), 60);
			var regression = ClockTraceReplay.Run(MacTrace, new RegressionClockSmoother(), 60);
			var offset = ClockTraceReplay.Run(MacTrace, new OffsetTrackingClockSmoother(), 60);
			Assert.Less(regression.jitterRms, raw.jitterRms);
			Assert.Less(offset.jitterRms, regression.jitterRms);
			Assert.AreEqual(0, regression.backwardsSteps);
		}

		[Test]
		public void FirstFrameReturnsAFiniteTime() {
			foreach (IClockSmoother smoother in new IClockSmoother[] { new RawClockSmoother(), new RegressionClockSmoother(), new OffsetTrackingClockSmoother() }) {
				var value = smoother.Update(10, 50, 0.02);
				Assert.IsFalse(double.IsNaN(value) || double.IsInfinity(value), smoother.GetType().Name);
			}
		}

		[Test]
		public void HoldsDuringAStallAndResyncsAfter() {
			var source = new FakeAudioTimeSource();
			var smoother = new OffsetTrackingClockSmoother();
			double Step(double dt) { source.Advance(dt); return smoother.Update(source.Realtime, source.DspTime, source.BufferDuration); }
			double TrueClock() => source.Realtime + source.DspOffset - source.BufferDuration * 0.5;

			for (var i = 0; i < 180; i++) Step(1 / 60.0);
			var baseline = Step(1 / 60.0) - TrueClock();

			source.Stall();
			var last = double.NegativeInfinity;
			for (var i = 0; i < 60; i++) {
				var value = Step(1 / 60.0);
				Assert.GreaterOrEqual(value, last, "Never goes backwards");
				last = value;
			}
			Assert.IsTrue(smoother.IsStalled);
			var heldAt = last;
			Step(1 / 60.0);
			Assert.AreEqual(heldAt, Step(1 / 60.0), 1e-12, "Holds while stalled");

			source.Unstall();
			for (var i = 0; i < 30; i++) {
				var value = Step(1 / 60.0);
				Assert.GreaterOrEqual(value, last, "Never goes backwards");
				last = value;
			}
			Assert.IsFalse(smoother.IsStalled);
			Assert.AreEqual(baseline, Step(1 / 60.0) - TrueClock(), 0.004, "Back in line within half a second");
		}
	}
}
