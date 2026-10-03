using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityX.HexGrid {
	public static class HexUtils {
	    public static float HexCoordDirectionToDegrees (HexCoord coord) {
			return Util.Degrees(coord.DirectionVector());
		}

	    // Gets the difference between the forward direction of unit and the direction to a target point
	    public static int DeltaDirectionBetweenForwardAndDirectionToTarget (HexCoord gridPoint, int directionIndex, HexCoord targetPoint) {
	        var directionIndexToTarget = HexCoord.GetClosestDirectionIndex(gridPoint, targetPoint);
			return HexUtils.SignedDeltaDirection(directionIndex, directionIndexToTarget);
	    }
	    public static int SignedDeltaDirection (int dirIndex1, int dirIndex2) {
	        var deltaDirection = dirIndex2 - dirIndex1;
			return Util.Mod(deltaDirection + 3, 6) - 3;
	    }
	    public static int RotateTowards (int currentDirectionIndex, int targetDirectionIndex, int maxRotationSteps) {
	        var deltaDirection = SignedDeltaDirection(currentDirectionIndex, targetDirectionIndex);
	        // Normalize so the result is always a valid 0..5 direction index rather than leaking out of range.
	        return HexCoord.NormalizeRotationIndex(currentDirectionIndex + Mathf.Clamp(deltaDirection, -maxRotationSteps, maxRotationSteps));
	    }

		public static List<HexCoord> OffsetRectPoints (Vector2Int rectSize) {
	        return OffsetRectPoints(new RectInt(Vector2Int.zero, rectSize));
	    }

	    public static List<HexCoord> OffsetRectPoints (RectInt gridSize) {
	        return new List<HexCoord>(HexShapes.Rectangle(gridSize));
	    }

	    public static IEnumerable<HexCoord> HexagonPoints (int circumference) {
	        int smallRadius = Mathf.FloorToInt(circumference * 0.5f);
	        int largeRadius = Mathf.CeilToInt(circumference * 0.5f);
	        for (int q = -smallRadius; q <= largeRadius; q++) {
	            int r1 = Mathf.Max(-smallRadius, -q - smallRadius);
	            int r2 = Mathf.Min(largeRadius, -q + largeRadius);
	            for (int r = r1; r <= r2; r++) {
	                var coord = new HexCoord(q, r);
	                yield return coord;
	            }
	        }
	    }

		// Corner offset vectors of a regular hexagon (side length `hexSize`) in a flat 2D frame, where corner c
		// sits at angle (2*PI * -c / 6) measured from +X. Grid-free: use this as a fallback for laying out a
		// hexagon when no live WorldSpaceHexGrid is available (edit-time / tests). For cells that match Unity's
		// actual grid (any swizzle/layout/size), use WorldSpaceHexGrid.GetCornerOffsets2D() instead.
		public static Vector2[] RegularCornerVectors2D (float hexSize = 1) {
			var corners = new Vector2[6];
			for (int c = 0; c < 6; c++) {
				var angle = 2f * Mathf.PI * (0f - c) / 6f;
				corners[c] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * hexSize;
			}
			return corners;
		}

		public static Vector2 RegularCornerVector2D (int corner) {
			return RegularCornerVectors2D()[((corner % 6) + 6) % 6];
		}
	}
}
