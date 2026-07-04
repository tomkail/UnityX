using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Line3D {
	public Vector3 start;
	public Vector3 end;
	
	public Vector3 direction {
		get { return Vector3.Normalize(end-start); }
	}

	public Vector3 vector {
		get { return end - start; }
	}

	public float length {
		get { return Vector3.Distance(start, end); }
	}

	public float sqrLength {
		get { return SqrDistance(start, end); }
	}


	public Line3D (Vector3 _start, Vector3 _end) {
		start = _start;
		end = _end;
	}

	public Line3D (Vector3[] _startEnd) {
		start = _startEnd[0];
		end = _startEnd[1];
	}

	public void Set (Vector3 _start, Vector3 _end) {
		start = _start;
		end = _end;
	}

	public Vector3 AtDistance (float distance) {
		return start + direction * distance;
	}

	public override string ToString() {
		return "Start: " + start + " End: " + end;
	}

	public static Line3D Add(Line3D left, Line3D right){
		return new Line3D(left.start+right.start, left.end+right.end);
	}

	public static Line3D Subtract(Line3D left, Line3D right){
		return new Line3D(left.start-right.start, left.end-right.end);
	}

	public override bool Equals(System.Object obj) {
		// obj may be null or a different type; the pattern handles both.
		return obj is Line3D other && Equals(other);
	}

	public bool Equals(Line3D l) {
		// Return true if the fields match:
		return (start == l.start && end == l.end) || (start == l.end && end == l.start);
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * 31 + start.GetHashCode();
			hash = hash * 31 + end.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (Line3D left, Line3D right) {
		return (left.start == right.start && left.end == right.end) || (left.start == right.end && left.end == right.start);
	}

	public static bool operator != (Line3D left, Line3D right) {
		return !(left == right);
	}

	public static Line3D operator +(Line3D left, Line3D right) {
		return Add(left, right);
	}

	public static Line3D operator -(Line3D left, Line3D right) {
		return Subtract(left, right);
	}
	
	public static bool LineIntersectionPoint(Line3D line1, Line3D line2, out Vector3 intersectionPoint) {
		// Get A,B,C of first line - points : line1.start to line1.end
		float A1 = line1.end.y - line1.start.y;
		float B1 = line1.start.x - line1.end.x;
		float C1 = A1 * line1.start.x + B1 * line1.start.y;
		
		// Get A,B,C of second line - points : line2.start to line2.end
		float A2 = line2.end.y - line2.start.y;
		float B2 = line2.start.x - line2.end.x;
		float C2 = A2 * line2.start.x + B2 * line2.start.y;
		
		// Get delta and check if the lines are parallel
		float delta = A1*B2 - A2*B1;
		if(delta == 0) {
			intersectionPoint = Vector3.zero;
			return false;
		}
		
		// Solve the XY intersection point.
		float x = (B2*C1 - B1*C2)/delta;
		float y = (A1*C2 - A2*C1)/delta;

		// This only solves the XY projection, so interpolate z along line1 at the
		// solved XY parameter so the returned point actually lies on line1.
		// (Limitation: if the two lines don't truly intersect in 3D, this returns
		// the point on line1 above/below the XY crossing, not a true 3D closest point.)
		float dx = line1.end.x - line1.start.x;
		float dy = line1.end.y - line1.start.y;
		float t = ((x - line1.start.x) * dx + (y - line1.start.y) * dy) / (dx*dx + dy*dy);
		float z = line1.start.z + t * (line1.end.z - line1.start.z);

		// now return the Vector3 intersection point
		intersectionPoint = new Vector3(x, y, z);
		return true;
	}

	public float GetClosestSqrDistanceFromLine(Vector3 p, bool clamped = true) {
//			// Return minimum distance between line segment vw and point p
		return SqrDistance(p, GetClosestPointOnLine(p, clamped));
	}
	public float GetClosestDistanceFromLine(Vector3 p, bool clamped = true) {
//			// Return minimum distance between line segment vw and point p
		return Vector3.Distance(p, GetClosestPointOnLine(p, clamped));
	}

	public Vector3 GetClosestPointOnLine(Vector3 p, bool clamped = true) {
		// Consider the line extending the segment, parameterized as v + t (w - v).
		// We find projection of point p onto the line. 
		// It falls where t = [(p-v) . (w-v)] / |w-v|^2
		float t = GetNormalizedDistanceOnLine(p, clamped);
		// Projection falls on the segment
		return Vector3.LerpUnclamped(start, end, t);
	}

	public float GetNormalizedDistanceOnLine(Vector3 p, bool clamped = true) {
		return GetNormalizedDistanceOnLineInternal(start, end, p, sqrLength, clamped);
	}

	public float GetDistanceOnLine(Vector3 p, bool clamped = true) {
		return GetNormalizedDistanceOnLine(p, clamped) * length;
	}


	public static float GetClosestDistanceFromLine(Vector3 start, Vector3 end, Vector3 p) {
		return Vector3.Distance(p, GetClosestPointOnLine(start, end, p));
	}

	public static Vector3 GetClosestPointOnLine(Vector3 start, Vector3 end, Vector3 p, bool clamped = true) {
		float t = GetNormalizedDistanceOnLine(start, end, p, clamped);
		return Vector3.LerpUnclamped(start, end, t);
	}

	public static float GetNormalizedDistanceOnLine(Vector3 start, Vector3 end, Vector3 p, bool clamped = true) {
		float sqrLength = SqrDistance(start, end);
		return GetNormalizedDistanceOnLineInternal(start, end, p, sqrLength, clamped);
	}

	public static float GetDistanceOnLine(Vector3 start, Vector3 end, Vector3 p, bool clamped = true) {
		return GetNormalizedDistanceOnLine(start, end, p, clamped) * Vector3.Distance(start, end);
	}

	static float GetNormalizedDistanceOnLineInternal(Vector3 start, Vector3 end, Vector3 p, float sqrLength, bool clamped = true) {
		if (sqrLength == 0f) return 0;
		// Divide by length squared so that we can save on normalising (end-start), since
		// we're effectively dividing by the length an extra time.
		float n = Vector3.Dot(p - start, end - start) / sqrLength;
		if(!clamped) return n;
		return Mathf.Clamp01(n);
	}



	static float SqrDistance (Vector3 a, Vector3 b) {
		return (a.x-b.x) * (a.x-b.x) + (a.y-b.y) * (a.y-b.y) + (a.z-b.z) * (a.z-b.z);
	}
}
