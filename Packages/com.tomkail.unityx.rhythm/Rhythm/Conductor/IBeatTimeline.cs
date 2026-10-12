using System;

namespace UnityX.Rhythm {
	// A clock plus the tempo map that turns its song time into beats. Conductor is one; BeatTimeline is a plain C# one.
	// Note and audio schedulers take this, so they work with either.
	public interface IBeatTimeline {
		IRhythmClock Clock { get; }
		TempoMap TempoMap { get; }
		// The clock played, paused, seeked or changed rate, or the tempo map was edited or replaced
		event Action TimelineChanged;
	}

	public static class BeatTimelineExtensions {
		public static double CurrentBeat(this IBeatTimeline timeline) => timeline.TempoMap.BeatAtTime(timeline.Clock.SongTime);
		// Infinity while paused: nothing is going to happen
		public static double DspTimeAtBeat(this IBeatTimeline timeline, double beat) => timeline.Clock.DspTimeAtSongTime(timeline.TempoMap.TimeAtBeat(beat));
		public static double BeatAtDspTime(this IBeatTimeline timeline, double dspTime) => timeline.TempoMap.BeatAtTime(timeline.Clock.SongTimeAtDspTime(dspTime));
	}
}
