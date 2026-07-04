using UnityEngine;

public static class ColliderX {

	/// <summary>
	/// Approximate closest point on a collider: raycasts from `from` toward the collider's transform position and returns the hit point, or the collider's pivot if the ray misses.
	/// NOTE: this is not a true closest-surface-point — prefer Collider.ClosestPoint.
	/// </summary>
	/// <returns>The ray hit point on the collider, or the collider's pivot if the ray misses.</returns>
	/// <param name="collider">The collider to test against.</param>
	/// <param name="from">The point to measure from.</param>
	public static Vector3 GetClosestPoint (Collider collider, Vector3 from) {
		Debug.Assert(collider != null, "Collider is null");
		// Delegates to Unity's built-in Collider.ClosestPoint (available since 2017.1), which returns the true closest point on the collider surface.
		return collider.ClosestPoint(from);
	}
}