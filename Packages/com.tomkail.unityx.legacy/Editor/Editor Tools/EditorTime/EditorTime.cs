using UnityEngine;
using UnityEditor;
using System.Collections;

public class EditorTime {

	public static float time {
		get {
			return Time.realtimeSinceStartup;
		}
	}
	
	public static float deltaTime {get; private set;}
	public static int frames {get; private set;}
	
	private static float lastTime;
	
	[InitializeOnLoadMethod]
	static void SubscribeEditorEvents () {
		lastTime = time;
		deltaTime = 0;
		EditorApplication.update -= Update;
		EditorApplication.update += Update;
		AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
		AssemblyReloadEvents.beforeAssemblyReload += UnsubscribeEditorEvents;
	}

	// Editor events outlive script assemblies, so unsubscribe before a code reload or the old handler keeps firing alongside the new one.
	static void UnsubscribeEditorEvents () {
		EditorApplication.update -= Update;
		AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
	}
	
	private static void Update () {
		deltaTime = time - lastTime;
		frames++;
		lastTime = Time.realtimeSinceStartup;
		Shader.SetGlobalFloat("_EditorTime", EditorTime.time);
	}
}
