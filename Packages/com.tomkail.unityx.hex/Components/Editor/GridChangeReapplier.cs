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
[InitializeOnLoad]
static class GridChangeReapplier {
	static GridChangeReapplier () {
		ObjectChangeEvents.changesPublished += OnChangesPublished;
	}

	static void OnChangesPublished (ref ObjectChangeEventStream stream) {
		if(Application.isPlaying) return;

		bool gridChanged = false;
		for(int i = 0; i < stream.length; i++) {
			if(stream.GetEventType(i) != ObjectChangeKind.ChangeGameObjectOrComponentProperties) continue;
			stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var data);
			var obj = EditorUtility.EntityIdToObject(data.entityId);
			if(AffectsGrid(obj)) {
				gridChanged = true;
				break;
			}
		}
		if(!gridChanged) return;

		var tiles = Object.FindObjectsByType<HexGridSnap>(FindObjectsInactive.Include);
		foreach(var tile in tiles) {
			if(tile == null) continue;
			if(UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(tile.gameObject) != null) continue;
			Undo.RecordObject(tile.transform, "Reapply Tile To Grid");
			tile.ReapplyFromStoredCoord();
			EditorUtility.SetDirty(tile.transform);
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
