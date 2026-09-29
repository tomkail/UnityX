using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityX {
	/// <summary>
	/// Simple struct for specifying a range between two ints (min..max).
	/// The integer sibling of <see cref="Range"/>, useful for index/grid spans.
	/// Lives in the `UnityX` namespace so it can keep the natural name `RangeInt` without colliding
	/// with UnityEngine.RangeInt (a start+length struct) — qualify as `UnityX.RangeInt` if both are in scope.
	/// </summary>
	[Serializable]
	public struct RangeInt : IEquatable<RangeInt> {
		public int min;
		public int max;

		public int length => max - min;

		public RangeInt negated => new RangeInt(-max, -min);

		public static readonly RangeInt zero = default;

		public RangeInt(int min, int max) {
			this.min = min;
			this.max = max;
		}

		public static RangeInt Auto(int x0, int x1) {
			if (x0 <= x1) return new RangeInt(x0, x1);
			else return new RangeInt(x1, x0);
		}

		public int RandomInt() {
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

		public RangeInt ExpandedToInclude (int valueToInclude) {
			if (valueToInclude < min) return new RangeInt(valueToInclude, max);
			else if (valueToInclude > max) return new RangeInt(min, valueToInclude);
			else return this;
		}

		public RangeInt ShrunkToExclude (int truncationValue, int directionToShrinkFrom) {
			// Only shrink if the value is strictly inside the range — otherwise there's nothing to exclude.
			if (truncationValue > min && truncationValue < max) {
				if (directionToShrinkFrom == -1) {
					return new RangeInt(truncationValue, max);
				} else if (directionToShrinkFrom == 1) {
					return new RangeInt(min, truncationValue);
				} else {
					Debug.LogWarning("directionToShrinkFrom must be -1 or 1, but is set to "+directionToShrinkFrom);
				}
			}
			return this;
		}


		// If a point is contained in the range
		public bool Contains(float x, bool startInclusive = true, bool endInclusive = true) => (startInclusive ? min <= x : min < x) && (endInclusive ? max >= x : max > x);

		// If another range is entirely contained by this range
		public bool Contains(RangeInt other, bool startInclusive = true, bool endInclusive = true) => (startInclusive ? min <= other.min : min < other.min) && (endInclusive ? max >= other.max : max > other.max);

		// If there's any intersection between this and another range
		// not( completely on either side of other range )
		public bool Intersects(RangeInt other, bool startInclusive = true, bool endInclusive = true) {
			return (startInclusive ? min <= other.max : min < other.max) && (endInclusive ? max >= other.min : max > other.min) ||
			       (startInclusive ? other.min <= max : other.min < max) && (endInclusive ? other.max >= min : other.max > min);
		}

		// The shared range between this range and another
		public RangeInt Intersection (RangeInt otherRange) {
			var intersectionMin = Math.Max (min, otherRange.min);
			var intersectionMax = Math.Min (max, otherRange.max);
			return new RangeInt(intersectionMin, intersectionMax);
		}

		// The magnitude of the shared range between this range and another
		public float GetAmountIncludedByRange (RangeInt otherRange) {
			return Mathf.Max(Mathf.Min(otherRange.max, max) - Mathf.Max(otherRange.min, min), 0);
		}

		public List<RangeInt> RemoveRange(RangeInt rangeToRemove, bool startInclusive = true, bool endInclusive = true) {
			List<RangeInt> newRanges = new List<RangeInt>();

			if (!Intersects(rangeToRemove, startInclusive, endInclusive)) {
				newRanges.Add(this);
				return newRanges;
			}

			// Clamp the removed range to our bounds so the emitted sub-ranges can never invert, even if
			// rangeToRemove extends past [min,max] (behaviour is unchanged for an already-contained range).
			var removeMin = Mathf.Clamp(rangeToRemove.min, min, max);
			var removeMax = Mathf.Clamp(rangeToRemove.max, min, max);
			if (startInclusive ? removeMin > min : removeMin >= min) newRanges.Add(new RangeInt(min, removeMin));
			if (endInclusive ? removeMax < max : removeMax <= max) newRanges.Add(new RangeInt(removeMax, max));

			return newRanges;
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
		public float GetNormalizedAmountIncludedByRange (RangeInt otherRange) {
			if (otherRange.length <= 0) return 1;
			return GetAmountIncludedByRange(otherRange) / length;
		}




		public static RangeInt FromVector2Int(Vector2Int vector) {
			return new RangeInt(vector.x, vector.y);
		}

		public static Vector2Int ToVector2Int(RangeInt range) {
			return new Vector2Int(range.min, range.max);
		}

		public Vector2Int ToVector2Int() {
			return ToVector2Int(this);
		}

		public static RangeInt Add(RangeInt left, RangeInt right){
			return new RangeInt(left.min+right.min, left.max+right.max);
		}

		public static RangeInt Add(RangeInt left, int right){
			return new RangeInt(left.min+right, left.max+right);
		}

		public static RangeInt Add(int left, RangeInt right){
			return new RangeInt(left+right.min, left+right.max);
		}


		public static RangeInt Subtract(RangeInt left, RangeInt right){
			return new RangeInt(left.min-right.min, left.max-right.max);
		}

		public static RangeInt Subtract(RangeInt left, int right){
			return new RangeInt(left.min-right, left.max-right);
		}

		public static RangeInt Subtract(int left, RangeInt right){
			return new RangeInt(left-right.min, left-right.max);
		}

		public override bool Equals(object obj) {
			return obj is RangeInt other && Equals(other);
		}

		public bool Equals(RangeInt p) {
			return min == p.min && max == p.max;
		}

		public override int GetHashCode() {
			unchecked // Overflow is fine, just wrap
			{
				int hash = 27;
				hash = hash * 31 + min.GetHashCode();
				hash = hash * 31 + max.GetHashCode();
				return hash;
			}
		}

		public static bool operator == (RangeInt left, RangeInt right) {
			return left.Equals(right);
		}

		public static bool operator != (RangeInt left, RangeInt right) {
			return !(left == right);
		}


		public static RangeInt operator +(RangeInt left, RangeInt right) {
			return Add(left, right);
		}

		public static RangeInt operator +(Vector2Int left, RangeInt right) {
			return Add(left, right);
		}

		public static RangeInt operator +(RangeInt left, Vector2Int right) {
			return Add(left, right);
		}

		public static RangeInt operator +(RangeInt left, int right) {
			return Add(left, right);
		}

		public static RangeInt operator +(int left, RangeInt right) {
			return Add(left, right);
		}

		public static RangeInt operator -(RangeInt left) {
			return new RangeInt(-left.min, -left.max);
		}

		public static RangeInt operator -(RangeInt left, RangeInt right) {
			return Subtract(left, right);
		}

		public static RangeInt operator -(Vector2Int left, RangeInt right) {
			return Subtract(left, right);
		}

		public static RangeInt operator -(RangeInt left, Vector2Int right) {
			return Subtract(left, right);
		}

		public static RangeInt operator -(RangeInt left, int right) {
			return Subtract(left, right);
		}

		public static RangeInt operator -(int left, RangeInt right) {
			return Subtract(left, right);
		}

		public static implicit operator RangeInt(Vector2Int src) {
			return FromVector2Int(src);
		}

		public static implicit operator Vector2Int(RangeInt src) {
			return src.ToVector2Int();
		}


		public override string ToString() {
			return $"[{min} to {max}]";
		}
	}
}
