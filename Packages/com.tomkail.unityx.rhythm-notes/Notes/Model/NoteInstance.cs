namespace UnityX.Rhythm {
	// A note placed on the song timeline: note.beat is the song beat it plays at
	public readonly struct NoteInstance {
		public readonly NoteId id;
		public readonly Note note;

		public NoteInstance(NoteId id, Note note) {
			this.id = id;
			this.note = note;
		}

		public double Beat => note.beat;
		public double EndBeat => note.EndBeat;

		public NoteInstance WithId(NoteId newId) => new(newId, note);

		public NoteInstance Offset(double beats) {
			var moved = note;
			moved.beat += beats;
			return new NoteInstance(id, moved);
		}

		public override string ToString() => $"{id} at beat {note.beat}";
	}
}
