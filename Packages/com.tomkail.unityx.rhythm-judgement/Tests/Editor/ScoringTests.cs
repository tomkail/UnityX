using System.Linq;
using NUnit.Framework;

namespace UnityX.Rhythm.JudgementTests {
	public class ScoringTests {
		static INoteSource Quarters() => new BeatGrid(1);

		[Test]
		public void GradeScoreAddsPointsAndCountsTheCombo() {
			var rig = new JudgeRig(Quarters());
			var score = new GradeScore(300, 100, 50);
			using var watching = score.Watch(rig.judge);
			rig.Play(-0.2);
			rig.Run(0.1);
			rig.PressAtBeat(0, 0);
			rig.Run(0.5);
			rig.PressAtBeat(0, 1, 0.05);
			Assert.AreEqual(400, score.Score);
			Assert.AreEqual(2, score.Combo);
			CollectionAssert.AreEqual(new[] { 1, 1 }, score.GradeCounts);
			rig.Run(1);
			Assert.AreEqual(0, score.Combo);
			Assert.AreEqual(2, score.MaxCombo);
			Assert.GreaterOrEqual(score.Misses, 1);
		}

		[Test]
		public void StrayHitsBreakTheComboOnlyWhenAskedTo() {
			var rig = new JudgeRig(Quarters());
			var score = new GradeScore();
			using var watching = score.Watch(rig.judge);
			rig.Play(-0.2);
			rig.Run(0.1);
			rig.PressAtBeat(0, 0);
			rig.PressAtBeat(0, 0.5);
			Assert.AreEqual(1, score.Combo);
			score.strayHitsBreakCombo = true;
			rig.PressAtBeat(0, 0.5);
			Assert.AreEqual(0, score.Combo);
		}

		[Test]
		public void DisposingTheWatchStopsScoring() {
			var rig = new JudgeRig(Quarters());
			var score = new GradeScore();
			var watching = score.Watch(rig.judge);
			rig.Play(-0.2);
			rig.Run(0.1);
			watching.Dispose();
			rig.PressAtBeat(0, 0);
			Assert.AreEqual(0, score.Score);
		}

		[Test]
		public void CurveScoreEvaluatesTheCurveAtTheOffset() {
			var rig = new JudgeRig(Quarters());
			var score = new CurveScore(offset => 1 - System.Math.Abs(offset) * 10) { hitScale = 2, missScore = -0.5, strayHitScore = -0.25 };
			using var watching = score.Watch(rig.judge);
			rig.Play(-0.2);
			rig.Run(0.1);
			rig.PressAtBeat(0, 0, -0.05);
			Assert.AreEqual(1, score.Score, 1e-9);
			rig.PressAtBeat(0, 0.5);
			Assert.AreEqual(0.75, score.Score, 1e-9);
			rig.Run(1);
			Assert.Less(score.Score, 0.75);
		}

		[Test]
		public void TimingVelocityScoreCombinesBothErrors() {
			var score = new TimingVelocityScore(0.1, WindowUnit.Seconds, 0.5);
			var note = new NoteInstance(new NoteId(0, 0, 0, 0), new Note(0, 0, 0, 0.5f));
			// 30ms late (0.3 of the tolerance) and 0.2 too hard (0.4 of it): 1 - 0.5
			var judgement = new Judgement(note, new RhythmInput(0, InputPhase.Press, 0.7f, 0.03), 0, "Perfect", 0.03, 0.06, 0.7);
			Assert.AreEqual(0.5, score.NoteScore(judgement), 1e-6);
			score.OnJudged(judgement);
			score.OnMissed(note);
			Assert.AreEqual(0.25, score.Score, 1e-6);
			Assert.AreEqual(2, score.Count);
			Assert.AreEqual(0, score.LastNoteScore);
		}

		[Test]
		public void TimingVelocityScoreCanMeasureTimingInBeatsAndIgnoreVelocity() {
			var score = new TimingVelocityScore(0.25, WindowUnit.Beats);
			var note = new NoteInstance(new NoteId(0, 0, 0, 0), new Note(0, 0, 0, 0.1f));
			var judgement = new Judgement(note, new RhythmInput(0, InputPhase.Press, 1, 0), 0, "Perfect", 0.05, 0.1, 0.5);
			Assert.AreEqual(0.6, score.NoteScore(judgement), 1e-9);
		}
	}
}
