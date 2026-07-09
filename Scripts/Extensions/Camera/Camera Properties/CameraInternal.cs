using UnityEngine;

// NaN checks inlined from UnityX's Vector3X / QuaternionX so the Camera package doesn't depend on those
// Assembly-CSharp helpers. Bodies match the originals verbatim.
static class CameraInternal {
    public static bool HasNaN (Vector3 v) {
        return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
    }
    public static bool IsNaN (Quaternion q) {
        return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
    }

    // Inlined from PlaneX. Distance along the ray to the plane; 0 when the plane isn't hit in the ray's
    // forward direction (so callers never get a point behind the origin).
    public static float GetDistanceToPointInDirection (this Plane plane, Vector3 origin, Vector3 direction) {
        if (plane.Raycast(new Ray(origin, direction), out float distance))
            return distance;
        return 0;
    }
}
