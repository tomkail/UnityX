namespace UnityX.Rhythm {
	public struct BeatEvent {
		// Beat, bar or subdivision number, depending on the event
		public long index;
		// Where it falls, in authored beats
		public double beat;
		// When it happens on the audio clock (swing and playback rate included)
		public double dspTime;
		public BarPosition bar;
	}

	public struct BeatPhase {
		// The grid line nearest the queried time, in authored beats
		public double nearestBeat;
		// How far the queried time is from it, in beats. Negative is early.
		public double beatOffset;
		// The same in seconds of audio clock. Negative is early.
		public double timeOffset;
	}
}
