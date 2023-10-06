using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MarkLayoutElementForRebuild : UIMonoBehaviour {
	public bool markForRebuildInUpdate;
	public bool forceImmediateRebuildInUpdate;
	void Update () {
		if(markForRebuildInUpdate) Mark();
		if(forceImmediateRebuildInUpdate) Force();
	}

	[ButtonAttribute("Mark", "Mark For Rebuild")]
	bool markProxy;
	void Mark () {
		LayoutRebuilder.MarkLayoutForRebuild (rectTransform);
	}

	[ButtonAttribute("Force", "Force Immediate Rebuild")]
	bool forceProxy;
	void Force () {
		LayoutRebuilder.ForceRebuildLayoutImmediate (rectTransform);
	}
}
