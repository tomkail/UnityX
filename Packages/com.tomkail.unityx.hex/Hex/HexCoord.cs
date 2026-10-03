using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = System.Object;

namespace UnityX.HexGrid {
/// <summary>
/// Axial Hexagon grid coordinate.
/// </summary>
/// <remarks>
/// Uses the q,r axial system detailed at http://www.redblobgames.com/grids/hexagons/.
/// These are "pointy topped" hexagons. The q axis points right, and the r axis points up-right.
/// When converting to and from Unity coordinates, the length of a hexagon side is 1 unit.
/// </remarks>
// Serialization: BCL [DataContract]/[DataMember] (no serializer dependency) persist only q and r, as {"q":..,"r":..}.
[Serializable, System.Runtime.Serialization.DataContract]
public partial struct HexCoord : IEquatable<HexCoord> {
	/// <summary>
	/// Position on the q axis.
	/// </summary>
	[SerializeField, System.Runtime.Serialization.DataMember(Name = "q", Order = 0)]
	public int q;
	/// <summary>
	/// Position on the r axis.
	/// </summary>
	[SerializeField, System.Runtime.Serialization.DataMember(Name = "r", Order = 1)]
	public int r;

	// s is derived (s = -q-r), so it has no [DataMember] and is never persisted.
	public int s => -q-r;

	public static readonly HexCoord zero = default(HexCoord);

	/// <summary>
	/// Initializes a new instance of the <see cref="Settworks.Hexagons.HexCoord"/> struct.
	/// </summary>
	/// <param name="q">Position on the q axis.</param>
	/// <param name="r">Position on the r axis.</param>
	public HexCoord(int q, int r) : this() {
		this.q = q;
		this.r = r;
	}

	public Vector2 DirectionVector() {
		var dir = q*Q_XY + r*R_XY;
		if(dir.sqrMagnitude != 1) dir.Normalize();
		return dir;
	}
	public Vector2 Direction(Vector2 fractionalHex) {
		var dir = fractionalHex.x*Q_XY + fractionalHex.y*R_XY;
		if(dir.sqrMagnitude != 1) dir.Normalize();
		return dir;
	}

	/// <summary>
	/// position of this hex.
	/// </summary>
	public Vector2 Position() {
		return q*Q_XY + r*R_XY;
	}

	/// <summary>
	/// Get the maximum absolute cubic coordinate.
	/// </summary>
	/// <remarks>
	/// In hexagonal space this is the polar radius, i.e. distance from 0,0.
	/// </remarks>
	public int AxialLength() {
		if (q == 0 && r == 0) return 0;
		if (q > 0 && r >= 0) return q + r;
		if (q <= 0 && r > 0) return (-q < r)? r: -q;
		if (q < 0) return -q - r;
		return (-r > q)? -r: q;
	}

	/// <summary>
	/// Get the minimum absolute cubic coordinate.
	/// </summary>
	/// <remarks>
	/// This is the number of hexagon steps from 0,0 which are not along the maximum axis.
	/// </remarks>
	public int AxialSkew() {
		if (q == 0 && r == 0) return 0;
		if (q > 0 && r >= 0) return (q < r)? q: r;
		if (q <= 0 && r > 0) return (-q < r)? Mathf.Min(-q, q + r): Mathf.Min(r, -q - r);
		if (q < 0) return (q > r)? -q: -r;
		return (-r > q)? Mathf.Min(q, -q -r): Mathf.Min(-r, q + r);
	}



	/// <summary>
	/// Get the counterclockwise position of this hex in the ring at its distance from 0,0.
	/// </summary>
	public int PolarIndex() {
		if (q == 0 && r == 0) return 0;
		if (q > 0 && r >= 0) return r;
		if (q <= 0 && r > 0) return (-q < r)? r - q: -3 * q - r;
		if (q < 0) return -4 * (q + r) + q;
		return (-r > q)? -4 * r + q: 6 * q + r;
	}

	/// <summary>
	/// Get a neighboring hex.
	/// </summary>
	/// <remarks>
	/// Neighbor 0 is to the right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="index">Index of the desired neighbor. Cyclically constrained 0..5.</param>
	public HexCoord Neighbor(int index) {
		return Direction(index) + this;
	}
	
	public HexCoord PolarNeighbor(bool CCW = false) {
		if (q > 0) {
			if (r < 0) {
				if (q > -r) return this + directions[CCW? 1: 4];
				if (q < -r) return this + directions[CCW? 0: 3];
				return this + directions[CCW? 1: 3];
			}
			if (r > 0) return this + directions[CCW? 2: 5];
			return this + directions[CCW? 2: 4];
		}
		if (q < 0) {
			if (r > 0) {
				if (r > -q) return this + directions[CCW? 3: 0];
				if (r < -q) return this + directions[CCW? 4: 1];
				return this + directions[CCW? 4: 0];
			}
			if (r < 0) return this + directions[CCW? 5: 2];
			return this + directions[CCW? 5: 1];
		}
		if (r > 0) return this + directions[CCW? 3: 5];
		if (r < 0) return this + directions[CCW? 0: 2];
		return this;
	}

	public int NeighborIndexOf(HexCoord normalizedDirection) {
		return System.Array.IndexOf(directions, normalizedDirection);
	}
	/// <summary>
	/// Enumerate this hex's six neighbors.
	/// </summary>
	/// <remarks>
	/// Neighbor 0 is to the right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="first">Index of the first neighbor to enumerate.</param>
	public IEnumerable<HexCoord> Neighbors(int first = 0) {
		foreach (HexCoord hex in Directions(first))
			yield return hex + this;
	}

	/// <summary>
	/// Get the position of a corner vertex.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
	public Vector2 Corner(int index) {
		return CornerVector(index) + Position();
	}

	public static Vector2 Corner(HexCoord coord, int index) {
		return coord.Corner(index);
	}

	/// <summary>
	/// Enumerate this hex's six corners.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="first">Index of the first corner to enumerate.</param>
	public IEnumerable<Vector2> Corners(int first = 0, float hexSize = 1) {
		Vector2 pos = Position();
		foreach (Vector2 v in CornerVectors(first, hexSize))
			yield return v + pos;
	}

	/// <summary>
	/// Get the polar angle to a corner vertex.
	/// </summary>
	/// <remarks>
	/// This is the angle in radians from the center of 0,0 to the selected corner of this hex.
	/// </remarks>
	/// <param name="index">Index of the desired corner.</param>
	public float CornerPolarAngle(int index) {
		Vector2 pos = Corner(index);
		return Mathf.Atan2(pos.y, pos.x);
	}

	/// <summary>
	/// Get the polar angle to the clockwise bounding corner.
	/// </summary>
	/// <remarks>
	/// The two polar bounding corners are those whose polar angles form the widest arc.
	/// </remarks>
	/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
	public float PolarBoundingAngle(bool CCW = false) {
		return CornerPolarAngle(PolarBoundingCornerIndex(CCW));
	}

	/// <summary>
	/// Get the XY position of the clockwise bounding corner.
	/// </summary>
	/// <remarks>
	/// The two polar bounding corners are those whose polar angles form the widest arc.
	/// </remarks>
	/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
	public Vector2 PolarBoundingCorner(bool CCW = false) {
		return Corner(PolarBoundingCornerIndex(CCW));
	}

	/// <summary>
	/// Get the index of the clockwise bounding corner.
	/// </summary>
	/// <remarks>
	/// The two polar bounding corners are those whose polar angles form the widest arc.
	/// </remarks>
	/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
	/// <param name="neighbor">If set to <c>true</c>, gets the other corner shared by the same ring-neighbor as normal return.</param>
	public int PolarBoundingCornerIndex(bool CCW = false) {
		if (q == 0 && r == 0) return 0;
		if (q > 0 && r >= 0) return CCW?
			(q > r)? 1: 2:
			(q < r)? 5: 4;
		if (q <= 0 && r > 0) return (-q < r)?
			CCW?
				(r > -2 * q)? 2: 3:
				(r < -2 * q)? 0: 5:
			CCW?
				(q > -2 * r)? 3: 4:
				(q < -2 * r)? 1: 0;
		if (q < 0) return CCW?
			(q < r)? 4: 5:
			(q > r)? 2: 1;
		return (-r > q)?
			CCW?
				(r < -2 * q)? 5: 0:
				(r > -2 * q)? 3: 2:
			CCW?
				(q < -2 * r)? 0: 1:
				(q > -2 * r)? 4: 3;
	}

	/// <summary>
	/// Get the half sextant of origin containing this hex.
	/// </summary>
	/// <remarks>
	/// CornerSextant is HalfSextant/2. NeighborSextant is (HalfSextant+1)/2.
	/// </remarks>
	public int HalfSextant() {
		if (q > 0 && r >= 0 || q == 0 && r == 0)
			return (q > r)? 0 : 1;
		if (q <= 0 && r > 0)
			return (-q < r)?
				(r > -2 * q)? 2: 3:
				(q > -2 * r)? 4: 5;
		if (q < 0)
			return (q < r)? 6: 7;
		return (-r > q)?
			(r < -2 * q)? 8: 9:
			(q < -2 * r)? 10: 11;
	} 

	/// <summary>
	/// Get the corner index of 0,0 closest to this hex's polar vector.
	/// </summary>
	public int CornerSextant() {
		if (q > 0 && r >= 0 || q == 0 && r == 0) return 0;
		if (q <= 0 && r > 0) return (-q < r)? 1: 2;
		if (q < 0) return 3;
		return (-r > q)? 4: 5;
	}

	/// <summary>
	/// Get the neighbor index of 0,0 through which this hex's polar vector passes.
	/// </summary>
	public int NeighborSextant() {
		if (q == 0 && r == 0) return 0;
		if (q > 0 && r >= 0) return (q <= r)? 1: 0;
		if (q <= 0 && r > 0) return (-q <= r)?
			(r <= -2 * q)? 2: 1:
			(q <= -2 * r)? 3: 2;
		if (q < 0) return (q >= r)? 4: 3;
		return (-r > q)?
			(r >= -2 * q)? 5: 4:
			(q >= -2 * r)? 0: 5;
	}

	/// <summary>
	/// Rotate around 0,0 in sextant increments.
	/// </summary>
	/// <returns>
	/// A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after rotation.
	/// </returns>
	/// <param name="sextants">How many sextants to rotate by.</param>
	public HexCoord SextantRotation(int sextants) {
		if (this == zero) return this;
		sextants = NormalizeRotationIndex(sextants);
		if (sextants == 0) return this;
		if (sextants == 1) return new HexCoord(-r, -s);
		if (sextants == 2) return new HexCoord(s, q);
		if (sextants == 3) return new HexCoord(-q, -r);
		if (sextants == 4) return new HexCoord(r, s);
		return new HexCoord(-s, -q);
	}

	/// <summary>
	/// Mirror across a cubic axis.
	/// </summary>
	/// <remarks>
	/// The cubic axes are "diagonal" to the hexagons, passing through two opposite corners: axis a passes
	/// through corners (2-a) and (5-a). <see cref="MirrorThroughCorners"/> takes the corner index directly.
	/// </remarks>
	/// <param name="axis">Cubic axis 0..2 (cyclically constrained).</param>
	/// <returns>A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after mirroring.</returns>
	public HexCoord Mirror(int axis = 1) {
		if (this == zero) return this;
		axis = NormalizeRotationIndex(axis, 3);
		if (axis == 0) return new HexCoord(r, q);
		if (axis == 1) return new HexCoord(s, r);
		return new HexCoord(q, s);
	}

	/// <summary>
	/// Scale as a vector, truncating result.
	/// </summary>
	/// <returns>A new <see cref="HexCoord"/>; this one is unchanged.</returns>
	public HexCoord Scale(float factor) {
		return new HexCoord((int)(q * factor), (int)(r * factor));
	}
	/// <summary>
	/// Scale as a vector.
	/// </summary>
	/// <returns>A new <see cref="HexCoord"/>; this one is unchanged.</returns>
	public HexCoord Scale(int factor) {
		return new HexCoord(q * factor, r * factor);
	}

	/// <summary>
	/// Determines whether this hex is on the ray starting at origin in a direction
	/// </summary>
	public bool IsOnLine(HexCoord origin, HexCoord direction) {
		var offsetOriginC = (this-origin).AxialToCube();
		if(offsetOriginC == Vector3Int.zero) return true;
		var dirC = direction.AxialToCube();
		// Must be collinear with the direction - every pairwise cube cross-product is zero. (Matching only
		// the per-axis signs, as this used to, gives false positives for non-axis-aligned directions.)
		if(offsetOriginC.x * dirC.y - offsetOriginC.y * dirC.x != 0) return false;
		if(offsetOriginC.y * dirC.z - offsetOriginC.z * dirC.y != 0) return false;
		if(offsetOriginC.x * dirC.z - offsetOriginC.z * dirC.x != 0) return false;
		// ...and on the ray (same side of the origin, not the opposite direction).
		if(offsetOriginC.x * dirC.x + offsetOriginC.y * dirC.y + offsetOriginC.z * dirC.z < 0) return false;
		return true;
	}



	/// <summary>
	/// Determines whether this hex is on the infinite line passing through points a and b.
	/// </summary>
	public bool IsOnCartesianLine(Vector2 a, Vector2 b) {
		Vector2 AB = b - a;
		bool bias = Vector3.Cross(AB, Corner(0) - a).z > 0;
		for (int i = 1; i < 6; i++) {
			if (bias != (Vector3.Cross(AB, Corner(i) - a).z > 0))
				return true;
		}
		return false;
	}

	/// <summary>
	/// Determines whether this the is on the line segment between points a and b.
	/// </summary>
	public bool IsOnCartesianLineSegment(Vector2 a, Vector2 b) {
		Vector2 AB = b - a;
		float mag = AB.sqrMagnitude;
		Vector2 AC = Corner(0) - a;
		bool within = AC.sqrMagnitude <= mag && Vector2.Dot(AB, AC) >= 0;
		int sign = Mathf.RoundToInt(Mathf.Sign(Vector3.Cross(AB, AC).z));
		for (int i = 1; i < 6; i++) {
			AC = Corner(i) - a;
			bool newWithin = AC.sqrMagnitude <= mag && Vector2.Dot(AB, AC) >= 0;
			int newSign =	Mathf.RoundToInt(Mathf.Sign(Vector3.Cross(AB, AC).z));
			if ((within || newWithin) && (sign * newSign <= 0))
				return true;
			within = newWithin;
			sign = newSign;
		}
		return false;
	}

	

	// return values of turns() method
	public static int LEFT = 1;
	public static int RIGHT = -1;
	public static int STRAIGHT = 0;
   
	// returns one of the 3 above constants, depending on whether the
	// three vertices constitute a left turn or a right turn.
	public static int turns(Vector2 v0,Vector2 v1,Vector2 v2) {
		var cross = (v1.x-v0.x)*(v2.y-v0.y) - (v2.x-v0.x)*(v1.y-v0.y);
		return((cross>0.0f) ? LEFT : ((cross==0.0f) ? STRAIGHT : RIGHT));
	}
	
	// A routine to tell if a hexagon has any part of it is "left" of a given
   // arc boundary (clockwise arm of the cone)
   public bool leftOfArc(HexCoord hc,Vector2 ac) {
      int i,carm;
      for (i=0;i<6;i++) {
		carm = turns(hc.Position(),ac,Corner(i));
		if (carm==LEFT) return true;
      }
      return false;
   }
   
   // A routine to tell if a hexagon has any part of it is "right" of a given
   // arc boundary (counter-clockwise arm of the cone)
   public bool rightOfArc(HexCoord hc,Vector2 acc) {
      int i,ccarm;
      for (i=0;i<6;i++) {
		ccarm = turns(hc.Position(),acc,Corner(i));
		if (ccarm==RIGHT) return true;
      }
      return false;
   }
   
   

	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="Settworks.Hexagons.HexCoord"/>.
	/// </summary>
	/// <remarks>
	/// Matches the formatting of <see cref="UnityEngine.Vector2.ToString()"/>.
	/// </remarks>
	public override string ToString () {
		return $"({q},{r})";
	}
	public string ToDirectionString () {
		return Position()+" "+Util.DirectionName(Position());
	}
	
	/*
	 * Static Methods
	 */

	/// <summary>
	/// Distance between two hexes.
	/// </summary>
	public static int Distance(HexCoord a, HexCoord b) {
		return (a - b).AxialLength();
	}

	static public Vector2 HexLerp(HexCoord a, HexCoord b, float t) {
		return Vector2.Lerp((Vector2)a, (Vector2)b, t);
    }
	static public Vector2 HexLerpUnclamped(HexCoord a, HexCoord b, float t) {
		return Vector2.LerpUnclamped((Vector2)a, (Vector2)b, t);
    }


	public static HexCoord[] GetPointsOnLine (HexCoord direction, int startLineDistance, int endLineDistance) {
		HexCoord[] results = new HexCoord[(endLineDistance - startLineDistance) + 1];
		for(int d = startLineDistance; d <= endLineDistance; d++) results[d - startLineDistance] = direction * d;
		return results;
	}

	public static HexCoord[] GetPointsOnLine (HexCoord gridPoint, HexCoord direction, int lineDistance) {
		var points = GetPointsOnLine(direction, 0, lineDistance-1);
		for (int i = 0; i < points.Length; i++) points [i] += gridPoint;
		return points;
	}


	public static HexCoord[] GetPointsInRing(int minRadius, int maxRadius) {
		int length = 0;
		for(int d = minRadius; d <= maxRadius; d++)
			length += d == 0 ? 1 : (d * 6);
		HexCoord[] results = new HexCoord[length];
		length = 0;
		for(int d = minRadius; d <= maxRadius; d++) {
			GetPointsOnRingNonAlloc(results, length, d);
			length += d == 0 ? 1 : (d * 6);
		}
		return results;
	}

	public static HexCoord[] GetPointsInRing(HexCoord gridPoint, int minRadius, int maxRadius) {
		var points = GetPointsInRing(minRadius, maxRadius);
		for(int i = 0; i < points.Length; i++) points[i] += gridPoint;
		return points;
	}

	public static HexCoord[] GetPointsOnRing(int ringDistance, int startIndex = 0) {
		if(ringDistance == 0) return new[] {zero};
		HexCoord[] results = new HexCoord[ringDistance * 6];
		GetPointsOnRingNonAlloc(results, 0, ringDistance, startIndex);
	    return results;
	}
	public static void GetPointsOnRingNonAlloc(HexCoord[] array, int arrayOffset, int ringDistance, int startIndex = 0) {
		if(ringDistance == 0) {
            array[arrayOffset] = zero;
        } else {
            //    # this code doesn't work for ringDistance == 0; can you see why?
            var cube = (Direction(4)) * ringDistance;
            int k = arrayOffset;
            for(int i = startIndex; i < startIndex+6; i++) {
                for(int j = 0; j < ringDistance; j++) {
                    array[k] = cube;
                    cube = cube.Neighbor(i);
                    k++;
                }
            }
        }
	}

	public static HexCoord[] GetPointsOnRing(HexCoord gridPoint, int ringDistance) {
		var points = GetPointsOnRing(ringDistance);
		for(int i = 0; i < points.Length; i++) points[i] += gridPoint;
		return points;
	}

    public static HexCoord[] GetPointsOnArc(int ringDirectionIndexA, int ringDirectionIndexB, int ringDistance) {
		List<HexCoord> results = new List<HexCoord>();
		if(ringDistance <= 0) { results.Add(zero); return results.ToArray(); }
        while(ringDirectionIndexB < ringDirectionIndexA) ringDirectionIndexB += 6;
		// Walk the ring from the corner toward direction A to the corner toward direction B, inclusive,
		// including the intermediate cells along each side. (The old version returned only the corner
		// tips, leaving gaps for ringDistance > 1.) The side from corner c to c+1 steps along Direction(c+2).
		var cube = Direction(ringDirectionIndexA) * ringDistance;
		results.Add(cube);
		for(int corner = ringDirectionIndexA; corner < ringDirectionIndexB; corner++) {
			var walkDir = Direction(corner + 2);
			for(int j = 0; j < ringDistance; j++) {
				cube += walkDir;
				results.Add(cube);
			}
		}
	    return results.ToArray();
	}

	public static HexCoord[] GetPointsOnArc(HexCoord gridPoint, int ringDirectionIndexA, int ringDirectionIndexB, int ringDistance) {
		var points = GetPointsOnArc(ringDirectionIndexA, ringDirectionIndexB, ringDistance);
		for(int i = 0; i < points.Length; i++) points[i] += gridPoint;
		return points;
	}


	public static HexCoord[] LineDraw(HexCoord a, HexCoord b) {
		var distance = Distance(a, b);
		var results = new HexCoord[distance + 1];
		if(distance == 0) { results[0] = a; return results; }
		var n = 1f/(distance);
	    for (int i = 0; i <= distance; i++) {
			results[i] = RoundFromQRVector(HexLerp(a, b, n * i));
	    }
	    return results;
    }

    /// <summary>
    /// If a straight line can be drawn from a which passes through b
    /// </summary>
    /// <returns>The <see cref="System.Boolean"/>.</returns>
    /// <param name="a">The alpha component.</param>
    /// <param name="b">The blue component.</param>
	public static bool StraightLineExistsBetween (HexCoord a, HexCoord b) {
		var ac = a.AxialToCube();
		var bc = b.AxialToCube();
		return ac.x == bc.x || ac.y == bc.y || ac.z == bc.z;
    }

	// The closest distance to either of the grid-aligned lines from the origin to the target
	public static int GetMinDistanceToCoordOnStraightLine (HexCoord linesOrigin, HexCoord targetCoord) {
		var coords = GetClosestPointsToCoordOnStraightLine(linesOrigin, targetCoord);
		// Scan every returned line cell for the one nearest the target, not just the first.
		int min = int.MaxValue;
		foreach(var c in coords) min = Mathf.Min(min, Distance(targetCoord, c));
		return min == int.MaxValue ? 0 : min;
	}
    /// <summary>
	/// Gets the points on a straight line from a closest to b. Note there can two lines!
	/// Returns all the points between the start and end for each line
    /// </summary>
    /// <returns>The closest points to coordinate on straight line.</returns>
    /// <param name="linesOrigin">The alpha component.</param>
    /// <param name="targetCoord">The blue component.</param>
	public static List<HexCoord> GetClosestPointsToCoordOnStraightLine (HexCoord linesOrigin, HexCoord targetCoord) {

		//NOTE THERES AN ISSUE HERE - Depending on the distance from the line, theres a correlated number of points of equal proximity from that line - not just 2!
		// It should be easily solvable, but I dont know what the intended function is.
		var p = new List<HexCoord>();
		var ac = linesOrigin.AxialToCube();
		var bc = targetCoord.AxialToCube();
		if(ac.x == bc.x || ac.y == bc.y || ac.z == bc.z) p.Add(targetCoord);
		else {
			var xMin = Util.Difference(ac.x, bc.x);
			var yMin = Util.Difference(ac.y, bc.y);
			var zMin = Util.Difference(ac.z, bc.z);
			// Then it's on the x axis
			if(xMin <= yMin && xMin <= zMin) {
				p.AddRange(LineDraw(CubeToAxial(new Vector3Int(ac.x, bc.y+(bc.x-ac.x), bc.z)), CubeToAxial(new Vector3Int(ac.x, bc.y, bc.z+(bc.x-ac.x)))));
			}
			if(yMin <= xMin && yMin <= zMin) {
				p.AddRange(LineDraw(CubeToAxial(new Vector3Int(bc.x+(bc.y-ac.y), ac.y, bc.z)), CubeToAxial(new Vector3Int(bc.x, ac.y, bc.z+(bc.y-ac.y)))));
			}
			if(zMin <= xMin && zMin <= yMin) {
				p.AddRange(LineDraw(CubeToAxial(new Vector3Int(bc.x+(bc.z-ac.z), bc.y, ac.z)), CubeToAxial(new Vector3Int(bc.x, bc.y+(bc.z-ac.z), ac.z))));
			}
		}
		return p;
    }

	/// <summary>
	/// Normalize a rotation index within 0 <= index < cycle.
	/// </summary>
	public static int NormalizeRotationIndex(int index, int cycle = 6) {
		if (index < 0 ^ cycle < 0)
			return (index % cycle + cycle) % cycle;
		else
			return index % cycle;
	}

	/// <summary>
	/// Determine the equality of two rotation indices for a given cycle.
	/// </summary>
	public static bool IsSameRotationIndex(int a, int b, int cycle = 6) {
		return 0 == NormalizeRotationIndex(a - b, cycle);
	}

	public static HexCoord GetClosestDirection (HexCoord a, HexCoord b) {
		if(Distance(a, b) <= 1) return (b - a);
		var direction = Util.NormalizedDirection(a.Position(), b.Position());
		return AtPosition(direction * 1.5f);
	}
	
	public static IEnumerable<HexCoord> GetClosestDirections (HexCoord a, HexCoord b) {
		var fromTo = b - a;
		var qSign = Util.Sign(fromTo.q, true);
		var rSign = Util.Sign(fromTo.r, true);
		var sSign = Util.Sign(fromTo.s, true);
		foreach(var direction in directions) {
			int numSameAxis = 0;
			// This could be micro-optimised by caching the sign of each direction
			if(Util.Sign(direction.q, true) == qSign) numSameAxis++;
			if(Util.Sign(direction.r, true) == rSign) numSameAxis++;
			if(Util.Sign(direction.s, true) == sSign) numSameAxis++;
			if(numSameAxis >= 2) {
				yield return direction;
			}
		}
	}

	public static IEnumerable<int> GetClosestDirectionIndicies (HexCoord a, HexCoord b) {
		foreach(var direction in GetClosestDirections(a,b)) {
			yield return ClosestDirectionIndex(direction);
		}
	}
	// The closest direction index from a to b as a float: halfway between two indices when b lies exactly between
	// them (e.g. 0.5), or the single index otherwise. Result is in 0..6.
	public static float GetClosestDirectionIndexFloat (HexCoord a, HexCoord b) {
		if(a == b) return 0;
		// Average around the circle: a plain average of 0 and 5 gives 2.5, the opposite side of the hex.
		// Each index is measured relative to the first, wrapped into -3..3, then the result is wrapped into 0..6.
		var indices = GetClosestDirections(a,b).Select(direction => ClosestDirectionIndex(direction)).ToList();
		var reference = indices[0];
		var meanOffset = indices.Average(index => (float)(NormalizeRotationIndex(index - reference + 3) - 3));
		var result = reference + meanOffset;
		return result < 0 ? result + 6 : result;
	}
	public static int GetClosestDirectionIndex (HexCoord a, HexCoord b) {
		return ClosestDirectionIndex(GetClosestDirection(a,b));
	}

	/// <summary>
	/// Vector from a hex to a neighbor.
	/// </summary>
	/// <remarks>
	/// Neighbor 0 is to the right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="index">Index of the desired neighbor vector. Cyclically constrained 0..5.</param>
	public static HexCoord Direction(int index)
	{ return directions[NormalizeRotationIndex(index)]; }

	/// <summary>
	/// Enumerate the six neighbor vectors.
	/// </summary>
	/// <remarks>
	/// Neighbor 0 is to the right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="first">Index of the first neighbor vector to enumerate.</param>
	public static IEnumerable<HexCoord> Directions(int first = 0) {
		first = NormalizeRotationIndex(first);
		for (int i = first; i < 6; i++)
			yield return directions[i];
		for (int i = 0; i < first; i++)
	     yield return directions[i];
	}

	public static IEnumerable<HexCoord> Directions(HexCoord c, int first = 0) {
		foreach(var d in Directions(first)) {
			yield return c+d;
		}
	}

	public static int ClosestDirectionIndex (HexCoord direction) {
		var hexCoord = RoundFromQRVector(new Vector2(direction.q, direction.r).normalized);
		return System.Array.IndexOf(directions, hexCoord);
	}

	public static HexCoord RotateDirection (HexCoord direction, int numRotationSteps) {
		int index = ClosestDirectionIndex(direction) + numRotationSteps;
		return Direction(index);
	}

	
	/// <summary>
	/// Neighbor index of 0,0 through which a polar angle passes.
	/// </summary>
	public static int AngleToNeighborIndex(float angle)
	{ return Mathf.RoundToInt(angle / SEXTANT); }
	
	/// <summary>
	/// Polar angle for a neighbor of 0,0.
	/// </summary>
	public static float NeighborIndexToAngle(int index)
	{ return index * SEXTANT; }

	/// <summary>
	/// position vector from hex center to a corner.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
	public static Vector2 CornerVector(int index) {
		index = NormalizeRotationIndex(index);
		// Corner i is the vertex shared with the neighbours toward direction indices (5-i) and (6-i); its
		// offset from the centre is the centroid of this cell and those two neighbours. Derived from the
		// direction vectors (via Position, which follows Q_XY/R_XY), so it's correct for pointy AND flat with
		// no hardcoded angle table, and its index convention matches HexCoordsSharingCornerIndex.
		var directionIndex = 5 - index;
		return (Direction(directionIndex).Position() + Direction(directionIndex + 1).Position()) / 3f;
	}

	/// <summary>
	/// Enumerate the six corner vectors.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="first">Index of the first corner vector to enumerate.</param>
	public static IEnumerable<Vector2> CornerVectors(int first = 0, float hexSize = 1) {
		var cachedCorners = corners;
		if (first == 0) {
            for (int i = 0; i < cachedCorners.Length; i++)
				yield return cachedCorners[i] * hexSize;
        } else {
			first = NormalizeRotationIndex(first);
			for (int i = first; i < 6; i++)
				yield return cachedCorners[i] * hexSize;
			for (int i = 0; i < first; i++)
				yield return cachedCorners[i] * hexSize;
		}
	}

	/// <summary>
	/// Corner of 0,0 closest to a polar angle.
	/// </summary>
	public static int AngleToCornerIndex(float angle)
	{ return Mathf.FloorToInt(angle / SEXTANT); }

	/// <summary>
	/// Polar angle for a corner of 0,0.
	/// </summary>
	public static float CornerIndexToAngle(int index)
	{ return (index + 0.5f) * SEXTANT; }

	/// <summary>
	/// Half sextant of 0,0 through which a polar angle passes.
	/// </summary>
	public static int AngleToHalfSextant(float angle)
	{ return Mathf.RoundToInt(2 * angle / SEXTANT); }

	/// <summary>
	/// Polar angle at which a half sextant begins.
	/// </summary>
	public static float HalfSextantToAngle(int index)
	{ return index * SEXTANT / 2; }
	
	/// <summary>
	/// <see cref="Settworks.Hexagons.HexCoord"/> containing a position.
	/// </summary>
	public static HexCoord AtPosition(Vector2 position) {
		var qr = VectorXYtoQR(position);
		return RoundFromQRVector(qr);
	}
	
	/// <summary>
	/// <see cref="Settworks.Hexagons.HexCoord"/> from hexagonal polar coordinates.
	/// </summary>
	/// <remarks>
	/// Hexagonal polar coordinates approximate a circle to a hexagonal ring.
	/// </remarks>
	/// <param name="radius">Hex distance from 0,0.</param>
	/// <param name="index">Counterclockwise index.</param>
	public static HexCoord AtPolar(int radius, int index) {
		if (radius == 0) return zero;
		if (radius < 0) radius = -radius;
		index = NormalizeRotationIndex(index, radius * 6);
		int sextant = index / radius;
		index %= radius;
		if (sextant == 0) return new HexCoord(radius - index, index);
		if (sextant == 1) return new HexCoord(-index, radius);
		if (sextant == 2) return new HexCoord(-radius, radius - index);
		if (sextant == 3) return new HexCoord(index - radius, -index);
		if (sextant == 4) return new HexCoord(index, -radius);
		return new HexCoord(radius, index - radius);
	}

	/// <summary>
	/// Find the hexagonal polar index closest to angle at radius.
	/// </summary>
	/// <remarks>
	/// Hexagonal polar coordinates approximate a circle to a hexagonal ring.
	/// </remarks>
	/// <param name="radius">Hex distance from 0,0.</param>
	/// <param name="angle">Desired polar angle.</param>
	public static int FindPolarIndex(int radius, float angle) {
		return (int)Mathf.Round(angle * radius * 3 / Mathf.PI);
	}

	/// <summary>
	/// <see cref="Settworks.Hexagons.HexCoord"/> containing a floating-point q,r vector.
	/// </summary>
	/// <remarks>
	/// Hexagonal geometry makes normal rounding inaccurate. If working with floating-point
	/// q,r vectors, use this method to accurately convert them back to
	/// <see cref="Settworks.Hexagons.HexCoord"/>.
	/// </remarks>
	public static HexCoord RoundFromQRVector(Vector2 QRvector) {
		float z = -QRvector.x -QRvector.y;
		int ix = (int)Mathf.Round(QRvector.x);
		int iy = (int)Mathf.Round(QRvector.y);
		int iz = (int)Mathf.Round(z);
		if (ix + iy + iz != 0) {
			float dx = Mathf.Abs(ix - QRvector.x);
			float dy = Mathf.Abs(iy - QRvector.y);
			float dz = Mathf.Abs(iz - z);
			if (dx >= dy && dx >= dz && (-iy-iz != 0))
				ix = -iy-iz;
			else
			if (Util.NearlyEqual(dy, dz) || dy >= dz)
				iy = -ix-iz;
		}
		return new HexCoord(ix, iy);
	}

	/// <summary>
	/// Convert an x,y vector to a q,r vector.
	/// </summary>
	public static Vector2 VectorXYtoQR(Vector2 XYvector) {
		return XYvector.x*X_QR + XYvector.y*Y_QR;
	}
	
	/// <summary>
	/// Convert a q,r vector to an x,y vector.
	/// </summary>
	public static Vector2 VectorQRtoXY(Vector2 QRvector) {
		return QRvector.x*Q_XY + QRvector.y*R_XY;
	}

	/// <summary>
	/// The two opposite corners (component-wise min and max in QR space) of the smallest QR-aligned
	/// rectangle that contains every cell overlapping the given XY-space rectangle. Iterate q in
	/// [min.q, max.q] and r in [min.r, max.r] to visit them. Conservative (may include a few non-touching
	/// cells) but never misses one. Orientation-agnostic: derived from the XY<->QR basis, so it's correct
	/// for pointy and flat alike (unlike the old version, which assumed a pointy half-height of 0.5).
	/// </summary>
	public static HexCoord[] CartesianRectangleBounds(Vector2 cornerA, Vector2 cornerB) {
		// Grow by one circumradius (1 in XY units) so a cell whose centre is just outside the rect but whose
		// hexagon still overlaps it is included.
		Vector2 min = new Vector2(Mathf.Min(cornerA.x, cornerB.x), Mathf.Min(cornerA.y, cornerB.y)) - Vector2.one;
		Vector2 max = new Vector2(Mathf.Max(cornerA.x, cornerB.x), Mathf.Max(cornerA.y, cornerB.y)) + Vector2.one;
		// The XY rect maps to a parallelogram in QR (the basis rotates/shears with orientation), so bound all
		// four mapped corners rather than just min/max.
		Vector2 c0 = VectorXYtoQR(min);
		Vector2 c1 = VectorXYtoQR(new Vector2(max.x, min.y));
		Vector2 c2 = VectorXYtoQR(new Vector2(min.x, max.y));
		Vector2 c3 = VectorXYtoQR(max);
		int minQ = Mathf.FloorToInt(Mathf.Min(Mathf.Min(c0.x, c1.x), Mathf.Min(c2.x, c3.x)));
		int maxQ = Mathf.CeilToInt(Mathf.Max(Mathf.Max(c0.x, c1.x), Mathf.Max(c2.x, c3.x)));
		int minR = Mathf.FloorToInt(Mathf.Min(Mathf.Min(c0.y, c1.y), Mathf.Min(c2.y, c3.y)));
		int maxR = Mathf.CeilToInt(Mathf.Max(Mathf.Max(c0.y, c1.y), Mathf.Max(c2.y, c3.y)));
		return new HexCoord[2] {
			new HexCoord(minQ, minR),
			new HexCoord(maxQ, maxR)
		};
	}

	public static int[] GetCornerIndiciesSharedWithOther (HexCoord coord, HexCoord otherCoord) {
		var distance = Distance(coord, otherCoord);
		if(distance == 0) {
			return new[] {0,1,2,3,4,5};
		} else if(distance == 1) {
			List<int> sharedIndicies = new List<int>();
			for(int i = 0; i < 6; i++) {
				if(GetTouchingCornerPointIndex(coord, i, otherCoord) == -1) continue;
				sharedIndicies.Add(i);
			}
			return sharedIndicies.ToArray();
		}
		return null;
	}

	public static int[] GetCornerIndiciesSharedWithOthers (HexCoord coord, params HexCoord[] otherCoords) {
		List<int> sharedIndicies = new List<int> {0,1,2,3,4,5};
		foreach(var otherCoord in otherCoords) {
			var newShared = GetCornerIndiciesSharedWithOther(coord, otherCoord);
			if(newShared == null) sharedIndicies.Clear();
			else sharedIndicies = newShared.Intersect(sharedIndicies).ToList();
		}
		return sharedIndicies.ToArray();
	}

	// Given a coord and corner index, find the corner index of another coord that shares the same vert.
	public static int GetTouchingCornerPointIndex (HexCoord coord, int coordCornerIndex, HexCoord otherCoord) {
		if(Distance(coord, otherCoord) != 1) return -1;
		int numCorners = 6;
		var cachedCorners = corners;
		
		var coordPosition = coord.Position();
		var otherCoordPosition = otherCoord.Position();

		if(coordCornerIndex < 0 || coordCornerIndex > 5) 
			coordCornerIndex = NormalizeRotationIndex(coordCornerIndex);

		var cornerPoint = coordPosition + cachedCorners[coordCornerIndex];
		for(int otherCornerIndex = 0; otherCornerIndex < numCorners; otherCornerIndex++) {
			var otherCornerPoint = otherCoordPosition + cachedCorners[otherCornerIndex];
			if(Util.SqrDistance(cornerPoint, otherCornerPoint) < 0.1f) {
				return otherCornerIndex;
			}
		}
		return -1;
	}

	// Finds the corner 
	public static int GetBestCornerIndex (HexCoord coord, Vector2 position) {
		var direction = Util.NormalizedDirection(coord.Position(), position);
		
		int index = 0;
		int bestIndex = -1;
		float bestDot = -1f;

		foreach(var corner in CornerVectors()) {
			var dot = Vector2.Dot(corner, direction);
			if(dot > bestDot) {
				bestDot = dot;
				bestIndex = index;
			}
			index++;
		}
		return bestIndex;
	}

	// Returns the two neighbours that share corner `cornerIndex`, in the single corner convention used
	// everywhere (matching CornerVector/Corner): corner i is the vertex toward direction indices (5-i) and
	// (6-i). Consistent for any orientation because it's defined by direction indices, not angles.
	public static HexCoord[] HexCoordsSharingCornerIndex (HexCoord coord, int cornerIndex) {
		cornerIndex = NormalizeRotationIndex(cornerIndex);
		var directionIndex = 5 - cornerIndex;
		return new HexCoord[2] {coord.Neighbor(directionIndex), coord.Neighbor(directionIndex + 1)};
	}

	// --- Corner <-> edge index relationships ----------------------------------------------------------
	// Edges are indexed by the direction they face; corner i is the vertex where directions (5-i) and (6-i)
	// meet (see CornerVector / HexCoordsSharingCornerIndex). Corners and edges therefore flank each other via
	// the same symmetric (5-x)/(6-x) mapping. Pure index maths, so the relationship holds for any
	// orientation / swizzle / transform.

	/// <summary>The two edge (direction) indices that meet at corner <paramref name="cornerIndex"/>.</summary>
	public static int[] EdgesSharingCornerIndex (int cornerIndex) {
		cornerIndex = NormalizeRotationIndex(cornerIndex);
		return new int[2] { NormalizeRotationIndex(5 - cornerIndex), NormalizeRotationIndex(6 - cornerIndex) };
	}

	/// <summary>The two corner indices at the ends of edge <paramref name="edgeIndex"/>.</summary>
	public static int[] CornersSharingEdgeIndex (int edgeIndex) {
		edgeIndex = NormalizeRotationIndex(edgeIndex);
		return new int[2] { NormalizeRotationIndex(5 - edgeIndex), NormalizeRotationIndex(6 - edgeIndex) };
	}

	/// <summary>The edge index between two corners, or -1 if they aren't adjacent (share no edge).</summary>
	public static int EdgeBetweenCorners (int cornerIndexA, int cornerIndexB) {
		var a = EdgesSharingCornerIndex(cornerIndexA);
		var b = EdgesSharingCornerIndex(cornerIndexB);
		if(a[0] == b[0] || a[0] == b[1]) return a[0];
		if(a[1] == b[0] || a[1] == b[1]) return a[1];
		return -1;
	}

	// It's often handy to consider where the edge is in relation to the corners, 
	// but this code has corners that go clockwise (this is fine) but directions which go counterclockwise. 
	// I'm not sure why this is, but I don't dare mess with it. This allows conversion between the two.
	// In this model, direction with index 0 is clockwise of 
	public static float ConvertCornerIndexToDirectionIndex (float cornerIndex) {
		return 5-cornerIndex;
	}

	/*
	 * Constants
	 */

	/// <summary>
	/// One sixth of a full rotation (radians).
	/// </summary>
	public static readonly float SEXTANT = Mathf.PI / 3;
	
	/// <summary>
	/// Square root of 3.
	/// </summary>
	public static readonly float SQRT3 = Mathf.Sqrt(3);

	// The directions array. These are private to prevent overwriting elements.
	static readonly HexCoord[] directions = {
		new HexCoord(1, 0),
		new HexCoord(0, 1),
		new HexCoord(-1, 1),
		new HexCoord(-1, 0),
		new HexCoord(0, -1),
		new HexCoord(1, -1)
	};




	public enum Orientation {
		Flat,
		Pointy,
	}
	public static Orientation orientation => LayoutToOrientation(offsetLayout);

	// Corner locations in XY space, derived from CornerVector (the neighbour-direction centroid) and cached
	// per orientation. Single source of truth - no hardcoded pointy/flat angle tables.
	static Vector2[] corners {
		get {
			if(_cornersCache == null || _cornersCacheOrientation != orientation) {
				_cornersCache = new Vector2[6];
				for(int i = 0; i < 6; i++) _cornersCache[i] = CornerVector(i);
				_cornersCacheOrientation = orientation;
			}
			return _cornersCache;
		}
	}
	static Vector2[] _cornersCache;
	static Orientation _cornersCacheOrientation;

	// Vector transformations between QR and XY space.
	// Private to keep IntelliSense tidy. Safe to make public, but sensible uses are covered above.
	static Vector2 Q_XY => orientation == Orientation.Flat ? Q_XY_Flat : Q_XY_Pointy;
	static Vector2 R_XY => orientation == Orientation.Flat ? R_XY_Flat : R_XY_Pointy;

	static readonly Vector2 Q_XY_Pointy = new Vector2(SQRT3, 0);
	static readonly Vector2 R_XY_Pointy = new Vector2(SQRT3/2, 1.5f);
	static readonly Vector2 Q_XY_Flat = new Vector2(1.5f, SQRT3/2);
	static readonly Vector2 R_XY_Flat = new Vector2(0, SQRT3);

	static Vector2 X_QR => orientation == Orientation.Flat ? X_QR_Flat : X_QR_Pointy;
	static Vector2 Y_QR => orientation == Orientation.Flat ? Y_QR_Flat : Y_QR_Pointy;
	
	static readonly Vector2 X_QR_Pointy = new Vector2(SQRT3/3, 0);
	static readonly Vector2 Y_QR_Pointy = new Vector2(-1/3f, 2/3f);
	static readonly Vector2 X_QR_Flat = new Vector2(2/3f, -1/3f);
	static readonly Vector2 Y_QR_Flat = new Vector2(0, SQRT3/3);

	public enum Layout {
		// Pointy topped
		OddR,
		EvenR,
		// Flat topped
		OddQ,
		EvenQ
	}
	// GLOBAL coordinate convention shared by ALL hex math (offset<->axial conversions, corner/direction
	// vectors, HexCoordVert frames) and their static caches. This is a single process-wide value, NOT
	// per-grid: multiple WorldSpaceHexGrids can coexist, but they must all use the SAME orientation/layout,
	// since whichever writes this last wins. Concurrent grids with different conventions would need this to
	// become instance state (a larger refactor moving layout-dependent geometry onto the grid). In practice
	// this is fixed to OddR-pointy to match Unity's hex grid (WorldSpaceHexGrid.OnValidate enforces it).
	static Layout _offsetLayout = Layout.OddR;
	public static Layout offsetLayout {
		get => _offsetLayout;
		set {
			if(_offsetLayout == value) return;
			_offsetLayout = value;
			if(OnChangeOffsetLayout != null) OnChangeOffsetLayout(_offsetLayout);
		}
	}
	public static Action<Layout> OnChangeOffsetLayout;

	public static Orientation LayoutToOrientation(Layout layout) {
		switch (layout) {
		case Layout.OddR: 
			return Orientation.Pointy;
		case Layout.EvenR: 
			return Orientation.Pointy;
		case Layout.OddQ: 
			return Orientation.Flat;
		default:
			return Orientation.Flat;
		}
	}

	public static HexCoord OffsetToAxial(int x, int y) {
		return OffsetToAxial(new Vector2Int(x, y));
	}
	public static HexCoord OffsetToAxial(Vector2Int offsetCoord) {
		return OffsetToAxial(offsetCoord, offsetLayout);
	}
	public static HexCoord OffsetToAxial(Vector2Int offsetCoord, Layout offsetLayout) {
		switch (offsetLayout) {
		case Layout.OddR: 
			return OddRToAxial(offsetCoord);
		case Layout.EvenR: 
			return EvenRToAxial(offsetCoord);
		case Layout.OddQ: 
			return OddQToAxial(offsetCoord);
		default:
			return EvenQToAxial(offsetCoord);
		}
	}

	public static HexCoord OddRToAxial(Vector2Int hex) {
		var q = hex.x - (hex.y - (hex.y&1)) / 2;
		var r = hex.y;
		return new HexCoord(q, r);
	}
	static HexCoord EvenRToAxial(Vector2Int hex) {
		var q = hex.x - (hex.y + (hex.y&1)) / 2;
		var r = hex.y;
		return new HexCoord(q, r);
	}
	static HexCoord OddQToAxial(Vector2Int hex) {
		var q = hex.x;
		var r = hex.y - (hex.x - (hex.x&1)) / 2;
		return new HexCoord(q, r);
	}
	static HexCoord EvenQToAxial(Vector2Int hex) {
		var q = hex.x;
		var r = hex.y - (hex.x + (hex.x&1)) / 2;
		return new HexCoord(q, r);
	}


	public Vector3Int AxialToCube() {
		return AxialToCube(this);
	}

	public static Vector3Int AxialToCube(HexCoord coord) {
		return new Vector3Int(coord.q, coord.s, coord.r);
	}

	public static HexCoord CubeToAxial(Vector3Int cubeCoord) {
		var q = cubeCoord.x;
		var r = cubeCoord.z;
		return new HexCoord(q, r);
    }

	public Vector2Int ToOffset() {
		return AxialToOffset(this);
	}

	public static Vector2Int AxialToOffset(HexCoord coord) {
		return AxialToOffset(coord, offsetLayout);
	}

	static Vector2Int AxialToOffset(HexCoord coord, Layout mode) {
		switch (mode) {
			case Layout.OddR: return ToOddR(coord);
			case Layout.EvenR: return ToEvenR(coord);
			case Layout.OddQ: return ToOddQ(coord);
			case Layout.EvenQ:
			default: return ToEvenQ(coord);
		}
	}
	
	static Vector2 HexToOffsetInterpolated(Vector2 coord, Layout mode) {
		switch (mode) {
			case Layout.OddR: return ToOddRInterpolated(coord);
			case Layout.EvenR: return ToEvenRInterpolated(coord);
			case Layout.OddQ: return ToOddQInterpolated(coord);
			case Layout.EvenQ:
			default: return ToEvenQInterpolated(coord);
		}
	}
	
	public static Vector2Int ToOddR(HexCoord coord) {
		var x = coord.q + (coord.r - (coord.r&1)) / 2;
		var y = coord.r;
		return new Vector2Int(x, y);
	}

	public static Vector2Int ToEvenR(HexCoord coord) {
		var x = coord.q + (coord.r + (coord.r&1)) / 2;
		var y = coord.r;
		return new Vector2Int(x, y);
	}

	public static Vector2Int ToOddQ(HexCoord coord) {
		var x = coord.q;
		var y = coord.r + (coord.q - (coord.q&1)) / 2;
		return new Vector2Int(x, y);
	}

	public static Vector2Int ToEvenQ(HexCoord coord) {
		var x = coord.q;
		var y = coord.r + (coord.q + (coord.q&1)) / 2;
		return new Vector2Int(x, y);
	}
	
	public static Vector2 ToOddRInterpolated(Vector2 coord) {
		var x = coord.x + (coord.y - ((coord.y < 0 ? Mathf.CeilToInt(coord.y) : Mathf.FloorToInt(coord.y))&1)) / 2f;
		var y = coord.y;
		return new Vector2(x, y);
	}

	public static Vector2 ToEvenRInterpolated(Vector2 coord) {
		var x = coord.x + (coord.y + ((coord.y < 0 ? Mathf.CeilToInt(coord.y) : Mathf.FloorToInt(coord.y))&1)) / 2f;
		var y = coord.y;
		return new Vector2(x, y);
	}

	public static Vector2 ToOddQInterpolated(Vector2 coord) {
		var x = coord.x;
		var y = coord.y + (coord.x - ((coord.x < 0 ? Mathf.CeilToInt(coord.x) : Mathf.FloorToInt(coord.x))&1)) / 2f;
		return new Vector2(x, y);
	}

	public static Vector2 ToEvenQInterpolated(Vector2 coord) {
		var x = coord.x;
		var y = coord.y + (coord.x + ((coord.x < 0 ? Mathf.CeilToInt(coord.x) : Mathf.FloorToInt(coord.x))&1)) / 2f;
		return new Vector2(x, y);
	}


	public static explicit operator Vector2(HexCoord src) {
		return new Vector2(src.q, src.r);
	}



	#region Operators

	public static HexCoord Add(HexCoord left, HexCoord right){
		return new HexCoord(left.q+right.q, left.r+right.r);
	}

	public static HexCoord Add(HexCoord left, int right){
		return new HexCoord(left.q+right, left.r+right);
	}

	public static HexCoord Add(int left, HexCoord right){
		return new HexCoord(left+right.q, left+right.r);
	}


	public static HexCoord Subtract(HexCoord left, HexCoord right){
		return new HexCoord(left.q-right.q, left.r-right.r);
	}

	public static HexCoord Subtract(HexCoord left, int right){
		return new HexCoord(left.q-right, left.r-right);
	}

	public static HexCoord Subtract(int left, HexCoord right){
		return new HexCoord(left-right.q, left-right.r);
	}


	public static HexCoord Multiply(HexCoord left, HexCoord right){
		return new HexCoord(left.q*right.q, left.r*right.r);
	}

	public static HexCoord Multiply(HexCoord left, int right){
		return new HexCoord(left.q*right, left.r*right);
	}

	public static HexCoord Multiply(int left, HexCoord right){
		return new HexCoord(left*right.q, left*right.r);
	}


	public static HexCoord Divide(HexCoord left, HexCoord right){
		return new HexCoord(left.q/right.q, left.r/right.r);
	}

	public static HexCoord Divide(HexCoord left, int right){
		return new HexCoord(left.q/right, left.r/right);
	}

	public static HexCoord Divide(int left, HexCoord right){
		return new HexCoord(left/right.q, left/right.r);
	}


	public static HexCoord operator +(HexCoord left, HexCoord right) {
		return Add(left, right);
	}

	
	public static HexCoord operator -(HexCoord left) {
		return new HexCoord(-left.q, -left.r);
	}

	public static HexCoord operator -(HexCoord left, HexCoord right) {
		return Subtract(left, right);
	}


	public static HexCoord operator *(HexCoord left, HexCoord right) {
		return Multiply(left, right);
	}

	public static HexCoord operator *(HexCoord left, int right) {
		return Multiply(left, right);
	}


	public static HexCoord operator /(HexCoord left, HexCoord right) {
		return Divide(left, right);
	}
	
	public static HexCoord operator /(HexCoord left, int right) {
		return Divide(left, right);
	}

	public override bool Equals(Object obj) {
		return obj is HexCoord && this == (HexCoord)obj;
	}

	public bool Equals(HexCoord p) {
		return q == p.q && r == p.r;
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * 31 + q.GetHashCode();
			hash = hash * 31 + r.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (HexCoord left, HexCoord right) {
		return left.Equals(right);
	}

	public static bool operator != (HexCoord left, HexCoord right) {
		return !(left == right);
	}
	#endregion


	// Map-shape generators (hexagon, rectangle) live in HexUtils - see HexagonPoints and OffsetRectPoints.
	// A triangular-region generator isn't implemented; add one there if ever needed.
}
}
