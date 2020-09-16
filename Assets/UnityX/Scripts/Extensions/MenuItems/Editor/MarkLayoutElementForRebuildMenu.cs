using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MarkLayoutElementForRebuildMenu {

	[MenuItem("CONTEXT/RectTransform/Mark Layout For Rebuild", true)]
	private static bool ValidateMarkLayoutForRebuild(MenuCommand command) {
		return true;
	}

	[MenuItem("CONTEXT/RectTransform/Mark Layout For Rebuild")]
	private static void MarkLayoutForRebuild(MenuCommand command) {
		RectTransform context = command.context as RectTransform;
		LayoutRebuilder.MarkLayoutForRebuild (context);
	}



	[MenuItem("CONTEXT/RectTransform/Force Rebuild Layout Immediate", true)]
	private static bool ValidateForceRebuildLayoutImmediate(MenuCommand command) {
		return true;
	}

	[MenuItem("CONTEXT/RectTransform/Force Rebuild Layout Immediate")]
	private static void ForceRebuildLayoutImmediate(MenuCommand command) {
		RectTransform context = command.context as RectTransform;
		LayoutRebuilder.ForceRebuildLayoutImmediate (context);
	}
}
