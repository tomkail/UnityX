using System;

namespace UnityX.Rhythm {
	// Detects when a position crosses grid lines (every beat, bar or subdivision), including several in one frame after
	// a hitch. Moving backwards (a seek) resyncs without firing.
	public sealed class BeatEventTracker {
		// Grid spacing in the units passed to Advance
		public double interval = 1;
		// The most crossings reported in one Advance, so a big jump forwards doesn't fire a flood
		public int maxCatchUp = 16;

		long? lastIndex;

		public BeatEventTracker(double interval = 1) { this.interval = interval; }

		public void Reset() => lastIndex = null;

		// Reports each grid index crossed since the last call. The first call only records the position.
		public void Advance(double position, Action<long> onCrossed) {
			var index = (long)Math.Floor(position / interval + 1e-9);
			if (lastIndex == null || index < lastIndex) {
				lastIndex = index;
				return;
			}
			var first = Math.Max(lastIndex.Value + 1, index - maxCatchUp + 1);
			for (var i = first; i <= index; i++) onCrossed(i);
			lastIndex = index;
		}
	}
}
