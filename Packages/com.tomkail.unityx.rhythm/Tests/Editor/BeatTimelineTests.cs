using NUnit.Framework;

namespace UnityX.Rhythm.Tests {
	public class BeatTimelineTests {
		[Test]
		public void MapsBeatsThroughTheClockAndTempoMap() {
			var source = new ManualAudioTimeSource();
			var clock = new AudioRhythmClock(source, new RawClockSmoother());
			using var timeline = new BeatTimeline(clock, new TempoMap(120));
			clock.Tick();
			clock.Play(0);
			Assert.AreEqual(clock.DspTime + 1, timeline.DspTimeAtBeat(2), 1e-9);
			Assert.AreEqual(2, timeline.BeatAtDspTime(clock.DspTime + 1), 1e-9);
			source.Advance(0.5);
			clock.Tick();
			Assert.AreEqual(1, timeline.CurrentBeat(), 0.05);
		}

		[Test]
		public void RaisesTimelineChangedForClockAndTempoChanges() {
			var clock = new AudioRhythmClock(new ManualAudioTimeSource(), new RawClockSmoother());
			var first = new TempoMap(120);
			using var timeline = new BeatTimeline(clock, first);
			var changes = 0;
			timeline.TimelineChanged += () => changes++;
			clock.Play(0);
			timeline.TempoMap.SetTempo(4, 90);
			timeline.TempoMap = new TempoMap(100);
			first.SetTempo(8, 60);
			timeline.TempoMap.SetTempo(8, 60);
			// The old map's edit is ignored
			Assert.AreEqual(4, changes);
		}

		[Test]
		public void IsPausedAtInfinity() {
			var clock = new AudioRhythmClock(new ManualAudioTimeSource(), new RawClockSmoother());
			using var timeline = new BeatTimeline(clock, new TempoMap(120));
			Assert.AreEqual(double.PositiveInfinity, timeline.DspTimeAtBeat(1));
		}
	}
}
