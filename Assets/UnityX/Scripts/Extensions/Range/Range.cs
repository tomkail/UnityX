using System;
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


	// If a point is contained in the range
	public bool Contains(float x) => x >= min && x <= max;

	// If another range is entirely contained by this range
	public bool Contains(Range otherRange)  => otherRange.min >= min && otherRange.max <= max;

	// If there's any intersection between this and another range
	// not( completely on either side of other range )
	public bool Intersects(Range otherRange) => !(otherRange.min > max || otherRange.max < min);

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