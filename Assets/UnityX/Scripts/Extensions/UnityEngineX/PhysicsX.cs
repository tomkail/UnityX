using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PhysicsX {
	
	// Like a spherecast, but only returns true if the entire sphere is occluded, not just any point on it
	// Works by raycasting around the circle projected by the sphere
	const int defaultFakeSphereCastRays = 5;
	public static bool InclusiveFakeSphereCast (Ray ray, float radius, float distance, int mask, int numRays = defaultFakeSphereCastRays) {
		return FakeSphereCastRays(ray, radius, numRays).All(r => Physics.Raycast(r, distance, mask));
	}

	public static IEnumerable<Ray> FakeSphereCastRays (Ray ray, float radius, int numRays = defaultFakeSphereCastRays) {
		yield return ray;
		// Subtract a ray for the center raycast, and then also clamp to make sure there's at least 3 casts
		numRays = Mathf.Max(numRays-1, 3);
		var rotation = Quaternion.LookRotation(ray.direction, Vector3.up);
		var intervalAngle = 360f/numRays;
		var angle = 0f;
		for(int i = 0; i < numRays; i++) {
			var newRay = new Ray(ray.origin + rotation * MathX.DegreesToVector2(angle) * radius, ray.direction);
			yield return newRay;
			angle += intervalAngle;
		}
	}


	public static bool InclusiveFakeConeCast (Ray ray, float radius, float distance, int mask, int numRays = defaultFakeSphereCastRays) {
		return FakeConeCastRays(ray, radius, distance, numRays).All(r => Physics.Raycast(r, distance, mask));
	}

	public static IEnumerable<Ray> FakeConeCastRays (Ray ray, float radius, float distance, int numRays = defaultFakeSphereCastRays) {
		yield return ray;
		// Subtract a ray for the center raycast, and then also clamp to make sure there's at least 3 casts
		numRays = Mathf.Max(numRays-1, 3);
		var rotation = Quaternion.LookRotation(ray.direction, Vector3.up);
		var intervalAngle = 360f/numRays;
		var angle = 0f;
		for(int i = 0; i < numRays; i++) {
			var targetPoint = ray.origin + ray.direction * distance + rotation * MathX.DegreesToVector2(angle) * radius;
			var newRay = new Ray(ray.origin, Vector3X.NormalizedDirection(ray.origin, targetPoint));
			yield return newRay;
			angle += intervalAngle;
		}
	}
}
