using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnityX.Rhythm.InputTests {
	public class RhythmInputSourceTests {
		GameObject gameObject;
		Conductor conductor;
		ManualAudioTimeSource source;
		RhythmInputSource input;
		RhythmLatency latency;
		List<RhythmInput> received;

		[SetUp]
		public void SetUp() {
			gameObject = new GameObject("Song");
			conductor = gameObject.AddComponent<Conductor>();
			conductor.smoothing = Conductor.SmoothingMode.Raw;
			source = new ManualAudioTimeSource { Realtime = 10 };
			conductor.Initialize(source);
			conductor.Tick();
			input = gameObject.AddComponent<RhythmInputSource>();
			input.conductor = conductor;
			received = new List<RhythmInput>();
			input.InputReceived += received.Add;
		}

		[TearDown]
		public void TearDown() {
			Object.DestroyImmediate(gameObject);
			if (latency != null) Object.DestroyImmediate(latency);
		}

		[Test]
		public void SubmitMapsRealtimeOntoTheAudioClock() {
			input.Submit(2, InputPhase.Press, 0.5f, 9.9);
			var press = received[0];
			Assert.AreEqual(2, press.lane);
			Assert.IsTrue(press.IsPress);
			Assert.AreEqual(0.5f, press.velocity);
			Assert.AreEqual(conductor.Clock.RealtimeToDspTime(9.9), press.dspTime, 1e-9);
		}

		[Test]
		public void InputLatencyIsAppliedOnce() {
			latency = ScriptableObject.CreateInstance<RhythmLatency>();
			latency.inputLatency = -0.02;
			conductor.Latency = latency;
			input.Submit(0, InputPhase.Press, 1, 10);
			Assert.AreEqual(conductor.Clock.RealtimeToDspTime(10) - 0.02, received[0].dspTime, 1e-9);
		}

		[Test]
		public void SubmitAtDspTimePassesTheTimeThrough() {
			latency = ScriptableObject.CreateInstance<RhythmLatency>();
			latency.inputLatency = -0.02;
			conductor.Latency = latency;
			input.SubmitAtDspTime(1, InputPhase.Release, 2, 123.5);
			Assert.AreEqual(123.5, received[0].dspTime);
			Assert.AreEqual(InputPhase.Release, received[0].phase);
			Assert.AreEqual(1, received[0].velocity);
			input.SubmitAtDspTime(1, InputPhase.Press, -0.5f, 124);
			Assert.AreEqual(0, received[1].velocity);
		}

		[Test]
		public void InputWithoutAConductorIsDropped() {
			input.conductor = null;
			input.Submit(0, InputPhase.Press, 1, 10);
			Assert.IsEmpty(received);
		}

		[Test]
		public void InputBeforeTheConductorHasAClockIsDropped() {
			// Never initialised: in edit mode its Awake doesn't run
			input.conductor = gameObject.AddComponent<Conductor>();
			Assert.IsNull(input.conductor.Clock);
			input.Submit(0, InputPhase.Press, 1, 10);
			Assert.IsEmpty(received);
		}
	}
}
