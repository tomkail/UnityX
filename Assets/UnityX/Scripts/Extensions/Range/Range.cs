using System;
using UnityEngine;

/// <summary>
/// Simple struct for specifying a range between two floats.
/// Has a property drawer for easy inspectorisification
/// </summary>
[Serializable]
public struct Range
{
	public float min;
	public float max;

	public float length {
        get {
            return max - min;
        }
    }

	public Range(float min, float max) {
		this.min = min;
		this.max = max;
	}

	public float Random() {
		return UnityEngine.Random.Range(min, max);
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


	// 1D AABB detection algorithm
	public bool IsAtAllIncludedByRange (Range otherRange) {
		return Mathf.Abs(otherRange.min - min) * 2 < (otherRange.length + length);
	}

	public float GetAmountIncludedByRange (Range otherRange) {
		return Mathf.Max(Mathf.Min(otherRange.max, max) - Mathf.Max(otherRange.min, min), 0);
	}

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
}
