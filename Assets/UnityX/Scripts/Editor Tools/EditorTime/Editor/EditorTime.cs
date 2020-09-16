using UnityEngine;
using UnityEditor;
using System.Collections;

[InitializeOnLoad]
public class EditorTime {

	public static float time {
		get {
			return Time.realtimeSinceStartup;
		}
	}
	
	public static float deltaTime {get; private set;}
	public static int frames {get; private set;}
	
	private static float lastTime;
	
	static EditorTime() {
		lastTime = time;
		deltaTime = 0;
		EditorApplication.update += Update;
	}
	
	private static void Update () {
		deltaTime = time - lastTime;
		frames++;
		lastTime = Time.realtimeSinceStartup;
		Shader.SetGlobalFloat("_EditorTime", EditorTime.time);
	}
}
