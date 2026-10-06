using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityX.Rhythm.InputTests {
	// Play mode, because OnEnable and OnDisable only run there: AddComponent enables, enabled = false disables
	public class InputActionLaneInputLifecycleTests : InputTestFixture {
		GameObject songObject;
		GameObject inputObject;
		Conductor conductor;
		InputActionLaneInput laneInput;
		Keyboard keyboard;
		InputAction action;
		InputActionAsset asset;
		InputActionReference reference;
		List<RhythmInput> received;

		public override void Setup() {
			base.Setup();
			songObject = new GameObject("Song");
			conductor = songObject.AddComponent<Conductor>();
			conductor.playOnStart = false;
			conductor.smoothing = Conductor.SmoothingMode.Raw;
			conductor.Initialize(new ManualAudioTimeSource { Realtime = 10 });
			conductor.Tick();
			received = new List<RhythmInput>();
			keyboard = InputSystem.AddDevice<Keyboard>();
			action = new InputAction("Hit", InputActionType.Button, "<Keyboard>/space");
		}

		public override void TearDown() {
			if (inputObject != null) Object.DestroyImmediate(inputObject);
			Object.DestroyImmediate(songObject);
			action.Dispose();
			if (reference != null) Object.DestroyImmediate(reference);
			if (asset != null) Object.DestroyImmediate(asset);
			base.TearDown();
		}

		void CreateLaneInput() {
			inputObject = new GameObject("Input");
			laneInput = inputObject.AddComponent<InputActionLaneInput>();
			laneInput.conductor = conductor;
			laneInput.InputReceived += received.Add;
		}

		[Test]
		public void AnActionOtherCodeEnabledStaysEnabledAfterDisable() {
			action.Enable();
			CreateLaneInput();
			laneInput.Bind(action, 0);
			laneInput.enabled = false;
			Assert.IsTrue(action.enabled);
		}

		[Test]
		public void AnActionTheComponentEnabledIsDisabledAgain() {
			CreateLaneInput();
			laneInput.Bind(action, 0);
			Assert.IsTrue(action.enabled);
			laneInput.enabled = false;
			Assert.IsFalse(action.enabled);
		}

		[Test]
		public void DisableThenEnableGivesOneInputPerPress() {
			CreateLaneInput();
			laneInput.Bind(action, 0);
			laneInput.enabled = false;
			laneInput.enabled = true;
			Press(keyboard.spaceKey, 9.5);
			Assert.AreEqual(1, received.Count);
		}

		[Test]
		public void CodeBindingsWorkWhileEnabledAndSurviveDisableAndEnable() {
			CreateLaneInput();
			Assert.IsTrue(laneInput.isActiveAndEnabled);
			laneInput.Bind(action, 4);
			Press(keyboard.spaceKey, 9.5);
			Release(keyboard.spaceKey, 9.6);
			laneInput.enabled = false;
			laneInput.enabled = true;
			Assert.IsTrue(action.enabled);
			Press(keyboard.spaceKey, 9.7);
			Assert.AreEqual(3, received.Count);
			CollectionAssert.AreEqual(new[] { 4, 4, 4 }, received.ConvertAll(r => r.lane));
			Assert.AreEqual(conductor.Clock.RealtimeToDspTime(9.7), received[2].dspTime, 1e-6);
		}

		[Test]
		public void SerializedBindingsUseTheirActionReference() {
			// InputActionReference needs an action that lives in an asset
			asset = ScriptableObject.CreateInstance<InputActionAsset>();
			var map = asset.AddActionMap("Gameplay");
			var assetAction = map.AddAction("Hit", InputActionType.Button, "<Keyboard>/space");
			reference = InputActionReference.Create(assetAction);

			// Set up while inactive so the binding is there before OnEnable runs
			inputObject = new GameObject("Input");
			inputObject.SetActive(false);
			laneInput = inputObject.AddComponent<InputActionLaneInput>();
			laneInput.conductor = conductor;
			laneInput.InputReceived += received.Add;
			laneInput.bindings.Add(new InputActionLaneInput.Binding { action = reference, lane = 2 });
			inputObject.SetActive(true);

			Assert.IsTrue(assetAction.enabled);
			Press(keyboard.spaceKey, 9.5);
			Assert.AreEqual(1, received.Count);
			Assert.AreEqual(2, received[0].lane);
			Assert.AreEqual(conductor.Clock.RealtimeToDspTime(9.5), received[0].dspTime, 1e-6);
			laneInput.enabled = false;
			Assert.IsFalse(assetAction.enabled);
		}
	}
}
