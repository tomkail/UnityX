using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIDrawer : MonoBehaviour {
	static Dictionary<object, System.Action> drawActions = new Dictionary<object, System.Action>();
	public static void StartDrawing (object obj, System.Action drawAction) {
		if(drawActions.ContainsKey(obj)) drawActions[obj] = drawAction;
		else drawActions.Add(obj, drawAction);
	}

	public static void StopDrawing (object obj) {
		if(drawActions.ContainsKey(obj)) drawActions.Remove(obj);
	}

	void OnGUI () {
		foreach(var drawAction in drawActions) {
			drawAction.Value();
		}
	}
}
