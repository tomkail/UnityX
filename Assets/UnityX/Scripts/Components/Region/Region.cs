using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;

public class Region : MonoBehaviour {
	#if UNITY_EDITOR
	[SerializeField]
	Color _selectedFillColor = Color.red.WithAlpha(0.5f);
	public Color selectedFillColor {
		get {
			return _selectedFillColor;
		}
	}
	#endif
	public static List<Region> activeRegions = new List<Region>();
	public static Region GetRegionAtPosition (Vector3 position) {
		foreach(Region region in activeRegions) {
			if(region.ContainsPoint(position))
				return region;
		}
		return null;
	}

	public static List<Region> GetRegionsAtPosition (Vector3 position) {
		List<Region> validRegions = new List<Region>();
		foreach(Region region in activeRegions) {
			if(region.ContainsPoint(position))
				validRegions.Add(region);
		}
		return validRegions;
	}

	[SerializeField]
	Polygon _polygon;
	public Polygon polygon {
		get {
			return _polygon;
		} set {
			if(_polygon == value) return;
			_polygon = value;
			RebuildProperties();
		}
	}
	
	[SerializeField]
	float _height = 1;
	public float height {
		get {
			return _height;
		} set {
			_height = value;
			RebuildProperties();
		}
	}
	public bool in2DMode {
		get {
			return height <= 0 || height == Mathf.Infinity;
		}
	}

	public Vector3 worldNormal {
		get {
			return matrix.MultiplyVector(Vector3.forward).normalized;
		}
	}
	public Plane floorPlane {
		get {
			return new Plane(worldNormal, matrix.MultiplyPoint3x4(Vector3.zero));
		}
	}

	public Plane frontPlane {
		get {
			var cachedWorldNormal = worldNormal;
			return new Plane(cachedWorldNormal, matrix.MultiplyPoint3x4(Vector3.zero) + cachedWorldNormal * height * 0.5f);
		}
	}
	public Plane backPlane {
		get {
			var cachedWorldNormal = worldNormal;
			return new Plane(cachedWorldNormal, matrix.MultiplyPoint3x4(Vector3.zero) + cachedWorldNormal * -height * 0.5f);
		}
	}

	public Plane[] edgePlanes {
		get {
			var _verts3D = verts3D;
			Plane[] planes = new Plane[_verts3D.Length];
			var cachedWorldNormal = worldNormal;
			for(int i = 0; i < _verts3D.Length; i++) {
				var vertA = _verts3D[i];
				var vertB = _verts3D.GetRepeating(i+1);
				var normal = Vector3.Cross(Vector3X.FromTo(vertA, vertB).normalized, cachedWorldNormal);
				planes[i] = new Plane(normal, Vector3.Lerp(vertA, vertB, 0.5f));
			}
			return planes;
		}
	}

	[SerializeField]
	float localBoundsSqrRadius;
	
	// The bounding rect of the polygon in local space
	public Rect polygonRect {
		get {
			return _polygonRect;
		} private set {
			_polygonRect = value;
		}
	}
	[SerializeField]
	Rect _polygonRect;

	// World space AABB of the region.
	public Bounds bounds {
		get {
			var _matrix = matrix;
			return BoundsX.CreateEncapsulating(localBounds.GetVertices().Select(corner => _matrix.MultiplyPoint3x4(corner)));
		}
	}

	// The polygon rect (in local space) with height in the Z axis
	public Bounds localBounds {
		get {
			return _localBounds;
		} private set {
			_localBounds = value;
		}
	}
	[SerializeField]
	Bounds _localBounds;

	public Vector3[] verts3D {
		get {
			var _matrix = matrix;
			Vector3[] verts = new Vector3[polygon.vertices.Length];
			for(int i = 0; i < polygon.vertices.Length; i++) verts[i] = _matrix.MultiplyPoint3x4(polygon.vertices[i]);
			return verts;
		}
	}

	public void GetVerts3DNonAlloc (ref Vector3[] verts) {
		var _matrix = matrix;
		if(verts == null) {
			verts = new Vector3[polygon.vertices.Length];
		} else if(verts.Length != polygon.vertices.Length) {
			System.Array.Resize(ref verts, polygon.vertices.Length);
		}
		for(int i = 0; i < polygon.vertices.Length; i++) verts[i] = _matrix.MultiplyPoint3x4(polygon.vertices[i]);
	}

	[SerializeField]
	Matrix4x4 _offsetMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
	public Matrix4x4 offsetMatrix {
		get {
			return _offsetMatrix;
		} set {
			if(_offsetMatrix == value) return;
			_offsetMatrix = value;
			_matrixDirty = true;
		}
	}
	Matrix4x4 _matrix;
	public Matrix4x4 matrix {
		get {
			if(!_matrixDirty && transform.localToWorldMatrix != cachedLocalToWorldMatrix) _matrixDirty = true;
			if(_matrixDirty) RefreshMatrix();
			return _matrix;
		}
	}
	Matrix4x4 _inverseMatrix;
	public Matrix4x4 inverseMatrix {
		get {
			if(!_matrixDirty && transform.localToWorldMatrix != cachedLocalToWorldMatrix) _matrixDirty = true;
			if(_matrixDirty) RefreshMatrix();
			return _inverseMatrix;
		}
	}
	void RefreshMatrix () {
		_matrix = transform.localToWorldMatrix * offsetMatrix;
		_inverseMatrix = _matrix.inverse;
		cachedLocalToWorldMatrix = transform.localToWorldMatrix;
		_matrixDirty = false;
	}
	bool _matrixDirty = true;
	Matrix4x4 cachedLocalToWorldMatrix;
	
	public System.Action<Region> OnChange;

	private void Reset () {
		polygon = new Polygon(new Vector2[] {
			new Vector2(-0.5f, 0.5f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.5f, -0.5f),
			new Vector2(-0.5f, -0.5f),
		});
		height = 1;
	}

    public Vector3 PolygonSpaceToWorldPoint (Vector3 worldPoint) {
        return matrix.MultiplyPoint3x4(worldPoint);
    }
    public Vector3 WorldToPolygonSpacePoint (Vector3 worldPoint) {
        return inverseMatrix.MultiplyPoint3x4(worldPoint);
    }
    public Vector3 PolygonSpaceToWorldVector (Vector3 worldPoint) {
        return matrix.MultiplyVector(worldPoint);
    }
    public Vector3 WorldToPolygonSpaceVector (Vector3 worldPoint) {
        return inverseMatrix.MultiplyVector(worldPoint);
    }

	public bool ContainsPoint (Vector3 point) {
		// if(!x.polygonRegion.bounds.Contains(point)) 
		// 	return false;
		var polygonSpace = WorldToPolygonSpacePoint(point);
		return ContainsPolygonSpacePoint(polygonSpace);
	}

	public bool ContainsPolygonSpacePoint (Vector3 polygonSpace) {
		if(in2DMode) {
			return ContainsPolygonSpacePoint2D(polygonSpace);
		} else {
			return ContainsPolygonSpacePoint3D(polygonSpace);
		}
	}
	bool ContainsPolygonSpacePoint3D (Vector3 polygonSpace) {
		if(Vector3X.SqrDistance(localBounds.center, polygonSpace) > localBoundsSqrRadius) return false;
		if(!localBounds.Contains(polygonSpace)) return false;
		if(polygonSpace.z > height * 0.5f) return false;
		return polygon.ContainsPoint(polygonSpace);
	}
	bool ContainsPolygonSpacePoint2D (Vector2 polygonSpace) {
		if(!polygonRect.Contains(polygonSpace)) return false;
		return polygon.ContainsPoint(polygonSpace);
	}

	public Vector3 ClosestPointInRegion (Vector3 point) {
		var polygonSpace = WorldToPolygonSpacePoint(point);
		polygonSpace = polygon.FindClosestPointInPolygon(polygonSpace);
		var worldPointOnSurface = matrix.MultiplyPoint3x4(polygonSpace);
		if(!in2DMode) {
			var distanceFromSurface = floorPlane.GetDistanceToPoint(point);
			distanceFromSurface = Mathf.Clamp(distanceFromSurface, -height, height);
			worldPointOnSurface += floorPlane.normal * distanceFromSurface;
		}
		return worldPointOnSurface;
	}

	public Vector3 ClosestPointInRegionIgnoringHeight (Vector3 point) {
		var polygonSpace = WorldToPolygonSpacePoint(point);
		polygonSpace = polygon.FindClosestPointInPolygon(polygonSpace);
		var worldPointOnSurface = matrix.MultiplyPoint3x4(polygonSpace);
		var distanceFromSurface = floorPlane.GetDistanceToPoint(point);
		worldPointOnSurface += floorPlane.normal * distanceFromSurface;
		return worldPointOnSurface;
	}

	public Vector3 ClosestPointOnRegionEdge (Vector3 point) {
		var polygonSpace = WorldToPolygonSpacePoint(point);
		polygonSpace = polygon.FindClosestPointOnPolygon(polygonSpace);
		var worldPointOnSurface = matrix.MultiplyPoint3x4(polygonSpace);
		if(!in2DMode) {
			var distanceFromSurface = floorPlane.GetDistanceToPoint(point);
			distanceFromSurface = Mathf.Clamp(distanceFromSurface, -height, height);
			worldPointOnSurface += floorPlane.normal * distanceFromSurface;
		}
		return worldPointOnSurface;
	}

	public float SignedDistanceFromPoint (Vector3 position) {
		float distance = 0;
		var pointOnEdge = ClosestPointOnRegionEdge(position);
		if(in2DMode) {
			distance = Vector3X.DistanceAgainstDirection(position, pointOnEdge, floorPlane.normal);
		} else {
			distance = Vector3.Distance(position, pointOnEdge);
		}
		if(ContainsPoint(position)) distance = -distance;
		return distance;
	}
    
	public Vector3 GetRandomPointInRegion () {
		var worldPoint = PolygonSpaceToWorldPoint(polygon.GetRandomPointInPolygon());
		if(!in2DMode)
			worldPoint += floorPlane.normal * Random.Range(-height,height) * 0.5f; 
		return worldPoint;
	}

	// I've not built this, it's not easy.
	// Here's how it might be done:
	// Project each face (front, back; and the top, bottom, left, right sides) the region into 2D space polygons
	// Foreach of these polygons, check find overlap in 2D space.
	// That won't deal very well with depth though...
	// Perhaps I need to check each line for intersection?
	// public bool IntersectsRegion (Region region) {

	// }
	
    // Returns true if the line intersects any plane of the region
	public bool Linecast (Line3D line) {
        if(!in2DMode) {
            var planeIntersectionDistance = 0f; 
            if(backPlane.LineIntersectionPoint(line, out planeIntersectionDistance)) {
                var point = line.AtDistance(planeIntersectionDistance);
                var polygonSpace = WorldToPolygonSpacePoint(point);
                if(ContainsPolygonSpacePoint2D(polygonSpace))
                    return true;
            }
            if(frontPlane.LineIntersectionPoint(line, out planeIntersectionDistance)) {
                var point = line.AtDistance(planeIntersectionDistance);
                var polygonSpace = WorldToPolygonSpacePoint(point);
                if(ContainsPolygonSpacePoint2D(polygonSpace))
                    return true;
            }
        }

        var polygonSpaceStartPoint = WorldToPolygonSpacePoint(line.start);
        var polygonSpaceEndPoint = WorldToPolygonSpacePoint(line.end);
		Line polygonSpaceLine2D = new Line(polygonSpaceStartPoint, polygonSpaceEndPoint);
		Line3D polygonSpaceLine3D = new Line3D(polygonSpaceStartPoint, polygonSpaceEndPoint);
		Vector2 polygonSpaceIntersectionPoint;
        
        var polygonLines = polygon.GetLines();
        foreach(var polygonLine in polygonLines) {
			if(Line.LineIntersectionPoint(polygonLine, polygonSpaceLine2D, out polygonSpaceIntersectionPoint)) {
				var n = Line.GetNormalizedDistanceOnLine(polygonSpaceLine2D.start, polygonSpaceLine2D.end, polygonSpaceIntersectionPoint);
                if(in2DMode) return true;
                else {
                    var distanceFromSurface = Mathf.Abs(polygonSpaceLine3D.AtDistance(n * line.length).z);
                    if(distanceFromSurface < height * 0.5f) return true;
                }
			}
		}
        return false;
    }
	// Returning more than one point means we've gone through our region! 
    // NOTE: PLANE RESULTS ARE UNSORTED!
	public bool LineIntersectionPoints (Line3D line, ref List<Vector3> intersectionPoints) {
        intersectionPoints.Clear();

		if(!in2DMode) {
            var planeIntersectionDistance = 0f; 
            if(backPlane.LineIntersectionPoint(line, out planeIntersectionDistance)) {
                var point = line.AtDistance(planeIntersectionDistance);
                var polygonSpace = WorldToPolygonSpacePoint(point);
                if(ContainsPolygonSpacePoint2D(polygonSpace))
                    intersectionPoints.Add(point);
            }
            if(frontPlane.LineIntersectionPoint(line, out planeIntersectionDistance)) {
                var point = line.AtDistance(planeIntersectionDistance);
                var polygonSpace = WorldToPolygonSpacePoint(point);
                if(ContainsPolygonSpacePoint2D(polygonSpace))
                    intersectionPoints.Add(point);
            }
        }

        var polygonSpaceStartPoint = WorldToPolygonSpacePoint(line.start);
        var polygonSpaceEndPoint = WorldToPolygonSpacePoint(line.end);
		Line polygonSpaceLine2D = new Line(polygonSpaceStartPoint, polygonSpaceEndPoint);
		Line3D polygonSpaceLine3D = new Line3D(polygonSpaceStartPoint, polygonSpaceEndPoint);
		Vector2 polygonSpaceIntersectionPoint;
        
        var polygonLines = polygon.GetLines();
        foreach(var polygonLine in polygonLines) {
			if(Line.LineIntersectionPoint(polygonLine, polygonSpaceLine2D, out polygonSpaceIntersectionPoint)) {
				var n = Line.GetNormalizedDistanceOnLine(polygonSpaceLine2D.start, polygonSpaceLine2D.end, polygonSpaceIntersectionPoint);
                if(in2DMode) intersectionPoints.Add(line.AtDistance(n * line.length));
                else {
                    var distanceFromSurface = Mathf.Abs(polygonSpaceLine3D.AtDistance(n * line.length).z);
                    if(distanceFromSurface < height * 0.5f) intersectionPoints.Add(line.AtDistance(n * line.length));
                }
			}
		}
        return intersectionPoints.Count > 0;
    }

	public static int LineIntersectionNormalizedDistances (Matrix4x4 inverseMatrix, IEnumerable<Line> polygonLines, Line3D line, ref List<float> intersectionDistances) {
        Debug.LogWarning("TODO - integrate region height! See LineIntersectionPoints, although this that function is unsorted");
        intersectionDistances.Clear();
		Line line2D = new Line(inverseMatrix.MultiplyPoint3x4(line.start), inverseMatrix.MultiplyPoint3x4(line.end));
		Vector2 intersectionPoint;
		int num = 0;
		int i = 0;
		foreach(var polygonLine in polygonLines) {
			if(Line.LineIntersectionPoint(polygonLine, line2D, out intersectionPoint)) {
				var n = Line.GetNormalizedDistanceOnLine(line2D.start, line2D.end, intersectionPoint);
				if(i >= intersectionDistances.Count) intersectionDistances.Add(n);
				else intersectionDistances[i] = n;
				num++;
			}
			i++;
		}
		return num;
	}

    [System.Serializable]
    public struct RegionRaycastHit {
        public float distance;
        public Vector3 point;
    }
    public List<RegionRaycastHit> RayRegionIntersections(Vector3 rayOrigin, Vector3 rayDirection) {
        var polygonSpaceRayOrigin = WorldToPolygonSpacePoint(rayOrigin);
        var polygonSpaceRayDirection = WorldToPolygonSpaceVector(rayDirection);

        var hits = new List<RegionRaycastHit>();
        
        RegionRaycastHit hit = new RegionRaycastHit();

        // Add intersections with the planes
        if(!in2DMode) {
            hit.distance = backPlane.GetDistanceToPointInDirection(rayOrigin, rayDirection);
            if(hit.distance > 0) {
                hit.point = rayOrigin + rayDirection * hit.distance;
                var polygonSpace = WorldToPolygonSpacePoint(hit.point);
                if(ContainsPolygonSpacePoint2D(polygonSpace)) hits.Add(hit);
            }
            hit.distance = frontPlane.GetDistanceToPointInDirection(rayOrigin, rayDirection);
            if(hit.distance > 0) {
                hit.point = rayOrigin + rayDirection * hit.distance;
                var polygonSpace = WorldToPolygonSpacePoint(hit.point);
                if(ContainsPolygonSpacePoint2D(polygonSpace)) hits.Add(hit);
            }
        }
        
        // Add intersections with the polygon
        var polygonHits = polygon.RayPolygonIntersections(polygonSpaceRayOrigin, polygonSpaceRayDirection);
        foreach(var polygonHit in polygonHits) {
            hit.distance = polygonHit.distance;
            hit.point = rayOrigin + rayDirection * polygonHit.distance;
            bool add = false;
            if(in2DMode) add = true;
            else {
                var polygonSpacePoint = polygonSpaceRayOrigin + polygonSpaceRayDirection * polygonHit.distance;
                var distanceFromSurface = Mathf.Abs(polygonSpacePoint.z);
                if(distanceFromSurface < height * 0.5f) add = true;
            }
            // Insert the new hit so distance is maintained across hits
            if(add) {
                for(int i = 0; i < hits.Count; i++) {
                    if(hit.distance < hits[i].distance) {
                        hits.Insert(i, hit);
                        add = false;
                        break;
                    }
                }
                if(add) hits.Add(hit);
            }
        }

        return hits;
    }

	public Vector3 GetCenter () {
		return matrix.MultiplyPoint3x4(polygon.center);
	}

	// This must be called when the properties of the region are changed.
	[ContextMenu("Refresh")]
	public void OnPropertiesChanged () {
		RebuildProperties();
		if(OnChange != null) OnChange(this);
	}

	void RebuildProperties () {
		polygonRect = polygon.GetRect();
		// bounds = Bounds.Create
		localBounds = BoundsX.CreateEncapsulating(
			new Vector3(polygonRect.x, polygonRect.y, -height * 0.5f), 
			new Vector3(polygonRect.x, polygonRect.y, height * 0.5f),
			new Vector3(polygonRect.xMax, polygonRect.yMax, -height * 0.5f), 
			new Vector3(polygonRect.xMax, polygonRect.yMax, height * 0.5f)
		);
		localBoundsSqrRadius = localBounds.extents.sqrMagnitude;
	}
}