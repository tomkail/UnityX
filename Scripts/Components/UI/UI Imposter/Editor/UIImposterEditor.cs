using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIImposter))]
public class UIImposterEditor : UnityEditor.UI.RawImageEditor {
	public override void OnInspectorGUI () {
		serializedObject.Update();
		// RawImage
		EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
		// EditorGUILayout.PropertyField(serializedObject.FindProperty("renderTexture"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("outputParams"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("updateMode"));
		EditorGUILayout.Space();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Render")) {
			((UIImposter) target).Render();
		}
		if(GUILayout.Button("Resize To Fit")) {
			((UIImposter) target).ResizeToFit();
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.Space();
		serializedObject.ApplyModifiedProperties();
		base.OnInspectorGUI();
	}
}