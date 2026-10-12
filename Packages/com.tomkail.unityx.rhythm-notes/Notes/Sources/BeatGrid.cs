using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityX.Rhythm {
	// A tap every Interval beats from Offset onwards, e.g. a click track or targets on every beat
	[Serializable]
	public class BeatGrid : INoteSource {
		[SerializeField] double interval = 1;
		[SerializeField] double offset;
		[SerializeField] int lane;
		[SerializeField, Range(0, 1)] float velocity = 1;
		[SerializeField] int data;

		public BeatGrid() {}

		public BeatGrid(double interval, double offset = 0, int lane = 0, float velocity = 1, int data = 0) {
			Interval = interval;
			this.offset = offset;
			this.lane = lane;
			this.velocity = velocity;
			this.data = data;
		}

		public event Action Changed;

		public double Interval {
			get => interval;
			set {
				if (!(value > 0) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value), "Grid interval must be positive");
				interval = value;
				Changed?.Invoke();
			}
		}

		public double Offset { get => offset; set { offset = value; Changed?.Invoke(); } }
		public int Lane { get => lane; set { lane = value; Changed?.Invoke(); } }
		public float Velocity { get => velocity; set { velocity = value; Changed?.Invoke(); } }
		public int Data { get => data; set { data = value; Changed?.Invoke(); } }

		// Call after editing the serialized fields directly, e.g. from OnValidate
		public void NotifyChanged() => Changed?.Invoke();

		public void GetNotes(double startBeat, double endBeat, List<NoteInstance> results) {
			// An endless or NaN window would never stop stepping
			if (!(endBeat > startBeat) || double.IsInfinity(endBeat) || double.IsNaN(startBeat)) return;
			if (!(interval > 0) || double.IsInfinity(interval)) return;
			// One step early, then filter, so rounding in the division can't skip a note exactly on startBeat
			var first = Math.Max(0, (long)Math.Ceiling((startBeat - offset) / interval) - 1);
			var tick = NoteId.ToTick(offset);
			for (var step = first; ; step++) {
				var beat = offset + step * interval;
				if (beat >= endBeat) break;
				if (beat < startBeat) continue;
				results.Add(new NoteInstance(new NoteId(0, tick, lane, step), new Note(beat, lane, 0, velocity, data)));
			}
		}
	}
}
