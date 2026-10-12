using System;
using System.Collections.Generic;

namespace UnityX.HexGrid {
// Diagonals, rotation/reflection about any centre, and range intersection.
//
// Index conventions match the rest of HexCoord: direction (and edge) i faces Neighbor(i); corner c is the vertex
// between directions (5-c) and (6-c). Everything here is pure axial/cube maths, so it holds for any layout.
public partial struct HexCoord {

	/*
	 * Diagonals
	 */

	/// <summary>
	/// Vector from a hex to the diagonal neighbour beyond corner <paramref name="cornerIndex"/>: the cell two steps
	/// away that touches this one only at that corner. Cyclically constrained 0..5.
	/// </summary>
	public static HexCoord Diagonal(int cornerIndex) {
		var directionIndex = 5 - NormalizeRotationIndex(cornerIndex);
		return Direction(directionIndex) + Direction(directionIndex + 1);
	}

	/// <summary>
	/// Enumerate the six diagonal vectors, indexed by corner.
	/// </summary>
	public static IEnumerable<HexCoord> Diagonals(int first = 0) {
		for (int i = 0; i < 6; i++)
			yield return Diagonal(first + i);
	}

	/// <summary>
	/// The diagonal neighbour beyond corner <paramref name="cornerIndex"/>.
	/// </summary>
	public HexCoord DiagonalNeighbor(int cornerIndex) {
		return this + Diagonal(cornerIndex);
	}

	/// <summary>
	/// Enumerate this hex's six diagonal neighbours, indexed by corner.
	/// </summary>
	public IEnumerable<HexCoord> DiagonalNeighbors(int first = 0) {
		foreach (var diagonal in Diagonals(first))
			yield return this + diagonal;
	}

	/*
	 * Rotation and reflection about a centre
	 */

	/// <summary>
	/// Rotate around <paramref name="center"/> in sextant increments. Positive sextants turn toward higher direction
	/// indices (Direction(i) becomes Direction(i + sextants)), as <see cref="SextantRotation"/> does around 0,0.
	/// </summary>
	public HexCoord RotateAround(HexCoord center, int sextants) {
		return (this - center).SextantRotation(sextants) + center;
	}

	/// <summary>
	/// Mirror across the axis through 0,0 and corner <paramref name="cornerIndex"/> (and the opposite corner).
	/// Cyclically constrained, so corners c and c+3 give the same axis.
	/// </summary>
	public HexCoord MirrorThroughCorners(int cornerIndex) {
		return Mirror(2 - cornerIndex);
	}

	/// <summary>
	/// Mirror across the axis through <paramref name="center"/> and its corner <paramref name="cornerIndex"/>.
	/// </summary>
	public HexCoord MirrorThroughCorners(HexCoord center, int cornerIndex) {
		return (this - center).MirrorThroughCorners(cornerIndex) + center;
	}

	/// <summary>
	/// Mirror across the axis through 0,0 and the neighbour in direction <paramref name="directionIndex"/>, which
	/// crosses the midpoints of edges directionIndex and directionIndex+3. Cyclically constrained, so directions d and
	/// d+3 give the same axis.
	/// </summary>
	public HexCoord MirrorThroughEdges(int directionIndex) {
		// Each edge axis is perpendicular to a corner axis, so this is that corner-axis reflection negated.
		return -Mirror(directionIndex + 1);
	}

	/// <summary>
	/// Mirror across the axis through <paramref name="center"/> and its neighbour in direction
	/// <paramref name="directionIndex"/>.
	/// </summary>
	public HexCoord MirrorThroughEdges(HexCoord center, int directionIndex) {
		return (this - center).MirrorThroughEdges(directionIndex) + center;
	}

	/*
	 * Range intersection
	 */

	/// <summary>
	/// Every hex within <paramref name="rangeA"/> of <paramref name="a"/> and within <paramref name="rangeB"/> of
	/// <paramref name="b"/>. Empty if the ranges don't overlap.
	/// </summary>
	public static List<HexCoord> GetPointsInRangeIntersection(HexCoord a, int rangeA, HexCoord b, int rangeB) {
		return GetPointsInRangeIntersection((a, rangeA), (b, rangeB));
	}

	/// <summary>
	/// Every hex within range of all the given centres. Walks the overlap's cube-coordinate bounds directly rather
	/// than testing each cell of one range against the others.
	/// </summary>
	public static List<HexCoord> GetPointsInRangeIntersection(params (HexCoord center, int range)[] ranges) {
		if (ranges == null || ranges.Length == 0) throw new ArgumentException("Need at least one range", nameof(ranges));

		int qMin = int.MinValue, qMax = int.MaxValue;
		int rMin = int.MinValue, rMax = int.MaxValue;
		int sMin = int.MinValue, sMax = int.MaxValue;
		foreach (var (center, range) in ranges) {
			qMin = Math.Max(qMin, center.q - range); qMax = Math.Min(qMax, center.q + range);
			rMin = Math.Max(rMin, center.r - range); rMax = Math.Min(rMax, center.r + range);
			sMin = Math.Max(sMin, center.s - range); sMax = Math.Min(sMax, center.s + range);
		}

		var results = new List<HexCoord>();
		for (int q = qMin; q <= qMax; q++) {
			int rFrom = Math.Max(rMin, -q - sMax);
			int rTo = Math.Min(rMax, -q - sMin);
			for (int r = rFrom; r <= rTo; r++)
				results.Add(new HexCoord(q, r));
		}
		return results;
	}
}
}
