using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityX.SceneManagement {

	// A serializable reference to a scene that is usable at runtime.
	//
	// In the editor you assign a SceneAsset (a .unity file); its asset path is cached into `scenePath`
	// so it survives into builds, where SceneAsset does not exist. The cached path is kept fresh two
	// ways: the property drawer writes it when you assign the asset, and the owning object refreshes it
	// on serialize (see RuntimeSceneSet.OnBeforeSerialize) — so moving or renaming the .unity file keeps
	// the reference correct without any global asset-postprocessor.
	[System.Serializable]
	public struct SceneReference {
		#if UNITY_EDITOR
		// Editor-only asset reference (GUID-based, so it survives moves/renames). Stripped from builds.
		[SerializeField] SceneAsset sceneAsset;
		#endif
		// The cached asset path, e.g. "Assets/Scenes/Main.unity". This is what ships and is used at runtime.
		[SerializeField] string scenePath;

		public string path => scenePath;
		public string name => string.IsNullOrEmpty(scenePath) ? string.Empty : Path.GetFileNameWithoutExtension(scenePath);
		public bool isValid => !string.IsNullOrEmpty(scenePath);

		#if UNITY_EDITOR
		public SceneReference (SceneAsset sceneAsset) {
			this.sceneAsset = sceneAsset;
			this.scenePath = sceneAsset != null ? AssetDatabase.GetAssetPath(sceneAsset) : string.Empty;
		}

		// Re-cache the path from the assigned SceneAsset. Called by the owner's OnBeforeSerialize so a
		// moved/renamed scene stays correct. Returns true if the path changed.
		public bool RefreshPath () {
			string newPath = sceneAsset != null ? AssetDatabase.GetAssetPath(sceneAsset) : string.Empty;
			if (newPath == scenePath) return false;
			scenePath = newPath;
			return true;
		}
		#endif

		public override string ToString () => scenePath;
	}
}
