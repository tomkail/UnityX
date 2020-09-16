using UnityEngine;
using System.Collections.Generic;
using UnityX.Geometry;

public class Region : MonoBehaviour {
	#if UNITY_EDITOR
	[SerializeField]
	Color selectedFillColor = Color.red.WithAlpha(0.5f);
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

	public Polygon polygon;
	
	[Info("If <= 0, height is ignored")]
	public float height = 40;
	public bool in2DMode {
		get {
			return height <= 0 || height == Mathf.Infinity;
		}
	}

	public Plane floorPlane {
		get {
			return new Plane(transform.up, transform.position);
		}
	}

	public Plane frontPlane {
		get {
			return new Plane(transform.up, transform.position + transform.up * height * 0.5f);
		}
	}
	public Plane backPlane {
		get {
			return new Plane(transform.up, transform.position + transform.up * -height * 0.5f);
		}
	}

	public Plane[] edgePlanes {
		get {
			var _verts3D = verts3D;
			Plane[] planes = new Plane[_verts3D.Length];
			for(int i = 0; i < _verts3D.Length; i++) {
				var vertA = _verts3D[i];
				var vertB = _verts3D.GetRepeating(i+1);
				var normal = Vector3.Cross(Vector3X.FromTo(vertA, vertB).normalized, transform.up);
				planes[i] = new Plane(normal, Vector3.Lerp(vertA, vertB, 0.5f));
			}
			return planes;
		}
	}

	[SerializeField, Disable]
	float sqrRadius;
	public Rect polygonRect;
	public Bounds bounds;

	public Vector3[] verts3D {
		get {
			Vector3[] verts = new Vector3[polygon.vertices.Length];
			for(int i = 0; i < polygon.vertices.Length; i++) verts[i] = matrix.MultiplyPoint3x4(polygon.vertices[i]);
			return verts;
		}
	}

	Matrix4x4 offsetMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
	public Matrix4x4 matrix {
		get {
			return Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale) * offsetMatrix;
		}
	}
	
	public System.Action<Region> OnChange;

	private void Reset () {
		Vector2[] verts = RectX.CreateFromCenter(Vector2.one, Vector2.one).GetVertices();
		polygon.vertices = verts;
	}

    public Vector3 PolygonSpaceToWorldPoint (Vector3 worldPoint) {
        return matrix.MultiplyPoint3x4(worldPoint);
    }
    public Vector3 WorldToPolygonSpacePoint (Vector3 worldPoint) {
        return matrix.inverse.MultiplyPoint3x4(worldPoint);
    }
    public Vector3 PolygonSpaceToWorldVector (Vector3 worldPoint) {
        return matrix.MultiplyVector(worldPoint);
    }
    public Vector3 WorldToPolygonSpaceVector (Vector3 worldPoint) {
        return matrix.inverse.MultiplyVector(worldPoint);
    }

	public bool ContainsPoint (Vector3 point) {
		if(!in2DMode) {
			if(Vector3X.SqrDistance(bounds.center, point) > sqrRadius) return false;
			if(!bounds.Contains(point)) return false;
			if(floorPlane.GetDistanceToPoint(point) > height * 0.5f) return false;
		}
		var polygonSpace = WorldToPolygonSpacePoint(point);
		if(in2DMode) {
			if(!polygonRect.Contains(polygonSpace)) return false;
		}
		return polygon.ContainsPoint(polygonSpace);
	}

	public Vector3 ClosestPointInRegion (Vector3 point) {
		var polygonSpace = WorldToPolygonSpacePoint(point);
		polygonSpace = polygon.FindClosestPointInPolygon(polygonSpace);
		var worldPointOnSurface = matrix.MultiplyPoint3x4(polygonSpace);
		if(!in2DMode) {
			var distanceFromSurface = floorPlane.GetDistanceToPoint(point);
			distanceFromSurface = Mathf.Clamp(distanceFromSurface, -height, height);
			worldPointOnSurface += transform.rotation * Vector3.up * distanceFromSurface;
		}
		return worldPointOnSurface;
	}

	public Vector3 ClosestPointOnRegionEdge (Vector3 point) {
		var polygonSpace = WorldToPolygonSpacePoint(point);
		polygonSpace = polygon.FindClosestPointOnPolygon(polygonSpace);
		var worldPointOnSurface = matrix.MultiplyPoint3x4(polygonSpace);
		if(!in2DMode) {
			var distanceFromSurface = floorPlane.GetDistanceToPoint(point);
			distanceFromSurface = Mathf.Clamp(distanceFromSurface, -height, height);
			worldPointOnSurface += transform.rotation * Vector3.up * distanceFromSurface;
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
    
    // Returns true if the line intersects any plane of the region
	public bool Linecast (Line3D line) {
        if(!in2DMode) {
            var planeIntersectionDistance = 0f; 
            if(backPlane.LineIntersectionPoint(line, out planeIntersectionDistance)) {
                var point = line.AtDistance(planeIntersectionDistance);
                var polygonSpace = WorldToPolygonSpacePoint(point);
                if(polygon.ContainsPoint(polygonSpace))
                    return true;
            }
            if(frontPlane.LineIntersectionPoint(line, out planeIntersectionDistance)) {
                var point = line.AtDistance(planeIntersectionDistance);
                var polygonSpace = WorldToPolygonSpacePoint(point);
                if(polygon.ContainsPoint(polygonSpace))
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
                if(polygon.ContainsPoint(polygonSpace))
                    intersectionPoints.Add(point);
            }
            if(frontPlane.LineIntersectionPoint(line, out planeIntersectionDistance)) {
                var point = line.AtDistance(planeIntersectionDistance);
                var polygonSpace = WorldToPolygonSpacePoint(point);
                if(polygon.ContainsPoint(polygonSpace))
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
                if(polygon.ContainsPoint(polygonSpace)) hits.Add(hit);
            }
            hit.distance = frontPlane.GetDistanceToPointInDirection(rayOrigin, rayDirection);
            if(hit.distance > 0) {
                hit.point = rayOrigin + rayDirection * hit.distance;
                var polygonSpace = WorldToPolygonSpacePoint(hit.point);
                if(polygon.ContainsPoint(polygonSpace)) hits.Add(hit);
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

	[ContextMenu("Refresh")]
	public void OnPropertiesChanged () {
		RebuildProperties();
		if(OnChange != null) OnChange(this);
	}

	void RebuildProperties () {
		polygonRect = polygon.GetRect();
		bounds = BoundsX.CreateEncapsulating(verts3D);
		sqrRadius = bounds.extents.sqrMagnitude;
	}

	#if UNITY_EDITOR
	void OnDrawGizmosSelected () {
		RebuildProperties();
		if(selectedFillColor.a < 0.01f) return;
		GizmosX.BeginColor(selectedFillColor);
		// GizmosX.DrawWireCube(bounds);
		// GizmosX.DrawPolygon(transform.position, transform.rotation.Rotate(new Vector3(90, 0, 0)), transform.lossyScale, polygon.vertices, true);
		GizmosX.DrawExtrudedPolygon(transform.position, transform.rotation.Rotate(new Vector3(90, 0, 0)), transform.lossyScale, height, polygon.vertices);
		GizmosX.DrawExtrudedWirePolygon(transform.position, transform.rotation.Rotate(new Vector3(90, 0, 0)), transform.lossyScale, height, polygon.vertices);
		GizmosX.EndColor();

		// GizmosX.DrawWireCube(bounds);
		// Gizmos.DrawWireSphere(bounds.center, Mathf.Sqrt(sqrRadius));
	}
	#endif
}