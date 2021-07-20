using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum LoadTaskMode {
	// Loads any scenes required by the scene set, unloads all others, and sets the last scene in the set to be the active scene.
	LoadSingle,
	// Loads any scenes required by the scene set
	LoadAdditive,
	// Unload any scenes that are only required by the specified scene set
	UnloadSoft,
	// Unload any scenes that are used by the specified scene set
	UnloadHard
}

/// <summary>
/// Scene loader class. Loads a RuntimeSceneSet, removing the previous set of scenes.
/// </summary>
public class RuntimeSceneSetLoader : MonoSingleton<RuntimeSceneSetLoader> {
	#if UNITY_EDITOR
	public static bool _debugLogging = false;
	public static bool debugLogging {
        get {
            return UnityEditor.EditorPrefs.GetBool(pathPlayerPrefsKey);
        } set {
            UnityEditor.EditorPrefs.SetBool(pathPlayerPrefsKey, value);
        }
    }
	#else
	public static bool debugLogging {
        get {
            return false;
        }
    }
	#endif


    public const string pathPlayerPrefsKeyPrefix = "RuntimeSceneSetLoader DebugLogging";
    public static string pathPlayerPrefsKey {
        get {
            return pathPlayerPrefsKeyPrefix+" "+Application.productName;
        }
    }
    
    public static Func<List<RuntimeSceneSet>> GetLoadedSceneSets;

	public bool loading {
		get {
			return currentLevelSetLoadTask != null;
		}
	}

	private RuntimeSceneSetLoadTask _lastLevelSetLoadTask;
	private RuntimeSceneSetLoadTask _currentLevelSetLoadTask;
	public RuntimeSceneSetLoadTask currentLevelSetLoadTask {
		get {
			return _currentLevelSetLoadTask;
		} private set {
			if(_currentLevelSetLoadTask == value)
				return;
			if(_currentLevelSetLoadTask != null) _lastLevelSetLoadTask = _currentLevelSetLoadTask;
			_currentLevelSetLoadTask = value;
		}
	}

//	public RuntimeSceneSetLoadTask pendingLevelSetLoadTask {get; private set;}

	private List<RuntimeSceneSetLoadTask> _pendingLevelSetLoadTasks = new List<RuntimeSceneSetLoadTask>();
	public List<RuntimeSceneSetLoadTask> pendingLevelSetLoadTasks {
		get {
			return _pendingLevelSetLoadTasks;
		} private set {
			_pendingLevelSetLoadTasks = value;
		}
	}

	private List<RuntimeSceneSetLoadTask> _tasksCompletedSinceQueue = new List<RuntimeSceneSetLoadTask>();

	public delegate void OnSceneSetTaskCompleteEvent(RuntimeSceneSetLoadTask task);
	public delegate void OnSceneSetLoadEvent(RuntimeSceneSet sceneSet);
	public delegate void OnSceneSetUnloadEvent();

	public event OnSceneSetTaskCompleteEvent OnCompleteTaskQueue;
	public event OnSceneSetTaskCompleteEvent OnCompleteTask;
	public event OnSceneSetTaskCompleteEvent OnAddTask;
//	public event OnSceneSetLoadEvent OnWillLoad;
//	public event OnSceneSetLoadEvent OnDidLoad;
	public event OnSceneSetUnloadEvent OnWillUnload;
//	public event OnSceneSetUnloadEvent OnDidUnload;

    // Helper function, cutting out the creation of a LoadTask. Remove this for the next project since, I think.
	public RuntimeSceneSetLoadTask LoadSceneSetup(RuntimeSceneSet sceneSet, LoadTaskMode sceneLoadMode, System.Action<RuntimeSceneSetLoadTask> whenLoaded = null, System.Action<RuntimeSceneSetLoadTask> whenActivated = null) {
		RuntimeSceneSetLoadTask newLevelSetLoadTask = new RuntimeSceneSetLoadTask(sceneSet, sceneLoadMode, whenLoaded, whenActivated);
		LoadSceneSetup(newLevelSetLoadTask);
		return newLevelSetLoadTask;
	}

	public void LoadSceneSetup(RuntimeSceneSetLoadTask newLevelSetLoadTask) {
		Debug.Log("LoadSceneSetup "+newLevelSetLoadTask.sceneSet.name);
		if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Add scene set load task to queue "+newLevelSetLoadTask);
		if(OnAddTask != null) OnAddTask(newLevelSetLoadTask);
		if(currentLevelSetLoadTask == null) {
			currentLevelSetLoadTask = newLevelSetLoadTask;
			StartCoroutine(LoadSceneSetupCR());
		} else {
			if(newLevelSetLoadTask.sceneLoadMode == LoadTaskMode.LoadSingle) {
				pendingLevelSetLoadTasks.Clear();
			} else if(newLevelSetLoadTask.sceneLoadMode == LoadTaskMode.LoadAdditive) {
				for (int i = pendingLevelSetLoadTasks.Count - 1; i >= 0; i--) {
					var loadTask = pendingLevelSetLoadTasks [i];
					if(loadTask.sceneLoadMode == LoadTaskMode.UnloadSoft && newLevelSetLoadTask.sceneSet == loadTask.sceneSet) {
						pendingLevelSetLoadTasks.RemoveAt(i);
					}
				}
			} else if(newLevelSetLoadTask.sceneLoadMode == LoadTaskMode.UnloadSoft) {
				for (int i = pendingLevelSetLoadTasks.Count - 1; i >= 0; i--) {
					var loadTask = pendingLevelSetLoadTasks [i];
					if(loadTask.sceneLoadMode == LoadTaskMode.LoadAdditive && newLevelSetLoadTask.sceneSet == loadTask.sceneSet) {
						pendingLevelSetLoadTasks.RemoveAt(i);
					}
				}
			}
			pendingLevelSetLoadTasks.Add(newLevelSetLoadTask);

			if(newLevelSetLoadTask.sceneLoadMode == LoadTaskMode.LoadSingle) {
				if(currentLevelSetLoadTask != null)
					currentLevelSetLoadTask.Cancel();
			}
		}
	}


	private IEnumerator LoadSceneSetupCR() {
        var startTime = Time.realtimeSinceStartup;
//		if(OnWillLoad != null) OnWillLoad(currentLevelSetLoadTask.sceneSet);
		currentLevelSetLoadTask.AssignTasks(); 

		if(currentLevelSetLoadTask.unloadTasks.Count != 0 && OnWillUnload != null)
			OnWillUnload();

		yield return StartCoroutine(currentLevelSetLoadTask.LoadSceneSetupCR(() => {
			Debug.Log("Scene set task "+currentLevelSetLoadTask.sceneSet+" "+currentLevelSetLoadTask.sceneLoadMode+" took "+(Time.realtimeSinceStartup-startTime)+" seconds");
			if(!currentLevelSetLoadTask.cancelled) {
				if (OnCompleteTask != null) OnCompleteTask(currentLevelSetLoadTask);
			}
        	OnPreCompleteSceneSetLoad();
		}));
		OnPostCompleteSceneSetLoad();
    }


    /// <summary>
	/// When a scene set is loaded, this starts the next load in the queue if one exists, or else calls CompleteLoadQueue.
    /// </summary>
	private void OnPreCompleteSceneSetLoad() {
		_tasksCompletedSinceQueue.Add(currentLevelSetLoadTask);
		if(!pendingLevelSetLoadTasks.IsEmpty()) {
			currentLevelSetLoadTask = pendingLevelSetLoadTasks[0];
			if(RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Queuing SceneSet Load: "+currentLevelSetLoadTask.sceneSet.name);
			pendingLevelSetLoadTasks.RemoveAt(0);
			StartCoroutine(LoadSceneSetupCR());
		} else {
			RuntimeSceneSetLoadTask lastLoadTask = currentLevelSetLoadTask;
			currentLevelSetLoadTask = null;
			Debug.Log("OnPreCompleteSceneSetLoad");
			OnCompleteLoadQueue(lastLoadTask);
		}
    }
	private void OnPostCompleteSceneSetLoad() {
		if(currentLevelSetLoadTask == null && pendingLevelSetLoadTasks.IsEmpty()) {
			Debug.Log("OnPostCompleteSceneSetLoad");
			OnCompleteLoadQueue(_lastLevelSetLoadTask);
			_lastLevelSetLoadTask = null;
		}
	}

    /// <summary>
    /// Called when the load queue is completed.
    /// </summary>
	private void OnCompleteLoadQueue (RuntimeSceneSetLoadTask lastLoadTask) {
		if (!lastLoadTask.cancelled && (lastLoadTask.sceneLoadMode == LoadTaskMode.LoadAdditive || lastLoadTask.sceneLoadMode == LoadTaskMode.LoadSingle)) {
			// Sets the active scene to be the last scene in the last setup to be loaded. 
			// This defines the lighting and some other properties.
			// Consider defining which scene this is in the scene set down the line.
			if(lastLoadTask.sceneSet != null && lastLoadTask.sceneLoadMode == LoadTaskMode.LoadSingle) {
				var lastPath = lastLoadTask.sceneSet.AllScenePaths().Last();
				var activeScene = SceneManager.GetSceneByPath(lastPath);
				SceneManager.SetActiveScene(activeScene);
			}

			if (OnCompleteTaskQueue != null) OnCompleteTaskQueue(lastLoadTask);
		} else {
			//Debug.LogWarning("Consider fixing this scene loading bug! More info at this line.");
			// There's a bug which will occur when the last load task is cancelled, or if the last item in the queue was to remove scenes.
			// In this case we want to set the active scene to the last uncancelled load task in the queue (or have another system for setting the main scene, as mentioned a few lines above.)

			// Another semi-related bug is that the OnCompleteTaskQueue event won't be called. 
			// We shouldn't call it if the last task was cancelled because the last task might have been the only task and in that case we want to act like nothing happened at all.

			// A solution to both these bugs is probably to have a list of load tasks completed since the load queue was last emptied,
			// and then scan through it to work out which scene to set as the main one, and also whether or not to call that event.
			for (int i = _tasksCompletedSinceQueue.Count - 1; i >= 0; i--) {
				var task = _tasksCompletedSinceQueue [i];
				if(!task.cancelled) {
					if (OnCompleteTaskQueue != null) OnCompleteTaskQueue(lastLoadTask);
					break;
				}
			}
		}

		_tasksCompletedSinceQueue.Clear();
    }




	// UTILS
	public static void Log (System.Object obj, System.Object a) {
		if(!debugLogging) return;
		var logString = LogString(obj, a);
		string LogString (System.Object obj, System.Object msg) {
			var sb = new System.Text.StringBuilder(msg.ToString());
			sb.AppendLine();
			sb.Append("(").Append(Time.unscaledTime.ToString()).Append(")");
			sb.Append(" ("+obj.GetType().Name+")");
			return sb.ToString();
		}
		if(obj is UnityEngine.Object) Debug.Log(logString, (UnityEngine.Object)obj);
		else Debug.Log(logString);
	}



	public static Scene[] GetCurrentScenes () {
		Scene[] scenes = new Scene[SceneManager.sceneCount];
		for(int i = 0; i < scenes.Length; i++)
			scenes[i] = SceneManager.GetSceneAt(i);
		return scenes;
	}

	public static string[] GetCurrentSceneNames () {
		string[] scenes = new string[SceneManager.sceneCount];
		for(int i = 0; i < scenes.Length; i++)
			scenes[i] = SceneManager.GetSceneAt(i).name;
		return scenes;
	}

	public static string[] GetCurrentScenePaths () {
		string[] scenes = new string[SceneManager.sceneCount];
		for(int i = 0; i < scenes.Length; i++)
			scenes[i] = SceneManager.GetSceneAt(i).path;
		return scenes;
	}


	public static void BroadcastMessageScene (UnityEngine.SceneManagement.Scene scene, string methodName) {
		BroadcastMessageScene(scene, methodName, SendMessageOptions.DontRequireReceiver);
    }

	public static void BroadcastMessageScene (UnityEngine.SceneManagement.Scene scene, string methodName, SendMessageOptions sendMessageOptions) {
		if(!scene.isLoaded) {
			Debug.LogWarning("Tried to BroadcastMessage '"+methodName+"' to scene '"+scene.name+"', but scene is not loaded.");
			return;
		}
		foreach (GameObject gameObject in scene.GetRootGameObjects().OrderBy(x => x.transform.GetSiblingIndex())) {
			BetterBroadcastMessage(gameObject, methodName, sendMessageOptions);
		}
    }

	public static void BroadcastMessageScene (UnityEngine.SceneManagement.Scene scene, string methodName, object parameter) {
		BroadcastMessageScene(scene, methodName, parameter, SendMessageOptions.DontRequireReceiver);
    }

	public static void BroadcastMessageScene (UnityEngine.SceneManagement.Scene scene, string methodName, object parameter, SendMessageOptions sendMessageOptions) {
		if(!scene.isLoaded) {
			Debug.LogWarning("Tried to BroadcastMessage '"+methodName+"' to scene '"+scene.name+"', but scene is not loaded.");
			return;
		}
		foreach (GameObject gameObject in scene.GetRootGameObjects().OrderBy(x => x.transform.GetSiblingIndex())) {
			BetterBroadcastMessage(gameObject, methodName, parameter, sendMessageOptions);
		}
    }

	/// <summary>
	/// Send message doesn't work in edit mode, so we use reflection in that case instead.
	/// </summary>
	/// <param name="message">Message.</param>
	public static void BetterSendMessage (GameObject go, string message, object obj = null, SendMessageOptions sendMessageOptions = SendMessageOptions.DontRequireReceiver) {
		#if UNITY_EDITOR
		if(!Application.isPlaying) {
			foreach(var component in go.GetComponents<Component>()) {
				try {
					var tMethod = component.GetType().GetMethod(message, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
					if(tMethod != null) tMethod.Invoke(component, obj == null ? null : new object[] {obj, sendMessageOptions});
				} catch {
					if(sendMessageOptions == SendMessageOptions.RequireReceiver) Debug.LogError("No reciever found for message "+message+" for type "+component.GetType()+" on gameobject "+go.name);
				}
			}
			return;
		}
		#endif
		go.SendMessage(message, obj, sendMessageOptions);
	}

	public static void BetterBroadcastMessage (GameObject go, string message, object obj = null, SendMessageOptions sendMessageOptions = SendMessageOptions.DontRequireReceiver) {
		#if UNITY_EDITOR
		if(!Application.isPlaying) {
			foreach(var transform in go.GetComponentsInChildren<Transform>()) 
				BetterSendMessage(go, message, obj, sendMessageOptions);
			return;
		}
		#endif
		go.BroadcastMessage(message, obj, sendMessageOptions);
	}
}