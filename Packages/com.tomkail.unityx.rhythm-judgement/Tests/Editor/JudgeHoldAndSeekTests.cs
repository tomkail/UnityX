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
	}
}
