using System.Linq;
using NUnit.Framework;

namespace UnityX.Rhythm.Audio.Tests {
	public class NoteSoundSchedulerTests {
		TestSong song;
		FakeVoicePlayer player;
		NoteSoundScheduler scheduler;

		[SetUp]
		public void SetUp() {
			song = new TestSong(120);
			player = new FakeVoicePlayer();
			scheduler = new NoteSoundScheduler(song.timeline, player, new BeatGrid(1), lookAhead: 0.2);
		}

		[TearDown]
		public void TearDown() => scheduler.Dispose();

		void Run(double seconds) => song.Run(seconds, scheduler.Update);

		double DspAtBeat(double beat) => song.timeline.DspTimeAtBeat(beat);

		[Test]
		public void QueuesEachNoteOnceAtItsExactDspTime() {
			song.clock.Play(0);
			scheduler.Update();
			Run(2.1);
			CollectionAssert.AreEqual(new[] { 0.0, 1, 2, 3, 4 }, player.voices.Select(v => v.note.Beat));
			foreach (var voice in player.voices) Assert.AreEqual(DspAtBeat(voice.note.Beat), voice.StartDspTime, 1e-9);
		}

		[Test]
		public void QueuesOnlyWithinTheLookAhead() {
			song.clock.Play(-1);
			scheduler.Update();
			Run(0.7);
			// Beat 0 is at 1s; nothing until 0.8s
			CollectionAssert.IsEmpty(player.voices);
			Run(0.15);
			Assert.AreEqual(1, player.voices.Count);
			Assert.Greater(player.voices[0].StartDspTime, song.clock.DspTime);
		}

		[Test]
		public void PassingNotesAreNotCut() {
			player.length = 5;
			song.clock.Play(0);
			scheduler.Update();
			Run(3);
			Assert.IsTrue(player.voices.All(v => !v.stopped));
		}

		[Test]
		public void TempoChangesRetimeQueuedSounds() {
			// Long enough that beat 2 stays in reach after the change; a note pushed out of reach is cancelled instead
			scheduler.LookAhead = 0.5;
			song.clock.Play(0);
			scheduler.Update();
			Run(0.85);
			var queued = player.voices.Single(v => v.note.Beat == 2);
			song.TempoMap.SetTempo(1.75, 60);
			scheduler.Update();
			Assert.AreEqual(DspAtBeat(2), queued.StartDspTime, 1e-9);
			Assert.AreEqual(1, queued.reschedules);
			Assert.IsFalse(queued.stopped);
			Assert.AreEqual(3, player.voices.Count);
		}

		[Test]
		public void RateChangesRetimeQueuedSounds() {
			// Long enough that beat 2 stays in reach after the change; a note pushed out of reach is cancelled instead
			scheduler.LookAhead = 0.5;
			song.clock.Play(0);
			scheduler.Update();
			Run(0.85);
			var queued = player.voices.Single(v => v.note.Beat == 2);
			song.clock.SetPlaybackRate(0.5);
			scheduler.Update();
			Assert.AreEqual(DspAtBeat(2), queued.StartDspTime, 1e-9);
			Assert.Greater(queued.StartDspTime, song.clock.DspTime + 0.25);
			Assert.AreEqual(1, queued.reschedules);
			Assert.IsFalse(queued.stopped);
			Assert.AreEqual(3, player.voices.Count);
		}

		[Test]
		public void RemovingANoteCancelsItsSound() {
			var pattern = new Pattern(4, new[] { new Note(0), new Note(2) });
			scheduler.Source = pattern;
			song.clock.Play(0);
			scheduler.Update();
			Run(0.85);
			var queued = player.voices.Single(v => v.note.Beat == 2);
			pattern.RemoveAt(1);
			scheduler.Update();
			Assert.IsTrue(queued.stopped);
		}

		[Test]
		public void PauseCancelsQueuedSoundsAndResumeQueuesThemAgain() {
			song.clock.Play(0);
			scheduler.Update();
			Run(0.85);
			var queued = player.voices.Single(v => v.note.Beat == 2);
			song.clock.Pause();
			Run(1);
			Assert.IsTrue(queued.stopped);
			Assert.AreEqual(3, player.voices.Count);
			song.clock.Resume();
			scheduler.Update();
			var again = player.voices.Last();
			Assert.AreEqual(2, again.note.Beat);
			Assert.AreEqual(DspAtBeat(2), again.StartDspTime, 1e-9);
		}

		[Test]
		public void SeekingCancelsSoundsThatHaveNotStarted() {
			song.clock.Play(0);
			scheduler.Update();
			Run(0.85);
			var queued = player.voices.Single(v => v.note.Beat == 2);
			song.clock.Seek(20);
			scheduler.Update();
			Assert.IsTrue(queued.stopped);
			song.clock.Seek(0.95);
			scheduler.Update();
			Assert.AreEqual(2, player.voices.Last().note.Beat);
			Assert.IsFalse(player.voices.Last().stopped);
		}

		[Test]
		public void SeekingBackReplaysANoteThatIsStillRinging() {
			player.length = 5;
			song.clock.Play(0);
			scheduler.Update();
			// Just past beat 2 (1s), still inside the 0.1s look-behind, so seeking back doesn't make it exit
			Run(1.05);
			var first = player.voices.Single(v => v.note.Beat == 2);
			song.clock.Seek(song.clock.SongTime - 0.1);
			scheduler.Update();
			Run(0.15);
			Assert.AreEqual(2, player.voices.Count(v => v.note.Beat == 2));
			Assert.IsFalse(first.stopped);
		}

		[Test]
		public void RetimingTooLateCancels() {
			song.clock.Play(0);
			scheduler.Update();
			Run(0.85);
			var queued = player.voices.Single(v => v.note.Beat == 2);
			// Beat 2 (1s) is now 0.05s behind: inside the look-behind, but past the late tolerance
			song.clock.Seek(1.05);
			scheduler.Update();
			Assert.IsTrue(queued.stopped);
			Assert.AreEqual(1, player.voices.Count(v => v.note.Beat == 2));
		}

		[Test]
		public void RetimingWithinToleranceReschedules() {
			song.clock.Play(0);
			scheduler.Update();
			Run(0.85);
			var queued = player.voices.Single(v => v.note.Beat == 2);
			// Beat 2 is now 0.01s behind, within the 0.02s late tolerance
			song.clock.Seek(1.01);
			scheduler.Update();
			Assert.IsFalse(queued.stopped);
			Assert.AreEqual(1, queued.reschedules);
			Assert.AreEqual(DspAtBeat(2), queued.StartDspTime, 1e-9);
			Assert.AreEqual(1, player.voices.Count(v => v.note.Beat == 2));
		}

		[Test]
		public void LateNotesAreSkipped() {
			// Starting 0.05s after beat 2 (inside the look-behind) is too late to play it
			song.clock.Play(1.05);
			scheduler.Update();
			Assert.IsFalse(player.voices.Any(v => v.note.Beat == 2));
		}

		[Test]
		public void PlayingFromABeatPlaysIt() {
			song.clock.Play(1);
			scheduler.Update();
			Assert.AreEqual(2, player.voices.Single().note.Beat);
			Assert.AreEqual(song.clock.DspTime, player.voices[0].StartDspTime, 1e-9);
		}

		[Test]
		public void SwappingTheSourceCancelsTheOldSounds() {
			song.clock.Play(0);
			scheduler.Update();
			Run(0.85);
			var queued = player.voices.Single(v => v.note.Beat == 2);
			scheduler.Source = new BeatGrid(1, 0.5, lane: 1);
			scheduler.Update();
			Assert.IsTrue(queued.stopped);
		}

		[Test]
		public void DisposeStopsEverything() {
			player.length = 5;
			song.clock.Play(0);
			scheduler.Update();
			Run(0.85);
			scheduler.Dispose();
			Assert.IsTrue(player.voices.All(v => v.stopped));
		}
	}
}
