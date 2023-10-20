using UnityEngine;

public static class ColliderX {

	/// <summary>
	/// Gets the closest point on the surface of a collider from a point.
	/// </summary>
	/// <returns>The closest point.</returns>
	/// <param name="collider">Collider.</param>
	/// <param name="from">From.</param>
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