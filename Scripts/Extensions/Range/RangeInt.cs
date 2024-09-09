// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// /// <summary>
// /// Simple struct for specifying a range between two ints.
// /// Has a property drawer for easy inspectorisification
// /// </summary>
// [Serializable]
// public struct RangeInt : IEquatable<RangeInt>
// {
//     public int min;
//     public int max;
//
//     public int length => max - min;
//
//     public RangeInt negated => new RangeInt(-max, -min);
//     
//     public static readonly RangeInt zero = default(RangeInt);
//
//     public RangeInt(int min, int max) {
//         this.min = min;
//         this.max = max;
//     }
//
//     public static RangeInt Auto(int x0, int x1) {
//         if( x0 <= x1 ) return new RangeInt(x0, x1);
//         else return new RangeInt(x1, x0);
//     }
//
//     public int RandomInt() {
//         return UnityEngine.Random.Range(min, max);
//     }
//
//     public float Lerp(float t) {
//         return Mathf.Lerp(min, max, t);
//     }
//
//     public float LerpUnclamped(float t) {
//         return Mathf.LerpUnclamped(min, max, t);
//     }
//
//     public float InverseLerp(float val) {
//         return Mathf.InverseLerp(min, max, val);
//     }
//
//     public float Clamp(float val) {
//         return Mathf.Clamp(val, min, max);
//     }
//
//     public RangeInt ExpandedToInclude (int valueToInclude) {
//         if (valueToInclude < min) return new RangeInt(valueToInclude, max);
//         else if (valueToInclude > max) return new RangeInt(min, valueToInclude);
//         else return this;
//     }
// 	
//     public RangeInt ShrunkToExclude (int trunctationValue, int directionToShrinkFrom) {
//         if (trunctationValue > min || trunctationValue < max) {
//             if (directionToShrinkFrom == -1) {
//                 return new RangeInt(trunctationValue, max);
//             } else if (directionToShrinkFrom == 1) {
//                 return new RangeInt(min, trunctationValue);
//             } else {
//                 Debug.LogWarning("directionToShrinkFrom must be -1 or 1, but is set to "+directionToShrinkFrom);
//             }
//         }
//         return this;
//     }
//
//
//     // If a point is contained in the range
//     public bool Contains(float x, bool startInclusive = true, bool endInclusive = true) => (startInclusive ? min <= x : min < x) && (endInclusive ? max >= x : max > x);
//
//     // If another range is entirely contained by this range
//     public bool Contains(RangeInt other, bool startInclusive = true, bool endInclusive = true) => (startInclusive ? min <= other.min : min < other.min) && (endInclusive ? max >= other.max : max > other.max);
//
//     // If there's any intersection between this and another range
//     // not( completely on either side of other range )
//     public bool Intersects(RangeInt other, bool startInclusive = true, bool endInclusive = true) {
//         return (startInclusive ? min <= other.max : min < other.max) && (endInclusive ? max >= other.min : max > other.min) ||
//                (startInclusive ? other.min <= max : other.min < max) && (endInclusive ? other.max >= min : other.max > min);
//     }
//
//     // The shared range between this range and another
//     public RangeInt Intersection (RangeInt otherRangeInt) {
//         var intersectionMin = Math.Max (min, otherRangeInt.min);
//         var intersectionMax = Math.Min (max, otherRangeInt.max);
//         return new RangeInt(intersectionMin, intersectionMax);
//     }
//
//     // The magnitude of the shared range between this range and another
//     public float GetAmountIncludedByRange (RangeInt otherRangeInt) {
//         return Mathf.Max(Mathf.Min(otherRangeInt.max, max) - Mathf.Max(otherRangeInt.min, min), 0);
//     }
// 	
//     public List<RangeInt> RemoveRange(RangeInt rangeToRemove, bool startInclusive = true, bool endInclusive = true) {
//         List<RangeInt> newRangeInts = new List<RangeInt>();
//
//         if (!Intersects(rangeToRemove, startInclusive, endInclusive)) {
//             newRangeInts.Add(this);
//             return newRangeInts;
//         }
// 		
//         if (startInclusive ? rangeToRemove.min > min : rangeToRemove.min >= min) newRangeInts.Add(new RangeInt(min, rangeToRemove.min));
//         if (endInclusive ? rangeToRemove.max < max : rangeToRemove.max <= max) newRangeInts.Add(new RangeInt(rangeToRemove.max, max));
//
//         return newRangeInts;
//     }
// 	
//     // The signed distance from the point to the edges of the range. If the point is inside the range values are negative; else positive.
//     public float SignedDistance (float x) {
//         return (Contains(x) ? -1 : 1) * Mathf.Min(Mathf.Abs(x - min), Mathf.Abs(x - max));
//     }
// 	
//     public float SignedDistanceFromMin (float x) {
//         return (Contains(x) ? -1 : 1) * Mathf.Abs(x - min);
//     }
// 	
//     public float SignedDistanceFromMax (float x) {
//         return (Contains(x) ? -1 : 1) * Mathf.Abs(x - max);
//     }
//
//     // The normalized magnitude of the shared range between this range and another, relative to the length of this range
//     public float GetNormalizedAmountIncludedByRangeInt (RangeInt otherRangeInt) {
//         if(otherRangeInt.length <= 0) return 1; 
//         return GetAmountIncludedByRange(otherRangeInt) / length;
//     }
//
//
//
//
//     public static RangeInt FromVector2Int(Vector2Int vector) {
//         return new RangeInt(vector.x, vector.y);
//     }
//
//     public static Vector2Int ToVector2Int(RangeInt range) {
//         return new Vector2Int(range.min, range.max);
//     }
//
//     public Vector2Int ToVector2Int() {
//         return ToVector2Int(this);
//     }
//
//     public static RangeInt Add(RangeInt left, RangeInt right){
//         return new RangeInt(left.min+right.min, left.max+right.max);
//     }
//
//     public static RangeInt Add(RangeInt left, int right){
//         return new RangeInt(left.min+right, left.max+right);
//     }
//
//     public static RangeInt Add(int left, RangeInt right){
//         return new RangeInt(left+right.min, left+right.max);
//     }
//
//
//     public static RangeInt Subtract(RangeInt left, RangeInt right){
//         return new RangeInt(left.min-right.min, left.max-right.max);
//     }
//
//     public static RangeInt Subtract(RangeInt left, int right){
//         return new RangeInt(left.min-right, left.max-right);
//     }
//
//     public static RangeInt Subtract(int left, RangeInt right){
//         return new RangeInt(left-right.min, left-right.max);
//     }
//
//     public override bool Equals(System.Object obj) {
//         // If parameter is null return false.
//         if (obj == null) {
//             return false;
//         }
//
//         // If parameter cannot be cast to RangeInt return false.
//         RangeInt p = (RangeInt)obj;
//         if ((System.Object)p == null) {
//             return false;
//         }
//
//         // Return true if the fields match:
//         return Equals(p);
//     }
//
//     public bool Equals(RangeInt p) {
//         // If parameter is null return false:
//         if ((object)p == null) {
//             return false;
//         }
//
//         // Return true if the fields match:
//         return (min == p.min) && (max == p.max);
//     }
//
//     public override int GetHashCode() {
//         unchecked // Overflow is fine, just wrap
//         {
//             int hash = 27;
//             hash = hash * min.GetHashCode();
//             hash = hash * max.GetHashCode();
//             return hash;
//         }
//     }
//
//     public static bool operator == (RangeInt left, RangeInt right) {
//         if (System.Object.ReferenceEquals(left, right))
//         {
//             return true;
//         }
//
//         // If one is null, but not both, return false.
//         if (((object)left == null) || ((object)right == null))
//         {
//             return false;
//         }
//
//         return left.Equals(right);
//     }
//
//     public static bool operator != (RangeInt left, RangeInt right) {
//         return !(left == right);
//     }
// 	
//
//     public static RangeInt operator +(RangeInt left, RangeInt right) {
//         return Add(left, right);
//     }
//
//     public static RangeInt operator +(Vector2Int left, RangeInt right) {
//         return Add(left, right);
//     }
//
//     public static RangeInt operator +(RangeInt left, Vector2Int right) {
//         return Add(left, right);
//     }
//
//     public static RangeInt operator +(RangeInt left, int right) {
//         return Add(left, right);
//     }
//
//     public static RangeInt operator +(int left, RangeInt right) {
//         return Add(left, right);
//     }
//
//     public static RangeInt operator -(RangeInt left) {
//         return new RangeInt(-left.min, -left.max);
//     }
//
//     public static RangeInt operator -(RangeInt left, RangeInt right) {
//         return Subtract(left, right);
//     }
//
//     public static RangeInt operator -(Vector2Int left, RangeInt right) {
//         return Subtract(left, right);
//     }
//
//     public static RangeInt operator -(RangeInt left, Vector2Int right) {
//         return Subtract(left, right);
//     }
//
//     public static RangeInt operator -(RangeInt left, int right) {
//         return Subtract(left, right);
//     }
//
//     public static RangeInt operator -(int left, RangeInt right) {
//         return Subtract(left, right);
//     }
//
//     public static implicit operator RangeInt(Vector2Int src) {
//         return FromVector2Int(src);
//     }
// 	
//     public static implicit operator Vector2Int(RangeInt src) {
//         return src.ToVector2Int();
//     }
//
// 	
//     public override string ToString() {
//         return string.Format("[{0:N1} to {1:N1}]", min, max);
//     }
// }