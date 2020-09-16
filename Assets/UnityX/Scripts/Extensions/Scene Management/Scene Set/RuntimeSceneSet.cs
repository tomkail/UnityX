using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Scene Set", menuName = "Scene Set", order = 1000)]
public class RuntimeSceneSet : ScriptableObject {

	public RuntimeSceneSet[] sets;

	/// <summary>
	/// The object representations of .unity scene files.
	/// </summary>
	// [SceneAttribute]
	public Object[] sceneAssets;

	/// <summary>
	/// The paths of the scenes locally included by this set.
	/// </summary>
	public string[] scenePaths;

	/// <summary>
	/// Calls the method named methodName on every MonoBehaviour in this game object or any of its children.
	/// </summary>
	/// <param name="message">Message.</param>
	public void BroadcastMessageToIncludedScenes (string methodName) {
		foreach(var scene in GetScenes()) {
			ComponentX.BroadcastMessageScene(scene, methodName);
		}
	}

	/// <summary>
	/// Calls the method named methodName on every MonoBehaviour in this game object or any of its children.
	/// </summary>
	/// <param name="message">Message.</param>
	public void BroadcastMessageToIncludedScenes (string methodName, object parameter) {
		foreach(var scene in GetScenes()) {
			ComponentX.BroadcastMessageScene(scene, methodName, parameter);
		}
	}

	/// <summary>
	/// Gets the scenes.
	/// </summary>
	/// <returns>The scenes.</returns>
	public Scene[] GetScenes () {
		List<string> paths = AllScenePaths();
		Scene[] scenes = new Scene[paths.Count];
		for (int i = 0; i < paths.Count; i++) {
			scenes [i] = SceneManager.GetSceneByPath (paths [i]);
		}
		return scenes;
	}

	/// <summary>
	/// Determines whether this set is currently loaded, or is loaded as part of another set.
	/// </summary>
	/// <returns><c>true</c> if this instance is currently included; otherwise, <c>false</c>.</returns>
	public bool IsCurrentlyIncluded () {
		string[] currentScenesNames = SceneManagerX.GetCurrentSceneNames();
		List<string> allScenesInSet = AllSceneNames();
		return allScenesInSet.Intersect(currentScenesNames).Count() == allScenesInSet.Count();
	}

	/// <summary>
	/// Determines whether the current scene manager setup exactly matches this setup.
	/// DO NOT use this for checking if a scene set is loaded after the game has started running. Instead use IsSetLoaded in SceneSetManager.
	/// </summary>
	/// <returns><c>true</c> if this instance is currently fully loaded; otherwise, <c>false</c>.</returns>
	public bool IsCurrentlyUniquelyLoaded () {
		var currentScenesPaths = SceneManagerX.GetCurrentScenePaths();
		var allScenePaths = AllScenePaths();
        return allScenePaths.SequenceEqual(currentScenesPaths);
	}

	private List<RuntimeSceneSet> GetSetsInHierarchy () {
		List<RuntimeSceneSet> allSets = new List<RuntimeSceneSet>(); 
		foreach(RuntimeSceneSet setupSet in sets) {
			if(setupSet == null) continue;
			allSets.AddRange(setupSet.GetSetsInHierarchy());
		}
		allSets.Add(this);
		return allSets;
	}

	/// <summary>
	/// Returns a list of all the scene names in the hierarchy of this scene set.
	/// </summary>
	/// <returns>The scene names.</returns>
	public List<string> AllSceneNames () {
		List<string> paths = AllScenePaths();
		for(int i = 0; i < paths.Count; i++) {
			paths[i] = System.IO.Path.GetFileNameWithoutExtension(paths[i]);
		}
		return paths;
	}

	/// <summary>
	/// Returns a list of all the scene paths in the hierarchy of this scene set.
	/// </summary>
	/// <returns>The scene paths.</returns>
	public List<string> AllScenePaths () {
		List<RuntimeSceneSet> allSets = GetSetsInHierarchy();
		List<string> setups = new List<string>(); 
		foreach(RuntimeSceneSet setupSet in allSets) {
			setups.AddRange(setupSet.scenePaths);
		}
		return setups;
	}

	/// <summary>
	/// Checks if this set includes another set anywhere in its hierarchy.
	/// </summary>
	/// <returns><c>true</c>, if set was includesed, <c>false</c> otherwise.</returns>
	/// <param name="setToFind">Set to find.</param>
	public bool IncludesSet (RuntimeSceneSet setToFind) {
		List<RuntimeSceneSet> allSets = GetSetsInHierarchy();
		foreach(var set in allSets) {
			if(set == setToFind)
				return true;
		}
		return false;
	}

	/// <summary>
	/// Checks if a scene with a specific name exists in the hierarchy of this set.
	/// </summary>
	/// <returns><c>true</c>, if scene name was includesed, <c>false</c> otherwise.</returns>
	/// <param name="name">Name.</param>
	public bool IncludesSceneName (string name) {
		return AllSceneNames().Contains(name);
	}

	#if UNITY_EDITOR
	public bool IsIncludedInBuildSettings () {
		List<string> scenesInBuildSettings = new List<string>();
		foreach(var scene in EditorBuildSettings.scenes) {
			scenesInBuildSettings.Add(scene.path);
		}
		List<string> allScenesInSet = AllScenePaths();
		return allScenesInSet.Except(scenesInBuildSettings).Count() > 0;
	}

	public void AddMissingToBuildSettings () {
		List<string> scenesInBuildSettings = new List<string>();
		foreach(var scene in EditorBuildSettings.scenes)
			scenesInBuildSettings.Add(scene.path);
		List<string> allScenesInSet = AllScenePaths();
		string[] missingScenes = allScenesInSet.Except(scenesInBuildSettings).ToArray();
		EditorBuildSettingsX.AddToBuildSettings(missingScenes);
	}

	/// <summary>
	/// Sets the scene paths from the scene .unity files.
	/// </summary>
	public void SetScenePaths () {
		bool changed = false;
		string[] newScenePaths = new string[sceneAssets.Length];
		if (scenePaths == null || newScenePaths.Length != scenePaths.Length) {
			changed = true;
		}
		for (int i = 0; i < newScenePaths.Length; i++) {
			newScenePaths [i] = UnityEditor.AssetDatabase.GetAssetPath (sceneAssets [i]);
			if (!changed && scenePaths [i] != newScenePaths [i]) {
				changed = true;
			}
		}

		if(changed) {
			scenePaths = newScenePaths;
			EditorUtility.SetDirty(this);
		}
	}

	public List<SceneSetup> ScenesToSceneSetup () {
		List<SceneSetup> setups = new List<SceneSetup>(); 
		foreach(RuntimeSceneSet setupSet in sets) {
			setups.AddRange(setupSet.ScenesToSceneSetup());
		}
		for(int i = 0; i < scenePaths.Length; i++) {
			SceneSetup setup = new SceneSetup();
			setup.path = scenePaths[i];
			if(string.IsNullOrWhiteSpace(setup.path)) {
				Debug.LogWarning("Scene path at index "+i+" in "+this.name+" is empty!");
				continue;
			}
			setup.isLoaded = true;
			setups.Add(setup);
		}
		if(setups.Count > 0)
			setups[0].isActive = true;
		return setups;
	}

	public SceneSetup[] ToSceneSetup () {
		return ScenesToSceneSetup().ToArray();
	}

	public void LoadInEditor () {
		var sceneSetup = ToSceneSetup();
		EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
		var lastScene = SceneManager.GetSceneAt(sceneSetup.Length-1);
		SceneManager.SetActiveScene(lastScene);
	}
	#endif
}