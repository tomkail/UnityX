using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnityX.Rhythm.MidiTests {
	public class MidiLaneInputTests {
		GameObject gameObject;
		Conductor conductor;
		MidiLaneMap map;
		MidiLaneInput midi;
		List<RhythmInput> received;

		[SetUp]
		public void SetUp() {
			gameObject = new GameObject("Song");
			conductor = gameObject.AddComponent<Conductor>();
			conductor.smoothing = Conductor.SmoothingMode.Raw;
			conductor.Initialize(new ManualAudioTimeSource { Realtime = 10 });
			conductor.Tick();
			map = ScriptableObject.CreateInstance<MidiLaneMap>();
			// General MIDI drums: kick 36, snare 38, and a snare on channel 9 that goes to its own lane
			map.entries.Add(new MidiLaneMap.Entry { note = 36, lane = 0 });
			map.entries.Add(new MidiLaneMap.Entry { note = 38, lane = 1 });
			map.entries.Add(new MidiLaneMap.Entry { note = 38, lane = 5, matchChannel = true, channel = 9 });
			midi = gameObject.AddComponent<MidiLaneInput>();
			midi.conductor = conductor;
			midi.map = map;
			received = new List<RhythmInput>();
			midi.InputReceived += received.Add;
		}

		[TearDown]
		public void TearDown() {
			Object.DestroyImmediate(gameObject);
			Object.DestroyImmediate(map);
		}

		[Test]
		public void TheMapPrefersAnEntryForTheNotesChannel() {
			Assert.IsTrue(map.TryGetLane(38, 0, out var lane));
			Assert.AreEqual(1, lane);
			Assert.IsTrue(map.TryGetLane(38, 9, out lane));
			Assert.AreEqual(5, lane);
			Assert.IsFalse(map.TryGetLane(40, 0, out _));
		}

		[Test]
		public void NoteOnsArePressesWithTheirVelocity() {
			midi.HandleNote(36, 0, 0.8f, true, 9.9);
			Assert.AreEqual(0, received[0].lane);
			Assert.IsTrue(received[0].IsPress);
			Assert.AreEqual(0.8f, received[0].velocity);
			Assert.AreEqual(conductor.Clock.RealtimeToDspTime(9.9), received[0].dspTime, 1e-9);
		}

		[Test]
		public void NoteOffsAndSilentNoteOnsAreReleases() {
			midi.HandleNote(38, 0, 0, false, 10);
			midi.HandleNote(38, 0, 0, true, 10);
			Assert.AreEqual(2, received.Count);
			Assert.IsTrue(received.TrueForAll(r => r.phase == InputPhase.Release && r.lane == 1));
		}

		[Test]
		public void ReleasesCanBeTurnedOff() {
			midi.sendReleases = false;
			midi.HandleNote(38, 0, 0, false, 10);
			Assert.IsEmpty(received);
		}

		[Test]
		public void UnmappedNotesAreIgnored() {
			midi.HandleNote(40, 0, 1, true, 10);
			midi.map = null;
			midi.HandleNote(36, 0, 1, true, 10);
			Assert.IsEmpty(received);
		}
	}
}
