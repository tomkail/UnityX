using UnityEngine;
using UnityEngine.EventSystems;

public static class UIBehaviourX {
	
	// Convenience wrapper: a UIBehaviour's transform is always a RectTransform.
	public static RectTransform GetRectTransform (this UIBehaviour uiBehaviour) {
		Debug.Assert(uiBehaviour != null);
		return uiBehaviour.GetComponent<RectTransform>();
	}

	// Convenience wrapper over GetComponentInParent<Canvas>().
	public static Canvas GetParentCanvas (this UIBehaviour uiBehaviour) {
		Debug.Assert(uiBehaviour != null);
		return uiBehaviour.transform.GetComponentInParent<Canvas>();
	}
}
