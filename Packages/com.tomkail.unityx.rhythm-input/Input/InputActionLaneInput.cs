using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityX.Rhythm {
	// Maps Input System actions to lanes, timed by each input event's own timestamp rather than the frame it arrived in.
	// An action is pressed when it performs and released when it cancels, which is how Button actions behave.
	// The Input System handles devices being plugged in and removed.
	public class InputActionLaneInput : RhythmInputSource {
		[Serializable]
		public class Binding {
			public InputActionReference action;
			public int lane;
		}

		sealed class Bound {
			public InputAction action;
			public int lane;
			public bool enabledByUs;
			public Action<InputAction.CallbackContext> pressed;
			public Action<InputAction.CallbackContext> released;
		}

		public List<Binding> bindings = new();

		readonly List<Bound> bound = new();
		readonly List<(InputAction action, int lane)> codeBindings = new();

		// Binds an action from code. Takes effect immediately if enabled, and survives disable and enable.
		public void Bind(InputAction action, int lane) {
			if (action == null) throw new ArgumentNullException(nameof(action));
			codeBindings.Add((action, lane));
			if (isActiveAndEnabled) Subscribe(action, lane);
		}

		void OnEnable() {
			foreach (var binding in bindings) {
				var action = binding.action != null ? binding.action.action : null;
				if (action != null) Subscribe(action, binding.lane);
			}
			foreach (var (action, lane) in codeBindings) Subscribe(action, lane);
		}

		void OnDisable() {
			foreach (var b in bound) {
				b.action.performed -= b.pressed;
				b.action.canceled -= b.released;
				// Leave actions other code enabled alone
				if (b.enabledByUs) b.action.Disable();
			}
			bound.Clear();
		}

		void Subscribe(InputAction action, int lane) {
			var b = new Bound { action = action, lane = lane };
			b.pressed = context => Submit(lane, InputPhase.Press, ReadVelocity(context), context.time);
			b.released = context => Submit(lane, InputPhase.Release, 0, context.time);
			action.performed += b.pressed;
			action.canceled += b.released;
			if (!action.enabled) {
				action.Enable();
				b.enabledByUs = true;
			}
			bound.Add(b);
		}

		// Analog buttons report how hard they were pressed; anything else counts as full velocity
		static float ReadVelocity(InputAction.CallbackContext context) {
			return context.valueType == typeof(float) ? context.ReadValue<float>() : 1;
		}
	}
}
