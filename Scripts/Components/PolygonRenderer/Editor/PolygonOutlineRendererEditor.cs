using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PolygonOutlineRenderer)), CanEditMultipleObjects]
public class PolygonOutlineRendererEditor : BaseEditor<PolygonOutlineRenderer> {
	// Keys we registered with PolygonEditorTool, so we can unregister on disable.
	readonly List<object> editorKeys = new List<object>();

	public override void OnEnable() {
		base.OnEnable();
		Undo.undoRedoPerformed += HandleUndoRedoCallback;
		RegisterPolygonEditors();
	}

	void OnDisable() {
		Undo.undoRedoPerformed -= HandleUndoRedoCallback;
		foreach(var key in editorKeys) PolygonEditorTool.StopDrawing(key);
		editorKeys.Clear();
	}

	// Register a polygon editing instance per selected renderer. Editing happens in the
	// scene view while the "Edit Polygon" tool is active (Tools overlay, or the U shortcut).
	void RegisterPolygonEditors() {
		foreach(var t in targets) {
			var renderer = t as BasePolygonRenderer;
			if(renderer == null) continue;
			var instance = new PolygonEditorInstance(renderer.transform, renderer.offsetRotation) {
				undoTarget = renderer,
				GetPolygon = () => renderer.polygon,
				OnPolygonChanged = _ => renderer.OnPropertiesChanged(),
			};
			PolygonEditorTool.StartDrawing(renderer, instance);
			editorKeys.Add(renderer);
		}
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
		if(EditorGUI.EndChangeCheck()) data.OnPropertiesChanged();
		serializedObject.ApplyModifiedProperties();
	}

	void HandleUndoRedoCallback () {
		data.OnPropertiesChanged();
	}
}
