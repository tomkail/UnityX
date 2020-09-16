using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridRenderer))]
public class GridRendererEditor : BaseEditor<GridRenderer> {
	public override void OnEnable() {
		base.OnEnable();
		Undo.undoRedoPerformed += HandleUndoRedoCallback;
	}		

	void OnDisable() {
		Undo.undoRedoPerformed -= HandleUndoRedoCallback;
	}

	public override void OnInspectorGUI () {
        EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
        if(EditorGUI.EndChangeCheck())
            data.Refresh();
	}

	void HandleUndoRedoCallback () {
        data.Refresh();
	}
}