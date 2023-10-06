using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.UI;

[CustomEditor(typeof(ExtendedButton), true)]
[CanEditMultipleObjects]
public class ExtendedButtonEditor : ButtonEditor {

	SerializedProperty onDownProperty;
	SerializedProperty onUpProperty;
	SerializedProperty onEnterProperty;
	SerializedProperty onExitProperty;
	SerializedProperty onSelectProperty;
	SerializedProperty onDeselectProperty;

	protected override void OnEnable()
    {
        base.OnEnable();
		onDownProperty = serializedObject.FindProperty("onDown");
		onUpProperty = serializedObject.FindProperty("onUp");
		onEnterProperty = serializedObject.FindProperty("onEnter");
		onExitProperty = serializedObject.FindProperty("onExit");
		onSelectProperty = serializedObject.FindProperty("onSelect");
		onDeselectProperty = serializedObject.FindProperty("onDeselect");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        serializedObject.Update();
		EditorGUILayout.PropertyField(onDownProperty);
		EditorGUILayout.PropertyField(onUpProperty);
		EditorGUILayout.PropertyField(onEnterProperty);
		EditorGUILayout.PropertyField(onExitProperty);
		EditorGUILayout.PropertyField(onSelectProperty);
		EditorGUILayout.PropertyField(onDeselectProperty);
        serializedObject.ApplyModifiedProperties();
    }
}