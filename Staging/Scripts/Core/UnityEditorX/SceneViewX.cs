#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

public static class SceneViewX {

	public static Vector2 GetMousePosition (this SceneView sceneView) {
		Vector2 mousePosition = Event.current.mousePosition;
		if (EditorApplicationX.IsRetina()) {
			mousePosition *= 2;
		}
		mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y;
		return mousePosition;
	}

	public static Vector3 WorldToGUI (this SceneView sceneView, Vector3 position) {
		Debug.Assert(sceneView != null, "Scene view is null!");

		Vector3 positionOnScreen = sceneView.camera.WorldToScreenPoint (position);
		Rect cameraRect = sceneView.camera.pixelRect;

		Vector3 positionOnGUI = new Vector3 (positionOnScreen.x, cameraRect.height - positionOnScreen.y, positionOnScreen.z);
		if (EditorApplicationX.IsRetina()) {
			positionOnGUI *= 0.5f;
		}
		return positionOnGUI;
	}
}
#endif