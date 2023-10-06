using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ExtendedCanvasScaler), true)]
[CanEditMultipleObjects]
public class ExtendedCanvasScalerEditor : CanvasScalerEditor {

	SerializedProperty m_useCameraSizeInsteadOfScreenSize;

	protected override void OnEnable() {
		base.OnEnable();
		m_useCameraSizeInsteadOfScreenSize = serializedObject.FindProperty("m_useCameraSizeInsteadOfScreenSize");
	}

	public override void OnInspectorGUI () {
		serializedObject.Update();
		EditorGUILayout.PropertyField(m_useCameraSizeInsteadOfScreenSize);
		serializedObject.ApplyModifiedProperties();
		base.OnInspectorGUI ();
	}	
}
