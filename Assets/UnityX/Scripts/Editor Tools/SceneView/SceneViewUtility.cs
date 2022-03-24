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
    public static float sceneViewCameraFieldOfView;

    static float minViewportSize = 0.05f;
    // static float maxDistance = 1100;
    static float minOrthographicSizeFraction = 0.04f;

    static SceneViewUtility () {
        UnityEditor.SceneView.beforeSceneGui += BeforeSceneGUI;
    }

    static void BeforeSceneGUI (UnityEditor.SceneView sceneView) {
        ready = true;
        GeometryUtility.CalculateFrustumPlanes(UnityEditor.SceneView.currentDrawingSceneView.camera, frustumPlanes);
        sceneViewCameraIsOrthographic = UnityEditor.SceneView.currentDrawingSceneView.camera.orthographic;
        sceneViewCameraOrthographicSize = UnityEditor.SceneView.currentDrawingSceneView.camera.orthographicSize;
        sceneViewCameraFieldOfView = UnityEditor.SceneView.currentDrawingSceneView.camera.fieldOfView;
        sceneViewCameraPosition = UnityEditor.SceneView.currentDrawingSceneView.camera.transform.position;
    }

    public static bool IsVisibleInSceneView (Vector3 point) {
        if(!ready) return false;
        if(sceneViewCameraIsOrthographic) {
            return TestPlanesPoint(frustumPlanes, point);
        } else {
            var distance = Vector3.Distance(sceneViewCameraPosition, point);
            if(DistanceAndDiameterToViewportSize(distance, 1, sceneViewCameraFieldOfView) < minViewportSize) return false;
            // if(distance > maxDistance) return false;
            return TestPlanesPoint(frustumPlanes, point);
        }
    }
    public static bool IsVisibleInSceneView (Vector3 point, float radius) {
        if(!ready) return false;
        if(sceneViewCameraIsOrthographic) {
            return radius > sceneViewCameraOrthographicSize*minOrthographicSizeFraction && TestPlanesSphere(frustumPlanes, point, radius);
        } else {
            var distance = Vector3.Distance(sceneViewCameraPosition, point);
            if(DistanceAndDiameterToViewportSize(distance, radius*2, sceneViewCameraFieldOfView) < minViewportSize) return false;
            // if(distance > maxDistance) return false;
            return TestPlanesSphere(frustumPlanes, point, radius);
        }
    }
    public static bool IsVisibleInSceneView (Bounds bounds) {
        if(!ready) return false;
        if(sceneViewCameraIsOrthographic) {
            return bounds.extents.magnitude > sceneViewCameraOrthographicSize*minOrthographicSizeFraction && GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
        } else {
            var distance = Vector3.Distance(sceneViewCameraPosition, bounds.center);
            var size = bounds.size;
            var maxDiameter = Mathf.Max(size.x, size.y, size.z);
            if(DistanceAndDiameterToViewportSize(distance, maxDiameter, sceneViewCameraFieldOfView) < minViewportSize) return false;
            // var closestPoint = bounds.ClosestPoint(sceneViewCameraPosition);
            // var distance = Vector3.Distance(sceneViewCameraPosition, closestPoint);
            // if(distance > maxDistance) return false;
            return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
        }
    }

    // Checks if a point is inside the space created from the bounding planes
    public static bool TestPlanesPoint (Plane[] planes, Vector3 point) {
		for(int i = 0; i < 6; i++)
			if(Vector3.Dot(planes[i].normal, point) + planes[i].distance < 0) 
				return false;
		return true;
	}

    // Checks if a sphere is inside the space created from the bounding planes
    public static bool TestPlanesSphere (Plane[] planes, Vector3 point, float radius) {
		for(int i = 0; i < 6; i++)
			if(Vector3.Dot(planes[i].normal, point) + planes[i].distance + radius < 0) 
				return false;
		return true;
	}



    //Get the screen size of an object in viewport size, given its distance and diameter.
    public static float DistanceAndDiameterToViewportSize(float distance, float diameter, float fieldOfView){
        
        float viewportHeight = (diameter * Mathf.Rad2Deg) / (distance * fieldOfView);
        return viewportHeight;
    }

    //Get the screen size of an object in pixels, given its distance and diameter.
    public static float DistanceAndDiameterToPixelSize(float distance, float diameter, float fieldOfView){
        
        float pixelSize = (diameter * Mathf.Rad2Deg * Screen.height) / (distance * fieldOfView);
        return pixelSize;
    }
    
    //Get the distance of an object, given its screen size in pixels and diameter.
    public static float PixelSizeAndDiameterToDistance(float pixelSize, float diameter, float fieldOfView){
    
        float distance = (diameter * Mathf.Rad2Deg * Screen.height) / (pixelSize * fieldOfView);
        return distance;
    }
    
    //Get the diameter of an object, given its screen size in pixels and distance.
    public static float PixelSizeAndDistanceToDiameter(float pixelSize, float distance, float fieldOfView){
    
        float diameter = (pixelSize * distance * fieldOfView) / (Mathf.Rad2Deg * Screen.height);
        return diameter;
    }
    #endif
}