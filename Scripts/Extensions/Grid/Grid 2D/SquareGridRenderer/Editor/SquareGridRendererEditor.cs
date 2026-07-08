using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SquareGridRenderer))]
public class SquareGridRendererEditor : BaseEditor<SquareGridRenderer> {
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
        if(data != null)
            data.Refresh();
	}
}