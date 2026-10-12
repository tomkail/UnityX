using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityX.SceneManagement {
	// Keeps track of which RuntimeSceneSets are loaded, loads the default set on start, and sends DidLoad to the
	// scenes of each set that finishes loading.
	[RequireComponent(typeof(RuntimeSceneSetLoader))]
	public class SceneSetManager : MonoSingleton<SceneSetManager> {

	    public bool autoLoadOnPlay = true;
		// Load and activate the default set's scenes one at a time, instead of loading them all and then activating them.
		public bool activateDuringLoad;

		public List<RuntimeSceneSet> loadedSceneSets = new List<RuntimeSceneSet>();

	//	private RuntimeSceneSet _currentSceneSet;
	//	public RuntimeSceneSet currentSceneSet {
	//		get {
	//			if(!Application.isPlaying) {
	//				_currentSceneSet = sceneSetDatabase.FindSetupFromCurrent();
	//			}
	//			return _currentSceneSet;
	//		} set {
	//			_currentSceneSet = value;
	//		}
	//	}

		public RuntimeSceneSet standaloneBuildDefaultSceneSet;

		private Dictionary<string, RuntimeSceneSet> _sceneSetDictionary;
		private Dictionary<string, RuntimeSceneSet> sceneSetDictionary {
			get {
				if(!Application.isPlaying || _sceneSetDictionary.IsNullOrEmpty()) {
					_sceneSetDictionary = new Dictionary<string, RuntimeSceneSet>();
					foreach(var sceneSet in sceneSetDatabase.sets) {
						if(sceneSet == null) {
							Debug.LogWarning("Scene set in database is null.");
							continue;
						} else if(_sceneSetDictionary.ContainsKey(sceneSet.name)) {
							Debug.LogWarning("Scene set dictionary already contains a set with the name '"+sceneSet.name+"'");
							continue;
						}
						_sceneSetDictionary.Add(sceneSet.name, sceneSet);
					}
				}
				return _sceneSetDictionary;
			}
		}

		public List<RuntimeSceneSet> sceneSets {
			get {
				return sceneSetDictionary.Values.ToList();
			}
		}

		public SceneSetDatabase sceneSetDatabase;


		/// <summary>
		/// Determines whether the scene set with the specified name is loaded.
		/// </summary>
		/// <returns><c>true</c> if this instance is set loaded the specified sceneSetName; otherwise, <c>false</c>.</returns>
		/// <param name="sceneSetName">Scene set name.</param>
		/// <param name="exactMatch">Return true only if this is the only scene set that's loaded.</param>
		public bool IsSetLoaded (string sceneSetName, bool exactMatch=false) {
			RuntimeSceneSet sceneSet = SceneSetManager.Instance.FindSceneSet(sceneSetName);
			if(sceneSet == null) {
				Debug.LogWarning("Scene set with name '"+sceneSetName+"' was not found while attempting to check if it was loaded.");
				return false;
			}
			return IsSetLoaded(sceneSet, exactMatch);
		}

		/// <summary>
		/// Determines whether the scene set is loaded.
		/// </summary>
		/// <returns><c>true</c> if this instance is set loaded the specified sceneSet; otherwise, <c>false</c>.</returns>
		/// <param name="sceneSet">Scene set.</param>
		/// <param name="exactMatch">Return true only if this is the only sceneet that's loaded.</param>
		public bool IsSetLoaded (RuntimeSceneSet sceneSet, bool exactMatch=false) {
	//		return sceneSet.IsCurrentlyLoaded();
			return loadedSceneSets.Contains(sceneSet) && (!exactMatch || loadedSceneSets.Count == 1);
		}

		/// <summary>
		/// Finds a scene set by asset name.
		/// </summary>
		/// <returns>The scene set.</returns>
		/// <param name="sceneSetName">Scene set name.</param>
		public RuntimeSceneSet FindSceneSet (string sceneSetName) {
			RuntimeSceneSet sceneSet;
			if(sceneSetDictionary.TryGetValue(sceneSetName, out sceneSet)) {
				return sceneSet;
			} else {
	//			string allSceneSetNames = "";
	//			foreach(var _sceneSet in sceneSetDictionary) {
	//				allSceneSetNames += "\n" + _sceneSet.Key;
	//			}
	//			Debug.LogWarning("Failed to find scene set with name "+sceneSetName+".\nPrinting a list of all loaded scene sets..."+allSceneSetNames);
				return null;
			}
		}

		/// <summary>
		/// Checks to see if the specified scene set is included by any other scene set.
		/// This is handy for finding which sets are "includes" and which are "level" scene sets.
		/// </summary>
		/// <returns><c>true</c>, if the scene set is included by any other, <c>false</c> otherwise.</returns>
		/// <param name="sceneSet">Scene set.</param>
		public bool GetIsSceneSetIncludedByAnother (RuntimeSceneSet sceneSet) {
			foreach(RuntimeSceneSet otherSceneSet in sceneSets) {
				if(otherSceneSet == sceneSet) continue;
				if(otherSceneSet.IncludesSet(sceneSet))
					return true;
			}
			return false;
		}



		protected override void Awake () {
			base.Awake();
			RuntimeSceneSetLoader.Instance.OnCompleteTask += OnCompleteTask;
			RuntimeSceneSetLoader.Instance.OnAddTask += OnAddTask;
	//		RuntimeSceneSetLoader.Instance.OnWillLoad += OnWillLoad;
	//		RuntimeSceneSetLoader.Instance.OnWillUnload += OnWillUnload;
	//		RuntimeSceneSetLoader.Instance.OnDidUnload += OnDidUnload;
			#if UNITY_EDITOR
			sceneSetDatabase.CreateSetsArray();
			#endif
			RefreshLoadedSceneSets();
		}

		private void Start () {
			#if !UNITY_EDITOR
			LoadDefaultSceneSet();
			#else
			if(loadedSceneSets.Count == 0 && autoLoadOnPlay) {
				LoadDefaultSceneSet();
			} else {
				CoroutineHelper.DelayFrame(() => {
					if(RuntimeSceneSetLoader.Instance.loading) return;
	//				foreach(var loadedSceneSet in loadedSceneSets) {
	//					Debug.Log(loadedSceneSet.name);
	//					loadedSceneSet.BroadcastMessageToIncludedScenes("DidLoad");
	//				}
					ComponentX.BroadcastMessageGlobal("DidLoad");
				}, 1);
			}
			#endif
		}

		void LoadDefaultSceneSet () {
			var loadTask = new RuntimeSceneSetLoadTask(standaloneBuildDefaultSceneSet, LoadTaskMode.LoadSingle, null, null);
			loadTask.activateDuringLoad = activateDuringLoad;
			RuntimeSceneSetLoader.Instance.LoadSceneSetup(loadTask);
		}

		/*
		/// <summary>
		/// Called when a scene set load begins
		/// </summary>
		/// <param name="sceneSet">Scene set.</param>
		void OnWillLoad (RuntimeSceneSet sceneSet) {
			if(currentSceneSet != null) {
	//			currentSceneSet.BroadcastMessageToIncludedScenes("WillLoad");
	//			currentSceneSet.BroadcastMessageToIncludedScenes("WillLoad", sceneSet);
			}
		}

		/// <summary>
		/// Called when a scene set load has just completed.
		/// </summary>
		void OnWillUnload () {
			if(currentSceneSet != null) {
	//			currentSceneSet.BroadcastMessageToIncludedScenes("WillUnload");
			}
		}

		/// <summary>
		/// Called when the old scenes have been removed
		/// </summary>
		void OnDidUnload () {
			if(RuntimeSceneSetLoader.Instance.currentLevelSetLoadTask.sceneSet != null) {
	//			RuntimeSceneSetLoader.Instance.currentLevelSetLoadTask.sceneSet.BroadcastMessageToIncludedScenes("DidUnload");
	//			RuntimeSceneSetLoader.Instance.currentLevelSetLoadTask.sceneSet.BroadcastMessageToIncludedScenes("DidUnload", currentSceneSet);
			}
		}

		/// <summary>
		/// Called after the load has completed and the old scenes have been removed
		/// </summary>
		/// <param name="sceneSet">Scene set.</param>
		void OnDidLoad (RuntimeSceneSet sceneSet) {
			currentSceneSet = sceneSet;
			currentSceneSet.BroadcastMessageToIncludedScenes("DidLoad");
		}
		*/

		// Called when a scene set load task is finished.
		void OnCompleteTask (RuntimeSceneSetLoadTask loadTask) {
			RefreshLoadedSceneSets();
			if(loadTask.sceneLoadMode == LoadTaskMode.LoadAdditive || loadTask.sceneLoadMode == LoadTaskMode.LoadSingle) {
	//			loadedSceneSets.Add(loadTask.sceneSet);
				BroadcastMessageToLoadTasks(loadTask, "DidLoad");
			}
	//		} else if(loadTask.sceneLoadMode == LoadTaskMode.UnloadSoft || loadTask.sceneLoadMode == LoadTaskMode.UnloadHard) {
	//			loadedSceneSets.Remove(loadTask.sceneSet);
	//		}
		}

		void OnAddTask (RuntimeSceneSetLoadTask loadTask) {
			RefreshLoadedSceneSets();
		}

		void BroadcastMessageToLoadTasks (RuntimeSceneSetLoadTask loadTask, string methodName) {
			foreach(var sceneLoadTask in loadTask.loadTasks) {
				var scene = SceneManager.GetSceneByName(sceneLoadTask.sceneName);
				ComponentX.BroadcastMessageScene(scene, methodName);
			}
		}

		void RefreshLoadedSceneSets () {
			loadedSceneSets.Clear();
			foreach(var sceneSet in sceneSetDictionary) {
				if(!GetIsSceneSetIncludedByAnother(sceneSet.Value) && sceneSet.Value.IsCurrentlyIncluded()) {
					loadedSceneSets.Add(sceneSet.Value);
				}
			}
		}
	}
}
