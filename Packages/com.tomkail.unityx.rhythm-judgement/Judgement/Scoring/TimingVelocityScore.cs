using System;

namespace UnityX.Rhythm {
	// Scores each note 0-1 on timing and velocity together: 1 for a hit dead on time at the note's velocity, falling
	// to 0 as (timing error / TimingTolerance, velocity error / velocityTolerance) reaches length 1. Misses score 0.
	// Score is the average over every note so far. A velocityTolerance of 0 ignores velocity.
	public sealed class TimingVelocityScore : IScoreModel {
		public WindowUnit timingUnit;
		public double velocityTolerance;

		double total;
		double timingTolerance;

		public TimingVelocityScore(double timingTolerance, WindowUnit timingUnit = WindowUnit.Seconds, double velocityTolerance = 0) {
			if (!(timingTolerance > 0)) throw new ArgumentOutOfRangeException(nameof(timingTolerance), "Timing tolerance must be positive");
			this.timingTolerance = timingTolerance;
			this.timingUnit = timingUnit;
			this.velocityTolerance = velocityTolerance;
		}

		// Divides the timing error, so it must be greater than 0
		public double TimingTolerance {
			get => timingTolerance;
			set {
				if (!(value > 0)) throw new ArgumentOutOfRangeException(nameof(value), "Timing tolerance must be positive");
				timingTolerance = value;
			}
		}

		public double Score => Count > 0 ? total / Count : 0;
		public int Count { get; private set; }
		public double LastNoteScore { get; private set; }

		public double NoteScore(Judgement judgement) {
			var timing = (timingUnit == WindowUnit.Beats ? judgement.beatOffset : judgement.timeOffset) / timingTolerance;
			var velocity = velocityTolerance > 0 ? judgement.velocityDifference / velocityTolerance : 0;
			return Math.Clamp(1 - Math.Sqrt(timing * timing + velocity * velocity), 0, 1);
		}

		public void Reset() {
			total = 0;
			Count = 0;
			LastNoteScore = 0;
		}

		public void OnJudged(Judgement judgement) => Add(NoteScore(judgement));
		public void OnMissed(NoteInstance note) => Add(0);
		public void OnStrayHit(RhythmInput input) {}
		public void OnHoldCompleted(NoteInstance note) {}
		public void OnHoldDropped(NoteInstance note) {}

		void Add(double noteScore) {
			LastNoteScore = noteScore;
			total += noteScore;
			Count++;
		}
	}
}
