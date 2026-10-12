using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIDrawer : MonoBehaviour {
	static readonly Dictionary<object, System.Action> drawActions = new Dictionary<object, System.Action>();

	// Callers that didn't StopDrawing before exiting play mode would otherwise draw over destroyed objects next session.
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void ResetStatics () {
		drawActions.Clear();
	}
	public static void StartDrawing (object obj, System.Action drawAction) {
		if(drawActions.ContainsKey(obj)) drawActions[obj] = drawAction;
		else drawActions.Add(obj, drawAction);
	}

	public static void StopDrawing (object obj) {
		if(drawActions.ContainsKey(obj)) drawActions.Remove(obj);
	}

	void OnGUI () {
		// Snapshot the values: a drawAction may Start/StopDrawing (mutating the dict) mid-iteration.
		foreach(var drawAction in new List<System.Action>(drawActions.Values)) {
			drawAction();
		}
	}
}
