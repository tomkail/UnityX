using UnityEngine;
using System;
using System.Collections.Generic;

namespace UnityX.Islands {
	// Traces the outline of a set of coordinates. Two flavours, both generic over the coordinate type:
	//
	//   • GetOutlinePoly   — walks the boundary corner-by-corner and returns a closed polygon
	//                        (List<Vector2>) suitable for rendering an outline mesh/line.
	//   • GetOutlineCoords — returns the *cells* that form a ring at a given signed distance from the
	//                        edge (0 = the edge, +N = N cells outside, -N = N cells inside).
	//
	// Like IslandDetector, all grid-shape knowledge is supplied through callbacks (corner lookups,
	// ring lookups), so this works for square grids, hex grids, etc.
	public static class OutlineDetector {

		// Wraps val into the half-open range [a, b) (b exclusive). Kept local so this module stays dependency-free.
		static float RepeatInRange (float a, float b, float val) {
			if(a == b) return val;
			b -= a;
			val -= a;
			val = Mathf.Repeat(val, b);
			return val + a;
		}

		// Walks the boundary of `points` and returns it as a closed polygon of corner positions.
		//   GetTouchingCornerPointIndex(coord, cornerIndex, otherCoord) — the corner index at which
		//       `otherCoord` touches `coord`'s corner `cornerIndex`, or -1 if they don't touch.
		//   GetCornerPoint(coord, cornerIndex) — the world position of a coord's corner.
		//   numCorners — corners per cell (4 for squares, 6 for hexes).
		public static List<Vector2> GetOutlinePoly<Coord> (List<Coord> points, Func<Coord, int, Coord, int> GetTouchingCornerPointIndex, Func<Coord, int, Vector2> GetCornerPoint, int numCorners) where Coord : IEquatable<Coord> {
			var outline = new List<Vector2>();
			Coord currentCoord = default(Coord);
			int rotIndex = -1;

			// Find a start coord + corner: a corner on the boundary is one that no other coord touches.
			bool found = true;
			foreach(var testCoord in points) {
				for(int i = 0; i < numCorners; i++) {
					// (re)initialise per corner: 'found' means "this corner touches no other coord"
					found = true;
					foreach(var otherCoord in points) {
						if(testCoord.Equals(otherCoord)) continue;
						var cornerTouchingCell = GetTouchingCornerPointIndex(testCoord, i, otherCoord);
						if(cornerTouchingCell != -1) {
							found = false;
							break;
						}
					}
					if(found) {
						rotIndex = i;
						break;
					}
				}
				if(found) {
					currentCoord = testCoord;
					break;
				}
			}

			// Walk the boundary: rotate around the current cell's corners while a corner touches no
			// other cell; when one does, hand off to the touching cell and continue from that corner.
			// Bounded loop (cap 1000) standing in for a while-loop; normally breaks early.
			// WARNING: if it ever hits the cap it falls through and returns a PARTIAL outline.
			for(int n = 0; n < 1000; n++) {
				bool foundNext = false;
				for(int i = rotIndex+1; i <= rotIndex + numCorners; i++) {
					var repeatingI = i % numCorners;
					var cornerPoint = GetCornerPoint(currentCoord, repeatingI);
					// Closed the loop back to the first corner → done.
					if(outline.Count > 0 && outline[0] == cornerPoint) return outline;
					outline.Add(cornerPoint);

					Coord bestAdjacentCoord = default(Coord);
					int bestAdjacentCoordCorner = -1;
					float bestAdjacentCoordCornerDelta = Mathf.Infinity;
					foreach(var otherCoord in points) {
						if(otherCoord.Equals(currentCoord)) continue;

						var cornerTouchingCell = GetTouchingCornerPointIndex(currentCoord, repeatingI, otherCoord);
						if(cornerTouchingCell == -1) continue;

						var cornerDelta = RepeatInRange(-numCorners * 0.5f, numCorners * 0.5f, cornerTouchingCell - i);
						if(cornerDelta < bestAdjacentCoordCornerDelta) {
							foundNext = true;
							bestAdjacentCoord = otherCoord;
							bestAdjacentCoordCorner = cornerTouchingCell;
							bestAdjacentCoordCornerDelta = cornerDelta;
						}
					}
					if(foundNext) {
						currentCoord = bestAdjacentCoord;
						rotIndex = bestAdjacentCoordCorner;
						break;
					}
				}
				if(!foundNext) break;
			}
			return outline;
		}

		// Returns the cells forming a ring at a signed distance from the edge of `points`.
		// outlineDistance: 0 = the edge itself, positive = outside, negative = inside (interior rings).
		//   GetCoordsOnRing(coord, radius) — the coords at ring `radius` around `coord`.
		public static IEnumerable<Coord> GetOutlineCoords<Coord> (List<Coord> points, int outlineDistance, Func<Coord, int, IList<Coord>> GetCoordsOnRing) where Coord : IEquatable<Coord> {
			HashSet<Coord> outline = new HashSet<Coord>();
			// A point is on the edge if any of its immediate neighbours is outside the set.
			foreach(var point in points) {
				bool all = true;
				foreach(var adjacentPoint in GetCoordsOnRing(point, 1)) {
					if(!points.Contains(adjacentPoint)) {
						all = false;
						break;
					}
				}
				if(!all) {
					outline.Add(point);
				}
			}

			// From every edge cell, walk outward/inward up to |outlineDistance| rings, recording the
			// nearest signed distance to each cell reached (negative = inside the set, positive = outside).
			Dictionary<Coord, int> coordDistanceDictionary = new Dictionary<Coord, int>();
			Dictionary<Coord, int> coordSignDictionary = new Dictionary<Coord, int>();
			foreach(var point in outline)
				coordDistanceDictionary.Add(point, 0);
			foreach(var point in outline) {
				for(int i = 1; i <= Mathf.Max(1, Mathf.Abs(outlineDistance)); i++) {
					foreach(var adjacentPoint in GetCoordsOnRing(point, i)) {
						int sign;
						if(!coordSignDictionary.TryGetValue(adjacentPoint, out sign)) {
							sign = points.Contains(adjacentPoint) ? -1 : 1;
							coordSignDictionary.Add(adjacentPoint, sign);
						}

						int currentDistance;
						if(!coordDistanceDictionary.TryGetValue(adjacentPoint, out currentDistance)) {
							coordDistanceDictionary.Add(adjacentPoint, i * sign);
						} else coordDistanceDictionary[adjacentPoint] = Mathf.Min(Mathf.Abs(currentDistance), i) * sign;
					}
				}
			}
			foreach(var x in coordDistanceDictionary) {
				if(x.Value == outlineDistance) yield return x.Key;
			}
		}
	}
}
