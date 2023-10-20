using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PolygonRenderer)), CanEditMultipleObjects]
public class PolygonRendererEditor : BaseEditor<PolygonRenderer> {
	PolygonEditorHandles polygonEditor;

	public override void OnEnable() {
		base.OnEnable();
		Undo.undoRedoPerformed += HandleUndoRedoCallback;
		polygonEditor = new PolygonEditorHandles(data.transform, data.offsetRotation);
	}		

	void OnDisable() {
		Undo.undoRedoPerformed -= HandleUndoRedoCallback;
        if(data == null) return;
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
		if(EditorGUI.EndChangeCheck()) data.OnPropertiesChanged();
		serializedObject.ApplyModifiedProperties();
	}

	protected override void OnMultiEditSceneGUI () {
		Undo.RecordObject(data, "Edit polygon");
		polygonEditor.OnSceneGUI(data.polygon);
	}

	void HandleUndoRedoCallback () {
		data.OnPropertiesChanged();
	}

	// [DrawGizmo(GizmoType.InSelectionHierarchy)]
	// static void DrawGizmoForMyScript(PolygonRenderer polygonRenderer, GizmoType gizmoType) {
	// 	polygonRenderer.OnPropertiesChanged();
	// }
}