using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Collections.Generic;
using System.Linq;

namespace UnityX.SceneManagement {

	[CreateAssetMenu(fileName = "New Scene Set", menuName = "Scene Set", order = 1000)]
	public class RuntimeSceneSet : ScriptableObject {
		public RuntimeSceneSet[] sets;

		/// <summary>
		/// The scenes locally included by this set. Each SceneReference stores an editor SceneAsset plus
		/// the cached asset path that is used at runtime.
		/// </summary>
		public SceneReference[] scenes;

		#if UNITY_EDITOR
		// Keep each reference's cached path in sync with its SceneAsset (handles renamed/moved scenes).
		void OnValidate () {
			if (scenes == null) return;
			bool changed = false;
			for (int i = 0; i < scenes.Length; i++)
				if (scenes[i].RefreshPath()) changed = true;
			if (changed) EditorUtility.SetDirty(this);
		}
		#endif

		/// <summary>
		/// Calls the method named methodName on every MonoBehaviour in each scene in this set.
		/// </summary>
		public void BroadcastMessageToIncludedScenes (string methodName) {
			foreach (var scene in GetScenes())
				RuntimeSceneSetLoader.BroadcastMessageScene(scene, methodName);
		}

		/// <summary>
		/// Calls the method named methodName (passing parameter) on every MonoBehaviour in each scene in this set.
		/// </summary>
		public void BroadcastMessageToIncludedScenes (string methodName, object parameter) {
			foreach (var scene in GetScenes())
				RuntimeSceneSetLoader.BroadcastMessageScene(scene, methodName, parameter);
		}

		/// <summary>
		/// Gets the scenes.
		/// </summary>
		public Scene[] GetScenes () {
			List<string> paths = AllScenePaths();
			Scene[] result = new Scene[paths.Count];
			for (int i = 0; i < paths.Count; i++)
				result[i] = SceneManager.GetSceneByPath(paths[i]);
			return result;
		}

		/// <summary>
		/// Determines whether this set is currently loaded, or is loaded as part of another set.
		/// </summary>
		public bool IsCurrentlyIncluded () {
			string[] currentSceneNames = RuntimeSceneSetLoader.GetCurrentSceneNames();
			List<string> allScenesInSet = AllSceneNames();
			return allScenesInSet.Intersect(currentSceneNames).Count() == allScenesInSet.Count();
		}

		/// <summary>
		/// Determines whether the current scene-manager setup exactly matches this setup, including scene ORDER (uses SequenceEqual).
		/// Scenes may be in the process of being loaded/unloaded so be careful when using this!
		/// A more robust solution is to manually track which scene sets are loaded.
		/// </summary>
		public bool IsCurrentlyUniquelyLoaded () {
			var currentScenePaths = RuntimeSceneSetLoader.GetCurrentScenePaths();
			return AllScenePaths().SequenceEqual(currentScenePaths);
		}

		List<RuntimeSceneSet> GetSetsInHierarchy () {
			List<RuntimeSceneSet> allSets = new List<RuntimeSceneSet>();
			if (sets != null) {
				foreach (RuntimeSceneSet set in sets) {
					if (set == null) continue;
					allSets.AddRange(set.GetSetsInHierarchy());
				}
			}
			allSets.Add(this);
			return allSets;
		}

		/// <summary>
		/// Returns a list of all the scene names in the hierarchy of this scene set.
		/// </summary>
		public List<string> AllSceneNames () {
			List<string> paths = AllScenePaths();
			for (int i = 0; i < paths.Count; i++)
				paths[i] = System.IO.Path.GetFileNameWithoutExtension(paths[i]);
			return paths;
		}

		/// <summary>
		/// Returns a list of all the scene paths in the hierarchy of this scene set.
		/// </summary>
		public List<string> AllScenePaths () {
			List<string> result = new List<string>();
			foreach (RuntimeSceneSet set in GetSetsInHierarchy()) {
				if (set.scenes == null) continue;
				foreach (SceneReference scene in set.scenes)
					if (scene.isValid) result.Add(scene.path);
			}
			return result;
		}

		/// <summary>
		/// Checks if this set includes another set anywhere in its hierarchy.
		/// </summary>
		public bool IncludesSet (RuntimeSceneSet setToFind) {
			return GetSetsInHierarchy().Contains(setToFind);
		}

		/// <summary>
		/// Checks if a scene with a specific name exists in the hierarchy of this set.
		/// </summary>
		public bool IncludesSceneName (string name) {
			return AllSceneNames().Contains(name);
		}

		#if UNITY_EDITOR
		public bool IsIncludedInBuildSettings () {
			var scenesInBuildSettings = EditorBuildSettings.scenes.Select(s => s.path);
			return AllScenePaths().Except(scenesInBuildSettings).Any() == false;
		}

		public void AddMissingToBuildSettings () {
			var scenesInBuildSettings = EditorBuildSettings.scenes.Select(s => s.path);
			string[] missingScenes = AllScenePaths().Except(scenesInBuildSettings).ToArray();
			AddToBuildSettings(missingScenes);
		}

		static void AddToBuildSettings (params string[] paths) {
			var newBuildSettings = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
			foreach (string path in paths)
				newBuildSettings.Add(new EditorBuildSettingsScene { path = path, enabled = true });
			EditorBuildSettings.scenes = newBuildSettings.ToArray();
		}

		List<SceneSetup> ScenesToSceneSetup (int sceneIndexToSetActive = -1) {
			List<SceneSetup> setups = new List<SceneSetup>();
			if (sets != null) {
				foreach (RuntimeSceneSet set in sets)
					if (set != null) setups.AddRange(set.ScenesToSceneSetup());
			}
			if (scenes != null) {
				for (int i = 0; i < scenes.Length; i++) {
					if (!scenes[i].isValid) {
						Debug.LogWarning("Scene at index " + i + " in " + name + " is empty!");
						continue;
					}
					setups.Add(new SceneSetup { path = scenes[i].path, isLoaded = true });
				}
			}
			if (sceneIndexToSetActive >= 0 && sceneIndexToSetActive < setups.Count)
				setups[sceneIndexToSetActive].isActive = true;
			return setups;
		}

		public SceneSetup[] ToSceneSetup (int sceneIndexToSetActive = -1) {
			return ScenesToSceneSetup(sceneIndexToSetActive).ToArray();
		}

		public void LoadInEditor () {
			var sceneSetup = ToSceneSetup();
			if (sceneSetup.Length == 0) return;
			EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
			SceneManager.SetActiveScene(SceneManager.GetSceneAt(sceneSetup.Length - 1));
		}
		#endif
	}
}
