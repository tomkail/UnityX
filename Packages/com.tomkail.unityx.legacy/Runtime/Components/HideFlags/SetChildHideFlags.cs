using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class SetChildHideFlags : MonoBehaviour {
	public HideFlags childHideFlags;
	public void ApplySettings () {
		foreach(Transform child in transform) {
			if(child.gameObject.hideFlags != childHideFlags) child.gameObject.hideFlags = childHideFlags;
		}
	}
}
