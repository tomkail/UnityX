using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityX.Rhythm {
	// Notes at fixed song beats, played once
	[Serializable]
	public class Chart : INoteSource {
		[SerializeField] List<Note> notes = new();

		public Chart() {}
		public Chart(IEnumerable<Note> notes) => this.notes.AddRange(notes);

		public event Action Changed;

		public IReadOnlyList<Note> Notes => notes;

		// Where the last note ends, e.g. to know when the song is over. 0 for an empty chart.
		public double EndBeat {
			get {
				var end = 0.0;
				foreach (var note in notes) end = Math.Max(end, note.EndBeat);
				return end;
			}
		}

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

		public Chart Clone() => new(notes);

		public void GetNotes(double startBeat, double endBeat, List<NoteInstance> results) {
			foreach (var note in notes) {
				if (note.Overlaps(startBeat, endBeat)) results.Add(new NoteInstance(new NoteId(0, NoteId.ToTick(note.beat), note.lane, 0), note));
			}
		}
	}
}
