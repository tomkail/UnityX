using UnityEngine;

public static class RayX {
    public static bool IntersectsSphere(this Ray ray, Vector3 sphereCenter, float sphereRadius) {
        Vector3 rayOriginToSphereCenter = sphereCenter - ray.origin;
        float rayOriginToSphereCenterLengthSquared = rayOriginToSphereCenter.sqrMagnitude;
        float sphereRadiusSquared = sphereRadius * sphereRadius;
        if(rayOriginToSphereCenterLengthSquared < sphereRadiusSquared) return true;
        float signedDistanceOnRay = Vector3.Dot(ray.direction, rayOriginToSphereCenter);
        if(signedDistanceOnRay < 0) return false;
        float sqrDist = sphereRadiusSquared + signedDistanceOnRay * signedDistanceOnRay - rayOriginToSphereCenterLengthSquared;
        if (sqrDist < 0) return false;
        return true;
    }

    public static bool IntersectsSphere(this Ray ray, Vector3 sphereCenter, float sphereRadius, out float distanceOnRay) {
        distanceOnRay = 0;
        Vector3 rayOriginToSphereCenter = sphereCenter - ray.origin;
        float rayOriginToSphereCenterLengthSquared = rayOriginToSphereCenter.sqrMagnitude;
        float sphereRadiusSquared = sphereRadius * sphereRadius;
        if(rayOriginToSphereCenterLengthSquared < sphereRadiusSquared) return true;
        float signedDistanceOnRay = Vector3.Dot(ray.direction, rayOriginToSphereCenter);
        if(signedDistanceOnRay < 0) return false;
        float sqrDist = sphereRadiusSquared + signedDistanceOnRay * signedDistanceOnRay - rayOriginToSphereCenterLengthSquared;
        if (sqrDist < 0) return false;
        distanceOnRay = signedDistanceOnRay - Mathf.Sqrt(sqrDist);
        return true;
    }

    /// <summary>Signed distance along the ray to the projection of the sphere's surface (distance to the projected centre minus the radius, clamped to >= 0).</summary>
    public static float GetClosestDistanceToSphere(this Ray ray, Vector3 sphereCenter, float sphereRadius) {
        Vector3 rayOriginToSphereCenter = sphereCenter - ray.origin;
        float signedDistanceOnRay = Vector3.Dot(rayOriginToSphereCenter, ray.direction);
        // Local fix: subtract the radius so this returns distance-to-surface, not distance-to-centre (clamped so points inside the sphere return 0).
        return Mathf.Max(signedDistanceOnRay - sphereRadius, 0f);
    }
}
