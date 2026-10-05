using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnityX.Rhythm.Notes.Tests {
	public class NoteAssetTests {
		[Test]
		public void PatternAssetPlaysACopy() {
			var asset = ScriptableObject.CreateInstance<PatternAsset>();
			asset.Pattern.Add(new Note(1));
			var copy = asset.CreateRuntimeCopy();
			copy.Add(new Note(2));
			Assert.AreEqual(1, asset.Pattern.Notes.Count);
			Assert.AreEqual(2, copy.Notes.Count);
			Object.DestroyImmediate(asset);
		}

		[Test]
		public void ChartAssetPlaysCopies() {
			var asset = ScriptableObject.CreateInstance<ChartAsset>();
			asset.Chart.Add(new Note(4));
			var chart = asset.CreateRuntimeChart();
			var tempoMap = asset.CreateRuntimeTempoMap();
			chart.Add(new Note(8));
			tempoMap.SetTempo(0, 90);
			Assert.AreEqual(1, asset.Chart.Notes.Count);
			Assert.AreEqual(120, asset.TempoMap.BpmAtBeat(0), 1e-9);
			Object.DestroyImmediate(asset);
		}

		[Test]
		public void PatternsSurviveSerialization() {
			var pattern = new Pattern(3, new[] { new Note(0.5, 2, 1, 0.75f, 7) });
			var copy = JsonUtility.FromJson<Pattern>(JsonUtility.ToJson(pattern));
			Assert.AreEqual(3, copy.LengthInBeats);
			var note = copy.Notes[0];
			Assert.AreEqual(0.5, note.beat);
			Assert.AreEqual(2, note.lane);
			Assert.AreEqual(1, note.length);
			Assert.AreEqual(0.75f, note.velocity);
			Assert.AreEqual(7, note.data);
			var results = new List<NoteInstance>();
			copy.GetNotes(0, 3, results);
			Assert.AreEqual(1, results.Count);
		}
	}
}
