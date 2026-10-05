using System;

namespace UnityX.Rhythm {
	// Detects when a position crosses grid lines (every beat, bar or subdivision), including several in one frame after
	// a hitch. A big move backwards (a seek) resyncs without firing; tiny backward wobbles are ignored.
	public sealed class BeatEventTracker {
		// Grid spacing in the units passed to Advance
		public double interval = 1;
		// The most crossings reported in one Advance, so a big jump forwards doesn't fire a flood
		public int maxCatchUp = 16;
		// Backward moves smaller than this fraction of the interval are treated as jitter and ignored; bigger ones resync
		public double backwardTolerance = 0.25;

		long? lastIndex;
		double lastPosition;
		// Bumped by Reset, so Advance can tell when a callback re-primed the tracker
		int generation;

		public BeatEventTracker(double interval = 1) { this.interval = interval; }

		public void Reset() {
			lastIndex = null;
			generation++;
		}

		// Starts tracking from a position, so the next Advance reports a grid line that sits exactly on it
		public void Prime(double position) {
			Reset();
			Advance(position - interval * 1e-6, _ => {});
		}

		// Reports each grid index crossed since the last call. The first call only records the position.
		public void Advance(double position, Action<long> onCrossed) {
			if (double.IsNaN(position) || double.IsInfinity(position) || !(interval > 0)) return;
			var index = (long)Math.Floor(position / interval + 1e-9);
			if (lastIndex == null) {
				lastIndex = index;
				lastPosition = position;
				return;
			}
			if (position < lastPosition) {
				if (lastPosition - position > backwardTolerance * interval) {
					lastIndex = index;
					lastPosition = position;
				}
				return;
			}
			var first = Math.Max(lastIndex.Value + 1, index - maxCatchUp + 1);
			var startGeneration = generation;
			for (var i = first; i <= index; i++) {
				onCrossed(i);
				// A callback reset or re-primed us (e.g. by seeking): keep that state rather than overwriting it
				if (generation != startGeneration) return;
			}
			lastIndex = index;
			lastPosition = position;
		}
	}
}
