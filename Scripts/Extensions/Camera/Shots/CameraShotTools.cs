using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CameraShotTools {
	
	/// <summary>
	/// Checks to see if gameobjects are in the camera frustrum. Expensive - mostly for use in editor.
	/// </summary>
	/// <returns>The cast.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="gameObjects">Game objects.</param>
	public static List<GameObject> FrustumCast (Camera camera, params GameObject[] gameObjects) {
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
		List<GameObject> results = new List<GameObject>();
		foreach(GameObject go in gameObjects) {
			Renderer renderer = go.GetComponent<Renderer>();
			if(renderer == null) continue;
			if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
				results.Add (go);
		}
		return results;
	}
		
	private static Vector3 GetPointInScreenSpaceFromViewportCoord (Camera camera, Vector2 point, float drawDistance) {
		return camera.ViewportToWorldPoint(new Vector3(point.x, point.y, drawDistance));
	}
	
	public static void DrawLineInScreenSpaceFromViewportCoords (Camera camera, Vector2 a, Vector2 b, float drawDistance) {
		Gizmos.DrawLine(GetPointInScreenSpaceFromViewportCoord(camera, a, drawDistance), GetPointInScreenSpaceFromViewportCoord(camera, b, drawDistance));
	}
	
	//
	// GIZMOS
	//
	public static void DrawSquareInScreenSpaceFromViewportRect (Camera camera, Rect rect, float drawDistance) {
		Vector3 origin = camera.ViewportToWorldPoint(new Vector3(rect.center.x, rect.center.y, drawDistance));
		Vector3 scale = camera.ViewportToWorldPoint(new Vector3(rect.xMin, rect.yMin, drawDistance)) - camera.ViewportToWorldPoint(new Vector3(rect.xMax, rect.yMax, drawDistance));
		Vector2 size = camera.transform.InverseTransformDirection(scale);

		var halfSize = size * 0.5f;
		Vector3 topLeft = origin + camera.transform.rotation * new Vector3(-halfSize.x, halfSize.y, 0);
		Vector3 topRight = origin + camera.transform.rotation * new Vector3(halfSize.x, halfSize.y, 0);
		Vector3 bottomRight = origin + camera.transform.rotation * new Vector3(halfSize.x, -halfSize.y, 0);
		Vector3 bottomLeft = origin + camera.transform.rotation * new Vector3(-halfSize.x, -halfSize.y, 0);
		
		Gizmos.DrawLine(topLeft, topRight);
		Gizmos.DrawLine(topRight, bottomRight);
		Gizmos.DrawLine(bottomRight, bottomLeft);
		Gizmos.DrawLine(bottomLeft, topLeft);
	}
	
	// public static void DrawPolygonInScreenSpaceFromViewportVertices (Camera camera, Vector2[] vertices, float drawDistance) {
	// 	Vector3[] worldSpaceVertices = new Vector3[vertices.Length];
	// 	for(int i = 0; i < worldSpaceVertices.Length; i++) GetPointInScreenSpaceFromViewportCoord(camera, vertices[i], drawDistance);
	// 	for(int i = 0; i < points.Count; i++) Gizmos.DrawLine(points.worldSpaceVertices(i), points.worldSpaceVertices(i+1));
	// }
}