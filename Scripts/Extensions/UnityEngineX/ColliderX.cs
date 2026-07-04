using UnityEngine;

public static class ColliderX {

	/// <summary>
	/// Closest point on the collider's surface. Thin convenience wrapper over Unity's
	/// Collider.ClosestPoint (2017.1+) — kept only so existing callers of this signature keep working.
	/// </summary>
	/// <param name="collider">The collider to test against.</param>
	/// <param name="from">The point to measure from.</param>
	public static Vector3 GetClosestPoint (Collider collider, Vector3 from) {
		Debug.Assert(collider != null, "Collider is null");
		return collider.ClosestPoint(from);
	}
}