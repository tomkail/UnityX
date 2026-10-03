using UnityEngine;
using UnityEditor;
using UnityX.HexGrid;

// Keeps HexGridSnap tiles on their authored cell when a global grid property changes.
//
// Swizzle and cell size live on the sibling UnityEngine.Grid component, and orientation lives on the
// grid's Transform, so none of those edits trigger WorldSpaceHexGrid.OnValidate. Instead we listen to
// the editor-wide ObjectChangeEvents stream: whenever a Grid / WorldSpaceHexGrid component (or its
// Transform) has its properties changed, we re-derive every affected tile's transform from its stored
// coord. Before this, changing the grid left tiles at their old world positions, which re-bucketed them
// into different cells and scrambled the level.
static class GridChangeReapplier {
	[InitializeOnLoadMethod]
	static void SubscribeEditorEvents () {
		ObjectChangeEvents.changesPublished -= OnChangesPublished;
		ObjectChangeEvents.changesPublished += OnChangesPublished;
		AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
		AssemblyReloadEvents.beforeAssemblyReload += UnsubscribeEditorEvents;
	}

	// Editor events outlive script assemblies, so unsubscribe before a code reload or the old handler keeps firing alongside the new one.
	static void UnsubscribeEditorEvents () {
		ObjectChangeEvents.changesPublished -= OnChangesPublished;
		AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
	}

	static void OnChangesPublished (ref ObjectChangeEventStream stream) {
		if(Application.isPlaying) return;

		// The objects carrying the changed grids. A grid object can itself be a tile on another grid (a room
		// snapped onto a world grid): the user is moving that tile on purpose, so it is left where they put it.
		var changedObjects = new System.Collections.Generic.HashSet<GameObject>();
		for(int i = 0; i < stream.length; i++) {
			if(stream.GetEventType(i) != ObjectChangeKind.ChangeGameObjectOrComponentProperties) continue;
			stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var data);
			var obj = EditorUtility.EntityIdToObject(data.entityId);
			if(AffectsGrid(obj)) changedObjects.Add(((Component)obj).gameObject);
		}
		if(changedObjects.Count == 0) return;

		var tiles = Object.FindObjectsByType<HexGridSnap>(FindObjectsInactive.Include);
		foreach(var tile in tiles) {
			if(tile == null) continue;
			if(changedObjects.Contains(tile.gameObject)) continue;
			if(tile.grid == null) continue;
			if(UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(tile.gameObject) != null) continue;
			var t = tile.transform;
			var before = (t.position, t.rotation);
			Undo.RecordObject(t, "Reapply Tile To Grid");
			tile.ReapplyFromStoredCoord();
			// Only dirty what actually moved, so editing one grid doesn't mark every scene with tiles as changed.
			if((t.position, t.rotation) != before) EditorUtility.SetDirty(t);
		}
	}

	// True for anything whose change alters where cells land: the Grid component, the WorldSpaceHexGrid
	// wrapper, or the Transform they share (moving/rotating the grid).
	static bool AffectsGrid (Object obj) {
		// Fully-qualify UnityEngine.Grid: a project-global `Grid` type otherwise shadows it here.
		if(obj is UnityEngine.Grid || obj is WorldSpaceHexGrid) return true;
		if(obj is Transform t) {
			return t.GetComponent<UnityEngine.Grid>() != null || t.GetComponent<WorldSpaceHexGrid>() != null;
		}
		return false;
	}
}
