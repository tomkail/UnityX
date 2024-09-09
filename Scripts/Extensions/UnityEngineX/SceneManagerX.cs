using UnityEngine.SceneManagement;

public static class SceneManagerX {

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
}
