using NUnit.Framework;

namespace UnityX.Rhythm.Tests {
	public class ClockTraceTests {
		[Test]
		public void RoundTripsAndLooksUpSamples() {
			var trace = new ClockTrace { bufferDuration = 0.02 };
			trace.samples.Add((1.0, 10.0));
			trace.samples.Add((1.02, 10.02));
			trace.marks.Add((1.01, "midi note 38"));
			var parsed = ClockTrace.Parse(trace.Serialize());
			Assert.AreEqual(0.02, parsed.bufferDuration, 1e-15);
			Assert.AreEqual(2, parsed.samples.Count);
			Assert.AreEqual("midi note 38", parsed.marks[0].label);
			Assert.AreEqual(10.0, parsed.DspTimeAt(1.019), 1e-15);
			Assert.AreEqual(10.02, parsed.DspTimeAt(1.5), 1e-15);
			Assert.AreEqual(10.0, parsed.DspTimeAt(0), 1e-15);
		}

		[Test]
		public void FixtureLoads() {
			var trace = Fixtures.LoadTrace("mac-44100-1024.txt");
			Assert.AreEqual(1024 / 44100.0, trace.bufferDuration, 1e-12);
			Assert.Greater(trace.samples.Count, 100);
		}
	}
}
