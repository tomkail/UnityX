using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SceneGUIDrawer {
	static Dictionary<object, System.Action> drawActions = new Dictionary<object, System.Action>();
	static Dictionary<object, System.Action> drawOnceActions = new Dictionary<object, System.Action>();

	public static void DrawOnce (object obj, System.Action drawAction) {
		drawOnceActions[obj] = drawAction;
	}

	public static void StartDrawing (object obj, System.Action drawAction) {
		drawActions[obj] = drawAction;
	}

	public static void StopDrawing (object obj) {
		if(drawActions.ContainsKey(obj)) drawActions.Remove(obj);
	}

	static SceneGUIDrawer () {
		SceneView.duringSceneGui += OnSceneGUI;
	}

	static void OnSceneGUI (SceneView sceneView) {
		foreach(var drawAction in drawActions) {
			drawAction.Value();
		}
		foreach(var drawAction in drawOnceActions) {
			drawAction.Value();
		}
    }
}
