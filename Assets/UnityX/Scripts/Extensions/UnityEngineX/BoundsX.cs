using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class BoundsX {
	public static Bounds Lerp (Bounds start, Bounds end, float lerp) {
		return new Bounds(Vector3.Lerp(start.center, end.center, lerp), Vector3.Lerp(start.size, end.size, lerp));
    }

	public static Bounds MoveTowards (Bounds start, Bounds end, float maxDistanceDelta) {
		return new Bounds(Vector3.MoveTowards(start.center, end.center, maxDistanceDelta), Vector3.MoveTowards(start.size, end.size, maxDistanceDelta));
    }
	public static Bounds MoveTowards (Bounds start, Bounds end, float maxPositionDistanceDelta, float maxSizeDistanceDelta) {
		return new Bounds(Vector3.MoveTowards(start.center, end.center, maxPositionDistanceDelta), Vector3.MoveTowards(start.size, end.size, maxSizeDistanceDelta));
    }

	/// <summary>
	/// Creates new bounds that encapsulates a list of vectors.
	/// </summary>
	/// <param name="vectors">Vectors.</param>
	public static Bounds CreateEncapsulating (params Vector3[] vectors) {
		if(vectors == null) return new Bounds(Vector3.zero, Vector3.zero);
		var count = vectors.Length;
        if(count == 0) return new Bounds(Vector3.zero, Vector3.zero);
        Vector3 min = vectors[0];
        Vector3 max = vectors[0];
		foreach(var vector in vectors) {
            if(vector.x < min.x) min.x = vector.x;
            else if(vector.x > max.x) max.x = vector.x;

            if(vector.y < min.y) min.y = vector.y;
            else if(vector.y > max.y) max.y = vector.y;

            if(vector.z < min.z) min.z = vector.z;
            else if(vector.z > max.z) max.z = vector.z;
        }
        return CreateMinMax(min, max);
	}

    public static Bounds CreateEncapsulating (IList<Vector3> vectors) {
		if(vectors == null) return new Bounds(Vector3.zero, Vector3.zero);
		var count = vectors.Count;
        if(count == 0) return new Bounds(Vector3.zero, Vector3.zero);
        Vector3 min = vectors[0];
        Vector3 max = vectors[0];
        for (int i = 1; i < count; i++) {
            Vector3 vector = vectors[i];
            if (vector.x < min.x) min.x = vector.x;
            else if(vector.x > max.x) max.x = vector.x;

            if(vector.y < min.y) min.y = vector.y;
            else if(vector.y > max.y) max.y = vector.y;

            if(vector.z < min.z) min.z = vector.z;
            else if(vector.z > max.z) max.z = vector.z;
        }
        return CreateMinMax(min, max);
	}

	public static Bounds CreateEncapsulating (IEnumerable<Vector3> vectors) {
		if(vectors == null || !vectors.Any()) return new Bounds(Vector3.zero, Vector3.zero);
		Bounds bounds = new Bounds(vectors.First(), Vector3.zero);
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
		foreach(var vector in vectors) {
            if(vector.x < min.x) min.x = vector.x;
            else if(vector.x > max.x) max.x = vector.x;

            if(vector.y < min.y) min.y = vector.y;
            else if(vector.y > max.y) max.y = vector.y;

            if(vector.z < min.z) min.z = vector.z;
            else if(vector.z > max.z) max.z = vector.z;
        }
        return CreateMinMax(min, max);
	}

	public static Bounds CreateMinMax (Vector3 min, Vector3 max) {
        var size = max - min;
        var center = min + size * 0.5f;
		return new Bounds(center, size);
    }

	public static Bounds CreateEncapsulating (IEnumerable<Bounds> allBounds) {
		if(allBounds == null || !allBounds.Any()) return new Bounds(Vector3.zero, Vector3.zero);
		Bounds bounds = allBounds.First();
		foreach(var _bounds in allBounds)
			bounds.Encapsulate(_bounds);
		return bounds;
	}

	public static IEnumerable<Vector3> GetVertices(this Bounds bounds) {
		var min = bounds.min;
		var max = bounds.max;
		yield return min;
		yield return max;
		yield return new Vector3(min.x, min.y, max.z);
		yield return new Vector3(min.x, max.y, min.z);
		yield return new Vector3(max.x, min.y, min.z);
		yield return new Vector3(min.x, max.y, max.z);
		yield return new Vector3(max.x, min.y, max.z);
		yield return new Vector3(max.x, max.y, min.z);
	}

	public static Vector3 GetRandomPointInBounds (this Bounds bounds) {
		return new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z));
	}

	// From https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection
	// When clamped, if the corresponding point is inside the bounds, use the point rather than point on the edge of the bounds
	static bool IntersectLineInternal (this Bounds bounds, Vector3 startPoint, Vector3 endPoint, Vector3 direction, out float inDistance, out float outDistance) {
		float tymin, tymax, tzmin, tzmax;
		var invdir = direction.Reciprocal(); 
		var sign = new int[] {
			(invdir.x < 0).ToInt(),
			(invdir.y < 0).ToInt(),
			(invdir.z < 0).ToInt()
		};

		inDistance = ((sign[0] == 0 ? bounds.min : bounds.max).x - startPoint.x) * invdir.x; 
		outDistance = ((1-sign[0] == 0 ? bounds.min : bounds.max).x - startPoint.x) * invdir.x; 
		tymin = ((sign[1] == 0 ? bounds.min : bounds.max).y - startPoint.y) * invdir.y; 
		tymax = ((1-sign[1] == 0 ? bounds.min : bounds.max).y - startPoint.y) * invdir.y; 

	    if ((inDistance > tymax) || (tymin > outDistance)) 
	        return false; 
	    if (tymin > inDistance) 
	        inDistance = tymin; 
	    if (tymax < outDistance) 
	        outDistance = tymax; 
	 
		tzmin = ((sign[2] == 0 ? bounds.min : bounds.max).z - startPoint.z) * invdir.z; 
		tzmax = ((1-sign[2] == 0 ? bounds.min : bounds.max).z - startPoint.z) * invdir.z; 
	 
	    if ((inDistance > tzmax) || (tzmin > outDistance)) 
	        return false; 
	    if (tzmin > inDistance) 
	        inDistance = tzmin; 
	    if (tzmax < outDistance) 
	        outDistance = tzmax;
	    return true; 
	}

	public static bool IntersectRay (this Bounds bounds, Line3D line, out float inDistance, out float outDistance) {
		return IntersectLineInternal(bounds, line.start, line.end, line.direction, out inDistance, out outDistance);
	}

	public static bool IntersectRay (this Bounds bounds, Vector3 startPoint, Vector3 endPoint, out float inDistance, out float outDistance) {
		return IntersectLineInternal(bounds, startPoint, endPoint, Vector3X.NormalizedDirection(startPoint, endPoint), out inDistance, out outDistance);
	}

	public static bool IntersectRay (this Bounds bounds, Ray ray, out float inDistance, out float outDistance) {
		return IntersectLineInternal(bounds, ray.origin, ray.origin + ray.direction, ray.direction, out inDistance, out outDistance);
	}
}