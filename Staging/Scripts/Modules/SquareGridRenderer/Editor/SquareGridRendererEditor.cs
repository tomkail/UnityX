using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SquareGridRenderer))]
public class SquareGridRendererEditor : UnityEditor.Editor {

	SquareGridRenderer data => (SquareGridRenderer)target;

	void OnEnable() {
		Undo.undoRedoPerformed += HandleUndoRedoCallback;
	}

	void OnDisable() {
		Undo.undoRedoPerformed -= HandleUndoRedoCallback;
	}

	public override void OnInspectorGUI () {
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		if (GUI.changed && target != null) EditorUtility.SetDirty(target);
		if (EditorGUI.EndChangeCheck())
			data.Refresh();
	}

	void HandleUndoRedoCallback () {
		if (data != null)
			data.Refresh();
	}
}
