using System;

namespace UnityX.Rhythm {
	// Points per grade, plus a combo of consecutive hits. Misses and dropped holds break the combo.
	public sealed class GradeScore : IScoreModel {
		// Indexed like JudgementWindows.grades
		public double[] pointsPerGrade;
		public double holdCompletedPoints;
		public bool strayHitsBreakCombo;

		public GradeScore(params double[] pointsPerGrade) {
			this.pointsPerGrade = pointsPerGrade.Length > 0 ? pointsPerGrade : new double[] { 300, 100, 50 };
		}

		public double Score { get; private set; }
		public int Combo { get; private set; }
		public int MaxCombo { get; private set; }
		// How many hits each grade got
		public int[] GradeCounts { get; private set; } = Array.Empty<int>();
		public int Misses { get; private set; }

		public void Reset() {
			Score = 0;
			Combo = 0;
			MaxCombo = 0;
			GradeCounts = Array.Empty<int>();
			Misses = 0;
		}

		public void OnJudged(Judgement judgement) {
			if (judgement.grade < pointsPerGrade.Length) Score += pointsPerGrade[judgement.grade];
			if (GradeCounts.Length <= judgement.grade) {
				var counts = GradeCounts;
				Array.Resize(ref counts, judgement.grade + 1);
				GradeCounts = counts;
			}
			GradeCounts[judgement.grade]++;
			Combo++;
			MaxCombo = Math.Max(MaxCombo, Combo);
		}

		public void OnMissed(NoteInstance note) {
			Misses++;
			Combo = 0;
		}

		public void OnStrayHit(RhythmInput input) {
			if (strayHitsBreakCombo) Combo = 0;
		}

		public void OnHoldCompleted(NoteInstance note) => Score += holdCompletedPoints;
		public void OnHoldDropped(NoteInstance note) => Combo = 0;
	}
}
