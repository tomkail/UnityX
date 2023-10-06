using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// This script saves the current project and scene (if there is one) whenever the Unity editor enters play mode.
/// </summary>
[InitializeOnLoad]
public class SaveAllOnEnterPlayMode {
	
	[System.Serializable]
	public class SaveAllOnEnterPlayModeSettings : SerializedEditorSettings<SaveAllOnEnterPlayModeSettings> {
		public bool enabled = true;
		const string enabledPath = "Tools/Save On Enter Play Mode"; 
		[MenuItem(enabledPath)]
		private static void ToggleEnabled() {
			SaveAllOnEnterPlayModeSettings.Instance.enabled = !SaveAllOnEnterPlayModeSettings.Instance.enabled;
			SaveAllOnEnterPlayModeSettings.Save();
		}
		[MenuItem (enabledPath, true)]
		public static bool ToggleEnabledValidate () {
			Menu.SetChecked(enabledPath, SaveAllOnEnterPlayModeSettings.Instance.enabled);
			return true;
		}
	}

	static SaveAllOnEnterPlayMode () {
		EditorApplication.playModeStateChanged += state => {
            if (state == PlayModeStateChange.ExitingEditMode) {
				if(AnySceneDirty()) {
					EditorSceneManager.SaveOpenScenes();
				}
				AssetDatabase.SaveAssets();
			}	
		};
	}


    static bool AnySceneDirty () {
        for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.loadedSceneCount; i++) {
            if(EditorSceneManager.GetSceneAt(i).isDirty) return true;
        }
        return false;
    }
}