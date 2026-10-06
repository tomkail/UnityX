namespace UnityX.Rhythm {
	// How one press matched its note
	public readonly struct Judgement {
		public readonly NoteInstance note;
		public readonly RhythmInput input;
		// Index into JudgementWindows.grades, narrowest first
		public readonly int grade;
		public readonly string gradeName;
		// Hit minus note in real seconds: negative is early
		public readonly double timeOffset;
		// Hit minus note in beats
		public readonly double beatOffset;
		// 1 dead on, falling to 0 at the edge of the widest window on that side
		public readonly double accuracy;
		// Hit velocity minus note velocity
		public readonly float velocityDifference;

		public Judgement(NoteInstance note, RhythmInput input, int grade, string gradeName, double timeOffset, double beatOffset, double accuracy) {
			this.note = note;
			this.input = input;
			this.grade = grade;
			this.gradeName = gradeName;
			this.timeOffset = timeOffset;
			this.beatOffset = beatOffset;
			this.accuracy = accuracy;
			velocityDifference = input.velocity - note.note.velocity;
		}

		public override string ToString() => $"{gradeName} on {note} ({timeOffset * 1000:+0.0;-0.0}ms)";
	}
}
