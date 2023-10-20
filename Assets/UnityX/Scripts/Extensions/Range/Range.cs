using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple struct for specifying a range between two floats.
/// Has a property drawer for easy inspectorisification
/// </summary>
[Serializable]
public struct Range : IEquatable<Range>
{
	public float min;
	public float max;
	public float mid => 0.5f*(min+max);

	public float length => max - min;

	public Range negated => new Range(-max, -min);


	public static readonly Range infinity = new Range(float.NegativeInfinity, float.PositiveInfinity);

	public static readonly Range zero = default(Range);

	public Range(float min, float max) {
		this.min = min;
		this.max = max;
	}

	public static Range Centered(float mid, float width) => new Range(mid-0.5f*width, mid+0.5f*width);

	public static Range Auto(float x0, float x1) {
		if( x0 <= x1 ) return new Range(x0, x1);
		else return new Range(x1, x0);
	}

	public float Random() {
		return UnityEngine.Random.Range(min, max);
	}

	// Higher iterations creates a steeper central spike
	// 1 = flat (standard random numbers)
	// 2 = triangular
	// 3 = soft squidgy middle (good balance?)
	// 4 = bit sharper
	// 8 = much sharper, ~2x as high central peak as triangular
	public float RandomBell(int iterations = 3) {
		float val = 0;
		for(int i=0; i<iterations; i++) {
			val += UnityEngine.Random.Range(min, max);
		}
		val /= iterations;
		return val;
	}

	public float Lerp(float t) {
		return Mathf.Lerp(min, max, t);
	}

	public float LerpUnclamped(float t) {
		return Mathf.LerpUnclamped(min, max, t);
	}

	public float InverseLerp(float val) {
		return Mathf.InverseLerp(min, max, val);
	}

	public float Clamp(float val) {
		return Mathf.Clamp(val, min, max);
	}

	public Range ExpandedToInclude (float valueToInclude) {
		if (valueToInclude < min) return new Range(valueToInclude, max);
		else if (valueToInclude > max) return new Range(min, valueToInclude);
		else return this;
	}
	
	public Range ShrunkToExclude (float trunctationValue, int directionToShrinkFrom) {
		if (trunctationValue > min || trunctationValue < max) {
			if (directionToShrinkFrom == -1) {
				 return new Range(trunctationValue, max);
			} else if (directionToShrinkFrom == 1) {
				return new Range(min, trunctationValue);
			} else {
				Debug.LogWarning("directionToShrinkFrom must be -1 or 1, but is set to "+directionToShrinkFrom);
			}
		}
		return this;
	}
	
	public Range ExpandedFromPivot (Range range, float expansion, float pivot) {
		return new Range(range.min - expansion * pivot, range.min + expansion * (1-pivot));
	}


	// If a point is contained in the range
	public bool Contains(float x, bool startInclusive = true, bool endInclusive = true) => (startInclusive ? min <= x : min < x) && (endInclusive ? max >= x : max > x);

	// If another range is entirely contained by this range
	public bool Contains(Range other, bool startInclusive = true, bool endInclusive = true) => (startInclusive ? min <= other.min : min < other.min) && (endInclusive ? max >= other.max : max > other.max);

	// If there's any intersection between this and another range
	// not( completely on either side of other range )
	public bool Intersects(Range other, bool startInclusive = true, bool endInclusive = true) {
		return (startInclusive ? min <= other.max : min < other.max) && (endInclusive ? max >= other.min : max > other.min) ||
		       (startInclusive ? other.min <= max : other.min < max) && (endInclusive ? other.max >= min : other.max > min);
	}

	// The shared range between this range and another
	public Range Intersection (Range otherRange) {
        var intersectionMin = Math.Max (min, otherRange.min);
        var intersectionMax = Math.Min (max, otherRange.max);
        return new Range(intersectionMin, intersectionMax);
	}

	// The magnitude of the shared range between this range and another
	public float GetAmountIncludedByRange (Range otherRange) {
		return Mathf.Max(Mathf.Min(otherRange.max, max) - Mathf.Max(otherRange.min, min), 0);
	}
	
	public List<Range> RemoveRange(Range rangeToRemove, bool startInclusive = true, bool endInclusive = true) {
		List<Range> newRanges = new List<Range>();

		if (!Intersects(rangeToRemove, startInclusive, endInclusive)) {
			newRanges.Add(this);
			return newRanges;
		}
		
		if (startInclusive ? rangeToRemove.min > min : rangeToRemove.min >= min) newRanges.Add(new Range(min, rangeToRemove.min));
		if (endInclusive ? rangeToRemove.max < max : rangeToRemove.max <= max) newRanges.Add(new Range(rangeToRemove.max, max));

		return newRanges;
	}

	// It's not really clear what this should do tbh. Remove!
	public static float SignedDistance (Range rangeA, Range rangeB) {
		if (rangeB.Contains(rangeA.mid)) {
			return -rangeA.length * 0.5f;
		}
		return Mathf.Min(rangeA.SignedDistance(rangeB.min), rangeA.SignedDistance(rangeB.max));
	}
	
	// The signed distance from the point to the edges of the range. If the point is inside the range values are negative; else positive.
	public float SignedDistance (float x) {
		return (Contains(x) ? -1 : 1) * Mathf.Min(Mathf.Abs(x - min), Mathf.Abs(x - max));
	}
	
	public float SignedDistanceFromMin (float x) {
		return (Contains(x) ? -1 : 1) * Mathf.Abs(x - min);
	}
	
	public float SignedDistanceFromMax (float x) {
		return (Contains(x) ? -1 : 1) * Mathf.Abs(x - max);
	}

	// The normalized magnitude of the shared range between this range and another, relative to the length of this range
	public float GetNormalizedAmountIncludedByRange (Range otherRange) {
		if(otherRange.length <= 0) return 1; 
		return GetAmountIncludedByRange(otherRange) / length;
	}




	public static Range FromVector2(Vector2 vector) {
		return new Range(vector.x, vector.y);
	}

	public static Vector2 ToVector2(Range range) {
		return new Vector2(range.min, range.max);
	}

	public Vector2 ToVector2() {
		return ToVector2(this);
	}

	public static Range Add(Range left, Range right){
		return new Range(left.min+right.min, left.max+right.max);
	}

	public static Range Add(Range left, float right){
		return new Range(left.min+right, left.max+right);
	}

	public static Range Add(float left, Range right){
		return new Range(left+right.min, left+right.max);
	}


	public static Range Subtract(Range left, Range right){
		return new Range(left.min-right.min, left.max-right.max);
	}

	public static Range Subtract(Range left, float right){
		return new Range(left.min-right, left.max-right);
	}

	public static Range Subtract(float left, Range right){
		return new Range(left-right.min, left-right.max);
	}


	public static Range Multiply(Range left, Range right){
		return new Range(left.min*right.min, left.max*right.max);
	}

	public static Range Multiply(Range left, float right){
		return new Range(left.min*right, left.max*right);
	}

	public static Range Multiply(float left, Range right){
		return new Range(left*right.min, left*right.max);
	}


	public static Range Divide(Range left, Range right){
		return new Range(left.min/right.min, left.max/right.max);
	}

	public static Range Divide(Range left, float right){
		return new Range(left.min/right, left.max/right);
	}

	public static Range Divide(float left, Range right){
		return new Range(left/right.min, left/right.max);
	}

	public override bool Equals(System.Object obj) {
		// If parameter is null return false.
		if (obj == null) {
			return false;
		}

		// If parameter cannot be cast to Range return false.
		Range p = (Range)obj;
		if ((System.Object)p == null) {
			return false;
		}

		// Return true if the fields match:
		return Equals(p);
	}

	public bool Equals(Range p) {
		// If parameter is null return false:
		if ((object)p == null) {
			return false;
		}

		// Return true if the fields match:
		return (min == p.min) && (max == p.max);
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * min.GetHashCode();
			hash = hash * max.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (Range left, Range right) {
		if (System.Object.ReferenceEquals(left, right))
		{
			return true;
		}

		// If one is null, but not both, return false.
		if (((object)left == null) || ((object)right == null))
		{
			return false;
		}

		return left.Equals(right);
	}

	public static bool operator != (Range left, Range right) {
		return !(left == right);
	}
	

	public static Range operator +(Range left, Range right) {
		return Add(left, right);
	}

	public static Range operator +(Vector2 left, Range right) {
		return Add(left, right);
	}

	public static Range operator +(Range left, Vector2 right) {
		return Add(left, right);
	}

	public static Range operator +(Range left, float right) {
		return Add(left, right);
	}

	public static Range operator +(float left, Range right) {
		return Add(left, right);
	}

	public static Range operator -(Range left) {
		return new Range(-left.min, -left.max);
	}

	public static Range operator -(Range left, Range right) {
		return Subtract(left, right);
	}

	public static Range operator -(Vector2 left, Range right) {
		return Subtract(left, right);
	}

	public static Range operator -(Range left, Vector2 right) {
		return Subtract(left, right);
	}

	public static Range operator -(Range left, float right) {
		return Subtract(left, right);
	}

	public static Range operator -(float left, Range right) {
		return Subtract(left, right);
	}


	public static Range operator *(Range left, Range right) {
		return Multiply(left, right);
	}

	public static Range operator *(Vector2 left, Range right) {
		return Multiply(left, right);
	}

	public static Range operator *(Range left, Vector2 right) {
		return Multiply(left, right);
	}
	
	public static Range operator *(Range left, float right) {
		return Multiply(left, right);
	}


	public static Range operator /(Range left, Range right) {
		return Divide(left, right);
	}

	public static Range operator /(Vector2 left, Range right) {
		return Divide(left, right);
	}

	public static Range operator /(Range left, Vector2 right) {
		return Divide(left, right);
	}
	
	public static Range operator /(Range left, float right) {
		return Divide(left, right);
	}

	public static implicit operator Range(Vector2 src) {
		return FromVector2(src);
	}
	
	public static implicit operator Vector2(Range src) {
		return src.ToVector2();
	}

	
	public override string ToString() {
		return string.Format("[{0:N1} to {1:N1}]", min, max);
	}
}


// Some visual tests for the range class
// public class RangeTests : MonoBehaviour {
// 	public Range container;
// 	public Range content;
// 	public Range intersection;
// 	public float testPoint;
// 	public float signedDistance;
// 	public float signedDistanceForRange;
// 	public float signedDistanceForRangeB;
//     
// 	public float signedDistanceFromMin;
// 	public float signedDistanceFromMax;
//
// 	public void OnDrawGizmos() {
// 		intersection = container.Intersection(content);
// 		signedDistance = container.SignedDistance(testPoint);
// 		signedDistanceForRange = Range.SignedDistance(container, content);
// 		signedDistanceForRangeB = Range.SignedDistance(content, container);
//
// 		signedDistanceFromMin = Mathf.Max((container.min-content.min), (container.min-content.max));
// 		signedDistanceFromMax = Mathf.Max((content.min-container.max), (content.max-container.max));
//         
// 		Gizmos.DrawSphere(transform.TransformPoint(new Vector2(testPoint,0)), 0.5f);
//         
// 		GizmosX.DrawArrowLine(transform.TransformPoint(new Vector2(container.min,0)), transform.TransformPoint(new Vector2(container.max,0)), Vector3.forward);
// 		GizmosX.DrawArrowLine(transform.TransformPoint(new Vector2(container.max,0)), transform.TransformPoint(new Vector2(container.min,0)), Vector3.forward);
// 		GizmosX.DrawArrowLine(transform.TransformPoint(new Vector2(content.min,1)), transform.TransformPoint(new Vector2(content.max,1)), Vector3.forward);
// 		GizmosX.DrawArrowLine(transform.TransformPoint(new Vector2(content.max,1)), transform.TransformPoint(new Vector2(content.min,1)), Vector3.forward);
//         
// 		GizmosX.DrawArrowLine(transform.TransformPoint(new Vector2(intersection.min,2)), transform.TransformPoint(new Vector2(intersection.max,2)), Vector3.forward);
// 		GizmosX.DrawArrowLine(transform.TransformPoint(new Vector2(intersection.max,2)), transform.TransformPoint(new Vector2(intersection.min,2)), Vector3.forward);
//
// 	}
// }
