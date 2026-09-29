using UnityEngine;

// Small helpers to give Vector3Int the few conveniences Point3 had, so call sites can migrate off Point3.
public static class Vector3IntX {
	// x * y * z — cell count / volume for a 3D grid size. (Point3 had this as the `.area` property.)
	public static int Area (this Vector3Int v) {
		return v.x * v.y * v.z;
	}
}
