using System.Linq;
using NUnit.Framework;

namespace UnityX.Rhythm.JudgementTests {
	public class JudgeTests {
		// Two lanes of quarter notes at 120bpm: lane 0 on the beat, lane 1 on the off-beat
		static INoteSource TwoLanes() => new Pattern(1, new[] { new Note(0, 0), new Note(0.5, 1) });

		[Test]
		public void AHitOnTimeIsPerfect() {
			var rig = new JudgeRig(TwoLanes());
			rig.Play(-0.5);
			rig.Run(0.4);
			rig.PressAtBeat(0, 0, 0.01);
			var judgement = rig.judged.Single();
			Assert.AreEqual("Perfect", judgement.gradeName);
			Assert.AreEqual(0, judgement.grade);
			Assert.AreEqual(0.01, judgement.timeOffset, 1e-9);
			Assert.AreEqual(0.02, judgement.beatOffset, 1e-9);
			Assert.AreEqual(0.9, judgement.accuracy, 1e-9);
			Assert.AreEqual(0, judgement.note.Beat);
			Assert.IsFalse(rig.judge.IsOpen(judgement.note.id));
		}

		[Test]
		public void EarlyAndLateHitsGetWiderGrades() {
			var rig = new JudgeRig(TwoLanes());
			rig.Play(-0.5);
			rig.Run(0.4);
			rig.PressAtBeat(0, 0, -0.05);
			rig.PressAtBeat(1, 0.5, 0.09);
			CollectionAssert.AreEqual(new[] { "Great", "Good" }, rig.judged.Select(j => j.gradeName));
			Assert.Less(rig.judged[0].timeOffset, 0);
		}

		[Test]
		public void APressOnlyMatchesItsOwnLane() {
			var rig = new JudgeRig(TwoLanes());
			rig.Play(-0.5);
			rig.Run(0.4);
			// Lane 1's note is 0.25s after lane 0's; a lane 1 press on beat 0 is too far from it
			rig.PressAtBeat(1, 0);
			Assert.IsEmpty(rig.judged);
			Assert.AreEqual(1, rig.strays.Count);
		}

		[Test]
		public void APressGoesToTheNearestOpenNote() {
			var rig = new JudgeRig(new Pattern(1, new[] { new Note(0), new Note(0.125) }));
			rig.Play(-0.5);
			rig.Run(0.4);
			// Beat 0.125 is 62.5ms after beat 0; a press at 50ms is nearer the second note
			rig.PressAtBeat(0, 0, 0.05);
			Assert.AreEqual(0.125, rig.judged.Single().note.Beat);
			rig.PressAtBeat(0, 0, 0.051);
			Assert.AreEqual(0, rig.judged[1].note.Beat);
		}

		[Test]
		public void AJudgedNoteCantBeHitAgain() {
			var rig = new JudgeRig(TwoLanes());
			rig.Play(-0.5);
			rig.Run(0.4);
			rig.PressAtBeat(0, 0);
			rig.PressAtBeat(0, 0, 0.02);
			Assert.AreEqual(1, rig.judged.Count);
			Assert.AreEqual(1, rig.strays.Count);
		}

		[Test]
		public void AnUnhitNoteIsMissedOnceItsWindowAndTheMissDelayHavePassed() {
			var rig = new JudgeRig(TwoLanes());
			rig.Play(-0.5);
			// Beat 0's window closes 0.1s after it, plus 0.05s of miss delay
			rig.Run(0.6);
			Assert.IsEmpty(rig.missed);
			rig.Run(0.1);
			Assert.AreEqual(0, rig.missed.First().Beat);
			Assert.AreEqual(1, rig.missed.Count(n => n.Beat == 0));
		}

		[Test]
		public void APressInsideTheMissDelayStillHits() {
			var rig = new JudgeRig(TwoLanes());
			rig.Play(-0.5);
			rig.Run(0.62);
			// Timed 90ms late, delivered after the window closed but before the miss
			rig.PressAtBeat(0, 0, 0.09);
			Assert.AreEqual(0, rig.judged.Single().note.Beat);
			rig.Run(0.2);
			Assert.IsFalse(rig.missed.Any(n => n.Beat == 0));
		}

		[Test]
		public void PressesBeforePlayingAreStrayHits() {
			var rig = new JudgeRig(TwoLanes());
			rig.judge.Update();
			rig.Press(0, rig.song.clock.DspTime);
			Assert.AreEqual(1, rig.strays.Count);
		}

		[Test]
		public void PressesWithNoNotesAreStrayHits() {
			var rig = new JudgeRig(new Chart());
			rig.Play(0);
			rig.Press(0, rig.song.clock.DspTime);
			Assert.AreEqual(1, rig.strays.Count);
		}

		[Test]
		public void ReleasesOutsideHoldsAreIgnored() {
			var rig = new JudgeRig(TwoLanes());
			rig.Play(-0.5);
			rig.Release(0, rig.DspAtBeat(0));
			Assert.IsEmpty(rig.strays);
			Assert.IsEmpty(rig.holdsDropped);
		}

		[Test]
		public void AnyLaneMatchesWhicheverLaneIsNearest() {
			var rig = new JudgeRig(TwoLanes());
			rig.judge.anyLane = true;
			rig.Play(-0.5);
			rig.Run(0.4);
			rig.PressAtBeat(5, 0.5, -0.01);
			Assert.AreEqual(1, rig.judged.Single().note.note.lane);
		}

		[Test]
		public void BeatWindowsScaleWithTempo() {
			var windows = new JudgementWindows { unit = WindowUnit.Beats };
			windows.grades.Clear();
			windows.grades.Add(new JudgementGrade("Hit", 0.1, 0.1));
			// At 60bpm a tenth of a beat is 100ms
			var rig = new JudgeRig(TwoLanes(), windows, 60);
			rig.Play(-0.5);
			rig.Run(0.4);
			rig.PressAtBeat(0, 0, 0.09);
			Assert.AreEqual(1, rig.judged.Count);
			Assert.AreEqual(0.09, rig.judged[0].beatOffset, 1e-9);
			rig.PressAtBeat(1, 0.5, 0.11);
			Assert.AreEqual(1, rig.strays.Count);
		}

		[Test]
		public void SecondWindowsStayInRealSecondsAtHalfSpeed() {
			var rig = new JudgeRig(TwoLanes());
			rig.song.clock.SetPlaybackRate(0.5);
			rig.Play(-0.5);
			rig.Run(0.8);
			// At half speed a 120bpm beat lasts a real second, so 90ms is still Good, and is 0.09 of a beat
			rig.PressAtBeat(0, 0, 0.09);
			Assert.AreEqual("Good", rig.judged.Single().gradeName);
			Assert.AreEqual(0.09, rig.judged[0].timeOffset, 1e-9);
			Assert.AreEqual(0.09, rig.judged[0].beatOffset, 1e-9);
		}

		[Test]
		public void TempoAndRateChangesNeverRejudge() {
			var rig = new JudgeRig(TwoLanes());
			rig.Play(-0.5);
			rig.Run(0.4);
			rig.PressAtBeat(0, 0);
			var id = rig.judged.Single().note.id;
			rig.song.TempoMap.SetTempo(0.25, 100);
			rig.song.clock.SetPlaybackRate(0.75);
			rig.Run(0.05);
			rig.PressAtBeat(0, 0);
			Assert.AreEqual(1, rig.judged.Count);
			Assert.IsFalse(rig.judge.IsOpen(id));
			Assert.IsFalse(rig.missed.Any(n => n.id == id));
		}

		[Test]
		public void VelocityDifferenceIsHitMinusNote() {
			var rig = new JudgeRig(new Pattern(1, new[] { new Note(0, 0, 0, 0.5f) }));
			rig.Play(-0.5);
			rig.Run(0.4);
			rig.PressAtBeat(0, 0, 0, 0.75f);
			Assert.AreEqual(0.25f, rig.judged.Single().velocityDifference, 1e-6f);
		}

		[Test]
		public void TheLookBehindCoversTheWidestWindow() {
			var windows = new JudgementWindows();
			windows.grades.Add(new JudgementGrade("Late", 0.1, 0.5));
			var rig = new JudgeRig(TwoLanes(), windows);
			rig.Play(-0.5);
			Assert.GreaterOrEqual(rig.notes.lookBehind, 0.5 + rig.judge.missDelay);
		}

		[Test]
		public void BeatWindowsKeepALateNoteThroughATempoIncrease() {
			var windows = new JudgementWindows { unit = WindowUnit.Beats };
			windows.grades.Clear();
			windows.grades.Add(new JudgementGrade("Hit", 0.25, 0.25));
			var rig = new JudgeRig(new Chart(new[] { new Note(1) }), windows, 60);
			// Beat 1's late window runs to beat 1.25 and is nearly all at 60bpm, though the playhead is already at 240bpm
			rig.song.TempoMap.SetTempo(1.15, 240);
			// Leaves the look-behind to the judge
			rig.notes.lookBehind = 0;
			rig.Play(1.16);
			rig.Run(0.05);
			rig.PressAtBeat(0, 1.2);
			Assert.AreEqual(1, rig.judged.Count);
			Assert.IsEmpty(rig.strays);
			Assert.IsEmpty(rig.missed);
		}

		[Test]
		public void AttachedSourcesFeedTheJudge() {
			var rig = new JudgeRig(TwoLanes());
			var source = new TestInputSource();
			rig.judge.Attach(source);
			rig.Play(-0.5);
			rig.Run(0.4);
			source.Send(new RhythmInput(0, InputPhase.Press, 1, rig.DspAtBeat(0)));
			Assert.AreEqual(1, rig.judged.Count);
			rig.judge.Detach(source);
			source.Send(new RhythmInput(1, InputPhase.Press, 1, rig.DspAtBeat(0.5)));
			Assert.AreEqual(1, rig.judged.Count);
		}

		[Test]
		public void UpdatingAJudgedSchedulerFromOutsideThrows() {
			var rig = new JudgeRig(TwoLanes());
			Assert.AreSame(rig.judge, rig.notes.Owner);
			Assert.Throws<System.InvalidOperationException>(rig.notes.Update);
		}

		[Test]
		public void ASecondJudgeOnTheSameSchedulerThrows() {
			var rig = new JudgeRig(TwoLanes());
			Assert.Throws<System.InvalidOperationException>(() => new Judge(rig.song.timeline, rig.notes, new JudgementWindows()));
		}

		[Test]
		public void ADisposedJudgeReleasesItsScheduler() {
			var rig = new JudgeRig(TwoLanes());
			rig.judge.Dispose();
			Assert.IsNull(rig.notes.Owner);
			Assert.DoesNotThrow(rig.notes.Update);
			using var next = new Judge(rig.song.timeline, rig.notes, new JudgementWindows());
			Assert.AreSame(next, rig.notes.Owner);
		}

		sealed class TestInputSource : IRhythmInputSource {
			public event System.Action<RhythmInput> InputReceived;
			public void Send(RhythmInput input) => InputReceived?.Invoke(input);
		}
	}
}
