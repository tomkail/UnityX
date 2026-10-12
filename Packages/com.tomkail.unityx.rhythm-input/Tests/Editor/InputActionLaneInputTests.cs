using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityX.Rhythm.InputTests {
	// InputTestFixture swaps in a private input system for each test, with fake devices and timestamps
	public class InputActionLaneInputTests : InputTestFixture {
		GameObject gameObject;
		Conductor conductor;
		InputActionLaneInput laneInput;
		Keyboard keyboard;
		InputAction action;
		List<RhythmInput> received;

		public override void Setup() {
			base.Setup();
			gameObject = new GameObject("Song");
			conductor = gameObject.AddComponent<Conductor>();
			conductor.smoothing = Conductor.SmoothingMode.Raw;
			conductor.Initialize(new ManualAudioTimeSource { Realtime = 10 });
			conductor.Tick();
			laneInput = gameObject.AddComponent<InputActionLaneInput>();
			laneInput.conductor = conductor;
			received = new List<RhythmInput>();
			laneInput.InputReceived += received.Add;
			keyboard = InputSystem.AddDevice<Keyboard>();
			action = new InputAction("Hit", InputActionType.Button, "<Keyboard>/space");
		}

		public override void TearDown() {
			Object.DestroyImmediate(gameObject);
			action.Dispose();
			base.TearDown();
		}

		[Test]
		public void PressesAndReleasesUseTheEventTimestamps() {
			laneInput.Bind(action, 3);
			Assert.IsTrue(action.enabled);
			Press(keyboard.spaceKey, 9.5);
			Release(keyboard.spaceKey, 9.75);
			Assert.AreEqual(2, received.Count);
			Assert.AreEqual(3, received[0].lane);
			Assert.IsTrue(received[0].IsPress);
			Assert.AreEqual(1, received[0].velocity);
			Assert.AreEqual(conductor.Clock.RealtimeToDspTime(9.5), received[0].dspTime, 1e-6);
			Assert.AreEqual(InputPhase.Release, received[1].phase);
			Assert.AreEqual(conductor.Clock.RealtimeToDspTime(9.75), received[1].dspTime, 1e-6);
		}

		[Test]
		public void EachBindingHasItsOwnLane() {
			var other = new InputAction("Other", InputActionType.Button, "<Keyboard>/f");
			laneInput.Bind(action, 0);
			laneInput.Bind(other, 1);
			Press(keyboard.fKey, 9.5);
			Press(keyboard.spaceKey, 9.6);
			CollectionAssert.AreEqual(new[] { 1, 0 }, received.ConvertAll(r => r.lane));
			other.Dispose();
		}
	}
}
