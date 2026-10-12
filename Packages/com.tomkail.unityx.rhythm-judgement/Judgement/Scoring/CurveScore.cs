using System;

namespace UnityX.Rhythm {
	// Each hit scores curve(timeOffset): the curve takes the hit's signed offset from its note in seconds (negative is
	// early), e.g. an AnimationCurve via x => curve.Evaluate((float)x). Misses and stray hits add fixed amounts,
	// usually negative.
	public sealed class CurveScore : IScoreModel {
		readonly Func<double, double> curve;
		public double hitScale = 1;
		public double missScore;
		public double strayHitScore;
		public double holdDroppedScore;

		public CurveScore(Func<double, double> curve) => this.curve = curve ?? throw new ArgumentNullException(nameof(curve));

		public double Score { get; private set; }

		public void Reset() => Score = 0;
		public void OnJudged(Judgement judgement) => Score += curve(judgement.timeOffset) * hitScale;
		public void OnMissed(NoteInstance note) => Score += missScore;
		public void OnStrayHit(RhythmInput input) => Score += strayHitScore;
		public void OnHoldCompleted(NoteInstance note) {}
		public void OnHoldDropped(NoteInstance note) => Score += holdDroppedScore;
	}
}
