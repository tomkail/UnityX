using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SetChildHideFlags))]
public class SetChildHideFlagsEditor : BaseEditor<SetChildHideFlags> {

	public override void OnEnable () {
		base.OnEnable ();
		data.ApplySettings();
		Undo.undoRedoPerformed += OnDoUndoRedo;
	}

	public void OnDisable () {
		Undo.undoRedoPerformed -= OnDoUndoRedo;
	}

	void OnDoUndoRedo () {
		foreach(var t in targets) {
			(t as SetChildHideFlags)?.ApplySettings();
		}
	}

	public override void OnInspectorGUI () {
		EditorGUI.BeginChangeCheck();
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("childHideFlags"));
		serializedObject.ApplyModifiedProperties();
		if(EditorGUI.EndChangeCheck()) {
			foreach(var t in targets) {
				(t as SetChildHideFlags)?.ApplySettings();
			}
		}
	}
}