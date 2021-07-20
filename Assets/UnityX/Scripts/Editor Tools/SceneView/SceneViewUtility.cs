using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A bunch of utilities for the scene view - useful for culling handles/gizmos for performance reasons.
public static class SceneViewUtility {
    #if UNITY_EDITOR
    static bool ready;
    static Plane[] frustumPlanes = new Plane[6];
    static Vector3 sceneViewCameraPosition;
    static bool sceneViewCameraIsOrthographic;
    public static float sceneViewCameraOrthographicSize;

    static float maxDistance = 1500;
    static float minOrthographicSizeFraction = 0.035f;

    static SceneViewUtility () {
        UnityEditor.SceneView.beforeSceneGui += BeforeSceneGUI;
    }

    static void BeforeSceneGUI (UnityEditor.SceneView sceneView) {
        ready = true;
        GeometryUtility.CalculateFrustumPlanes(UnityEditor.SceneView.currentDrawingSceneView.camera, frustumPlanes);
        sceneViewCameraIsOrthographic = UnityEditor.SceneView.currentDrawingSceneView.camera.orthographic;
        sceneViewCameraOrthographicSize = UnityEditor.SceneView.currentDrawingSceneView.camera.orthographicSize;
        sceneViewCameraPosition = UnityEditor.SceneView.currentDrawingSceneView.camera.transform.position;
    }

    public static bool IsVisibleInSceneView (Vector3 point) {
        return ready && 
        (sceneViewCameraIsOrthographic ? true : Vector3.Distance(sceneViewCameraPosition, point) < maxDistance)
         && 
        TestPlanesPoint(frustumPlanes, point);
    }
    public static bool IsVisibleInSceneView (Vector3 point, float radius) {
        return ready && 
        (sceneViewCameraIsOrthographic ? radius > sceneViewCameraOrthographicSize*minOrthographicSizeFraction : Vector3.Distance(sceneViewCameraPosition, point) < maxDistance)
         && 
        TestPlanesSphere(frustumPlanes, point, radius);
    }
    public static bool IsVisibleInSceneView (Bounds bounds) {
        return ready && 
        (sceneViewCameraIsOrthographic ? bounds.extents.magnitude > sceneViewCameraOrthographicSize*minOrthographicSizeFraction : Vector3.Distance(sceneViewCameraPosition, bounds.ClosestPoint(sceneViewCameraPosition)) < maxDistance)
         && 
        GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
    }

    public static bool TestPlanesPoint (Plane[] planes, Vector3 point) {
		for(int i = 0; i < 6; i++)
			if(Vector3.Dot(planes[i].normal, point) + planes[i].distance < 0) 
				return false;
		return true;
	}
    public static bool TestPlanesSphere (Plane[] planes, Vector3 point, float radius) {
		for(int i = 0; i < 6; i++)
			if(Vector3.Dot(planes[i].normal, point) + planes[i].distance + radius < 0) 
				return false;
		return true;
	}
    #endif
}