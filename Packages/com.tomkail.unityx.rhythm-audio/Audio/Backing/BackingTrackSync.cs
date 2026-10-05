namespace UnityX.Rhythm {
	// Where and when to play a backing clip so it stays in step with a clock. The clip plays at pitch = playback rate,
	// which changes its pitch too; to keep the pitch, route it through an Audio Mixer pitch shifter set to 1 / rate.
	public static class BackingTrackSync {
		public struct Start {
			public bool play;
			public double dspTime;
			// Seconds into the clip to start from
			public double clipTime;
			public double pitch;
		}

		// clipStartSongTime is the song time at which the clip's first sample plays. startDelay leaves time for the
		// start to be scheduled sample-accurately.
		public static Start Resync(IRhythmClock clock, double clipStartSongTime, double clipLength, double startDelay) {
			if (!clock.IsPlaying) return default;
			var dspTime = clock.DspTime + startDelay;
			var clipTime = clock.SongTimeAtDspTime(dspTime) - clipStartSongTime;
			if (clipTime >= clipLength) return default;
			if (clipTime < 0) {
				dspTime = clock.DspTimeAtSongTime(clipStartSongTime);
				clipTime = 0;
			}
			return new Start { play = true, dspTime = dspTime, clipTime = clipTime, pitch = clock.PlaybackRate };
		}

		// How far ahead of the clock the clip is at dspTime, in seconds of song
		public static double Drift(IRhythmClock clock, double clipStartSongTime, double clipTime, double dspTime) {
			return clipTime - (clock.SongTimeAtDspTime(dspTime) - clipStartSongTime);
		}
	}
}
