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
		RuntimeSceneSet[] assets = AssetDatabaseX.LoadAllAssetsOfType<RuntimeSceneSet>();
		foreach(RuntimeSceneSet asset in assets) {
			asset.SetScenePaths();
		}
	}
}
