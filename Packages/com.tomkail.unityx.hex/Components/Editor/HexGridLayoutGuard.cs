using UnityEngine;
using UnityEditor;
using UnityX.HexGrid;

// Keeps any Grid paired with a WorldSpaceHexGrid on the Hexagon cell layout. Unity's built-in Grid inspector
// can't be disabled, so instead of preventing the edit we undo it: the instant the Grid's Cell Layout is changed
// to anything other than Hexagon, we snap it back and warn. Rectangle/Isometric would make WorldToCell return
// non-hex coordinates, silently breaking every coord conversion.
//
// The runtime backstop (add / load / recompile) lives in WorldSpaceHexGrid.Reset/OnValidate; this catches the
// case those miss - editing the Grid component's own dropdown, which doesn't fire the sibling's OnValidate.
[InitializeOnLoad]
static class HexGridLayoutGuard {
	static HexGridLayoutGuard () {
		ObjectChangeEvents.changesPublished += OnChangesPublished;
	}

	static void OnChangesPublished (ref ObjectChangeEventStream stream) {
		for(int i = 0; i < stream.length; i++) {
			if(stream.GetEventType(i) != ObjectChangeKind.ChangeGameObjectOrComponentProperties) continue;
			stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var data);
			var obj = EditorUtility.EntityIdToObject(data.entityId);

			// Fully-qualify UnityEngine.Grid: a project-global `Grid` type otherwise shadows it here.
			UnityEngine.Grid grid = obj as UnityEngine.Grid;
			if(grid == null && obj is Component component) grid = component.GetComponent<UnityEngine.Grid>();
			if(grid == null) continue;

			if(grid.GetComponent<WorldSpaceHexGrid>() == null) continue;
			if(grid.cellLayout == GridLayout.CellLayout.Hexagon) continue;

			var was = grid.cellLayout;
			Undo.RecordObject(grid, "Enforce Hexagon Layout");
			grid.cellLayout = GridLayout.CellLayout.Hexagon;
			EditorUtility.SetDirty(grid);
			Debug.LogWarning($"{grid.name}: WorldSpaceHexGrid requires a Hexagon cell layout; changed it back from {was} to Hexagon.", grid);
		}
	}
}
