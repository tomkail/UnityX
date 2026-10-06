using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace UnityX.Rhythm.InputTests {
	// Against the real Input System, not the fixture: RhythmInputSource.Submit assumes input event times are on the
	// Time.realtimeSinceStartupAsDouble timeline
	public class InputEventTimeBaseTests {
		Keyboard keyboard;
		InputAction action;
		InputSettings originalSettings;
		InputSettings settings;

		[SetUp]
		public void Setup() {
			// A batch-mode test run has no focused game view, so the Input System would hold keyboard events back.
			// Use a copy of the settings that ignores focus, so the project's settings asset isn't touched.
			originalSettings = InputSystem.settings;
			settings = Object.Instantiate(originalSettings);
			settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
			settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
			InputSystem.settings = settings;
			keyboard = InputSystem.AddDevice<Keyboard>();
		}

		[TearDown]
		public void TearDown() {
			action?.Dispose();
			InputSystem.RemoveDevice(keyboard);
			InputSystem.settings = originalSettings;
			Object.Destroy(settings);
		}

		[Test]
		public void EventTimesAreOnTheRealtimeSinceStartupTimeline() {
			// Bind to this keyboard only, so a real one can't interfere
			action = new InputAction("Hit", InputActionType.Button, keyboard.spaceKey.path);
			double? eventTime = null;
			action.performed += context => eventTime = context.time;
			action.Enable();

			InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
			InputSystem.Update();
			var now = Time.realtimeSinceStartupAsDouble;

			Assert.IsTrue(eventTime.HasValue, $"The press didn't perform the action (keyboard enabled {keyboard.enabled}, " +
				$"space pressed {keyboard.spaceKey.isPressed}, focused {Application.isFocused})");
			Assert.AreEqual(now, eventTime.Value, 0.1);
		}
	}
}
