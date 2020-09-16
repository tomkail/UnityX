using UnityEditor;

/// <summary>
/// This script exits play mode whenever script compilation is detected during an editor update.
/// </summary>
[InitializeOnLoad]
public class ExitPlayModeOnScriptCompile {

	[System.Serializable]
	public class ExitPlayModeOnScriptCompileSettings : SerializedEditorSettings<ExitPlayModeOnScriptCompileSettings> {
		public bool enabled = true;
		const string enabledPath = "Tools/Exit Play Mode On Script Compile"; 
		[MenuItem(enabledPath)]
		private static void ToggleEnabled() {
			ExitPlayModeOnScriptCompileSettings.Instance.enabled = !ExitPlayModeOnScriptCompileSettings.Instance.enabled;
			ExitPlayModeOnScriptCompileSettings.Save();
		}
		[MenuItem (enabledPath, true)]
		public static bool ToggleEnabledValidate () {
			Menu.SetChecked(enabledPath, ExitPlayModeOnScriptCompileSettings.Instance.enabled);
			return true;
		}
	}

	static ExitPlayModeOnScriptCompile () {
		EditorApplication.update += OnEditorUpdate;
	}

	private static void OnEditorUpdate () {
		if (EditorApplication.isPlaying && EditorApplication.isCompiling) {
			UnityEngine.Debug.Log ("Exiting play mode due to script compilation.");
			EditorApplication.isPlaying = false;
		}
	}
}