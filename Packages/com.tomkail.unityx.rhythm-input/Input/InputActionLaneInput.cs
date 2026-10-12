using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityX.Rhythm {
	// Maps Input System actions to lanes, timed by each input event's own timestamp rather than the frame it arrived in.
	// An action is pressed when it performs and released when it cancels, which is how Button actions behave.
	// The Input System handles devices being plugged in and removed.
	// Each binding is independent: binding the same action twice, or to two lanes, gives two inputs per press.
	public class InputActionLaneInput : RhythmInputSource {
		[Serializable]
		public class Binding {
			public InputActionReference action;
			public int lane;
		}

		sealed class Bound {
			public InputAction action;
			public bool enabledByUs;
			public bool fromCode;
			public Action<InputAction.CallbackContext> pressed;
			public Action<InputAction.CallbackContext> released;
		}

		public List<Binding> bindings = new();

		readonly List<Bound> bound = new();
		readonly List<(InputAction action, int lane)> codeBindings = new();

		// Binds an action from code. Takes effect immediately if enabled, and survives disable and enable.
		// Not deduplicated: binding the same action twice gives two inputs per press.
		public void Bind(InputAction action, int lane) {
			if (action == null) throw new ArgumentNullException(nameof(action));
			codeBindings.Add((action, lane));
			if (isActiveAndEnabled) Subscribe(action, lane, true);
		}

		// Undoes Bind for this action, on every lane it was bound to. Call it before disposing an action you bound.
		// Inspector bindings to the same action are left alone.
		public void Unbind(InputAction action) {
			if (action == null) throw new ArgumentNullException(nameof(action));
			codeBindings.RemoveAll(b => b.action == action);
			for (var i = bound.Count - 1; i >= 0; i--) {
				if (bound[i].action == action && bound[i].fromCode) RemoveAt(i);
			}
		}

		void OnEnable() {
			foreach (var binding in bindings) {
				var action = binding.action != null ? binding.action.action : null;
				if (action != null) Subscribe(action, binding.lane, false);
			}
			foreach (var (action, lane) in codeBindings) Subscribe(action, lane, true);
		}

		// An input still held when this is disabled gets no release, since its cancel arrives after we unsubscribe
		void OnDisable() {
			for (var i = bound.Count - 1; i >= 0; i--) RemoveAt(i);
		}

		void RemoveAt(int index) {
			var b = bound[index];
			bound.RemoveAt(index);
			b.action.performed -= b.pressed;
			b.action.canceled -= b.released;
			// Leave actions other code enabled alone
			if (!b.enabledByUs) return;
			// Another binding still uses it, so it takes over disabling it when it goes
			var other = bound.Find(o => o.action == b.action);
			if (other != null) other.enabledByUs = true;
			else b.action.Disable();
		}

		void Subscribe(InputAction action, int lane, bool fromCode) {
			var b = new Bound { action = action, fromCode = fromCode };
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

		// performed fires as the press threshold is crossed, so an analog button gives its pressure at that moment,
		// not its peak. Anything else counts as full velocity.
		static float ReadVelocity(InputAction.CallbackContext context) {
			return context.valueType == typeof(float) ? context.ReadValue<float>() : 1;
		}
	}
}
