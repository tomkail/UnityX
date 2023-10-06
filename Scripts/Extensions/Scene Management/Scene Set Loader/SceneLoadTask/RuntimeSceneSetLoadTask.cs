using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Consider removing the scene set from this. Assign tasks can easily be called from RuntimeSceneSetLoader instead, using a KeyPairValue to combine a load task with a scene set
// This would also allow us to remove the reference to scenesetmanager.
public class RuntimeSceneSetLoadTask {
	public RuntimeSceneSet sceneSet {get; private set;}
	public LoadTaskMode sceneLoadMode {get; private set;}

	public List<SceneLoadTask> loadTasks = new List<SceneLoadTask>();
	public List<SceneUnloadTask> unloadTasks = new List<SceneUnloadTask>();

	public enum State {
		NotStarted,
		Unloading,
		Loading,
		WaitingForAllowActivation,
		Activating,
		Cancelling,
		Complete,
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
    
    // Load all scenes before activation
	public bool onlyActivateWhenAllLoaded = true;

	// Prevents activation when true
	public bool yieldActivation = false;
	// Have we cancelled this task?
	public bool cancelled {get; private set;}

    // When this is checked activation occurs during load, rather than as a pass after all scenes are loaded
    public bool activateDuringLoad {get; set;}

	
	// When all tasks have loaded (but not necessarily activated)
	public bool loadingDone {
		get {
			return loadTasks.All(x => x.loadingDone);
		}
	}

	// When all tasks are allowed to activate
	public bool allTasksAllowedToActivate {
		get {
			return loadTasks.All(x => x.allowActivation);
		}
	}


	// Called when loading is done, but before activation or unloading
	public Action<RuntimeSceneSetLoadTask> whenTasksAssigned {get; set;}
	public Action<RuntimeSceneSetLoadTask> whenUnloaded {get; set;}
	public Action<RuntimeSceneSetLoadTask> whenLoaded {get; set;}
	public Action<RuntimeSceneSetLoadTask> whenComplete {get; set;}

	public RuntimeSceneSetLoadTask (RuntimeSceneSet sceneSetup, LoadTaskMode sceneLoadMode) {
		this.sceneSet = sceneSetup;
		this.sceneLoadMode = sceneLoadMode;
		Debug.Assert(sceneSetup != null, "Scene setup is null");
	}

	public RuntimeSceneSetLoadTask (RuntimeSceneSet sceneSetup, LoadTaskMode sceneLoadMode, Action<RuntimeSceneSetLoadTask> whenCompleted) : this (sceneSetup, sceneLoadMode) {
		this.whenComplete = whenCompleted;
	}

	public RuntimeSceneSetLoadTask (RuntimeSceneSet sceneSetup, LoadTaskMode sceneLoadMode, Action<RuntimeSceneSetLoadTask> whenLoaded, Action<RuntimeSceneSetLoadTask> whenCompleted) : this (sceneSetup, sceneLoadMode) {
		this.whenLoaded = whenLoaded;
		this.whenComplete = whenCompleted;
	}

	public void AssignTasks () {
		loadTasks.Clear();
		unloadTasks.Clear();

		var sceneSetPaths = sceneSet.AllScenePaths();
		if(sceneLoadMode == LoadTaskMode.LoadSingle || sceneLoadMode == LoadTaskMode.LoadAdditive) {
			foreach(string scenePath in sceneSetPaths) {
				Scene scene = SceneManager.GetSceneByPath(scenePath);
				if(scene.isLoaded)
					continue;
				string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
				var sceneLoadTask = new SceneLoadTask(sceneName);
				sceneLoadTask.allowActivation = !yieldActivation && !onlyActivateWhenAllLoaded;
                loadTasks.Add(sceneLoadTask);
			}
		}
		
		string[] currentPaths = RuntimeSceneSetLoader.GetCurrentScenePaths();
		List<string> pathsToUnload = new List<string>();
		if(sceneLoadMode == LoadTaskMode.LoadSingle) {
			pathsToUnload = currentPaths.Except(sceneSetPaths).ToList();
			pathsToUnload.Reverse();
		} else if(sceneLoadMode == LoadTaskMode.UnloadSoft) {
			foreach(var scenePath in sceneSetPaths) {
				foreach(var loadedSceneSet in RuntimeSceneSetLoader.GetLoadedSceneSets()) {
					bool found = false;
					if(loadedSceneSet.AllScenePaths().Contains(scenePath)) {
						found = true;
						break;
					}
					if(!found) {
						pathsToUnload.Add(scenePath);
					}
				}
			}
			// From all the scenes in this set, scan through all the active scene sets and select any scenes which aren't used in an others
		} else if(sceneLoadMode == LoadTaskMode.UnloadHard) {
			pathsToUnload = sceneSetPaths.ToList();
		}
		pathsToUnload = pathsToUnload.Distinct().ToList();
		foreach(string scenePath in pathsToUnload) {
			Scene scene = SceneManager.GetSceneByPath(scenePath);
			if(!scene.isLoaded)
				continue;
			string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
			unloadTasks.Add(new SceneUnloadTask(sceneName));
		}

        if(whenTasksAssigned != null) whenTasksAssigned(this);
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "AssignedTasks:\nUnload: "+DebugX.ListAsString(unloadTasks)+"\nLoad:"+DebugX.ListAsString(loadTasks));
	}

	public IEnumerator LoadSceneSetupCR(Action OnPreComplete) {
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Begin "+GetType().Name+" Load: "+sceneSet.name);
        
		// Unloading
        IEnumerator unloadIE = Unload();
        while (unloadIE.MoveNext()) yield return null;
        
        // Loading
        IEnumerator loadIE = Load();
        while (loadIE.MoveNext()) yield return null;
        
		// Activation
		if(loadTasks.Any()) {
			// Only allow yield activation if not already activated or not cancelled
			while(!allTasksAllowedToActivate && !cancelled && yieldActivation) {
				state = State.WaitingForAllowActivation;
				yield return null;
			}

			if(!allTasksAllowedToActivate)
				Activate();
			else if(allTasksAllowedToActivate && yieldActivation) 
				Debug.LogWarning("All tasks were allowed to activate, but load task is marked as yieldActivation. This can happen if tasks are all individually allowed to activate, and isn't a bug if you're aware of it!");
		}
        if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name+" completed Activating '"+sceneSet.name+"'. Load Tasks:\n"+DebugX.ListAsString(loadTasks));
		
		// Completion
		{
			while(loadTasks.Any(x => !x.complete)) yield return null;
			
			state = State.Complete;
            if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Complete "+GetType().Name+" '"+sceneSet.name+"'.");
            if(OnPreComplete != null) OnPreComplete();
            if(whenComplete != null) whenComplete(this);
		}
    }

	IEnumerator Unload () {
		state = State.Unloading;
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name+" beginning Unloading '"+sceneSet.name+"'. Unload Tasks:\n"+DebugX.ListAsString(unloadTasks));
		foreach (var task in unloadTasks) {
			RuntimeSceneSetLoader.Instance.StartCoroutine(task.UnloadCR());
            if(activateDuringLoad) while(!task.complete) yield return null;
        }
        while(unloadTasks.Any(task => !task.complete)) {
            yield return null;
        }
		
        // When all scenes are unloaded, also unload unused assets. We believe that Unity doesn't do this automatically when doing additive loading.
        Resources.UnloadUnusedAssets();

        if(whenUnloaded != null) {
            whenUnloaded(this);
        }
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name+" completed Unloading '"+sceneSet.name+"'. Unload Tasks:\n"+DebugX.ListAsString(unloadTasks));
	}

	IEnumerator Load () {
		state = State.Loading;
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name+" beginning Loading '"+sceneSet.name+"'. Load Tasks:\n"+DebugX.ListAsString(loadTasks));
		foreach (var task in loadTasks) {
			RuntimeSceneSetLoader.Instance.StartCoroutine(task.LoadCR());
            // If this is true we also activate during this step
            if(activateDuringLoad) {
                task.allowActivation = true;
                while(!task.complete) {
                    task.allowActivation = true;
                    yield return null;
                }
            }
        }

        while(loadTasks.Any(task => !task.loadingDone)) {
            yield return null;
        }
        if(whenLoaded != null) {
            whenLoaded(this);
        }
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name+" completed Loading '"+sceneSet.name+"'. Load Tasks:\n"+DebugX.ListAsString(loadTasks));
	}

	void Activate () {
		state = State.Activating;
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name+" beginning Activating '"+sceneSet.name+"'. Load Tasks:\n"+DebugX.ListAsString(loadTasks));
		foreach(SceneLoadTask task in loadTasks) {
			task.allowActivation = true;
		}
	}

	public void Cancel() {
		state = State.Cancelling;
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Cancel "+GetType().Name+" '"+sceneSet.name+"'. Load Tasks:\n"+DebugX.ListAsString(loadTasks));
		cancelled = true;
		foreach(SceneLoadTask task in loadTasks) {
			task.Cancel();
		}
    }

	public void Uncancel() {
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Uncancel "+GetType().Name+" '"+sceneSet.name+"'. Load Tasks:\n"+DebugX.ListAsString(loadTasks));
		cancelled = false;
		foreach(SceneLoadTask task in loadTasks) {
			task.Uncancel();
		}
    }
    /*
	public string[] GetSceneNamesToLoad () {
		return loadTasks.Select(x => x.sceneName).ToArray();
	}

	public bool IsEqual (SceneSetLoadTask other) {
		return GetSceneNamesToLoad().SequenceEqual(other.GetSceneNamesToLoad()) && unloadTasks.SequenceEqual(other.unloadTasks);
    }
    */

    public override string ToString () {
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		sb.AppendLine(string.Format("[RuntimeSceneSetLoadTask: sceneSet={0}, sceneLoadMode={1}, state={2}, cancelled={3}, yieldActivation={4}]", sceneSet, sceneLoadMode, state, cancelled, yieldActivation));
		sb.AppendLine("Load Tasks:");
		foreach(var loadTask in loadTasks) sb.AppendLine(loadTask.sceneName);
		sb.AppendLine("Unload Tasks:");
		foreach(var unloadTask in unloadTasks) sb.AppendLine(unloadTask.sceneName);
		return sb.ToString();
	}
}