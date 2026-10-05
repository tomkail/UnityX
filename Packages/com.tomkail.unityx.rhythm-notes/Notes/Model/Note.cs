using System;
using UnityEngine;

namespace UnityX.Rhythm {
	// One note as authored. Beats are straight: the tempo map applies swing. Notes carry no audio.
	[Serializable]
	public struct Note {
		public double beat;
		public int lane;
		[Tooltip("In beats. 0 is a tap.")]
		public double length;
		[Range(0, 1)]
		public float velocity;
		[Tooltip("Game-defined, e.g. which hand")]
		public int data;

		public Note(double beat, int lane = 0, double length = 0, float velocity = 1, int data = 0) {
			this.beat = beat;
			this.lane = lane;
			this.length = length;
			this.velocity = velocity;
			this.data = data;
		}

		public double EndBeat => beat + length;

		// Whether the note's span touches [startBeat, endBeat): a tap at startBeat counts, and so does a hold that
		// started earlier and is still going at startBeat
		public bool Overlaps(double startBeat, double endBeat) => beat < endBeat && EndBeat >= startBeat;

		public override string ToString() => $"Note(beat {beat}, lane {lane}, length {length}, velocity {velocity}, data {data})";
	}
}
