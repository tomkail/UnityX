using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class PolygonEditorInstance {
	public Polygon polygon {
		get {
			if(GetPolygon == null) return null;
			return GetPolygon();
		}
	}
	public bool Valid {
		get {
			return polygon != null;
		}
	}

	public Transform transform;
	public Matrix4x4 offsetMatrix = Matrix4x4.identity;
	
	public Matrix4x4 matrix {
		get {
			var lossyScale = transform.lossyScale;
			if(lossyScale.x == 0) lossyScale.x = 1;//Debug.LogWarning("PolygonEditor: Transform's lossyScale.x is 0. This can lead to the editor not functioning correctly.");
			if(lossyScale.y == 0) lossyScale.y = 1;//Debug.LogWarning("PolygonEditor: Transform's lossyScale.y is 0. This can lead to the editor not functioning correctly.");
			if(lossyScale.z == 0) lossyScale.z = 1;//Debug.LogWarning("PolygonEditor: Transform's lossyScale.z is 0. This can lead to the editor not functioning correctly.");
			return Matrix4x4.TRS(transform.position, transform.rotation, lossyScale) * offsetMatrix;
		}
	}
	public Matrix4x4 directionMatrix {
		get {
			return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one) * offsetMatrix;
		}
	}
	
	public Plane plane {
		get {
			return new Plane(directionMatrix.MultiplyVector(Vector3.forward), matrix.MultiplyPoint3x4(Vector3.zero));
		}
	}
	

	public Vector2 WorldToPolygonPoint (Vector3 point) {
		return matrix.inverse.MultiplyPoint3x4(point);
	}

	public Vector3 PolygonToWorldPoint (Vector2 point) {
		return matrix.MultiplyPoint3x4(point);
	}

	public Vector3 PolygonToWorldDirection (Vector2 vector) {
		return matrix.MultiplyVector(vector);
	}

	public Component undoTarget;
	public Func<Polygon> GetPolygon;
	public Action<Polygon> OnPolygonChanged;
	public Func<Vector2[]> DefaultPolygonFunc;
	public Func<Polygon, bool> PolygonInvalidFunc;
	
	public bool closed = true;

	public bool drawPolygon = true;
	public bool drawEdgeNormals;
	public bool drawVertNormals;
	
	public bool forceSnapToPoint;
    
    // 0 is unenforced
    // 1 is clockwise
    // -1 is anticlockwise
	public int enforcedWindingOrder;
	
	
	/// <summary>
	/// Allows editing polygons in world space via the scene view.
	/// </summary>
	/// <param name="transform">Transform.</param>
	public PolygonEditorInstance (Transform transform) {
		this.transform = transform;
		PolygonInvalidFunc = (Polygon polygon) => {
			return polygon == null || polygon.vertices.Length < (closed ? 3 : 2);
		};
	}
	/// <summary>
	/// Allows editing polygons in world space via the scene view. Offset matrix allows the editing plane to be flipped to the XZ plane.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="offsetMatrix">Offset matrix.</param>
	public PolygonEditorInstance (Transform transform, Matrix4x4 offsetMatrix) : this(transform) {
		this.offsetMatrix = offsetMatrix;
	}
	public PolygonEditorInstance (Transform transform, Quaternion offsetRotation) : this(transform, Matrix4x4.TRS(Vector3.zero, offsetRotation, Vector3.one)) {}
	public PolygonEditorInstance (Transform transform, bool onXZPlane) : this(transform, onXZPlane ? Quaternion.Euler(new Vector3(90,0,0)) : Quaternion.identity) {}



	public static Vector2 SnapToPolygonInterval (Vector2 vector, float snapInterval) {
		vector.x = RoundToNearest(vector.x, snapInterval);
		vector.y = RoundToNearest(vector.y, snapInterval);
		return vector;
	}
	static float RoundToNearest(float newNum, float nearestValue){
		return Mathf.Round(newNum/nearestValue)*nearestValue;
	}

	public Vector3 SnapToWorldInterval (Vector3 vector, float snapInterval) {
		return PolygonToWorldPoint(SnapToPolygonInterval(WorldToPolygonPoint(vector), snapInterval));
	}

	public void InsertPoint (int index, Vector2 point) {
		var vertList = polygon.vertices.ToList();
		vertList.Insert(index, point);
		polygon.vertices = vertList.ToArray();
    }

	public void RemovePoint (int index) {
		var vertList = polygon.vertices.ToList();
		vertList.RemoveAt(index);
		polygon.vertices = vertList.ToArray();
    }

	public bool GetScreenPointIntersectingRegionPlane (Vector2 screenPoint, ref Vector3 point) {
		Ray ray = HandleUtility.GUIPointToWorldRay (screenPoint);
        float rayDistance;
		if (plane.Raycast(ray, out rayDistance)) {
			point = ray.GetPoint(rayDistance);
			return true;
		}
		return false;
    }
}