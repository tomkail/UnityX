using UnityEngine;
using UnityEngine.EventSystems;

public static class UIBehaviourX {
	
	public static RectTransform GetRectTransform (this UIBehaviour uiBehaviour) {
		Debug.Assert(uiBehaviour != null);
		return uiBehaviour.GetComponent<RectTransform>();
	}
	
	public static Canvas GetParentCanvas (this UIBehaviour uiBehaviour) {
		Debug.Assert(uiBehaviour != null);
		return uiBehaviour.transform.GetComponentInParent<Canvas>();
	}
}
