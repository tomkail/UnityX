using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnityX.Rhythm.JudgementTests {
	// The whole chain on Unity objects: a Conductor's clock, an input component and a windows asset
	public class JudgeOnAConductorTests {
		GameObject gameObject;
		JudgementWindowsAsset windows;

		[TearDown]
		public void TearDown() {
			Object.DestroyImmediate(gameObject);
			Object.DestroyImmediate(windows);
		}

		[Test]
		public void APressFromAnInputComponentIsJudged() {
			gameObject = new GameObject("Song");
			var conductor = gameObject.AddComponent<Conductor>();
			conductor.smoothing = Conductor.SmoothingMode.Raw;
			conductor.TempoMap = new TempoMap(120);
			var time = new ManualAudioTimeSource();
			conductor.Initialize(time);
			conductor.Tick();
			var input = gameObject.AddComponent<RhythmInputSource>();
			input.conductor = conductor;
			windows = ScriptableObject.CreateInstance<JudgementWindowsAsset>();
			var notes = new NoteScheduler(conductor, new BeatGrid(1));
			using var judge = new Judge(conductor, notes, windows.windows);
			judge.Attach(input);
			var judged = new List<Judgement>();
			judge.Judged += judged.Add;

			conductor.Clock.Play(-0.2);
			judge.Update();
			for (var i = 0; i < 9; i++) {
				time.Advance(1 / 60.0);
				conductor.Tick();
				judge.Update();
			}
			// Press at the moment beat 0 plays, given as real time
			var realtimeOfBeatZero = time.Realtime + (conductor.DspTimeAtBeat(0) - conductor.Clock.DspTime);
			input.Submit(0, InputPhase.Press, 1, realtimeOfBeatZero);
			Assert.AreEqual(1, judged.Count);
			Assert.AreEqual("Perfect", judged[0].gradeName);
			Assert.AreEqual(0, judged[0].timeOffset, 1e-6);
		}
	}
}
