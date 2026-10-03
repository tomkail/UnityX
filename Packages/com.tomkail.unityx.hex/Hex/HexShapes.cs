using System.Collections.Generic;
using UnityEngine;

namespace UnityX.HexGrid {
	// Standard map shapes, as sets of cells. See redblobgames.com/grids/hexagons/implementation.html#map-shapes.
	// Rotate or mirror a shape cell-by-cell (HexCoord.RotateAround / MirrorThroughCorners) for other orientations.
	public static class HexShapes {
		// Every cell within `radius` of `center`.
		public static IEnumerable<HexCoord> Hexagon (HexCoord center, int radius) {
			for (int q = -radius; q <= radius; q++) {
				int rFrom = Mathf.Max(-radius, -q - radius);
				int rTo = Mathf.Min(radius, -q + radius);
				for (int r = rFrom; r <= rTo; r++)
					yield return center + new HexCoord(q, r);
			}
		}

		// The axial parallelogram spanning `min` to `max` (inclusive) on the q and r axes. Rotate it by one sextant
		// for the r/s parallelogram, two for s/q.
		public static IEnumerable<HexCoord> Parallelogram (HexCoord min, HexCoord max) {
			for (int q = min.q; q <= max.q; q++)
				for (int r = min.r; r <= max.r; r++)
					yield return new HexCoord(q, r);
		}

		// A triangle with `size`+1 cells along each side and a corner at 0,0. `flipped` gives the triangle pointing
		// the other way, filling the rest of the size×size parallelogram plus its diagonal.
		public static IEnumerable<HexCoord> Triangle (int size, bool flipped = false) {
			for (int q = 0; q <= size; q++) {
				int rFrom = flipped ? size - q : 0;
				int rTo = flipped ? size : size - q;
				for (int r = rFrom; r <= rTo; r++)
					yield return new HexCoord(q, r);
			}
		}

		// A rectangle in offset coordinates (rows and columns), using the global HexCoord.offsetLayout.
		public static IEnumerable<HexCoord> Rectangle (RectInt offsetRect) {
			for (int y = offsetRect.yMin; y < offsetRect.yMax; y++)
				for (int x = offsetRect.xMin; x < offsetRect.xMax; x++)
					yield return HexCoord.OffsetToAxial(x, y);
		}
	}
}
