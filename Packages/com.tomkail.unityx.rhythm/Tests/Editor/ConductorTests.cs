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
			// Playing from 0 reports beat 0 and bar 0 too
			CollectionAssert.AreEqual(new long[] { 0, 1, 2, 3, 4 }, beats.ConvertAll(e => e.index));
			Assert.AreEqual(startDsp, beats[0].dspTime, 1e-9);
			Assert.AreEqual(startDsp + 0.5, beats[1].dspTime, 1e-9);
			CollectionAssert.AreEqual(new long[] { 0, 1 }, bars.ConvertAll(e => e.index));
			Assert.AreEqual(4, bars[1].beat, 1e-9);
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
			var offBeat = subdivisions.Find(e => e.index == 1);
			Assert.AreEqual(0.5, offBeat.beat, 1e-9);
			// At 120bpm the swung off-beat lands two thirds of the way through the beat
			Assert.AreEqual(startDsp + 1.0 / 3, offBeat.dspTime, 1e-9);
		}

		[Test]
		public void ScheduledEventsFireAheadOfTheBeat() {
			conductor.lookAheadTime = 0.25;
			var scheduledBeforeCrossed = false;
			var scheduled = false;
			var scheduledLead = 0.0;
			var scheduledDsp = 0.0;
			conductor.BeatScheduled += e => {
				if (e.index != 1) return;
				scheduled = true;
				scheduledLead = e.dspTime - conductor.Clock.DspTime;
				scheduledDsp = e.dspTime;
			};
			conductor.BeatCrossed += e => { if (e.index == 1) scheduledBeforeCrossed = scheduled; };
			conductor.Clock.Play(0);
			Run(0.6);
			Assert.IsTrue(scheduledBeforeCrossed);
			Assert.AreEqual(conductor.DspTimeAtBeat(1), scheduledDsp, 1e-9);
			// Fires within a frame and an audio buffer of lookAheadTime before the beat
			Assert.AreEqual(0.25, scheduledLead, 1 / 60.0 + source.BufferDuration);
		}

		[Test]
		public void SeekingDoesNotFloodEvents() {
			var count = 0;
			conductor.BeatCrossed += _ => count++;
			conductor.Clock.Play(0.1);
			Run(0.2);
			conductor.Clock.Seek(60.1);
			Run(0.1);
			Assert.AreEqual(0, count);
		}

		[Test]
		public void SeekingOntoABeatReportsIt() {
			var beats = new List<long>();
			conductor.BeatCrossed += e => beats.Add(e.index);
			conductor.Clock.Play(0.1);
			Run(0.1);
			conductor.Clock.Seek(30);
			Run(0.1);
			CollectionAssert.AreEqual(new long[] { 60 }, beats);
		}

		[Test]
		public void PauseAndResumeDoNotRepeatBeats() {
			var beats = new List<long>();
			conductor.BeatCrossed += e => beats.Add(e.index);
			conductor.Clock.Play(0);
			Run(0.6);
			conductor.Clock.Pause();
			Run(0.5);
			conductor.Clock.Resume();
			Run(0.6);
			CollectionAssert.AreEqual(new long[] { 0, 1, 2 }, beats);
		}

		[Test]
		public void TempoEditsDoNotFloodOrRepeatBeats() {
			var beats = new List<long>();
			conductor.BeatCrossed += e => beats.Add(e.index);
			conductor.Clock.Play(0);
			Run(1.1);
			CollectionAssert.AreEqual(new long[] { 0, 1, 2 }, beats);
			// Doubling the tempo moves the position from about beat 2.2 to 4.4
			conductor.TempoMap.SetTempo(0, 240);
			Run(1 / 60.0);
			CollectionAssert.AreEqual(new long[] { 0, 1, 2 }, beats, "No flood of the skipped beats");
			Run(0.2);
			Assert.AreEqual(5, beats[beats.Count - 1]);
		}

		[Test]
		public void ReplacingTheTempoMapReprimes() {
			var beats = new List<long>();
			conductor.BeatCrossed += e => beats.Add(e.index);
			conductor.Clock.Play(0);
			Run(1.1);
			conductor.TempoMap = new TempoMap(480);
			Run(1 / 60.0);
			CollectionAssert.AreEqual(new long[] { 0, 1, 2 }, beats);
		}

		[Test]
		public void BarsFollowTimeSignatureChanges() {
			conductor.TempoMap.SetTimeSignature(4, 3, 4);
			var bars = new List<BeatEvent>();
			conductor.BarCrossed += bars.Add;
			conductor.Clock.Play(0);
			Run(3.6);
			CollectionAssert.AreEqual(new long[] { 0, 1, 2 }, bars.ConvertAll(e => e.index));
			CollectionAssert.AreEqual(new double[] { 0, 4, 7 }, bars.ConvertAll(e => e.beat));
		}

		[Test]
		public void ChangingSubdivisionsMidPlayDoesNotFlood() {
			conductor.subdivisionsPerBeat = 2;
			var subdivisions = new List<long>();
			conductor.SubdivisionCrossed += e => subdivisions.Add(e.index);
			conductor.Clock.Play(0.01);
			Run(0.6);
			var before = subdivisions.Count;
			conductor.subdivisionsPerBeat = 8;
			Run(1 / 60.0);
			Assert.LessOrEqual(subdivisions.Count - before, 1);
		}

		[Test]
		public void SeekingFromABeatHandlerDoesNotFlood() {
			var beats = new List<long>();
			var bars = new List<long>();
			conductor.BeatCrossed += e => {
				beats.Add(e.index);
				if (e.index == 2) conductor.Clock.Seek(conductor.TempoMap.TimeAtBeat(32.5));
			};
			conductor.BarCrossed += e => bars.Add(e.index);
			conductor.Clock.Play(0);
			Run(1.5);
			CollectionAssert.AreEqual(new long[] { 0, 1, 2 }, beats.GetRange(0, 3));
			Assert.Greater(beats.Count, 3, "Carries on after the seek");
			for (var i = 3; i < beats.Count; i++) Assert.GreaterOrEqual(beats[i], 33, $"Beat {beats[i]} was skipped by the seek");
			for (var i = 1; i < bars.Count; i++) Assert.Greater(bars[i], bars[i - 1], "Bars are monotonic");
			Assert.IsFalse(bars.Exists(b => b >= 1 && b <= 7), "No flood of the skipped bars");
		}

		[Test]
		public void LoopingBackFromABarHandlerReportsTheRestart() {
			var beats = new List<long>();
			var bars = new List<long>();
			var looped = false;
			conductor.BeatCrossed += e => beats.Add(e.index);
			// Only loop once, so the run can reach beat 5
			conductor.BarCrossed += e => {
				bars.Add(e.index);
				if (e.index == 1 && !looped) {
					looped = true;
					conductor.Clock.Seek(0);
				}
			};
			conductor.Clock.Play(0);
			for (var frame = 0; frame < 600 && !(looped && conductor.Beat > 5); frame++) Run(1 / 60.0);
			Assert.IsTrue(looped);
			CollectionAssert.AreEqual(new long[] { 0, 1, 0, 1 }, bars);
			// Seeking exactly onto beat 0 fires it again
			CollectionAssert.AreEqual(new long[] { 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 5 }, beats);
		}

		[Test]
		public void PlayFromZeroSchedulesBeatZero() {
			var beats = new List<BeatEvent>();
			var bars = new List<BeatEvent>();
			conductor.BeatScheduled += beats.Add;
			conductor.BarScheduled += bars.Add;
			conductor.Clock.Play(0);
			Run(1 / 60.0);
			var beat = beats.Find(e => e.index == 0);
			var bar = bars.Find(e => e.index == 0);
			Assert.IsTrue(beats.Exists(e => e.index == 0), "Beat 0 scheduled");
			Assert.IsTrue(bars.Exists(e => e.index == 0), "Bar 0 scheduled");
			Assert.AreEqual(conductor.DspTimeAtBeat(0), beat.dspTime, 1e-9);
			Assert.AreEqual(conductor.DspTimeAtBeat(0), bar.dspTime, 1e-9);
		}

		[Test]
		public void SeekingJustBeforeABeatStillSchedulesIt() {
			conductor.lookAheadTime = 0.1;
			var beats = new List<BeatEvent>();
			conductor.BeatScheduled += beats.Add;
			conductor.Clock.Play(0.1);
			Run(0.1);
			conductor.Clock.Seek(conductor.TempoMap.TimeAtBeat(60) - 0.05);
			Run(1 / 60.0);
			Assert.IsTrue(beats.Exists(e => e.index == 60), "Beat 60 scheduled");
			Assert.AreEqual(conductor.DspTimeAtBeat(60), beats.Find(e => e.index == 60).dspTime, 1e-9);
		}

		[Test]
		public void TimelineChangedFiresForClockAndTempoChanges() {
			var count = 0;
			conductor.TimelineChanged += () => count++;
			conductor.Clock.Play(0);
			conductor.TempoMap.SetTempo(0, 90);
			conductor.TempoMap = new TempoMap(100);
			conductor.Clock.SetPlaybackRate(0.5);
			Assert.AreEqual(4, count);
		}

		[Test]
		public void RateChangeKeepsBeatsInOrder() {
			var beats = new List<long>();
			conductor.BeatCrossed += e => beats.Add(e.index);
			conductor.Clock.Play(0);
			Run(1.1);
			CollectionAssert.AreEqual(new long[] { 0, 1, 2 }, beats);
			conductor.Clock.SetPlaybackRate(2);
			Run(1);
			for (var i = 1; i < beats.Count; i++) Assert.AreEqual(beats[i - 1] + 1, beats[i], "Consecutive beats");
			Assert.GreaterOrEqual(beats[beats.Count - 1], 5);
		}

		[Test]
		public void ReenablingSubdivisionsDoesNotFlood() {
			conductor.subdivisionsPerBeat = 2;
			var subdivisions = new List<long>();
			conductor.SubdivisionCrossed += e => subdivisions.Add(e.index);
			conductor.Clock.Play(0);
			Run(0.6);
			conductor.subdivisionsPerBeat = 0;
			Run(1);
			var before = subdivisions.Count;
			conductor.subdivisionsPerBeat = 2;
			Run(1 / 60.0);
			Assert.LessOrEqual(subdivisions.Count - before, 1);
		}

		[Test]
		public void SeekingMidBarDoesNotReportTheBarInProgress() {
			var crossed = new List<long>();
			var scheduled = new List<long>();
			conductor.BarCrossed += e => crossed.Add(e.index);
			conductor.BarScheduled += e => scheduled.Add(e.index);
			conductor.Clock.Play(0.1);
			Run(0.1);
			crossed.Clear();
			scheduled.Clear();
			// Beat 10 is half way through bar 2; bar 3 (beat 12) is a second away
			conductor.Clock.Seek(conductor.TempoMap.TimeAtBeat(10));
			Run(0.3);
			CollectionAssert.IsEmpty(crossed);
			CollectionAssert.IsEmpty(scheduled);
		}

		[Test]
		public void SeekingOntoABarLineReportsIt() {
			var crossed = new List<long>();
			conductor.BarCrossed += e => crossed.Add(e.index);
			conductor.Clock.Play(0.1);
			Run(0.1);
			crossed.Clear();
			conductor.Clock.Seek(conductor.TempoMap.TimeAtBeat(8));
			Run(0.3);
			CollectionAssert.AreEqual(new long[] { 2 }, crossed);
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
