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

		// One connected piece of a traced region: its outer boundary plus any holes inside it.
		// Outer loops wind the same way as a single cell's corners; holes wind the opposite way.
		public class OutlineShape {
			public List<Vector2> outer;
			public List<List<Vector2>> holes = new List<List<Vector2>>();
		}

		// Like GetOutlinePoly, but returns every boundary loop, so it handles disconnected sets and sets with holes
		// (a ring of land around a lake comes back as one shape with one hole instead of a filled-in blob).
		// Works on edges rather than walking cells: each cell contributes its edges in corner order, an edge shared by
		// two cells appears once in each direction and cancels, and what's left chains into closed loops.
		//   GetCornerPoint(coord, cornerIndex) — the position of a coord's corner.
		//   numCorners — corners per cell (4 for squares, 6 for hexes).
		//   weldDistance — corners closer than this are treated as the same point (corner positions computed from
		//       neighbouring cells rarely match exactly).
		// Limitation: where only a corner is shared (squares touching diagonally — can't happen on a hex grid) the two
		// loops through that corner may be joined into one self-touching loop.
		public static List<OutlineShape> GetOutlineLoops<Coord> (IEnumerable<Coord> points, Func<Coord, int, Vector2> GetCornerPoint, int numCorners, float weldDistance = 0.001f) {
			Vector2Int Key (Vector2 p) => new Vector2Int(Mathf.RoundToInt(p.x / weldDistance), Mathf.RoundToInt(p.y / weldDistance));

			// Directed boundary edges, keyed (from, to). Adding an edge whose reverse is already present cancels both.
			var edges = new HashSet<(Vector2Int, Vector2Int)>();
			var positions = new Dictionary<Vector2Int, Vector2>();
			foreach(var coord in points) {
				for(int i = 0; i < numCorners; i++) {
					var a = GetCornerPoint(coord, i);
					var b = GetCornerPoint(coord, (i + 1) % numCorners);
					var ka = Key(a);
					var kb = Key(b);
					positions[ka] = a;
					positions[kb] = b;
					if(!edges.Remove((kb, ka))) edges.Add((ka, kb));
				}
			}
			// Outer loops wind like a cell; holes wind the other way. Measure one cell to find out which way that is.
			float cellWinding = 0;
			foreach(var coord in points) {
				var corners = new List<Vector2>();
				for(int i = 0; i < numCorners; i++) corners.Add(GetCornerPoint(coord, i));
				cellWinding = SignedArea(corners);
				break;
			}

			var next = new Dictionary<Vector2Int, Vector2Int>();
			foreach(var (from, to) in edges) next[from] = to;

			// Chain edges into loops.
			var outers = new List<List<Vector2>>();
			var holes = new List<List<Vector2>>();
			while(next.Count > 0) {
				var loop = new List<Vector2>();
				Vector2Int start = default;
				foreach(var key in next.Keys) { start = key; break; }
				var current = start;
				do {
					loop.Add(positions[current]);
					var to = next[current];
					next.Remove(current);
					current = to;
				} while(current != start && next.ContainsKey(current));

				(Mathf.Sign(SignedArea(loop)) == Mathf.Sign(cellWinding) ? outers : holes).Add(loop);
			}

			// Give each hole to the smallest outer loop that contains it (an island in a lake in an island gets the inner one).
			var shapes = new List<OutlineShape>();
			foreach(var outer in outers) shapes.Add(new OutlineShape { outer = outer });
			foreach(var hole in holes) {
				OutlineShape best = null;
				float bestArea = float.MaxValue;
				foreach(var shape in shapes) {
					if(!ContainsPoint(shape.outer, hole[0])) continue;
					var area = Mathf.Abs(SignedArea(shape.outer));
					if(area < bestArea) {
						best = shape;
						bestArea = area;
					}
				}
				best?.holes.Add(hole);
			}
			return shapes;
		}

		static float SignedArea (List<Vector2> loop) {
			float area = 0;
			for(int i = 0; i < loop.Count; i++) {
				var a = loop[i];
				var b = loop[(i + 1) % loop.Count];
				area += a.x * b.y - b.x * a.y;
			}
			return area * 0.5f;
		}

		// Even-odd ray cast.
		static bool ContainsPoint (List<Vector2> loop, Vector2 p) {
			bool inside = false;
			for(int i = 0, j = loop.Count - 1; i < loop.Count; j = i++) {
				if((loop[i].y > p.y) != (loop[j].y > p.y) && p.x < (loop[j].x - loop[i].x) * (p.y - loop[i].y) / (loop[j].y - loop[i].y) + loop[i].x)
					inside = !inside;
			}
			return inside;
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
