using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

// Consider adding events for when the various things here happen so this might be used for loading outside of the runtimescenesetloader system. 
// The runtimescenesetloader might use them, although it doesnt need to.
[System.Serializable]
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
		get {
			return _state;
		} set {
			_state = value;
			if(OnChangeState != null) OnChangeState(_state);
		}
	}
	public Action<State> OnChangeState;

    
	public bool loadingDone = false;

	bool _allowActivation = true;
	public bool allowActivation {
		get {
			return _allowActivation;
		} set {
			if(_allowActivation == value) return;
            _allowActivation = value;
			if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Change allow activation of '"+sceneName+"' to "+_allowActivation+".");
			UpdateAllowSceneActivation();
		}
	}

    bool _cancel = false;
	public bool cancel {
		get {
			return _cancel;
		} set {
			if(_cancel == value) return;
            _cancel = value;
			if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Change cancel of '"+sceneName+"' to "+_cancel+".");
			UpdateAllowSceneActivation();
		}
	}

	public delegate void LoadSceneTaskDelegate (SceneTask loadTask);
	public event LoadSceneTaskDelegate OnStartLoad;
	public event LoadSceneTaskDelegate OnCompleteLoad;
	public event LoadSceneTaskDelegate OnCompleteCancel;
	public event LoadSceneTaskDelegate OnCompleteTask;

	private const float activationLoadStopMagicNumber = 0.9f;

	public SceneLoadTask(string sceneName) : base (sceneName) {}
	
	public IEnumerator LoadCR () {
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Begin "+GetType().Name+" for '"+sceneName+"'");
        state = State.Loading;
		op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		UpdateAllowSceneActivation();
		if(OnStartLoad != null) OnStartLoad(this);
        float startTime = Time.realtimeSinceStartup;
		// when "allowSceneActivation" is false Unity will stop at "0.9f"
        // until you set allowSceneActivation back to true
		while(op.progress < activationLoadStopMagicNumber)
			yield return null;
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Initial load (no activation) for '"+sceneName+"' took " +(Time.realtimeSinceStartup-startTime)+" seconds");
		loadingDone = true;

        if(!op.allowSceneActivation && RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Waiting for activation to be allowed for "+GetType().Name+" for '"+sceneName+"'");
		
        while (!op.isDone) {
            if(op.allowSceneActivation) state = State.Activating;
            else state = State.WaitingForAllowActivation;
			yield return null;
        }
		Debug.Assert(SceneManager.GetSceneByName(sceneName).isLoaded);
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Load for '"+sceneName+"' took " +(Time.realtimeSinceStartup-startTime)+" seconds");
		if(OnCompleteLoad != null) OnCompleteLoad(this);
		if(cancel) {
			if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Cancelling ongoing load coroutine of '"+sceneName+"'");
            state = State.UnloadingDueToCancel;
			SceneUnloadTask unloadTask = new SceneUnloadTask(sceneName);
			RuntimeSceneSetLoader.Instance.StartCoroutine(unloadTask.UnloadCR());
			while(!unloadTask.complete)
				yield return null;
		}
        state = State.Complete;
		complete = true;
		op = null;

		if(cancel) if(OnCompleteCancel != null) OnCompleteCancel(this);
		if(OnCompleteTask != null) OnCompleteTask(this);
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Completed "+GetType().Name+" for '"+sceneName+"'. Did cancel? "+cancel);
    }
	
	public void Cancel() {
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Cancel load of '"+sceneName+"'.");
		cancel = true;
    }

	public void Uncancel() {
        if(!cancel) return;
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Uncancel load of '"+sceneName+"'.");
		if(!complete) {
			cancel = false;
		} else {
			Debug.Log("Attempted to uncancel a cancelled scene load for scene '"+sceneName+"', but scene load has already completed");
		}
    }

    private void UpdateAllowSceneActivation () {
        if(op == null) return;
		if(complete) {
			Debug.LogWarning("Attempted to update allowSceneActivation for scene '"+sceneName+"', but activation is already complete.");
			return;
		}
		op.allowSceneActivation = allowActivation || cancel;
    }
    
    public override string ToString () {
		return string.Format ("[{0}] SceneName:{1} state:{2} complete:{3} allowActivation:{4} cancel:{5}", GetType(), sceneName, state, complete, allowActivation, cancel);
	}
}