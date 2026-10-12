using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// This script saves the current project and scene (if there is one) whenever the Unity editor enters play mode.
/// </summary>
public class SaveAllOnEnterPlayMode {
	
	[FilePath("UserSettings/SaveAllOnEnterPlayModeSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	public class SaveAllOnEnterPlayModeSettings : UnityEditor.ScriptableSingleton<SaveAllOnEnterPlayModeSettings> {
		public bool enabled = true;
		const string enabledPath = "Tools/Save On Enter Play Mode";
		[MenuItem(enabledPath)]
		private static void ToggleEnabled() {
			instance.enabled = !instance.enabled;
			instance.Save(true);
		}
		[MenuItem (enabledPath, true)]
		public static bool ToggleEnabledValidate () {
			Menu.SetChecked(enabledPath, instance.enabled);
			return true;
		}
	}

	[InitializeOnLoadMethod]
	static void SubscribeEditorEvents () {
		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
		AssemblyReloadEvents.beforeAssemblyReload += UnsubscribeEditorEvents;
	}

	// Editor events outlive script assemblies, so unsubscribe before a code reload or the old handler keeps firing alongside the new one.
	static void UnsubscribeEditorEvents () {
		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
	}

	static void OnPlayModeStateChanged (PlayModeStateChange state) {
		if (state == PlayModeStateChange.ExitingEditMode && SaveAllOnEnterPlayModeSettings.instance.enabled) {
			if(AnySceneDirty()) {
				EditorSceneManager.SaveOpenScenes();
			}
			AssetDatabase.SaveAssets();
		}
	}


    static bool AnySceneDirty () {
        for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.loadedSceneCount; i++) {
            if(EditorSceneManager.GetSceneAt(i).isDirty) return true;
        }
        return false;
    }
}