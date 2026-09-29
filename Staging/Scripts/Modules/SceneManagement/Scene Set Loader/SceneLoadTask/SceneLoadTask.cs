using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace UnityX.SceneManagement {

	// Consider adding events for when the various things here happen so this might be used for loading outside of the runtimescenesetloader system.
	// The runtimescenesetloader might use them, although it doesnt need to.
	public class SceneLoadTask : SceneTask {
		public enum State {
			NotStarted,
			Loading,
			WaitingForAllowActivation,
			Activating,
			UnloadingDueToCancel,
			Complete
		}

		State _state;
		public State state {
			get => _state;
			set {
				_state = value;
				OnChangeState?.Invoke(_state);
			}
		}
		public Action<State> OnChangeState;

		public bool loadingDone = false;

		bool _allowActivation = true;
		public bool allowActivation {
			get => _allowActivation;
			set {
				if (_allowActivation == value) return;
				_allowActivation = value;
				if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Change allow activation of '" + sceneName + "' to " + _allowActivation + ".");
				UpdateAllowSceneActivation();
			}
		}

		bool _cancel = false;
		public bool cancel {
			get => _cancel;
			set {
				if (_cancel == value) return;
				_cancel = value;
				if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Change cancel of '" + sceneName + "' to " + _cancel + ".");
				UpdateAllowSceneActivation();
			}
		}

		public event Action<SceneTask> OnStartLoad;
		public event Action<SceneTask> OnCompleteLoad;
		public event Action<SceneTask> OnCompleteCancel;
		public event Action<SceneTask> OnCompleteTask;

		private const float activationLoadStopMagicNumber = 0.9f;

		public SceneLoadTask(string sceneName) : base (sceneName) {}

		public async Awaitable LoadAsync () {
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Begin " + GetType().Name + " for '" + sceneName + "'");
			state = State.Loading;
			op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			UpdateAllowSceneActivation();
			OnStartLoad?.Invoke(this);
			float startTime = Time.realtimeSinceStartup;
			// When "allowSceneActivation" is false Unity stops at 0.9 until you set it back to true, so we
			// poll progress rather than awaiting the operation directly (which would block on activation).
			while (op.progress < activationLoadStopMagicNumber)
				await Awaitable.NextFrameAsync();
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Initial load (no activation) for '" + sceneName + "' took " + (Time.realtimeSinceStartup - startTime) + " seconds");
			loadingDone = true;

			if (!op.allowSceneActivation && RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Waiting for activation to be allowed for " + GetType().Name + " for '" + sceneName + "'");

			while (!op.isDone) {
				state = op.allowSceneActivation ? State.Activating : State.WaitingForAllowActivation;
				await Awaitable.NextFrameAsync();
			}
			Debug.Assert(SceneManager.GetSceneByName(sceneName).isLoaded);
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Load for '" + sceneName + "' took " + (Time.realtimeSinceStartup - startTime) + " seconds");
			OnCompleteLoad?.Invoke(this);
			if (cancel) {
				if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Cancelling ongoing load of '" + sceneName + "'");
				state = State.UnloadingDueToCancel;
				await new SceneUnloadTask(sceneName).UnloadAsync();
			}
			state = State.Complete;
			complete = true;
			op = null;

			if (cancel) OnCompleteCancel?.Invoke(this);
			OnCompleteTask?.Invoke(this);
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Completed " + GetType().Name + " for '" + sceneName + "'. Did cancel? " + cancel);
		}

		public void Cancel() {
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Cancel load of '" + sceneName + "'.");
			cancel = true;
		}

		public void Uncancel() {
			if (!cancel) return;
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Uncancel load of '" + sceneName + "'.");
			if (!complete) {
				cancel = false;
			} else {
				Debug.Log("Attempted to uncancel a cancelled scene load for scene '" + sceneName + "', but scene load has already completed");
			}
		}

		private void UpdateAllowSceneActivation () {
			if (op == null) return;
			if (complete) {
				Debug.LogWarning("Attempted to update allowSceneActivation for scene '" + sceneName + "', but activation is already complete.");
				return;
			}
			op.allowSceneActivation = allowActivation || cancel;
		}

		public override string ToString () {
			return string.Format ("[{0}] SceneName:{1} state:{2} complete:{3} allowActivation:{4} cancel:{5}", GetType(), sceneName, state, complete, allowActivation, cancel);
		}
	}
}
