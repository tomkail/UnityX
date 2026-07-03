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
		Vector3 hitPoint = collider.transform.position;
		Vector3 direction = Vector3X.FromTo(from, collider.transform.position);
		RaycastHit[] raycastHits = Physics.RaycastAll(new Ray(from, direction), Vector3.Distance(from, collider.transform.position));
		foreach(var raycastHit in raycastHits) {
			if(raycastHit.collider == collider) {
				hitPoint = raycastHit.point;
				return hitPoint;
			}
		}
		return hitPoint;
	}
}