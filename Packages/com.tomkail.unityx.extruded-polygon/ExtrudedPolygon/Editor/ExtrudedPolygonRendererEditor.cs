using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

// Scene-view polygon editing for ExtrudedPolygonRenderer: shown in the Tools overlay while one is selected.
[EditorTool("Edit Polygon", typeof(ExtrudedPolygonRenderer))]
class ExtrudedPolygonRendererPolygonEditorTool : PolygonEditorTool {
	protected override PolygonEditorInstance CreateInstance (Object target) {
		var renderer = (ExtrudedPolygonRenderer)target;
		if(!renderer.editable) return null;
		return new PolygonEditorInstance(renderer.transform, renderer.input.transformMatrix) {
			undoTarget = renderer,
			GetPolygon = () => renderer.input.polygon,
			OnPolygonChanged = _ => renderer.RebuildMesh(),
		};
	}

	// Keep the editing plane in sync with the renderer.
	protected override void UpdateInstance (Object target, PolygonEditorInstance instance) {
		instance.offsetMatrix = ((ExtrudedPolygonRenderer)target).input.transformMatrix;
	}
}

[CustomEditor(typeof(ExtrudedPolygonRenderer), true)]
[CanEditMultipleObjects]
public class ExtrudedPolygonRendererEditor : BaseEditor<ExtrudedPolygonRenderer> {
	public override void OnEnable() {
		base.OnEnable();
		Undo.undoRedoPerformed += HandleUndoRedoCallback;
	}

	void OnDisable() {
		Undo.undoRedoPerformed -= HandleUndoRedoCallback;
		EditorApplication.delayCall -= DoQueuedRebuild;
		_rebuildQueued = false;
	}

	public override VisualElement CreateInspectorGUI () {
		var root = new VisualElement();
		// FillDefaultInspector draws every serialized field (including subclass fields, since this editor
		// targets child classes too).
		InspectorElement.FillDefaultInspector(root, serializedObject, this);

		// Rebuild the mesh when a serialized input changes. Deferred off the binding/scheduler pass so we don't
		// mutate the object mid-update (that trips "Assertion failed" in RetainedMode.UpdateSchedulers), and
		// coalesced so a burst of edits rebuilds once. RebuildMesh regenerates the mesh in place, so the mesh
		// reference is unchanged and this doesn't re-trigger itself.
		root.TrackSerializedObjectValue(serializedObject, _ => QueueRebuild());

		return root;
	}

	bool _rebuildQueued;
	void QueueRebuild () {
		if(_rebuildQueued) return;
		_rebuildQueued = true;
		EditorApplication.delayCall += DoQueuedRebuild;
	}
	void DoQueuedRebuild () {
		_rebuildQueued = false;
		if(this == null) return; // editor destroyed before the deferred call ran
		RebuildAll();
	}

	void HandleUndoRedoCallback () {
		RebuildAll();
	}

	void RebuildAll () {
		foreach(var d in datas) if(d != null) d.RebuildMesh();
	}
}
