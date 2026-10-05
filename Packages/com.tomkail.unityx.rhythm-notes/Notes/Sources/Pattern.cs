using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityX.Rhythm {
	// Notes that loop every LengthInBeats, from beat 0 forever. Each note's beat is within the loop
	// (0 <= beat < LengthInBeats); notes outside it are ignored. A hold may run on into the next loop.
	[Serializable]
	public class Pattern : INoteSource {
		[SerializeField] double lengthInBeats = 4;
		[SerializeField] List<Note> notes = new();

		public Pattern() {}

		public Pattern(double lengthInBeats, IEnumerable<Note> notes = null) {
			LengthInBeats = lengthInBeats;
			if (notes != null) this.notes.AddRange(notes);
		}

		public event Action Changed;

		public double LengthInBeats {
			get => lengthInBeats;
			set {
				if (!(value > 0) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value), "Pattern length must be positive");
				lengthInBeats = value;
				Changed?.Invoke();
			}
		}

		public IReadOnlyList<Note> Notes => notes;

		public void Add(Note note) { notes.Add(note); Changed?.Invoke(); }
		public void RemoveAt(int index) { notes.RemoveAt(index); Changed?.Invoke(); }
		public void Set(int index, Note note) { notes[index] = note; Changed?.Invoke(); }

		public void SetNotes(IEnumerable<Note> newNotes) {
			notes.Clear();
			notes.AddRange(newNotes);
			Changed?.Invoke();
		}

		// Call after editing the serialized fields directly, e.g. from OnValidate
		public void NotifyChanged() => Changed?.Invoke();

		public Pattern Clone() => new(lengthInBeats, notes);

		public void GetNotes(double startBeat, double endBeat, List<NoteInstance> results) {
			// An endless or NaN window would never stop looping
			if (!(endBeat > startBeat) || double.IsInfinity(endBeat) || double.IsNaN(startBeat)) return;
			// Bad inspector data plays nothing rather than looping forever
			if (notes.Count == 0 || !(lengthInBeats > 0) || double.IsInfinity(lengthInBeats) || endBeat <= 0) return;
			var longest = 0.0;
			foreach (var note in notes) {
				if (!double.IsNaN(note.length)) longest = Math.Max(longest, note.length);
			}
			var firstRepeat = Math.Max(0, (long)Math.Floor((startBeat - longest) / lengthInBeats));
			var lastRepeat = (long)Math.Floor(endBeat / lengthInBeats);
			for (var repeat = firstRepeat; repeat <= lastRepeat; repeat++) {
				var offset = repeat * lengthInBeats;
				foreach (var note in notes) {
					// A NaN length is bad inspector data, like a beat outside the loop, so it's skipped the same way rather
					// than guessed at. It would also have no end for the scheduler to see pass.
					if (note.beat < 0 || note.beat >= lengthInBeats || double.IsNaN(note.length)) continue;
					var placed = note;
					placed.beat = offset + note.beat;
					if (!placed.Overlaps(startBeat, endBeat)) continue;
					results.Add(new NoteInstance(new NoteId(0, NoteId.ToTick(note.beat), note.lane, repeat), placed));
				}
			}
		}
	}
}
