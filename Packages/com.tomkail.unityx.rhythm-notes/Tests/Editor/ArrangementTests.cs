using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace UnityX.Rhythm.Notes.Tests {
	public class ArrangementTests {
		static List<NoteInstance> Query(INoteSource source, double start, double end) {
			var results = new List<NoteInstance>();
			source.GetNotes(start, end, results);
			return results.OrderBy(n => n.Beat).ThenBy(n => n.note.lane).ToList();
		}

		[Test]
		public void ArrangementPlaysEachRegionFromItsStart() {
			var a = new Pattern(1, new[] { new Note(0, 0) });
			var b = new Pattern(1, new[] { new Note(0.5, 1) });
			var arrangement = new Arrangement();
			arrangement.Add(a, 0, 2);
			arrangement.Add(b, 2);
			var notes = Query(arrangement, 0, 4);
			CollectionAssert.AreEqual(new[] { 0, 1, 2.5, 3.5 }, notes.Select(n => n.Beat));
			CollectionAssert.AreEqual(new[] { 0, 0, 1, 1 }, notes.Select(n => n.note.lane));
		}

		[Test]
		public void SamePatternInTwoRegionsGivesDifferentIds() {
			var pattern = new Pattern(1, new[] { new Note(0) });
			var arrangement = new Arrangement();
			var first = arrangement.Add(pattern, 0, 1);
			var second = arrangement.Add(pattern, 1, 2);
			var notes = Query(arrangement, 0, 2);
			Assert.AreEqual(first.Id, notes[0].id.source);
			Assert.AreEqual(second.Id, notes[1].id.source);
			Assert.AreNotEqual(notes[0].id, notes[1].id);
		}

		[Test]
		public void SwitchAtEndsTheCurrentRegionAndReplacesQueuedOnes() {
			var a = new Pattern(1, new[] { new Note(0, 0) });
			var b = new Pattern(1, new[] { new Note(0, 1) });
			var c = new Pattern(1, new[] { new Note(0, 2) });
			var arrangement = new Arrangement();
			var playing = arrangement.Add(a, 0);
			arrangement.Add(b, 8);
			arrangement.SwitchAt(4, c);
			Assert.AreEqual(4, playing.EndBeat);
			Assert.AreEqual(2, arrangement.Regions.Count);
			CollectionAssert.AreEqual(new[] { 0, 0, 2, 2 }, Query(arrangement, 2, 6).Select(n => n.note.lane));
			CollectionAssert.AreEqual(new[] { 2 }, Query(arrangement, 8, 9).Select(n => n.note.lane));
		}

		[Test]
		public void SwitchingKeepsTheIdsOfNotesBeforeTheSwitch() {
			var arrangement = new Arrangement();
			arrangement.Add(new Pattern(1, new[] { new Note(0) }), 0);
			var before = Query(arrangement, 0, 4).Select(n => n.id).ToList();
			arrangement.SwitchAt(4, new Pattern(1, new[] { new Note(0.5) }));
			CollectionAssert.AreEqual(before, Query(arrangement, 0, 4).Select(n => n.id));
		}

		[Test]
		public void ArrangementForwardsSourceChanges() {
			var pattern = new Pattern(1);
			var arrangement = new Arrangement();
			var region = arrangement.Add(pattern, 0);
			var changes = 0;
			arrangement.Changed += () => changes++;
			pattern.Add(new Note(0));
			Assert.AreEqual(1, changes);
			arrangement.Remove(region);
			pattern.Add(new Note(0.5));
			Assert.AreEqual(2, changes);
		}

		[Test]
		public void RegionsMustEndAfterTheyStart() {
			var arrangement = new Arrangement();
			Assert.Throws<System.ArgumentOutOfRangeException>(() => arrangement.Add(new Pattern(1), 4, 4));
			var region = arrangement.Add(new Pattern(1), 4);
			Assert.Throws<System.ArgumentOutOfRangeException>(() => arrangement.SetEnd(region, 2));
		}
	}
}
