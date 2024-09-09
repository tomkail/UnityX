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
    
    // public static Vector3 GetClosestPointOnSphere(Vector3 sphereCenter, float sphereRadius) {
    // 	Vector3 rayOriginToSphereCenter = sphereCenter - ray.origin;
    //     float rayOriginToSphereCenterLengthSquared = rayOriginToSphereCenter.sqrMagnitude;
    //     float sphereRadiusSquared = sphereRadius * sphereRadius;
    // 	float signedDistanceOnRay = Vector3.Dot(ray.direction, rayOriginToSphereCenter);
    // 	Debug.Log(rayOriginToSphereCenterLengthSquared +" "+signedDistanceOnRay);
    //     if(rayOriginToSphereCenterLengthSquared < sphereRadiusSquared) {
    // 		// if(signedDistanceOnRay < 0) return false;
    // 		float sqrDist = sphereRadiusSquared + signedDistanceOnRay * signedDistanceOnRay - rayOriginToSphereCenterLengthSquared;
    // 		// if (sqrDist < 0) return false;
    // 		Debug.Log(sqrDist);
    // 		signedDistanceOnRay -= Mathf.Sqrt(sqrDist);
    // 	}
        
    // 	return ray.origin+ray.direction*signedDistanceOnRay;
        
    // 	// Vector3 rayOriginToSphereCenter = sphereCenter - ray.origin;

    // 	// float signedDistanceOnRay = Vector3.Dot(rayOriginToSphereCenter, ray.direction);
        
        
    //     // float rayOriginToSphereCenterLengthSquared = rayOriginToSphereCenter.sqrMagnitude;
    //     // float sqrDist = sphereRadius * sphereRadius + signedDistanceOnRay * signedDistanceOnRay - rayOriginToSphereCenterLengthSquared;
    // 	// signedDistanceOnRay -= Mathf.Sqrt(sqrDist);

    //     // var pointOnRay = ray.origin + ray.direction * signedDistanceOnRay;
    // 	// return pointOnRay;

    //     // var pointOnRay = GetClosestPointOnRay(ray, sphere);
    // 	// var directionFromSphere = (pointOnRay-sphereCenter).normalized;
    // 	// return sphereCenter + directionFromSphere * Mathf.Min(directionFromSphere.magnitude, sphereRadius);
    // }

    public static float GetClosestDistanceToSphere(this Ray ray, Vector3 sphereCenter, float sphereRadius) {
        Vector3 rayOriginToSphereCenter = sphereCenter - ray.origin;
        float signedDistanceOnRay = Vector3.Dot(rayOriginToSphereCenter, ray.direction);
        return signedDistanceOnRay;
        
        // distanceOnRay = 0;
        // Vector3 rayOriginToSphereCenter = sphereCenter - ray.origin;
        // float rayOriginToSphereCenterLengthSquared = rayOriginToSphereCenter.sqrMagnitude;
        // float sphereRadiusSquared = sphereRadius * sphereRadius;
        // if(rayOriginToSphereCenterLengthSquared < sphereRadiusSquared) return true;
        // float signedDistanceOnRay = Vector3.Dot(ray.direction, rayOriginToSphereCenter);
        // if(signedDistanceOnRay < 0) return false;
        // float sqrDist = sphereRadiusSquared + signedDistanceOnRay * signedDistanceOnRay - rayOriginToSphereCenterLengthSquared;
        // if (sqrDist < 0) return false;
        // distanceOnRay = signedDistanceOnRay - Mathf.Sqrt(sqrDist);
        // return true;

        // Vector3 rayOriginToSphereCenter = sphereCenter - ray.origin;
        // float rayOriginToSphereCenterLengthSquared = rayOriginToSphereCenter.sqrMagnitude;
        // float sphereRadiusSquared = sphereRadius * sphereRadius;
        // float signedDistanceOnRay = Vector3.Dot(ray.direction, rayOriginToSphereCenter);
        // float sqrDist = sphereRadiusSquared + signedDistanceOnRay * signedDistanceOnRay - rayOriginToSphereCenterLengthSquared;
        // return ray.origin + ray.direction * (signedDistanceOnRay - Mathf.Sqrt(sqrDist));
    }
}
