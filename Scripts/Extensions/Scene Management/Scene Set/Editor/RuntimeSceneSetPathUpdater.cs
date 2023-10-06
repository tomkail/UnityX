using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using UnityEditorInternal;
using System.Linq;

[InitializeOnLoad]
public static class RuntimeSceneSetPathUpdater {
	static RuntimeSceneSetPathUpdater () {
		UnityEditorX.SceneManagement.EditorSceneManagerX.OnChangeSceneAssets += () => {
			Refresh();
		};
	}

	static void Refresh () {
		RuntimeSceneSet[] assets = LoadAllAssetsOfType<RuntimeSceneSet>();
		foreach(RuntimeSceneSet asset in assets) {
			asset.SetScenePaths();
		}

		static T[] LoadAllAssetsOfType<T>(string optionalPath = "") where T : Object {
			string[] GUIDs;
			if(optionalPath != "") {
				if(optionalPath.EndsWith("/")) {
					optionalPath = optionalPath.TrimEnd('/');
				}
				GUIDs = AssetDatabase.FindAssets("t:" + typeof (T).ToString(),new string[] { optionalPath });
			} else {
				GUIDs = AssetDatabase.FindAssets("t:" + typeof (T).ToString());
			}
			T[] objectList = new T[GUIDs.Length];
			
			for (int index = 0; index < GUIDs.Length; index++) {
				string guid = GUIDs[index];
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				T asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
				objectList[index] = asset;
			}
			
			return objectList;
		}
	}
}
