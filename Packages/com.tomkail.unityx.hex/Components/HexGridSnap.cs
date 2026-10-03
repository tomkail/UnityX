using UnityEngine;

namespace UnityX.HexGrid {
	// Portable component: pins a transform to a hex cell and keeps it there across grid changes.
	//
	// The cell coordinate + facing direction are the authoritative data; the transform is derived from them.
	// That's what lets a tile keep its grid cell when a global grid property changes (swizzle, cell size, grid
	// rotation): we re-place the transform from the stored coord under the new grid, instead of re-deriving the
	// cell from a now-stale world position.
	//
	// Project-specific behaviour hooks in via three virtuals rather than living here:
	//   GetGrid()            - resolve the grid for custom find modes (base handles Parent).
	//   GetPlacementOffset() - extra world offset when placing (e.g. terrain height). Zero by default.
	//   OnPlaced()           - notify listeners after an edit-time placement.
	// See the game's SnapToGrid subclass for the Sea-Rising versions of these.
	[ExecuteInEditMode]
	public class HexGridSnap : MonoBehaviour {
	    [Tooltip("Place the transform on the stored cell's centre.")]
	    public bool position = true;
	    [Tooltip("Rotate the transform to face the stored direction.")]
	    public bool rotation = true;

	    public enum GridFindMode {
	        Master,
	        Parent,
	        Manual
	    }
	    [Tooltip("How the grid is found. Parent: the nearest WorldSpaceHexGrid on this object or its parents. Manual: the grid assigned below. Master: resolved by a project subclass (override GetGrid).")]
	    public GridFindMode gridFindMode;

	    [Space]
	    [SerializeField, Tooltip("The WorldSpaceHexGrid this object snaps to. Assign it in Manual mode; other modes fill it in.")]
	    WorldSpaceHexGrid _grid;
	    public WorldSpaceHexGrid grid {
	        get {
	            if(_grid == null) grid = GetGrid();
	            return _grid;
	        } private set {
	            #if UNITY_EDITOR
	            if(UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()) {
	                _grid = null;
	                return;
	            }
	            #endif
	            if(_grid == value) return;
	            _grid = value;
	            #if UNITY_EDITOR
	            if(!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
	            #endif
	        }
	    }

	    // The cell coordinate is the authoritative data; the transform is derived from it. _hasCoord distinguishes
	    // "authored on a cell" from "freshly added / migrated from an older scene", where we seed the coord from
	    // the current transform once.
	    [SerializeField, HideInInspector]
	    HexCoord _coord;
	    [SerializeField, HideInInspector]
	    int _directionIndex;
	    [SerializeField, HideInInspector]
	    bool _hasCoord;

	    #if UNITY_EDITOR
	    bool loaded = false;
	    bool canUseGridFunctions {
	        get {
	            return loaded && !UnityEditor.BuildPipeline.isBuildingPlayer && !UnityEditor.EditorApplication.isCompiling && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
	        }
	    }
	    protected virtual void OnEnable () {
	        loaded = true;
	        // Capture the cell now, while the grid is still in its authored state.
	        if(!Application.isPlaying) EnsureCoordInitialized();
	    }
	    public virtual void OnValidate () {
	        // canUseGridFunctions checks `loaded` (set in OnEnable), guarding against OnValidate firing during
	        // deserialization/domain-reload before the editor stage system is ready.
	        if(!canUseGridFunctions) return;
	        if(UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) return;
	        // Re-place from the stored cell so toggling position/rotation (or any inspector edit) snaps the
	        // transform back onto the authoritative coord rather than re-reading world space.
	        ReapplyFromStoredCoord();
	        if(!Application.isPlaying && UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this)) {
	            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
	        }
	    }
	    #endif

	    // Resolve the grid for the current find mode. Base handles Parent (and returns null for modes it doesn't
	    // know). Override to add project find modes such as a global singleton, then fall back to base.GetGrid().
	    protected virtual WorldSpaceHexGrid GetGrid () {
	        if(gridFindMode == GridFindMode.Parent) {
	            return GetComponentInParent<WorldSpaceHexGrid>();
	        }
	        return null;
	    }

	    // Seed the authoritative coord/direction from the current transform the first time we have a grid.
	    void EnsureCoordInitialized () {
	        if(_hasCoord) return;
	        if(grid == null) grid = GetGrid();
	        if(grid == null) return;
	        _coord = grid.WorldToAxial(transform.position);
	        _directionIndex = HexCoord.ClosestDirectionIndex(grid.RotationToHexCoordDirection(transform.rotation));
	        _hasCoord = true;
	    }

	    // Authoritative setter: records the tile as living on `coord` and updates the transform to match.
	    public void SnapToCoord (HexCoord coord) {
	        _coord = coord;
	        _hasCoord = true;
	        ApplyToTransform();
	    }

	    // Re-derive the transform from the stored coord/direction under the current grid.
	    public void ReapplyFromStoredCoord () {
	        EnsureCoordInitialized();
	        ApplyToTransform();
	    }

	    void ApplyToTransform () {
	        if(grid == null) grid = GetGrid();
	        if(grid == null) {
	            Debug.LogWarning("No grid!", this);
	            return;
	        }
	        if(position) {
	            SetTransformPosition(grid, _coord);
	        }
	        if(rotation) {
	            transform.rotation = grid.HexCoordDirectionIndexToRotation(_directionIndex);
	        }
	        if(!Application.isPlaying) {
	            OnPlaced(_coord);
	        }
	    }

	    // Extra world-space offset added to the cell centre when placing (e.g. lifting a tile to terrain height).
	    // Zero by default; override in a subclass. Called for both play-mode and edit-time placement.
	    protected virtual Vector3 GetPlacementOffset (HexCoord coord) => Vector3.zero;

	    // Called after an edit-time placement. Override to notify listeners (base does nothing).
	    protected virtual void OnPlaced (HexCoord coord) {}

	    void SetTransformPosition (WorldSpaceHexGrid grid, HexCoord coord) {
	        transform.position = grid.AxialToWorld(coord) + GetPlacementOffset(coord);
	    }

	    public HexCoord GetPosition () {
	        if(!_hasCoord) {
	            EnsureCoordInitialized();
	            if(!_hasCoord) return grid == null ? HexCoord.zero : grid.WorldToAxial(transform.position);
	        }
	        return _coord;
	    }
	    public void SetPosition (HexCoord coord) {
	        SnapToCoord(coord);
	    }
	    public HexCoord GetDirection () {
	        return HexCoord.Direction(GetDirectionIndex());
	    }
	    public int GetDirectionIndex () {
	        if(!_hasCoord) EnsureCoordInitialized();
	        return _directionIndex;
	    }
	    public void SetDirection (int directionIndex) {
	        _directionIndex = directionIndex;
	        _hasCoord = true;
	        if(grid == null) return;
	        transform.rotation = grid.HexCoordDirectionIndexToRotation(_directionIndex);
	    }
	    public void SetDirection (HexCoord direction) {
	        SetDirection(HexCoord.ClosestDirectionIndex(direction));
	    }
	}
}
