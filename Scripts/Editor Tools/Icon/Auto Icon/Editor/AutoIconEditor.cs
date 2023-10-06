using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using UnityX.Editor.Icon;

[CustomEditor(typeof(AutoIcon))]
[CanEditMultipleObjects]
public class AutoIconEditor : BaseEditor<AutoIcon> {
	public override void OnEnable() {
		base.OnEnable();
		RefreshEnabled();
    }

	void OnDisable() {
		RefreshEnabled();
	}

	void RefreshEnabled () {
		foreach (var d in datas)
			if(d != null) d.Refresh();
	}

	public override void OnInspectorGUI () {
		serializedObject.Update();
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_customForSelected"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_defaultIconProperties"));
		if(data.customForSelected) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_selectedIconProperties"));
		}
		serializedObject.ApplyModifiedProperties();
		if (EditorGUI.EndChangeCheck()) {
			RefreshEnabled();
		}
	}
}