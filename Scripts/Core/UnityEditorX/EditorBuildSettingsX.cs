#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class EditorBuildSettingsX {
	public static List<string> GetEnabledBuildSettingsPaths () {
		List<string> paths = new List<string>();
		foreach(var scene in EditorBuildSettings.scenes) {
			if(scene.enabled)
				paths.Add(scene.path);
		}
		return paths;
	}

	public static void AddToBuildSettings (params string[] paths) {
		EditorBuildSettingsScene[] newBuildSettings = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length + paths.Length];
		System.Array.Copy(EditorBuildSettings.scenes, newBuildSettings, EditorBuildSettings.scenes.Length);
		for(int i = 0; i < paths.Length; i++) {
			EditorBuildSettingsScene settings = new EditorBuildSettingsScene();
			settings.path = paths[i];
			settings.enabled = true;
			newBuildSettings[EditorBuildSettings.scenes.Length + i] = settings;
		}
		EditorBuildSettings.scenes = newBuildSettings;
	}
}
#endif