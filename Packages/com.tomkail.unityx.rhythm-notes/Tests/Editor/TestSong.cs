// CircularRhythm/UnityX/Packages/com.tomkail.unityx.rhythm-notes/Tests/Editor/TestSong.cs
namespace UnityX.Rhythm.Notes.Tests {
	// A song on a hand-driven audio clock, without smoothing, so times in tests are exact
	public sealed class TestSong {
		public readonly ManualAudioTimeSource source = new();
		public readonly AudioRhythmClock clock;
		public readonly BeatTimeline timeline;

		public TestSong(double bpm = 120) {
			clock = new AudioRhythmClock(source, new RawClockSmoother());
			timeline = new BeatTimeline(clock, new TempoMap(bpm));
			clock.Tick();
		}

		public TempoMap TempoMap => timeline.TempoMap;

		// Advances in 60fps frames, ticking the clock and then calling onFrame
		public void Run(double seconds, System.Action onFrame = null) {
			for (var t = 0.0; t < seconds - 1e-9; t += 1 / 60.0) {
				source.Advance(1 / 60.0);
				clock.Tick();
				onFrame?.Invoke();
			}
		}
	}
}
