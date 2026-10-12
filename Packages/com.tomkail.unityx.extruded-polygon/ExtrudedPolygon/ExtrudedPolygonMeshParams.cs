using System.Collections.Generic;
using UnityEngine;
using UnityX.Geometry;

// Input for ExtrudedPolygonMeshGenerator. The polygon lies in its own XY plane and is extruded along Z, from
// bottomHeight to topHeight; `rotation` and then `offsetMatrix` place the result.
[System.Serializable]
public class ExtrudedPolygonMeshParams {
	public Polygon polygon;
	// Leaves out the last side (from the last vertex to the one before it).
	public bool openSides;
	// Per side (side i runs from vertex i to vertex i-1): true leaves side i out of the first side pass, which draws
	// the back faces of a clockwise polygon and the front faces of an anticlockwise one.
	public bool[] ignoredFaces;
	// Holes cut out of the polygon. The generator doesn't read these; they're carried for components that do.
	public List<Polygon> holes = new List<Polygon>();

	public Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);

	// Per-vertex heights. Each array is used when its length matches the polygon's vertex count.
	public float[] topHeights;
	public float[] bottomHeights;
	public float topHeight = 1;
	public float bottomHeight = 0;

	public Color topColor = Color.white;
	public Color bottomColor = Color.white;
	public Color[] topColors;
	public Color[] bottomColors;

	public MeshDrawFaces topFaces = MeshDrawFaces.Front;
	public MeshDrawFaces bottomFaces = MeshDrawFaces.Front;
	public MeshDrawFaces sideFaces = MeshDrawFaces.Front;

	// Applied to every vertex first, then offsetMatrix. A mirroring offsetMatrix (negative determinant) flips the
	// winding so faces still point the same way.
	public Quaternion rotation = Quaternion.identity;
	public Matrix4x4 offsetMatrix = Matrix4x4.identity;
	public Matrix4x4 transformMatrix => offsetMatrix * Matrix4x4.Rotate(rotation);

	// Top and bottom triangles in the opposite vertex order (Sea-Rising's convention; ArcadeTactics' is the default).
	public bool reverseCapWinding;
	// Which two coordinates of the placed top/bottom vertex the cap UVs are taken from, normalised in the polygon's rect.
	public CapUVPlane capUVPlane = CapUVPlane.XZ;
	public enum CapUVPlane {
		XZ,
		XY,
	}

	// --- Slope-surface controls (read by Sea-Rising's HexSlopeMeshGenerator, not by ExtrudedPolygonMeshGenerator) ---
	// The top face can be built as a centre fan so the surface matches a height map (a centre vertex plus
	// interpolated corners). When hasTopCenterHeight is set, topCenterHeight is the fan's centre height; otherwise
	// the mean of topHeights is used.
	public bool hasTopCenterHeight;
	public float topCenterHeight;
	// Subdivision level for the Subdivided slope mode (0 = none).
	public int subdivisions = 2;
	// Per-edge target heights (length 6). A non-NaN entry fixes that edge flat at that height, so a vertical cliff
	// can be built where two adjacent fixed edges disagree. NaN leaves the edge following topHeights. Null disables cliffs.
	public float[] topEdgeHeights;
}
