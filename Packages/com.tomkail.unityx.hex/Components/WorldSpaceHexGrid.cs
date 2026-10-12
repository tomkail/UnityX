using UnityEngine;

namespace UnityX.HexGrid {
// Deliberately NOT a singleton: any number of WorldSpaceHexGrids may exist at once. All per-grid state
// (the UnityEngine.Grid, transform, corner cache) is instance state, so grids are independent for
// position/geometry. The one shared piece is HexCoord's global offsetLayout/orientation convention - see
// the note there; concurrent grids must use the same convention. Code that needs "the primary grid" should
// hold an explicit reference (e.g. the project's MasterGrid), not look up a static instance.
[RequireComponent(typeof(UnityEngine.Grid))]
[DisallowMultipleComponent]
public class WorldSpaceHexGrid : MonoBehaviour {
    [Tooltip("The UnityEngine.Grid this wraps; normally the one on this GameObject. Must use the Hexagon cell layout.")]
    public UnityEngine.Grid grid;

    // Orientation of the grid plane in world space as a proper (right-handed) frame:
    //   axis * Vector3.up      == floorNormal      (straight out of the board)
    //   axis * Vector3.forward == in-plane forward (the grid's own forward, flattened onto the plane)
    // Derived from the Grid's transform, so it leans/rotates with the grid and reduces to identity for an
    // unrotated ground grid REGARDLESS of cell swizzle. It deliberately does NOT bake in the swizzle's
    // in-plane 90-degree turn: that only changes which cell sits where, which Unity's Grid already handles
    // inside CellToWorld. Baking it into 'axis' (as the previous version did) spuriously yawed everything
    // that orients against the grid whenever the swizzle wasn't XZY.
    public Quaternion axis {
        get {
            var normal = floorNormal;
            var forward = Vector3.ProjectOnPlane(grid.transform.forward, normal);
            if (forward.sqrMagnitude < 1e-6f) forward = Vector3.ProjectOnPlane(grid.transform.up, normal);
            return Quaternion.LookRotation(forward, normal);
        }
    }

    // World-space normal of the grid plane: where the cell's +Z (depth) axis points after the cell swizzle
    // and the grid's rotation. Correct for every swizzle, including reflective ones.
    public Vector3 floorNormal => grid.transform.rotation * UnityEngine.Grid.Swizzle(grid.cellSwizzle, Vector3.forward);

    public Plane floorPlane => new(floorNormal, grid.transform.position);

	
    // NOTES ON UNITY'S HEX GRID SYSTEM
    // Unity's coordinate system is Offset OddR; which is a pointy orientation
    // The various Swizzle functions rotate their grid 90 degrees in various directions, which can give the illusion of a flat orientation
    // If you need a flat orientation you should rotate the grid manually or use the Swizzle functions; but leave the as Offset OddR, since that's what Unity's LocalToCell/WorldToCell return.
    void Reset () {
	    // RequireComponent adds a Grid that defaults to Rectangle - this system needs Hexagon. Also auto-wire the
	    // grid reference so a freshly added component works without manual assignment.
	    if(grid == null) grid = GetComponent<UnityEngine.Grid>();
	    EnforceHexagonLayout();
    }

    void OnValidate () {
	    // Unity's LocalToCell/WorldToCell always report OddR (pointy) offset coords, whatever the swizzle - so
	    // HexCoord's offset<->axial conversions must use OddR to match. Flat layouts come from rotating/swizzling
	    // the grid transform, NOT from switching this to a Q-offset (see the NOTES above).
	    HexCoord.offsetLayout = HexCoord.Layout.OddR;
	    EnforceHexagonLayout();
    }

    // This system only works with a Hexagon cell layout; Rectangle/Isometric make WorldToCell return non-hex
    // coordinates. Unity's built-in Grid dropdown can't be disabled, so we snap it back to Hexagon whenever it's
    // something else. This covers add/load/recompile; immediate correction when the Grid's own dropdown is edited
    // (which doesn't fire this component's OnValidate) is handled by the editor guard, HexGridLayoutGuard.
    void EnforceHexagonLayout () {
	    var g = grid != null ? grid : GetComponent<UnityEngine.Grid>();
	    if(g != null && g.cellLayout != GridLayout.CellLayout.Hexagon) {
		    var was = g.cellLayout;
		    g.cellLayout = GridLayout.CellLayout.Hexagon;
		    Debug.LogWarning($"{name}: WorldSpaceHexGrid requires a Hexagon cell layout; changed it back from {was} to Hexagon.", this);
	    }
    }
    
    public HexCoord LocalToAxial (Vector3 localPosition) {
	    var offsetPos = grid.LocalToCell(localPosition);
        return HexCoord.OffsetToAxial(offsetPos.x, offsetPos.y);
    }
    public HexCoord WorldToAxial (Vector3 worldPosition) {
        var offsetPos = grid.WorldToCell(worldPosition);
        return HexCoord.OffsetToAxial(offsetPos.x, offsetPos.y);
    }
    public Vector3 AxialToWorldInterpolated (Vector2 fractionalCell) {
        var offsetPos = HexCoord.ToOddRInterpolated(fractionalCell);
        return grid.LocalToWorld(grid.CellToLocalInterpolated(offsetPos));
    }
    public Vector3 AxialToWorldVectorInterpolated (Vector2 fractionalCell) {
		return AxialToWorldInterpolated(fractionalCell) - AxialToWorldInterpolated(Vector2.zero);
    }
    public Vector3 AxialToWorld (HexCoord coord) {
		var offsetPos = HexCoord.AxialToOffset(coord);
		return grid.CellToWorld(new Vector3Int(offsetPos.x, offsetPos.y, 0));
    }
    public Vector3 AxialToWorldVector (HexCoord coord) {
		return AxialToWorld(coord) - AxialToWorld(HexCoord.zero);
    }
    public Vector3 AxialToLocal (HexCoord coord) {
	    var offsetPos = HexCoord.AxialToOffset(coord);
		return grid.CellToLocal(new Vector3Int(offsetPos.x, offsetPos.y, 0));
    }
    public Vector3 AxialToLocalVector (HexCoord coord) {
		return AxialToLocal(coord) - AxialToLocal(HexCoord.zero);
    }

    public float HexCoordDirectionToDegreesAgainstNormal (HexCoord coord) {
		// Express the hex step in the grid plane's own frame (up = plane normal), then measure its
		// angle about that normal. Uses the real world-space direction, so it's correct for any
		// plane/swizzle rather than assuming the grid lies on the world XZ plane.
		var inPlane = Quaternion.Inverse(axis) * AxialToWorldVector(coord);
		return Util.Degrees(Util.XZ(inPlane));
	}
    public Quaternion HexCoordDirectionIndexToRotation (int directionIndex) {
		return HexCoordDirectionToRotation(HexCoord.Direction(directionIndex));
	}
    public Quaternion HexCoordDirectionToRotation (HexCoord coord) {
		return DegreesToRotation(HexCoordDirectionToDegreesAgainstNormal(coord));
	}

	public Quaternion DegreesToRotation (float degrees) {
		return Util.Rotate(axis, Vector3.up * degrees);
	}

	public HexCoord RotationToHexCoordDirection (Quaternion rotation) {
		// True inverse of HexCoordDirectionToRotation: take the rotation's forward, measure its angle
		// about the plane normal in the SAME frame the forward direction uses, then pick the hex direction
		// whose own angle is closest. Using the identical angle convention on both sides makes this a
		// guaranteed round-trip for any swizzle/transform. (The old version fed a direction vector into
		// LocalToCell, which treats it as a position - not a rotation inverse at all.)
		var worldForward = rotation * Vector3.forward;
		var targetDegrees = Util.Degrees(Util.XZ(Quaternion.Inverse(axis) * worldForward));
		var best = HexCoord.Direction(0);
		var bestDelta = float.MaxValue;
		for (int i = 0; i < 6; i++) {
			var dir = HexCoord.Direction(i);
			var delta = Mathf.Abs(Mathf.DeltaAngle(targetDegrees, HexCoordDirectionToDegreesAgainstNormal(dir)));
			if (delta < bestDelta) { bestDelta = delta; best = dir; }
		}
		return best;
	}
	
	
	
	// public Quaternion Rotation () {
	// 	return Rotation(HexCoord.ClosestDirectionIndex(this));
	// }
	// public static Quaternion Rotation (int directionIndex) {
	// 	Quaternion rotation = Quaternion.LookRotation(Vector3.forward, HexCoord.Direction(directionIndex).DirectionVector());
	// 	if(HexCoord.orientation == HexCoord.Orientation.Flat) rotation = rotation.Rotate(new Vector3(0,0,30));
	// 	// HexCoord.orientation == HexCoord.Orientation.Flat ? 30 : 0
	// 	return rotation;
	// }

	// Corner offsets from a cell centre, indexed by HexCoord corner index. Identical for every cell in a
	// uniform grid, so they're computed once and reused. The cache is rebuilt only when something the offsets
	// actually depend on changes - cell size, swizzle, layout, or grid rotation/scale (grid position doesn't
	// affect offsets), which the key check below detects lazily, so no explicit invalidation is needed.
	Vector3[] _cornerWorldOffsets;   // world-space, by corner index
	Vector2[] _cornerPlaneOffsets;   // grid-plane 2D frame (x = axis right, y = axis forward), by corner index
	bool _cornerCacheValid;
	Vector3 _cornerKeyCellSize;
	GridLayout.CellSwizzle _cornerKeySwizzle;
	Quaternion _cornerKeyRotation;
	Vector3 _cornerKeyScale;
	HexCoord.Layout _cornerKeyLayout;

	void EnsureCornerCache () {
		var t = grid.transform;
		if (_cornerCacheValid
			&& _cornerKeyCellSize == grid.cellSize
			&& _cornerKeySwizzle == grid.cellSwizzle
			&& _cornerKeyRotation == t.rotation
			&& _cornerKeyScale == t.lossyScale
			&& _cornerKeyLayout == HexCoord.offsetLayout)
			return;

		// Corner c is the vertex where this cell meets the two neighbours that share it
		// (HexCoordsSharingCornerIndex). That shared vertex is the circumcentre of the three cell centres, which
		// for a hex grid is simply their centroid - one third of the way out along the sum of the two neighbour
		// vectors. Deriving it straight from real neighbour positions is canonical for any swizzle / layout /
		// rotation, and - unlike matching corners against a template hexagon - it is bijective by construction:
		// six distinct corners in correct rotational order, so the tile polygon is always a clean convex hexagon.
		// (The old template match could assign two corner indices to the same template vertex when the template's
		// orientation was ~30 degrees off the real neighbour directions, leaving a duplicated corner and a missing
		// one - the degenerate wedge that stretched a single triangle across the whole tile.)
		var center = AxialToWorld(HexCoord.zero);
		var inv = Quaternion.Inverse(axis);
		_cornerWorldOffsets = new Vector3[6];
		_cornerPlaneOffsets = new Vector2[6];
		for (int corner = 0; corner < 6; corner++) {
			var sharing = HexCoord.HexCoordsSharingCornerIndex(HexCoord.zero, corner);
			var worldOffset = ((AxialToWorld(sharing[0]) - center) + (AxialToWorld(sharing[1]) - center)) / 3f;
			_cornerWorldOffsets[corner] = worldOffset;
			var v = inv * worldOffset;
			_cornerPlaneOffsets[corner] = new Vector2(v.x, v.z);
		}

		_cornerKeyCellSize = grid.cellSize;
		_cornerKeySwizzle = grid.cellSwizzle;
		_cornerKeyRotation = t.rotation;
		_cornerKeyScale = t.lossyScale;
		_cornerKeyLayout = HexCoord.offsetLayout;
		_cornerCacheValid = true;
	}

	// World position of a cell corner: the cell centre plus the cached shared offset. Correct for any swizzle,
	// layout, cell size and grid transform. The offset is the centroid of the three cells meeting at the corner,
	// so it's exact for a gapless regular hex grid (what we use); a nonzero cell gap would push corners outward.
	public Vector3 GetCornerPosition (HexCoord coord, int corner) {
		EnsureCornerCache();
		return AxialToWorld(coord) + _cornerWorldOffsets[((corner % 6) + 6) % 6];
	}

	// The cell's six corner offsets from its centre, in the grid plane's own 2D frame (x = axis right,
	// y = axis forward) at the grid's real size. Swizzle/layout-general replacement for a hardcoded unit
	// hexagon: feed it to anything that lays a 2D hexagon onto the plane via `axis.Rotate((90,0,0))` and it
	// will match Unity's actual cells exactly. Returns the shared cached array - treat as read-only.
	public Vector2[] GetCornerOffsets2D () {
		EnsureCornerCache();
		return _cornerPlaneOffsets;
	}
	
	public Vector3 GetCornerPositionFloat (HexCoord coord, float corner) {
		corner = Mathf.Repeat(corner, 6);
		float frac = corner % 1;
		if (frac == 0) return GetCornerPosition(coord, Mathf.RoundToInt(corner));
		int start = Mathf.FloorToInt(corner);
		int end = Mathf.CeilToInt(corner);
		return Vector3.Lerp(GetCornerPosition(coord, start), GetCornerPosition(coord, end), frac);
	}

	public Vector3 GetEdgePosition (HexCoord coord, int edge) {
		// Edge midpoint = halfway to the neighbour across that edge. Use the world *vector* to the neighbour
		// (not its absolute position) so this stays correct when the grid origin isn't at the world origin.
		return AxialToWorld(coord) + AxialToWorldVector(HexCoord.Direction(edge)) * 0.5f;
	}
	public Vector3 GetEdgePositionFloat (HexCoord coord, float edge) {
		edge = Mathf.Repeat(edge, 6);
		float frac = edge % 1;
		int start = Mathf.FloorToInt(edge);
		int end = Mathf.CeilToInt(edge);
		return AxialToWorld(coord) + Vector3.Lerp(AxialToWorldVector(HexCoord.Direction(start)), AxialToWorldVector(HexCoord.Direction(end)), frac) * 0.5f;
	}
	
	public Vector3 GetWorldPositionOnCoordInCornerDirection (HexCoord coord, float corner, float normalizedDistanceFromCenterTowardsEdge = 1) {
		var centerPos = AxialToWorld(coord);
		var edgePos = GetCornerPositionFloat(coord, corner);
		return Vector3.Lerp(centerPos, edgePos, normalizedDistanceFromCenterTowardsEdge);
	}
	public Vector3 GetWorldPositionOnCoordInEdgeDirection (HexCoord coord, float direction, float normalizedDistanceFromCenterTowardsEdge = 1) {
		return GetWorldPositionOnCoordInCornerDirection(coord, DirectionToCorner(direction), normalizedDistanceFromCenterTowardsEdge);
	}



	public Vector3 GetRayHitPoint (Ray ray) {
		float distance;
		floorPlane.Raycast(ray, out distance);
		return ray.GetPoint(distance);
	}

	

	public static float DirectionToCorner (float direction) {
		// Fractional corner index at the midpoint of edge `direction`, in the unified corner convention:
		// edge e's two corners are at indices -e and -e-1, so their midpoint is -e-0.5.
		return -direction - 0.5f;
	}
	// public float EdgeToDirection (float edge) {

	// }
}
}