using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Region)), CanEditMultipleObjects]
public class RegionEditor : BaseEditor<Region> {
	PolygonEditorHandles polygonEditor;

	public override void OnEnable() {
		base.OnEnable();
		Undo.undoRedoPerformed += HandleUndoRedoCallback;
		polygonEditor = new PolygonEditorHandles(data.transform, true);
	}		

	void OnDisable() {
		Undo.undoRedoPerformed -= HandleUndoRedoCallback;
        polygonEditor.Destroy();
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
		if(EditorGUI.EndChangeCheck()) data.OnPropertiesChanged();
		serializedObject.ApplyModifiedProperties();
	}

	void OnSceneGUI () {
		Undo.RecordObject(data, "Edit polygon");
		polygonEditor.OnSceneGUI(data.polygon);
		
		if(!polygonEditor.editing) {
			var handleSize = HandleUtility.GetHandleSize(data.transform.position) * 0.2f;
			Vector3 newPos = Handles.FreeMoveHandle(data.transform.position + data.transform.up * data.height * 0.5f, data.transform.rotation, handleSize, Vector3.zero, ((int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType) => {
				rotation = Quaternion.LookRotation(data.transform.up, data.transform.forward);
				Handles.DrawDottedLine(data.transform.position, position, 5);
				Handles.ConeHandleCap(controlID, position, rotation, handleSize, eventType);
			}));
			data.height = Vector3X.DistanceInDirection(data.transform.position, newPos, data.transform.up) * 2;
		}
	}

	void HandleUndoRedoCallback () {
		data.OnPropertiesChanged();
	}

	[DrawGizmo(GizmoType.InSelectionHierarchy)]
	static void DrawGizmoForMyScript(Region region, GizmoType gizmoType) {
		region.OnPropertiesChanged();
	}
}