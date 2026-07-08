using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Shared editor for the PolygonRenderer family (PolygonRenderer / PolygonOutlineRenderer), which share the
// BasePolygonRenderer runtime base. The concrete subclasses below only supply the [CustomEditor] target type.
public abstract class BasePolygonRendererEditor<T> : Editor where T : BasePolygonRenderer {
	// BaseEditor<T> used to supply `data`; inlined here so this editor doesn't depend on it.
	protected T data => target as T;

	// Keys we registered with PolygonEditorTool, so we can unregister on disable.
	readonly List<object> editorKeys = new List<object>();

	void OnEnable() {
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
		DrawDefaultInspector();
		if(GUI.changed && target != null) EditorUtility.SetDirty(target);
		if(EditorGUI.EndChangeCheck()) data.OnPropertiesChanged();
		serializedObject.ApplyModifiedProperties();
	}

	void HandleUndoRedoCallback () {
		data.OnPropertiesChanged();
	}
}
