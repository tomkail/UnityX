using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoundRect)), CanEditMultipleObjects]
public class RoundRectEditor : Editor {

    public override void OnInspectorGUI() {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_cornerRadius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_fillColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_outlineColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Color"), new GUIContent("Tint color"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_outlineWidth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_outlineMode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_antiAliasWidth"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastTarget"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastPadding"));
        serializedObject.ApplyModifiedProperties();
    }
}