using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public static class UIBehaviourX {
	
	public static RectTransform GetRectTransform (this UIBehaviour uiBehaviour) {
		Debug.Assert(uiBehaviour != null);
		return uiBehaviour.GetComponent<RectTransform>();
	}
	
	public static Canvas GetParentCanvas (this UIBehaviour uiBehaviour) {
		Debug.Assert(uiBehaviour != null);
		var canvas = uiBehaviour.transform.GetComponent<Canvas>();
		if(canvas != null) return canvas;
		canvas = uiBehaviour.transform.GetComponentInAncestors<Canvas>();
		return canvas;
	}
}
