using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityX.SceneManagement {

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
			get => _state;
			set {
				_state = value;
				OnChangeState?.Invoke(_state);
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
		public bool loadingDone => loadTasks.All(x => x.loadingDone);

		// When all tasks are allowed to activate
		public bool allTasksAllowedToActivate => loadTasks.All(x => x.allowActivation);

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
			if (sceneLoadMode == LoadTaskMode.LoadSingle || sceneLoadMode == LoadTaskMode.LoadAdditive) {
				foreach (string scenePath in sceneSetPaths) {
					Scene scene = SceneManager.GetSceneByPath(scenePath);
					if (scene.isLoaded)
						continue;
					string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
					var sceneLoadTask = new SceneLoadTask(sceneName);
					sceneLoadTask.allowActivation = !yieldActivation && !onlyActivateWhenAllLoaded;
					loadTasks.Add(sceneLoadTask);
				}
			}

			string[] currentPaths = RuntimeSceneSetLoader.GetCurrentScenePaths();
			List<string> pathsToUnload = new List<string>();
			if (sceneLoadMode == LoadTaskMode.LoadSingle) {
				pathsToUnload = currentPaths.Except(sceneSetPaths).ToList();
				pathsToUnload.Reverse();
			} else if (sceneLoadMode == LoadTaskMode.UnloadSoft) {
				// From all the scenes in this set, scan through all the active scene sets and select any scenes which aren't used in any others.
				// A scene is only unloaded if it isn't contained by ANY of the loaded scene sets, so a scene still used by another set is kept.
				var loadedSceneSets = RuntimeSceneSetLoader.GetLoadedSceneSets();
				foreach (var scenePath in sceneSetPaths) {
					bool containedByAnySet = false;
					foreach (var loadedSceneSet in loadedSceneSets) {
						if (loadedSceneSet.AllScenePaths().Contains(scenePath)) {
							containedByAnySet = true;
							break;
						}
					}
					if (!containedByAnySet)
						pathsToUnload.Add(scenePath);
				}
			} else if (sceneLoadMode == LoadTaskMode.UnloadHard) {
				pathsToUnload = sceneSetPaths.ToList();
			}
			pathsToUnload = pathsToUnload.Distinct().ToList();
			foreach (string scenePath in pathsToUnload) {
				Scene scene = SceneManager.GetSceneByPath(scenePath);
				if (!scene.isLoaded)
					continue;
				string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
				unloadTasks.Add(new SceneUnloadTask(sceneName));
			}

			whenTasksAssigned?.Invoke(this);
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "AssignedTasks:\nUnload: " + string.Join(", ", unloadTasks) + "\nLoad:" + string.Join(", ", loadTasks));
		}

		public async Awaitable LoadSceneSetupAsync (Action onPreComplete) {
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Begin " + GetType().Name + " Load: " + sceneSet.name);

			await UnloadScenesAsync();
			await LoadScenesAsync();

			// Activation
			if (loadTasks.Any()) {
				// Only allow yield activation if not already activated or not cancelled
				while (!allTasksAllowedToActivate && !cancelled && yieldActivation) {
					state = State.WaitingForAllowActivation;
					await Awaitable.NextFrameAsync();
				}

				if (!allTasksAllowedToActivate)
					Activate();
				else if (yieldActivation)
					Debug.LogWarning("All tasks were allowed to activate, but load task is marked as yieldActivation. This can happen if tasks are all individually allowed to activate, and isn't a bug if you're aware of it!");
			}
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name + " completed Activating '" + sceneSet.name + "'. Load Tasks:\n" + string.Join(", ", loadTasks));

			// Completion
			while (loadTasks.Any(x => !x.complete)) await Awaitable.NextFrameAsync();

			state = State.Complete;
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Complete " + GetType().Name + " '" + sceneSet.name + "'.");
			onPreComplete?.Invoke();
			whenComplete?.Invoke(this);
		}

		async Awaitable UnloadScenesAsync () {
			state = State.Unloading;
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name + " beginning Unloading '" + sceneSet.name + "'. Unload Tasks:\n" + string.Join(", ", unloadTasks));
			foreach (var task in unloadTasks) {
				// Fire-and-forget: the task drives itself off the player loop (was StartCoroutine).
				_ = task.UnloadAsync();
				if (activateDuringLoad)
					while (!task.complete) await Awaitable.NextFrameAsync();
			}
			while (unloadTasks.Any(task => !task.complete)) await Awaitable.NextFrameAsync();

			// When all scenes are unloaded, also unload unused assets. We believe that Unity doesn't do this automatically when doing additive loading.
			// Fire-and-forget (discarded) to match the original coroutine, which didn't wait on it.
			_ = Resources.UnloadUnusedAssets();

			whenUnloaded?.Invoke(this);
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name + " completed Unloading '" + sceneSet.name + "'. Unload Tasks:\n" + string.Join(", ", unloadTasks));
		}

		async Awaitable LoadScenesAsync () {
			state = State.Loading;
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name + " beginning Loading '" + sceneSet.name + "'. Load Tasks:\n" + string.Join(", ", loadTasks));
			foreach (var task in loadTasks) {
				// Fire-and-forget: each load task runs to its activation gate on its own (was StartCoroutine).
				_ = task.LoadAsync();
				// If this is true we also activate during this step
				if (activateDuringLoad) {
					task.allowActivation = true;
					while (!task.complete) {
						task.allowActivation = true;
						await Awaitable.NextFrameAsync();
					}
				}
			}

			// Wait only until scenes are loaded (held at 0.9), not activated — activation is a later step.
			while (loadTasks.Any(task => !task.loadingDone)) await Awaitable.NextFrameAsync();

			whenLoaded?.Invoke(this);
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name + " completed Loading '" + sceneSet.name + "'. Load Tasks:\n" + string.Join(", ", loadTasks));
		}

		void Activate () {
			state = State.Activating;
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, GetType().Name + " beginning Activating '" + sceneSet.name + "'. Load Tasks:\n" + string.Join(", ", loadTasks));
			foreach (SceneLoadTask task in loadTasks)
				task.allowActivation = true;
		}

		public void Cancel() {
			state = State.Cancelling;
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Cancel " + GetType().Name + " '" + sceneSet.name + "'. Load Tasks:\n" + string.Join(", ", loadTasks));
			cancelled = true;
			foreach (SceneLoadTask task in loadTasks)
				task.Cancel();
		}

		public void Uncancel() {
			if (RuntimeSceneSetLoader.debugLogging) RuntimeSceneSetLoader.Log(this, "Uncancel " + GetType().Name + " '" + sceneSet.name + "'. Load Tasks:\n" + string.Join(", ", loadTasks));
			cancelled = false;
			foreach (SceneLoadTask task in loadTasks)
				task.Uncancel();
		}

		public override string ToString () {
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine(string.Format("[RuntimeSceneSetLoadTask: sceneSet={0}, sceneLoadMode={1}, state={2}, cancelled={3}, yieldActivation={4}]", sceneSet, sceneLoadMode, state, cancelled, yieldActivation));
			sb.AppendLine("Load Tasks:");
			foreach (var loadTask in loadTasks) sb.AppendLine(loadTask.sceneName);
			sb.AppendLine("Unload Tasks:");
			foreach (var unloadTask in unloadTasks) sb.AppendLine(unloadTask.sceneName);
			return sb.ToString();
		}
	}
}
