using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UnityX.Rhythm.Notes.Tests {
	public class NoteSchedulerTests {
		TestSong song;
		NoteScheduler scheduler;
		List<NoteInstance> entered;
		List<(NoteInstance note, NoteExitReason reason)> exited;

		[SetUp]
		public void SetUp() {
			song = new TestSong(120);
			// One beat (0.5s at 120bpm) behind and two ahead
			scheduler = new NoteScheduler(song.timeline, new BeatGrid(1)) { lookBehind = 0.5, lookAhead = 1 };
			entered = new List<NoteInstance>();
			exited = new List<(NoteInstance, NoteExitReason)>();
			scheduler.NoteEntered += entered.Add;
			scheduler.NoteExited += (note, reason) => exited.Add((note, reason));
		}

		void Run(double seconds) => song.Run(seconds, scheduler.Update);

		void RunWithoutJumps(double seconds) => song.Run(seconds, () => {
			scheduler.Update();
			Assert.AreEqual(0, scheduler.LastJump);
		});

		static long[] Repeats(IEnumerable<NoteInstance> notes) => notes.Select(n => n.id.repeat).ToArray();

		[Test]
		public void KeepsTheNotesInsideTheWindow() {
			song.clock.Play(0);
			scheduler.Update();
			CollectionAssert.AreEqual(new long[] { 0, 1 }, Repeats(scheduler.ActiveNotes));
			Run(1.25);
			// Song time 1.25s is beat 2.5: the window is beats 1.5 to 4.5
			CollectionAssert.AreEqual(new long[] { 2, 3, 4 }, Repeats(scheduler.ActiveNotes));
		}

		[Test]
		public void RaisesEachNoteOnceAsItEntersAndPasses() {
			song.clock.Play(0);
			Run(3);
			// Song time 3s is beat 6, so beats up to 7 have entered and beats up to 4 have passed
			CollectionAssert.AreEqual(Enumerable.Range(0, entered.Count).Select(i => (long)i), Repeats(entered));
			Assert.GreaterOrEqual(entered.Count, 8);
			CollectionAssert.AreEqual(Enumerable.Range(0, exited.Count).Select(i => (long)i), Repeats(exited.Select(e => e.note)));
			Assert.GreaterOrEqual(exited.Count, 5);
			Assert.IsTrue(exited.All(e => e.reason == NoteExitReason.Passed));
		}

		[Test]
		public void TempoRateAndSwingChangesKeepNoteIds() {
			song.clock.Play(0);
			Run(1);
			// The raw clock moves in whole buffers, so song time is just under 1s (beat 2): the window is beats 1 to 4
			CollectionAssert.AreEqual(new long[] { 1, 2, 3 }, Repeats(scheduler.ActiveNotes));
			entered.Clear();
			exited.Clear();
			song.TempoMap.SetTempo(2, 90);
			song.TempoMap.SetSwing(0, 0.5, 0.6);
			song.clock.SetPlaybackRate(0.75);
			scheduler.Update();
			// The window shrank to about beats 1.2 to 3.1: beat 1 fell out of the back and passed, beats 2 and 3 kept
			// their IDs, and nothing re-entered
			CollectionAssert.AreEqual(new long[] { 2, 3 }, Repeats(scheduler.ActiveNotes));
			CollectionAssert.IsEmpty(entered);
			Assert.AreEqual(1, exited.Count);
			Assert.AreEqual(1, exited[0].note.id.repeat);
			Assert.AreEqual(NoteExitReason.Passed, exited[0].reason);
		}

		[Test]
		public void AHoldCutAtItsRegionEndPasses() {
			var arrangement = new Arrangement();
			arrangement.Add(new Chart(new[] { new Note(3, 0, 2) }), 0, 4);
			scheduler.Source = arrangement;
			song.clock.Play(0);
			// Song time 3s is beat 6: the window starts at beat 5, past the region end at 4
			Run(3);
			Assert.AreEqual(1, entered.Count);
			Assert.AreEqual(1, exited.Count);
			Assert.AreEqual(3, exited[0].note.Beat);
			Assert.AreEqual(NoteExitReason.Passed, exited[0].reason);
		}

		[Test]
		public void PausingKeepsTheWindow() {
			song.clock.Play(0);
			Run(1);
			var before = Repeats(scheduler.ActiveNotes);
			song.clock.Pause();
			Run(2);
			CollectionAssert.AreEqual(before, Repeats(scheduler.ActiveNotes));
		}

		[Test]
		public void HalfSpeedCoversHalfAsManyBeats() {
			song.clock.SetPlaybackRate(0.5);
			song.clock.Play(10);
			scheduler.Update();
			// Song time 10s is beat 20. At half speed, one second ahead is half a second of song (one beat), and half a
			// second behind is half a beat.
			Assert.AreEqual(21, scheduler.WindowEndBeat, 1e-9);
			Assert.AreEqual(19.5, scheduler.WindowStartBeat, 1e-9);
		}

		[Test]
		public void SeekingBackRemovesNotesAndSeekingForwardSkipsThem() {
			song.clock.Play(10);
			scheduler.Update();
			exited.Clear();
			song.clock.Seek(0);
			scheduler.Update();
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Removed));
			exited.Clear();
			song.clock.Seek(10);
			scheduler.Update();
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Skipped));
		}

		[Test]
		public void ReportsHowFarASeekJumped() {
			song.clock.Play(0);
			scheduler.Update();
			RunWithoutJumps(0.5);
			var before = song.clock.SongTime;
			song.clock.Seek(before + 3);
			scheduler.Update();
			Assert.AreEqual(3, scheduler.LastJump, 1e-9);
			song.clock.Seek(before);
			scheduler.Update();
			Assert.AreEqual(-3, scheduler.LastJump, 1e-9);
			RunWithoutJumps(0.1);
		}

		[Test]
		public void AHitchPassesNotesRatherThanSkippingThem() {
			song.clock.Play(0);
			scheduler.Update();
			Run(0.5);
			exited.Clear();
			// One long frame: the song kept playing, so the notes in it were there to be played
			song.source.Advance(1.5);
			song.clock.Tick();
			scheduler.Update();
			Assert.AreEqual(0, scheduler.LastJump);
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Passed));
		}

		[Test]
		public void SeekingWhilePausedSkips() {
			song.clock.Play(0);
			scheduler.Update();
			Run(0.5);
			song.clock.Pause();
			scheduler.Update();
			exited.Clear();
			var songTimeBeforeTheSeek = song.clock.SongTime;
			song.clock.Seek(5);
			scheduler.Update();
			Assert.AreEqual(5 - songTimeBeforeTheSeek, scheduler.LastJump, 1e-9);
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Skipped));
		}

		[Test]
		public void PausingResumingAndChangingRateAreNotJumps() {
			song.clock.Play(0);
			scheduler.Update();
			RunWithoutJumps(0.5);
			song.clock.Pause();
			scheduler.Update();
			Assert.AreEqual(0, scheduler.LastJump);
			RunWithoutJumps(1);
			song.clock.Resume();
			scheduler.Update();
			Assert.AreEqual(0, scheduler.LastJump);
			song.clock.SetPlaybackRate(0.5);
			RunWithoutJumps(0.5);
		}

		// In the hitch tests, song time moves from about 0.25s to 0.55s, so beat 0 (at 0s) falls out of the back
		[Test]
		public void PausingAfterTheClockTicksOnAHitchIsNotAJump() {
			song.clock.Play(0);
			scheduler.Update();
			RunWithoutJumps(0.25);
			exited.Clear();
			// The paused song time includes this frame's advance
			song.source.Advance(0.3);
			song.clock.Tick();
			song.clock.Pause();
			scheduler.Update();
			Assert.AreEqual(0, scheduler.LastJump);
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Passed));
		}

		[Test]
		public void ResumingBeforeTheClockTicksOnAHitchIsNotAJump() {
			song.clock.Play(0);
			scheduler.Update();
			RunWithoutJumps(0.25);
			song.clock.Pause();
			scheduler.Update();
			RunWithoutJumps(1);
			exited.Clear();
			// Resume anchors at the last tick's DSP time, so this frame's advance plays
			song.source.Advance(0.3);
			song.clock.Resume();
			song.clock.Tick();
			scheduler.Update();
			Assert.AreEqual(0, scheduler.LastJump);
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Passed));
		}

		[Test]
		public void ChangingRateAfterTheClockTicksOnAHitchIsNotAJump() {
			song.clock.Play(0);
			scheduler.Update();
			RunWithoutJumps(0.25);
			exited.Clear();
			// The frame played at the old rate, but the clock reports the new one
			song.source.Advance(0.3);
			song.clock.Tick();
			song.clock.SetPlaybackRate(0.5);
			scheduler.Update();
			Assert.AreEqual(0, scheduler.LastJump);
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Passed));
		}

		[Test]
		public void IncreasingRateAfterTheClockTicksOnAHitchIsNotAJump() {
			// At double speed the look-behind covers twice the song, so it's short enough here for beat 1 (0.5s) to pass
			scheduler.lookBehind = 0.02;
			song.clock.Play(0);
			scheduler.Update();
			RunWithoutJumps(0.25);
			exited.Clear();
			// The frame played at the old rate, the slow edge of what the old and new rates allow
			song.source.Advance(0.3);
			song.clock.Tick();
			song.clock.SetPlaybackRate(2);
			scheduler.Update();
			Assert.AreEqual(0, scheduler.LastJump);
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Passed));
		}

		[Test]
		public void AnAudioDeviceRestartIsNotAJump() {
			song.clock.Play(0);
			scheduler.Update();
			RunWithoutJumps(0.5);
			exited.Clear();
			// The device restarts on a timeline 90 seconds earlier; the clock carries song time on across it
			song.source.DspOffset -= 90;
			song.source.Advance(1 / 60.0);
			song.clock.Tick();
			Assert.Less(song.clock.DspTime, 20);
			scheduler.Update();
			Assert.AreEqual(0, scheduler.LastJump);
			RunWithoutJumps(1);
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Passed));
		}

		[Test]
		public void GivesHoldsTheirEndDspTime() {
			scheduler.Source = new Chart(new[] { new Note(1, 0, 2) });
			song.clock.Play(0);
			var start = song.clock.DspTime;
			scheduler.Update();
			Assert.AreEqual(start + 1.5, scheduler.EndDspTimeOf(scheduler.ActiveNotes[0]), 1e-9);
		}

		[Test]
		public void SourceEditsOnlyMoveTheNotesThatChanged() {
			var pattern = new Pattern(4, new[] { new Note(0), new Note(1), new Note(2), new Note(3) });
			scheduler.Source = pattern;
			scheduler.lookAhead = 2;
			song.clock.Play(0);
			scheduler.Update();
			entered.Clear();
			exited.Clear();
			pattern.RemoveAt(2);
			pattern.Add(new Note(2.5));
			scheduler.Update();
			Assert.AreEqual(1, exited.Count);
			Assert.AreEqual(NoteExitReason.Removed, exited[0].reason);
			Assert.AreEqual(2, exited[0].note.Beat);
			CollectionAssert.AreEqual(new[] { 2.5 }, entered.Select(n => n.Beat));
		}

		[Test]
		public void ActiveNotesAreInBeatOrder() {
			scheduler.Source = new Chart(new[] { new Note(1, 2), new Note(0.5, 1), new Note(0.5, 0) });
			song.clock.Play(0);
			scheduler.Update();
			CollectionAssert.AreEqual(new[] { 0.5, 0.5, 1 }, scheduler.ActiveNotes.Select(n => n.Beat));
			CollectionAssert.AreEqual(new[] { 0, 1, 2 }, scheduler.ActiveNotes.Select(n => n.note.lane));
		}

		[Test]
		public void DuplicateIdsAreDropped() {
			scheduler.Source = new Chart(new[] { new Note(1, 0, 0, 1, 1), new Note(1, 0, 0, 1, 2) });
			song.clock.Play(0);
			scheduler.Update();
			Assert.AreEqual(1, scheduler.ActiveNotes.Count);
		}

		[Test]
		public void GivesEachNoteItsDspTime() {
			song.clock.Play(0);
			var start = song.clock.DspTime;
			scheduler.Update();
			Assert.AreEqual(start + 0.5, scheduler.DspTimeOf(scheduler.ActiveNotes[1]), 1e-9);
			Assert.IsTrue(scheduler.TryGetNote(scheduler.ActiveNotes[1].id, out var note));
			Assert.AreEqual(1, note.Beat);
		}

		[Test]
		public void NoteSchedulerRunsOnAConductor() {
			var gameObject = new GameObject("Conductor");
			var conductor = gameObject.AddComponent<Conductor>();
			conductor.smoothing = Conductor.SmoothingMode.Raw;
			conductor.TempoMap = new TempoMap(120);
			var source = new ManualAudioTimeSource();
			conductor.Initialize(source);
			conductor.Tick();
			var scheduler = new NoteScheduler(conductor, new BeatGrid(1)) { lookBehind = 0, lookAhead = 1 };
			conductor.Clock.Play(0);
			conductor.Tick();
			scheduler.Update();
			Assert.AreEqual(2, scheduler.ActiveNotes.Count);
			Assert.AreEqual(conductor.DspTimeAtBeat(1), scheduler.DspTimeOf(scheduler.ActiveNotes[1]), 1e-9);
			Object.DestroyImmediate(gameObject);
		}
	}
}
