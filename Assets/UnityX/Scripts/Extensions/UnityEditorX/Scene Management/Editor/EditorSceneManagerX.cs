using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnityEditorX.SceneManagement {

	/// <summary>
	/// Provides additional functions to get info about the scene structure of a unity editor project
	/// </summary>
	[InitializeOnLoad]
	public static class EditorSceneManagerX {

		/// <summary>
		/// Refreshes the scene array when the scene setup changes.
		/// </summary>
		class EditorScenePostProcessor : AssetPostprocessor {
			private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
				bool found = false;
				if(deletedAssets.Length > 0) {
					found = SceneFileInAssetArray(deletedAssets);
				}
				if(!found && movedAssets.Length > 0) {
					found = SceneFileInAssetArray(movedAssets);
				}
				if(!found && importedAssets.Length > 0) {
					found = SceneFileInAssetArray(importedAssets);
				}
				if(found)
					EditorSceneManagerX.ChangeSceneAssets();
			}
			
			private static bool SceneFileInAssetArray (string[] assets) {
				for (var i = 0; i < assets.Length; i++) {
					if(Path.GetExtension(assets[i]) == EditorSceneManagerX.sceneFileExtension) {
						return true;
					}
				}
				return false;
			}
		}
		
		public const string sceneFileExtension = ".unity";
		public static string[] sceneNames;
		public static string[] scenePaths;

		public delegate void OnChangeSceneAssetsEvent();
		public static event OnChangeSceneAssetsEvent OnChangeSceneAssets;

		static EditorSceneManagerX () {
			Refresh();
		}

		public static bool AnySceneDirty () {
			for(int i = 0; i < SceneManager.loadedSceneCount; i++) {
				if(EditorSceneManager.GetSceneAt(i).isDirty) return true;
			}
			return false;
		}

		public static List<Scene> GetDirtyScenes () {
			List<Scene> scenes = new List<Scene>();
			for(int i = 0; i < SceneManager.loadedSceneCount; i++) {
				var scene = EditorSceneManager.GetSceneAt(i);
				if(scene.isDirty) scenes.Add(scene);
			}
			return scenes;
		}

		public static void ChangeSceneAssets () {
			Refresh();
			if(OnChangeSceneAssets != null)
				OnChangeSceneAssets();
		}

		private static void Refresh () {
			sceneNames = GetSceneNamesInProject();
			scenePaths = GetScenePathsInProject();

		}

		private static string[] GetSceneNamesInProject (string path = "") {
			string[] files = GetScenePathsInProject(path);
			for(int i = 0; i < files.Length; i++) {
				files[i] = Path.GetFileNameWithoutExtension(files[i]);
			}
			return files;
		}
		
		private static string[] GetScenePathsInProject (string path = "") {
			string[] files = System.IO.Directory.GetFiles(Path.Combine(Application.dataPath, path), "*.unity", System.IO.SearchOption.AllDirectories);
			if(files == null) files = new string[0];

			var assetsPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
			for(int i = 0; i < files.Length; i++) {
				files[i] = files[i].Substring(files[i].IndexOf(assetsPath) + assetsPath.Length).Replace("\\", "/");
			}
			return files;
		}
	}
}