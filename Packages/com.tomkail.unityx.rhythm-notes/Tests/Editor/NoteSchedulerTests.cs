// CircularRhythm/UnityX/Packages/com.tomkail.unityx.rhythm-notes/Tests/Editor/NoteSchedulerTests.cs
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
			entered.Clear();
			song.TempoMap.SetTempo(2, 90);
			song.TempoMap.SetSwing(0, 0.5, 0.6);
			song.clock.SetPlaybackRate(0.75);
			scheduler.Update();
			// The window shrank in beats, so notes only left from the far end; none re-entered
			CollectionAssert.IsEmpty(entered);
			Assert.IsTrue(exited.Where(e => e.note.Beat > 2).All(e => e.reason == NoteExitReason.Removed));
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
		public void SeekingBackRemovesNotesAndSeekingForwardPassesThem() {
			song.clock.Play(10);
			scheduler.Update();
			exited.Clear();
			song.clock.Seek(0);
			scheduler.Update();
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Removed));
			exited.Clear();
			song.clock.Seek(10);
			scheduler.Update();
			Assert.IsTrue(exited.Count > 0 && exited.All(e => e.reason == NoteExitReason.Passed));
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
