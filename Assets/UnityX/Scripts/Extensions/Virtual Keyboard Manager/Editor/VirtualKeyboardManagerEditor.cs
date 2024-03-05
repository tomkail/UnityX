using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VirtualKeyboardManager))]
public class VirtualKeyboardManagerEditor : Editor {
	VirtualKeyboardManager virtualKeyboardManager;
	void OnEnable() {
		virtualKeyboardManager = (VirtualKeyboardManager) target;
	}

	public override bool RequiresConstantRepaint() => true;

	public override void OnInspectorGUI () {
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("editorSimulationMode"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("visualizeKeyboardAreaInEditor"));
		serializedObject.ApplyModifiedProperties();
		
		if (Application.isPlaying) {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.Separator();
			EditorGUILayout.EnumPopup(new GUIContent("Visibility State"), virtualKeyboardManager.visibilityState);
			EditorGUILayout.Slider(new GUIContent("Animation Progress"), virtualKeyboardManager.showAmount, 0, 1);
			EditorGUILayout.ObjectField(new GUIContent("Selected Input Field"), virtualKeyboardManager.selectedInputField, typeof(RectTransform), true);
			EditorGUI.EndDisabledGroup();
		}
	}
}