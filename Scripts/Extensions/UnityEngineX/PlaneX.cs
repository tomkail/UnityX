using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

public static class PlaneX {
	public static bool TryGetHitPoint (this Plane plane, Ray ray, ref Vector3 hitPoint) {
		float distance = 0;
		if(plane.Raycast(ray, out distance)) {
			hitPoint = ray.GetPoint(distance);
			return true;
		}
		return false;
	}
	public static Vector3 GetHitPoint (this Plane plane, Ray ray) {
		return ray.GetPoint(plane.GetDistanceToPointInDirection(ray));
	}
	
	public static Vector3 GetHitPoint(this Plane plane, Vector3 origin, Vector3 direction) { 
		return plane.GetHitPoint(new Ray(origin, direction));
	}

	public static float GetDistanceToPointInDirection(this Plane plane, Ray ray) { 
		float distance = 0;
		plane.Raycast(ray, out distance);
		return distance;
	}

	public static float GetDistanceToPointInDirection(this Plane plane, Vector3 origin, Vector3 direction) { 
		return plane.GetDistanceToPointInDirection(new Ray(origin, direction));
	}


	// https://stackoverflow.com/questions/5666222/3d-line-plane-intersection
	public static bool LineIntersectionPoint (this Plane plane, Line3D line, out float intersectionLineDistance) {
		var u = Vector3.Normalize(line.end - line.start);
		var dot = Vector3.Dot(plane.normal, u);
		if(Mathf.Abs(dot) > Mathf.Epsilon) {
			var planePoint = -plane.normal * plane.distance;
			var w = line.start - planePoint;
			intersectionLineDistance = -Vector3.Dot(plane.normal, w) / dot;
			if(intersectionLineDistance < 0 || intersectionLineDistance > line.length) return false;
			else return true;
		} else {
			// The segment is parallel to plane
			intersectionLineDistance = 0;
			return false;
		}
	}

	public static Plane GetBestFitPlane (IEnumerable<Vector3> points) {
		Vector3 total = Vector3.zero;
		int num = 0;
		foreach(var value in points) {
			total += value;
			num++;
		}
		var centroid = total/num;

	    // Calc full 3x3 covariance matrix, excluding symmetries:
		float xx = 0; float xy = 0; float xz = 0;
		float yy = 0; float yz = 0; float zz = 0;

	    foreach (var p in points) {
	        Vector3 r = p - centroid;
	        xx += r.x * r.x;
	        xy += r.x * r.y;
	        xz += r.x * r.z;
	        yy += r.y * r.y;
	        yz += r.y * r.z;
	        zz += r.z * r.z;
	    }

	    float det_x = yy*zz - yz*yz;
		float det_y = xx*zz - xz*xz;
		float det_z = xx*yy - xy*xy;

	    var det_max = Mathf.Max(det_x, det_y, det_z);
	    if(det_max <= 0f) {
			Debug.LogWarning("The points don't span a plane");
			return new Plane();
	    }

	    // Pick path with best conditioning:
	    Vector3 dir = Vector3.zero;
        if (det_max == det_x) {
			float a = (xz*yz - xy*zz) / det_x;
			float b = (xy*yz - xz*yy) / det_x;
			dir = new Vector3(1, a, b);
        } else if (det_max == det_y) {
			float a = (yz*xz - xy*zz) / det_y;
			float b = (xy*xz - yz*xx) / det_y;
			dir = new Vector3(a, 1, b);
        } else {
            float a = (yz*xy - xz*yy) / det_z;
			float b = (xz*xy - yz*xx) / det_z;
			dir = new Vector3(a, b, 1);
        }

		return new Plane(dir.normalized, centroid);
	}
}