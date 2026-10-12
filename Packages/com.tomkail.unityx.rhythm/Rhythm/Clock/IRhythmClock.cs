using System;

namespace UnityX.Rhythm {
	// Song time (seconds since the song started) and how it maps to the audio clock
	public interface IRhythmClock {
		double SongTime { get; }
		bool IsPlaying { get; }
		// 1 is normal speed. Separate from the tempo map: a 120bpm song at 0.5x still reports 120bpm.
		double PlaybackRate { get; }
		// The smoothed audio clock (AudioSettings.dspTime timeline) for this frame
		double DspTime { get; }
		double DspTimeAtSongTime(double songTime);
		double SongTimeAtDspTime(double dspTime);
		// Maps a real-time timestamp (e.g. an input event's) onto the audio clock
		double RealtimeToDspTime(double realtime);
		// Raised on play, pause, seek and rate changes, so anything that scheduled against the old timeline can re-time
		event Action TimelineChanged;
	}
}
