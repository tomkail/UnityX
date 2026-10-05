using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnityX.Rhythm.Tests {
	public class ConductorTests {
		GameObject gameObject;
		Conductor conductor;
		FakeAudioTimeSource source;

		[SetUp]
		public void SetUp() {
			gameObject = new GameObject("Conductor");
			conductor = gameObject.AddComponent<Conductor>();
			conductor.smoothing = Conductor.SmoothingMode.Raw;
			conductor.TempoMap = new TempoMap(120);
			source = new FakeAudioTimeSource();
			conductor.Initialize(source);
			conductor.Tick();
		}

		[TearDown]
		public void TearDown() => Object.DestroyImmediate(gameObject);

		void Run(double seconds) {
			for (var t = 0.0; t < seconds; t += 1 / 60.0) {
				source.Advance(1 / 60.0);
				conductor.Tick();
			}
		}

		[Test]
		public void RaisesEachBeatAndBarWithItsDspTime() {
			var beats = new List<BeatEvent>();
			var bars = new List<BeatEvent>();
			conductor.BeatCrossed += beats.Add;
			conductor.BarCrossed += bars.Add;
			conductor.Clock.Play(0);
			var startDsp = conductor.Clock.DspTime;
			Run(2.1);
			CollectionAssert.AreEqual(new long[] { 1, 2, 3, 4 }, beats.ConvertAll(e => e.index));
			Assert.AreEqual(startDsp + 0.5, beats[0].dspTime, 1e-9);
			Assert.AreEqual(1, bars.Count);
			Assert.AreEqual(1, bars[0].index);
			Assert.AreEqual(4, bars[0].beat, 1e-9);
		}

		[Test]
		public void SubdivisionsFollowSwing() {
			conductor.subdivisionsPerBeat = 2;
			conductor.TempoMap.SetSwing(0, 0.5, 2.0 / 3);
			var subdivisions = new List<BeatEvent>();
			conductor.SubdivisionCrossed += subdivisions.Add;
			conductor.Clock.Play(0);
			var startDsp = conductor.Clock.DspTime;
			Run(0.6);
			Assert.AreEqual(1, subdivisions[0].index);
			Assert.AreEqual(0.5, subdivisions[0].beat, 1e-9);
			// At 120bpm the swung off-beat lands two thirds of the way through the beat
			Assert.AreEqual(startDsp + 1.0 / 3, subdivisions[0].dspTime, 1e-9);
		}

		[Test]
		public void ScheduledEventsFireAheadOfTheBeat() {
			conductor.lookAheadTime = 0.25;
			var scheduledBeforeCrossed = false;
			var scheduled = false;
			conductor.BeatScheduled += e => { if (e.index == 1) scheduled = true; };
			conductor.BeatCrossed += e => { if (e.index == 1) scheduledBeforeCrossed = scheduled; };
			conductor.Clock.Play(0);
			Run(0.6);
			Assert.IsTrue(scheduledBeforeCrossed);
		}

		[Test]
		public void SeekingDoesNotFloodEvents() {
			var count = 0;
			conductor.BeatCrossed += _ => count++;
			conductor.Clock.Play(0);
			Run(0.2);
			conductor.Clock.Seek(60);
			Run(0.1);
			Assert.LessOrEqual(count, 1);
		}

		[Test]
		public void BeatPhaseMeasuresDistanceToTheGrid() {
			conductor.Clock.Play(0);
			Run(0.1);
			var phase = conductor.GetBeatPhase(conductor.DspTimeAtBeat(2) + 0.01);
			Assert.AreEqual(2, phase.nearestBeat, 1e-9);
			Assert.AreEqual(0.01, phase.timeOffset, 1e-9);
			Assert.AreEqual(0.02, phase.beatOffset, 1e-9);
		}

		[Test]
		public void AudibleBeatAllowsForOutputLatency() {
			var latency = ScriptableObject.CreateInstance<RhythmLatency>();
			latency.audioOutputLatency = 0.1;
			conductor.Latency = latency;
			conductor.Clock.Play(0);
			Run(1);
			Assert.AreEqual(conductor.Beat - 0.2, conductor.AudibleBeat, 1e-9);
			Object.DestroyImmediate(latency);
		}
	}
}
