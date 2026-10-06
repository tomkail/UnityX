using System.Linq;
using NUnit.Framework;

namespace UnityX.Rhythm.JudgementTests {
	public class JudgeHoldAndSeekTests {
		// A two-beat hold from beat 1 (0.5s to 1.5s at 120bpm)
		static INoteSource Hold() => new Chart(new[] { new Note(1, 0, 2) });

		[Test]
		public void AHoldIsJudgedOnThePressAndCompletesAtItsEnd() {
			var rig = new JudgeRig(Hold());
			rig.Play(0);
			rig.Run(0.45);
			rig.PressAtBeat(0, 1);
			Assert.AreEqual(1, rig.judged.Count);
			Assert.IsTrue(rig.judge.IsHolding(rig.judged[0].note.id));
			rig.Run(0.9);
			Assert.IsEmpty(rig.holdsCompleted);
			rig.Run(0.2);
			Assert.AreEqual(1, rig.holdsCompleted.Count);
			Assert.IsEmpty(rig.missed);
		}

		[Test]
		public void ReleasingNearTheEndCompletesTheHold() {
			var rig = new JudgeRig(Hold());
			rig.Play(0);
			rig.Run(0.45);
			rig.PressAtBeat(0, 1);
			rig.Run(1);
			// 0.08s before the end is inside the 0.1s release allowance
			rig.Release(0, rig.DspAtBeat(3) - 0.08);
			Assert.AreEqual(1, rig.holdsCompleted.Count);
			Assert.IsEmpty(rig.holdsDropped);
		}

		[Test]
		public void ReleasingEarlyDropsTheHold() {
			var rig = new JudgeRig(Hold());
			rig.Play(0);
			rig.Run(0.45);
			rig.PressAtBeat(0, 1);
			rig.Run(0.5);
			rig.Release(0, rig.song.clock.DspTime);
			Assert.AreEqual(1, rig.holdsDropped.Count);
			rig.Run(1);
			Assert.IsEmpty(rig.holdsCompleted);
		}

		[Test]
		public void AnUnpressedHoldIsMissed() {
			var rig = new JudgeRig(Hold());
			rig.Play(0);
			rig.Run(1);
			Assert.AreEqual(1, rig.missed.Count);
		}

		[Test]
		public void NotesASeekForwardJumpsOverAreNotMissed() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(0.15);
			rig.PressAtBeat(0, 0);
			// Beats 1 to 7 are jumped over: some were active, some never were, and beat 8 lands just behind the playhead
			rig.song.clock.Seek(4.2);
			rig.Run(0.3);
			Assert.IsFalse(rig.missed.Any(n => n.Beat < 9), string.Join(", ", rig.missed.Select(n => n.Beat)));
			// Playing on from there, notes are missed as normal
			rig.Run(1);
			Assert.IsTrue(rig.missed.Any(n => n.Beat == 9));
		}

		[Test]
		public void AHitchStillMissesTheNotesItSkipsPast() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(0.15);
			rig.PressAtBeat(0, 0);
			rig.song.source.Advance(1.5);
			rig.song.clock.Tick();
			rig.judge.Update();
			Assert.IsTrue(rig.missed.Any(n => n.Beat == 1));
			Assert.IsTrue(rig.missed.Any(n => n.Beat == 2));
		}

		[Test]
		public void ASeekBackLetsTheNotesBePlayedAgain() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(0.15);
			rig.PressAtBeat(0, 0);
			rig.Run(0.4);
			rig.PressAtBeat(0, 1);
			rig.Run(1);
			// Beat 2 was missed, beats 0 and 1 were hit
			Assert.IsTrue(rig.missed.Any(n => n.Beat == 2));
			rig.song.clock.Seek(-0.2);
			rig.judge.Update();
			rig.Run(0.15);
			rig.PressAtBeat(0, 0);
			Assert.AreEqual(3, rig.judged.Count);
			Assert.AreEqual(0, rig.judged[2].note.Beat);
			rig.Run(1.5);
			Assert.AreEqual(2, rig.missed.Count(n => n.Beat == 2));
			Assert.AreEqual(1, rig.missed.Count(n => n.Beat == 1));
		}

		[Test]
		public void PausedTimeNeverMissesNotes() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(0.15);
			rig.PressAtBeat(0, 0);
			rig.song.clock.Pause();
			rig.Run(5);
			Assert.IsEmpty(rig.missed);
			rig.song.clock.Resume();
			rig.Run(0.5);
			Assert.IsEmpty(rig.missed);
		}

		[Test]
		public void ANoteAddedTooLateToPlayIsNotMissed() {
			var pattern = new Pattern(4, new[] { new Note(0) });
			var rig = new JudgeRig(pattern);
			rig.Play(-0.2);
			rig.Run(0.15);
			rig.PressAtBeat(0, 0);
			rig.Run(0.4);
			// Beat 0.25 is 0.125s in, so already past its window and the miss delay
			pattern.Add(new Note(0.25));
			rig.Run(0.1);
			Assert.IsEmpty(rig.missed);
		}

		[Test]
		public void SeekingForwardWhilePausedDoesNotMiss() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(1.2);
			rig.song.clock.Pause();
			var missedBefore = rig.missed.Count;
			// Beat 6 (3.0s) ends up just behind the playhead, past its deadline but inside the look-behind
			rig.song.clock.Seek(3.2);
			rig.judge.Update();
			rig.song.clock.Resume();
			rig.judge.Update();
			rig.Run(0.2);
			var missedAfter = rig.missed.Skip(missedBefore).ToList();
			Assert.IsEmpty(missedAfter, string.Join(", ", missedAfter.Select(n => n.Beat)));
			// Playing on from there, notes are missed as normal
			rig.Run(0.5);
			Assert.AreEqual(7, rig.missed.Skip(missedBefore).Single().Beat);
		}

		[Test]
		public void SeekingBackWhilePausedLetsNotesBePlayedAgain() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(0.6);
			// Beat 0 was missed; beats 1 and 2 are hit
			rig.PressAtBeat(0, 1);
			rig.Run(0.5);
			rig.PressAtBeat(0, 2);
			rig.Run(0.3);
			CollectionAssert.AreEqual(new[] { 0.0 }, rig.missed.Select(n => n.Beat));
			rig.song.clock.Pause();
			// Beat 0 lands just behind the playhead, past its deadline: it mustn't be missed a second time
			rig.song.clock.Seek(0.2);
			rig.judge.Update();
			rig.song.clock.Resume();
			rig.judge.Update();
			rig.Run(0.2);
			rig.PressAtBeat(0, 1);
			rig.Run(0.5);
			rig.PressAtBeat(0, 2);
			rig.Run(0.2);
			CollectionAssert.AreEqual(new[] { 1.0, 2.0, 1.0, 2.0 }, rig.judged.Select(j => j.note.Beat));
			CollectionAssert.AreEqual(new[] { 0.0 }, rig.missed.Select(n => n.Beat));
		}

		[Test]
		public void AShortSeekForwardSkipsANoteItLandsJustBeyond() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(0.25);
			var id = rig.notes.ActiveNotes.First(n => n.Beat == 0).id;
			Assert.IsTrue(rig.judge.IsOpen(id));
			// Beat 0's deadline is 0.15s; at 0.22s it's past that but still inside the 0.25s look-behind
			rig.song.clock.Seek(0.22);
			rig.judge.Update();
			Assert.IsTrue(rig.notes.IsActive(id));
			Assert.IsFalse(rig.judge.IsOpen(id));
			rig.Run(0.3);
			Assert.IsFalse(rig.missed.Any(n => n.Beat == 0));
		}

		[Test]
		public void AShortSeekBackReopensANoteItWasHit() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(0.25);
			rig.PressAtBeat(0, 0);
			rig.Run(0.1);
			// Beat 0 stays active, and its deadline (0.15s) is ahead again
			rig.song.clock.Seek(0);
			rig.judge.Update();
			rig.PressAtBeat(0, 0);
			CollectionAssert.AreEqual(new[] { 0.0, 0.0 }, rig.judged.Select(j => j.note.Beat));
			Assert.IsEmpty(rig.strays);
			rig.Run(0.3);
			Assert.IsEmpty(rig.missed);
		}
	}
}
