using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

// Used by `Tools/unityx check`: compiles every package's runtime code as a player build would, which catches
// editor-only API used outside `#if UNITY_EDITOR` (the normal editor compile can't see those errors).
public static class PlayerCompileCheck {
	public static void Run () {
		var settings = new ScriptCompilationSettings {
			target = EditorUserBuildSettings.activeBuildTarget,
			group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget),
			options = ScriptCompilationOptions.None,
		};
		var result = PlayerBuildInterface.CompilePlayerScripts(settings, "Temp/PlayerCompileCheck");
		var count = result.assemblies == null ? 0 : result.assemblies.Count;
		Debug.Log("PLAYERCOMPILE assemblies=" + count);
		EditorApplication.Exit(count > 0 ? 0 : 1);
	}
}
