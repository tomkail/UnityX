using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SceneUnloadTask : SceneTask {
    
    public enum State {
        NotStarted,
        Unloading,
        Complete
    }

    State _state;
	public State state {
		get {
			return _state;
		} set {
			_state = value;
			if(OnChangeState != null) OnChangeState(_state);
		}
	}
	public Action<State> OnChangeState;


	public delegate void UnloadSceneTaskDelegate (SceneUnloadTask unloadTask);
	public event UnloadSceneTaskDelegate OnCompleteUnload;

	public SceneUnloadTask(string sceneName) : base (sceneName) {}

	public IEnumerator UnloadCR () {
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Begin "+GetType().Name+" for '"+sceneName+"'");
		state = State.Unloading;
        op = SceneManager.UnloadSceneAsync(sceneName);
        while (op != null && !op.isDone)
			yield return null;
        state = State.Complete;
		complete = true;
		op = null;
		if(OnCompleteUnload != null) OnCompleteUnload(this);
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Completed "+GetType().Name+" for '"+sceneName+"'");
    }

    public override string ToString () {
		return string.Format ("[{0}] SceneName:{1} state:{2} complete:{3}", GetType(), sceneName, state, complete);
	}
}