using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

// Scene-view polygon editing for the PolygonRenderer family: shown in the Tools overlay while any
// BasePolygonRenderer subclass is selected (component tool target types match derived classes).
[EditorTool("Edit Polygon", typeof(BasePolygonRenderer))]
class PolygonRendererPolygonEditorTool : PolygonEditorTool {
	protected override PolygonEditorInstance CreateInstance (Object target) {
		var renderer = (BasePolygonRenderer)target;
		return new PolygonEditorInstance(renderer.transform, renderer.offsetRotation) {
			undoTarget = renderer,
			GetPolygon = () => renderer.polygon,
			OnPolygonChanged = _ => renderer.OnPropertiesChanged(),
		};
	}

	protected override void UpdateInstance (Object target, PolygonEditorInstance instance) {
		instance.offsetMatrix = Matrix4x4.TRS(Vector3.zero, ((BasePolygonRenderer)target).offsetRotation, Vector3.one);
	}
}

// Shared editor for the PolygonRenderer family (PolygonRenderer / PolygonOutlineRenderer), which share the
// BasePolygonRenderer runtime base. The concrete subclasses below only supply the [CustomEditor] target type.
public abstract class BasePolygonRendererEditor<T> : Editor where T : BasePolygonRenderer {
	// BaseEditor<T> used to supply `data`; inlined here so this editor doesn't depend on it.
	protected T data => target as T;

	void OnEnable() {
		Undo.undoRedoPerformed += HandleUndoRedoCallback;
	}

	void OnDisable() {
		Undo.undoRedoPerformed -= HandleUndoRedoCallback;
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		if(GUI.changed && target != null) EditorUtility.SetDirty(target);
		if(EditorGUI.EndChangeCheck()) data.OnPropertiesChanged();
		serializedObject.ApplyModifiedProperties();
	}

	void HandleUndoRedoCallback () {
		data.OnPropertiesChanged();
	}
}
