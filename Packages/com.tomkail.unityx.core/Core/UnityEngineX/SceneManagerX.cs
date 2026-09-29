using System.Linq;
using UnityEngine.SceneManagement;

public static class SceneManagerX {

	public static Scene[] GetCurrentScenes () {
		Scene[] scenes = new Scene[SceneManager.sceneCount];
		for(int i = 0; i < scenes.Length; i++)
			scenes[i] = SceneManager.GetSceneAt(i);
		return scenes;
	}

	public static string[] GetCurrentSceneNames () {
		return GetCurrentScenes().Select(s => s.name).ToArray();
	}

	public static string[] GetCurrentScenePaths () {
		return GetCurrentScenes().Select(s => s.path).ToArray();
	}
}
