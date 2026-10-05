using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UnityX.Rhythm.Notes.Tests {
	public class NoteSourceTests {
		static List<NoteInstance> Query(INoteSource source, double start, double end) {
			var results = new List<NoteInstance>();
			source.GetNotes(start, end, results);
			return results.OrderBy(n => n.Beat).ThenBy(n => n.note.lane).ToList();
		}

		[Test]
		public void NoteIdsAreEqualByValue() {
			var a = new NoteId(1, 480, 2, 3);
			Assert.AreEqual(a, new NoteId(1, 480, 2, 3));
			Assert.AreNotEqual(a, new NoteId(1, 480, 2, 4));
			Assert.AreEqual(a.GetHashCode(), new NoteId(1, 480, 2, 3).GetHashCode());
			Assert.AreEqual(480, NoteId.ToTick(0.5));
		}

		[Test]
		public void PatternLoopsFromBeatZero() {
			var pattern = new Pattern(2, new[] { new Note(0, 0), new Note(1.5, 1) });
			var notes = Query(pattern, 0, 6);
			CollectionAssert.AreEqual(new[] { 0, 1.5, 2, 3.5, 4, 5.5 }, notes.Select(n => n.Beat));
			CollectionAssert.AreEqual(new long[] { 0, 0, 1, 1, 2, 2 }, notes.Select(n => n.id.repeat));
			Assert.AreEqual(new NoteId(0, NoteId.ToTick(1.5), 1, 2), notes[5].id);
		}

		[Test]
		public void PatternPlaysNothingBeforeBeatZero() {
			var pattern = new Pattern(4, new[] { new Note(0), new Note(2) });
			CollectionAssert.AreEqual(new[] { 0.0 }, Query(pattern, -8, 1).Select(n => n.Beat));
		}

		[Test]
		public void WindowIncludesStartAndExcludesEnd() {
			var pattern = new Pattern(1, new[] { new Note(0) });
			CollectionAssert.AreEqual(new[] { 2.0, 3.0 }, Query(pattern, 2, 4).Select(n => n.Beat));
		}

		[Test]
		public void HoldsOverlappingTheWindowAreIncluded() {
			var pattern = new Pattern(4, new[] { new Note(0, 0, 3) });
			// The hold from beat 4 is still going at 6.5; the one from 0 ended at 3
			CollectionAssert.AreEqual(new[] { 4.0 }, Query(pattern, 6.5, 7).Select(n => n.Beat));
		}

		[Test]
		public void AddingANoteKeepsTheOtherIds() {
			var pattern = new Pattern(4, new[] { new Note(1), new Note(3) });
			var before = Query(pattern, 0, 4).Select(n => n.id).ToList();
			pattern.Add(new Note(0));
			var after = Query(pattern, 0, 4).Select(n => n.id).ToList();
			CollectionAssert.IsSubsetOf(before, after);
			Assert.AreEqual(3, after.Count);
		}

		[Test]
		public void EditsRaiseChanged() {
			var pattern = new Pattern(4);
			var changes = 0;
			pattern.Changed += () => changes++;
			pattern.Add(new Note(1));
			pattern.Set(0, new Note(2));
			pattern.RemoveAt(0);
			pattern.LengthInBeats = 8;
			pattern.SetNotes(new[] { new Note(0) });
			Assert.AreEqual(5, changes);
		}

		[Test]
		public void CloneIsIndependent() {
			var pattern = new Pattern(4, new[] { new Note(1) });
			var copy = pattern.Clone();
			copy.Add(new Note(2));
			Assert.AreEqual(1, pattern.Notes.Count);
			Assert.AreEqual(2, copy.Notes.Count);
		}

		[Test]
		public void InvalidPatternLengthThrows() {
			Assert.Throws<System.ArgumentOutOfRangeException>(() => new Pattern(0));
			Assert.Throws<System.ArgumentOutOfRangeException>(() => new Pattern(double.NaN));
		}

		[Test]
		public void ChartPlaysOnce() {
			var chart = new Chart(new[] { new Note(1), new Note(2, 1, 2), new Note(8) });
			CollectionAssert.AreEqual(new[] { 1.0, 2.0 }, Query(chart, 0, 4).Select(n => n.Beat));
			CollectionAssert.AreEqual(new[] { 2.0 }, Query(chart, 3.5, 4).Select(n => n.Beat));
			Assert.AreEqual(8, chart.EndBeat);
			Assert.AreEqual(new NoteId(0, NoteId.ToTick(2), 1, 0), Query(chart, 0, 4)[1].id);
		}

		[Test]
		public void BeatGridPlacesATapEveryInterval() {
			var grid = new BeatGrid(0.5, 0.25, lane: 3);
			var notes = Query(grid, 1, 2.5);
			CollectionAssert.AreEqual(new[] { 1.25, 1.75, 2.25 }, notes.Select(n => n.Beat));
			Assert.IsTrue(notes.All(n => n.note.lane == 3));
			CollectionAssert.AreEqual(new long[] { 2, 3, 4 }, notes.Select(n => n.id.repeat));
		}

		[Test]
		public void BeatGridDoesNotSkipANoteOnTheWindowStart() {
			var grid = new BeatGrid(0.1);
			// 0.3 / 0.1 is 2.9999999999999996 in doubles
			Assert.AreEqual(0.3, Query(grid, 0.30000000000000004 - 1e-17, 0.35)[0].Beat, 1e-12);
			Assert.AreEqual(3, Query(grid, 0.3, 0.35)[0].id.repeat);
		}

		[Test]
		public void BeatGridPlaysNothingForANaNWindowStart() {
			CollectionAssert.IsEmpty(Query(new BeatGrid(1), double.NaN, 4));
		}

		[Test]
		public void NonFiniteWindowsPlayNothingAndFinish() {
			var sources = new INoteSource[] { new BeatGrid(1), new Pattern(4, new[] { new Note(0), new Note(2, 0, 1) }) };
			var windows = new[] { (0.0, double.PositiveInfinity), (0.0, double.NaN), (double.NaN, 4.0), (double.NaN, double.NaN), (double.NegativeInfinity, double.PositiveInfinity) };
			foreach (var source in sources) {
				foreach (var (start, end) in windows) {
					var results = new List<NoteInstance>();
					// On another thread, so a regression fails the test instead of hanging the run
					var query = Task.Run(() => source.GetNotes(start, end, results));
					Assert.IsTrue(query.Wait(TimeSpan.FromSeconds(5)), $"{source.GetType().Name} didn't finish for [{start}, {end})");
					CollectionAssert.IsEmpty(results, $"{source.GetType().Name} for [{start}, {end})");
				}
			}
		}

		[Test]
		public void PatternSkipsNotesWithoutALength() {
			var pattern = new Pattern(4, new[] { new Note(0, 0, double.NaN), new Note(1) });
			CollectionAssert.AreEqual(new[] { 1.0, 5.0 }, Query(pattern, 0, 8).Select(n => n.Beat));
		}
	}
}
