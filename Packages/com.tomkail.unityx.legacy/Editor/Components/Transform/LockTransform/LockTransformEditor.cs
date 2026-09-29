using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(LockTransform))]
public class LockTransformEditor : BaseEditor<LockTransform> {
	
	public override void OnInspectorGUI () {
		serializedObject.Update();
		
		EditorGUILayout.PropertyField(serializedObject.FindProperty("hideHandles"));

		serializedObject.ApplyModifiedProperties();
		HideTools(data != null ? (data.enabled && data.hideHandles) : false);
	}
	
	public override void OnEnable() {
		base.OnEnable();
		HideTools(data.enabled);
	}
	
	void OnDisable() {
		HideTools(false);
	}
	
	private void HideTools (bool hideTools = true) {
		Tools.hidden = hideTools;
	}
}