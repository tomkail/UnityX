using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace UnityX.SceneManagement {

	public class SceneUnloadTask : SceneTask {

		public enum State {
			NotStarted,
			Unloading,
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

		public event Action<SceneUnloadTask> OnCompleteUnload;

		public SceneUnloadTask(string sceneName) : base (sceneName) {}

		public async Awaitable UnloadAsync () {
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Begin " + GetType().Name + " for '" + sceneName + "'");
			state = State.Unloading;
			op = SceneManager.UnloadSceneAsync(sceneName);
			if (op != null) await op;
			state = State.Complete;
			complete = true;
			op = null;
			OnCompleteUnload?.Invoke(this);
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Completed " + GetType().Name + " for '" + sceneName + "'");
		}

		public override string ToString () {
			return string.Format ("[{0}] SceneName:{1} state:{2} complete:{3}", GetType(), sceneName, state, complete);
		}
	}
}
