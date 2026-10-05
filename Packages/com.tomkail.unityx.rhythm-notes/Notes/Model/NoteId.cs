using System;

namespace UnityX.Rhythm {
	// Identifies a note across tempo, rate, swing and seek changes. It depends only on where the note is authored, so
	// adding or removing one note never changes the IDs of the others. One note per lane per tick: the note scheduler drops duplicates.
	public readonly struct NoteId : IEquatable<NoteId> {
		public const int TicksPerBeat = 960;

		// The region within an Arrangement; 0 for a source played on its own
		public readonly int source;
		// The note's authored beat within its source (within the loop, for a pattern), in ticks
		public readonly long tick;
		public readonly int lane;
		// Which loop of a pattern or step of a beat grid; 0 for a chart
		public readonly long repeat;

		public NoteId(int source, long tick, int lane, long repeat) {
			this.source = source;
			this.tick = tick;
			this.lane = lane;
			this.repeat = repeat;
		}

		public static long ToTick(double beat) => (long)Math.Round(beat * TicksPerBeat);

		public NoteId WithSource(int newSource) => new(newSource, tick, lane, repeat);

		public bool Equals(NoteId other) => source == other.source && tick == other.tick && lane == other.lane && repeat == other.repeat;
		public override bool Equals(object obj) => obj is NoteId other && Equals(other);
		public override int GetHashCode() => HashCode.Combine(source, tick, lane, repeat);
		public static bool operator ==(NoteId a, NoteId b) => a.Equals(b);
		public static bool operator !=(NoteId a, NoteId b) => !a.Equals(b);
		public override string ToString() => $"NoteId(source {source}, tick {tick}, lane {lane}, repeat {repeat})";
	}
}
