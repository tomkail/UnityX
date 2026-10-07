using System.Linq;
using NUnit.Framework;
using UnityEngine;

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

		[Test]
		public void AShortSeekForwardWhilePausedKeepsAnEarlyHitHit() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(0.35);
			rig.PressAtBeat(0, 1, -0.05);
			rig.song.clock.Pause();
			// Beat 1 (0.5s) is still ahead of its deadline (0.65s) after the seek
			rig.song.clock.Seek(0.4);
			rig.judge.Update();
			rig.song.clock.Resume();
			rig.judge.Update();
			rig.PressAtBeat(0, 1);
			Assert.AreEqual(1, rig.judged.Count(j => j.note.Beat == 1));
			Assert.AreEqual(1, rig.strays.Count);
		}

		// Seeks in steps no bigger than the scheduler's seekThreshold, so no single Update sees a jump
		static void Scrub(JudgeRig rig, double to) {
			var clock = rig.song.clock;
			var step = to > clock.SongTime ? 0.04 : -0.04;
			while (System.Math.Abs(to - clock.SongTime) > 1e-9) {
				var next = System.Math.Abs(to - clock.SongTime) < 0.04 ? to : clock.SongTime + step;
				clock.Seek(next);
				rig.judge.Update();
			}
		}

		[Test]
		public void ScrubbingForwardWhilePausedDoesNotMiss() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(1.2);
			rig.song.clock.Pause();
			rig.judge.Update();
			Assert.IsTrue(rig.judge.IsOpen(rig.notes.ActiveNotes.First(n => n.Beat == 2).id));
			var missedBefore = rig.missed.Count;
			// Well past beat 2's window, and past beat 3's deadline (1.65s) while it's still in the look-behind
			Scrub(rig, 1.7);
			Assert.AreEqual(missedBefore, rig.missed.Count);
			rig.song.clock.Resume();
			rig.judge.Update();
			rig.Run(0.2);
			var missedAfter = rig.missed.Skip(missedBefore).ToList();
			Assert.IsEmpty(missedAfter, string.Join(", ", missedAfter.Select(n => n.Beat)));
		}

		[Test]
		public void ScrubbingBackWhilePausedLetsANoteBePlayedAgain() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(1.0);
			rig.PressAtBeat(0, 2);
			rig.Run(0.3);
			rig.song.clock.Pause();
			rig.judge.Update();
			var missedBefore = rig.missed.Count;
			Scrub(rig, 0.7);
			rig.song.clock.Resume();
			rig.judge.Update();
			rig.Run(0.2);
			rig.PressAtBeat(0, 2);
			Assert.AreEqual(2, rig.judged.Count(j => j.note.Beat == 2));
			Assert.IsEmpty(rig.strays);
			Assert.AreEqual(missedBefore, rig.missed.Count);
		}

		[Test]
		public void ATempoEditWhilePausedDoesNotMiss() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(1.2);
			rig.song.clock.Pause();
			rig.judge.Update();
			var beat2 = rig.notes.ActiveNotes.First(n => n.Beat == 2).id;
			Assert.IsTrue(rig.judge.IsOpen(beat2));
			var missedBefore = rig.missed.Count;
			// At 200bpm beat 2 is at 0.6s, behind the look-behind
			rig.song.TempoMap.SetTempo(0, 200);
			rig.judge.Update();
			Assert.IsFalse(rig.notes.IsActive(beat2));
			Assert.AreEqual(missedBefore, rig.missed.Count);
			rig.song.clock.Resume();
			rig.judge.Update();
			Assert.IsFalse(rig.missed.Any(n => n.id == beat2));
		}

		[Test]
		public void PausingLateInAHitchStillMissesTheNotesThatPassedBeforeThePause() {
			var rig = new JudgeRig(new BeatGrid(0.5));
			rig.Play(-0.2);
			rig.Run(0.5);
			var missedBefore = rig.missed.Count;
			// One frame plays from 0.3s to 0.95s and the pause lands at its end. Beats 0.5 and 1 (0.25s and 0.5s) fall
			// out of the look-behind, and beat 1.5 (0.75s) stays in it, past its deadline (0.9s).
			rig.song.source.Advance(0.65);
			rig.song.clock.Tick();
			rig.song.clock.Pause();
			rig.judge.Update();
			rig.Run(1);
			rig.song.clock.Resume();
			rig.judge.Update();
			CollectionAssert.AreEqual(new[] { 0.5, 1.0, 1.5 }, rig.missed.Skip(missedBefore).Select(n => n.Beat));
		}

		[Test]
		public void ATempoEditWhilePausedThatLeavesANoteJustBehindDoesNotMissIt() {
			var rig = new JudgeRig(new BeatGrid(1));
			rig.Play(-0.2);
			rig.Run(1.1);
			rig.song.clock.Pause();
			rig.judge.Update();
			var beat2 = rig.notes.ActiveNotes.First(n => n.Beat == 2).id;
			Assert.IsTrue(rig.judge.IsOpen(beat2));
			// At 170bpm beat 2 is at 0.71s: behind the playhead (0.9s), past its deadline (0.86s), inside the look-behind
			rig.song.TempoMap.SetTempo(0, 170);
			rig.judge.Update();
			Assert.IsTrue(rig.notes.IsActive(beat2));
			rig.song.clock.Resume();
			rig.judge.Update();
			rig.Run(0.1);
			Assert.IsFalse(rig.missed.Any(n => n.id == beat2));
		}

		[Test]
		public void LateArrivingInputMovesTheMissDeadlineBack() {
			var rig = new JudgeRig(new BeatGrid(1));
			var latency = ScriptableObject.CreateInstance<RhythmLatency>();
			try {
				// Inputs reach the game 0.2s after they're played
				latency.inputLatency = -0.2;
				rig.judge.latency = latency;
				rig.Play(-0.2);
				rig.Run(0.25);
				rig.PressAtBeat(0, 0);
				// Beat 1 is at 0.5s. Its window closes at 0.6s and the miss delay ends at 0.65s, but a press played then
				// arrives 0.2s later, so it isn't missed until 0.85s.
				rig.Run(0.55);
				Assert.IsEmpty(rig.missed);
				// The late press, played 50ms after the beat, arrives now and is stamped back to when it was played
				rig.Press(0, rig.DspAtBeat(1) + 0.05);
				Assert.AreEqual(2, rig.judged.Count);
				Assert.IsEmpty(rig.strays);
				// Beat 2 (1s) is missed only after 1.35s
				rig.Run(0.45);
				Assert.IsEmpty(rig.missed);
				rig.Run(0.35);
				Assert.AreEqual(1, rig.missed.Count);
				Assert.AreEqual(2, rig.missed[0].Beat);
			} finally {
				Object.DestroyImmediate(latency);
			}
		}

		[Test]
		public void EarlyArrivingInputNeverShortensTheMissDeadline() {
			var rig = new JudgeRig(new BeatGrid(1));
			var latency = ScriptableObject.CreateInstance<RhythmLatency>();
			try {
				latency.inputLatency = 0.2;
				rig.judge.latency = latency;
				rig.Play(-0.2);
				rig.Run(0.25);
				rig.PressAtBeat(0, 0);
				// Beat 1's deadline is still 0.65s: an exact mapping would have moved it to 0.45s
				rig.Run(0.55);
				Assert.IsEmpty(rig.missed);
				rig.Run(0.1);
				Assert.AreEqual(1, rig.missed.Count);
			} finally {
				Object.DestroyImmediate(latency);
			}
		}
	}
}
