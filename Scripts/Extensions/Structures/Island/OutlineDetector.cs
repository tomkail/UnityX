using UnityEngine;
using System;
using System.Collections.Generic;

public static class OutlineDetector {
	public static List<Vector2> GetOutlinePoly<Coord> (List<Coord> points, Func<Coord, int, Coord, int> GetTouchingCornerPointIndex, Func<Coord, int, Vector2> GetCornerPoint, int numCorners) where Coord : IEquatable<Coord> {
		var outline = new List<Vector2>();
		// List<HexCoord> hexPoints = points.Cast<HexCoord>().ToList();
		Coord currentCoord = default(Coord);
		int rotIndex = -1;

		// Get start coord/corner
		bool found = true;
		foreach(var testCoord in points) {
			found = true;
			for(int i = 0; i < numCorners; i++) {
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
        // var startCoord = currentCoord;
		// var startRotIndex = rotIndex;

		// Execute
		// We'll break out of this loop before this loop ends, but that's fine - it's just a safe while loop.
		for(int n = 0; n < 1000; n++) {
			bool foundNext = false;
			for(int i = rotIndex+1; i <= rotIndex + numCorners; i++) {
                var repeatingI = i%numCorners;
				var cornerPoint = GetCornerPoint(currentCoord, repeatingI);
				if(outline.Count > 0 && outline.First() == cornerPoint) return outline;
				outline.Add(cornerPoint);

                Coord bestAdjacentCoord = default(Coord);
                int bestAdjacentCoordCorner = -1;
                float bestAdjacentCoordCornerDelta = Mathf.Infinity;
				foreach(var otherCoord in points) {
					if(otherCoord.Equals(currentCoord)) continue;
                    
                    var cornerTouchingCell = GetTouchingCornerPointIndex(currentCoord, repeatingI, otherCoord);
                    if(cornerTouchingCell == -1) continue;
                    
                    var cornerDelta = MathX.RepeatInRange(-numCorners * 0.5f, numCorners * 0.5f, cornerTouchingCell-i);
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
		// Find a starting cell and vert.
		// Rotate around that cell's verts while the vert is not touching any other cells
		// If it is, keep going using the cell that we're touching and the rotation index where it touches.
		return outline;
	}
	
	// Outline distance of 0 is the edge of the shape and 1 is outside the shape
	public static IEnumerable<Coord> GetOutlineCoords<Coord> (List<Coord> points, int outlineDistance, Func<Coord, int, IList<Coord>> GetCoordsOnRing) where Coord : IEquatable<Coord> {
		HashSet<Coord> outline = null;
		// if(outlineDistance != 0) 
			outline = new HashSet<Coord>();
		foreach(var point in points) {
			bool all = true;
			foreach(var adjacentPoint in GetCoordsOnRing(point, 1)) {
				if(!points.Contains(adjacentPoint)) {
					all = false;
					break;
				}
			}
			if(!all) {
				// if(outlineDistance == 0) {
				// 	yield return point;	
				// } else {
					outline.Add(point);
				// }
			}
		};
		// foreach(var x in outline) {
		// 	yield return x;
		// }yield break;	
		
		Dictionary<Coord, int> coordDistanceDictionary = new Dictionary<Coord, int>();
		Dictionary<Coord, int> coordSignDictionary = new Dictionary<Coord, int>();
		// HashSet<Coord> pointsToSearch = new HashSet<Coord>(points);
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