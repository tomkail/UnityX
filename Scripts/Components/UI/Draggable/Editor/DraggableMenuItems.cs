using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class DraggableMenuItems {
	[MenuItem("GameObject/UI (Canvas)/Draggable", false, 2100)]
	static void CreateDraggable (MenuCommand menuCommand) {
		var go = new GameObject("Draggable", typeof(RectTransform), typeof(Image), typeof(Draggable));
		go.GetComponent<Draggable>().targetGraphic = go.GetComponent<Image>();

		var parent = GetOrCreateCanvasParent(menuCommand.context as GameObject);
		if (parent != null) GameObjectUtility.SetParentAndAlign(go, parent);
		go.name = GameObjectUtility.GetUniqueNameForSibling(go.transform.parent, go.name);

		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeGameObject = go;
	}

	// Parents under the right-clicked object if it's inside a canvas; otherwise finds a scene canvas,
	// or creates one via the built-in menu item (which also sets up an EventSystem).
	static GameObject GetOrCreateCanvasParent (GameObject context) {
		if (context != null && context.GetComponentInParent<Canvas>(true) != null) return context;
		var canvas = Object.FindAnyObjectByType<Canvas>();
		if (canvas == null) {
			EditorApplication.ExecuteMenuItem("GameObject/UI (Canvas)/Canvas");
			canvas = Object.FindAnyObjectByType<Canvas>();
		}
		return canvas != null ? canvas.gameObject : null;
	}
}
