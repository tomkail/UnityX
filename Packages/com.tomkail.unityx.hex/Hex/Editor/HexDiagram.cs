using System.Collections.Generic;
using UnityEngine;

namespace UnityX.HexGrid {
	// Pure geometry and index maths behind the inspectors' little hexagon diagrams, kept free of any GUI calls so
	// it can be unit tested. Everything is in GUI space (x right, y DOWN) and draws the canonical pointy-topped
	// hexagon: side i faces HexCoord.Direction(i), so side 0 faces right and indices run counter-clockwise on screen
	// (1 = upper right, 2 = upper left, ...). That's also how a default (XZY) WorldSpaceHexGrid looks from above.
	public static class HexDiagram {
		public const int Sides = 6;
		static readonly float Cos30 = Mathf.Sqrt(3f) * 0.5f;

		public static int Mod (int value, int modulus) {
			return ((value % modulus) + modulus) % modulus;
		}

		// Unit vector from the centre toward side `side` (the direction that side faces).
		public static Vector2 SideNormal (int side) {
			float a = Mod(side, Sides) * 60f * Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(a), -Mathf.Sin(a));
		}

		// Largest circumradius whose pointy hexagon fits inside `rect`, less `padding` pixels.
		public static float FitRadius (Rect rect, float padding = 0f) {
			return Mathf.Max(0f, Mathf.Min(rect.height * 0.5f, rect.width / (2f * Cos30)) - padding);
		}

		// Vertex between side `side` and side `side + 1`.
		public static Vector2 Corner (Vector2 centre, float radius, int side) {
			float a = (Mod(side, Sides) * 60f + 30f) * Mathf.Deg2Rad;
			return centre + new Vector2(Mathf.Cos(a), -Mathf.Sin(a)) * radius;
		}

		// The two vertices bounding side `side`, in counter-clockwise order.
		public static void SideEndpoints (Vector2 centre, float radius, int side, out Vector2 a, out Vector2 b) {
			a = Corner(centre, radius, side - 1);
			b = Corner(centre, radius, side);
		}

		public static Vector2 SideMidpoint (Vector2 centre, float radius, int side) {
			return centre + SideNormal(side) * radius * Cos30;
		}

		// Counter-clockwise angle on screen in degrees (0 = right, 90 = up), 0..360.
		public static float ScreenAngle (Vector2 offset) {
			return Mathf.Repeat(Mathf.Atan2(-offset.y, offset.x) * Mathf.Rad2Deg, 360f);
		}

		// The side whose wedge contains `point`, or -1 if the point is in the central dead zone (a fraction
		// `deadZone` of the radius) or well outside the hexagon. Wedges are generous (they extend a little past
		// the outline) so small diagrams are easy to hit.
		public static int SideAtPoint (Vector2 centre, float radius, Vector2 point, float deadZone = 0.2f) {
			var offset = point - centre;
			float distance = offset.magnitude;
			if(radius <= 0f || distance < radius * deadZone || distance > radius * 1.2f) return -1;
			return Mod(Mathf.RoundToInt(ScreenAngle(offset) / 60f), Sides);
		}

		// Which of the six sides a HexArc covers (indices wrapped into 0..5).
		public static bool[] ArcCoverage (int initialDirectionIndex, int signedSteps) {
			var covered = new bool[Sides];
			var arc = new HexArc(initialDirectionIndex, signedSteps);
			foreach(var index in arc.directionIndicesCovered) covered[Mod(index, Sides)] = true;
			return covered;
		}

		// Signed step count that grows/shrinks an arc starting at `initialDirectionIndex` so it ends on `target`,
		// keeping the current direction of travel (counter-clockwise when the arc is empty). Clicking the start of a
		// one-side arc empties it.
		public static int StepsToReach (int initialDirectionIndex, int signedSteps, int target) {
			int sign = signedSteps < 0 ? -1 : 1;
			int delta = Mod((target - initialDirectionIndex) * sign, Sides);
			if(delta == 0 && Mathf.Abs(signedSteps) == 1) return 0;
			return sign * (delta + 1);
		}

		// The same run of sides walked the other way: starts where it used to end. Covers the same sides.
		public static HexArc Reversed (HexArc arc) {
			if(arc.signedSteps == 0) return arc;
			return new HexArc(Mod(arc.finalDirectionIndex, Sides), -arc.signedSteps);
		}

		// Side flags rotated by `offset` sides; positive turns counter-clockwise, matching HexEdgeDirections.Rotate.
		public static bool[] Rotated (IList<bool> sides, int offset) {
			var rotated = new bool[sides.Count];
			for(int i = 0; i < sides.Count; i++) rotated[Mod(i + offset, sides.Count)] = sides[i];
			return rotated;
		}

		// Comma separated indices of the set sides, e.g. "0, 2, 4", or "none".
		public static string SidesToString (IList<bool> sides) {
			var parts = new List<string>();
			for(int i = 0; i < sides.Count; i++) if(sides[i]) parts.Add(i.ToString());
			return parts.Count == 0 ? "none" : string.Join(", ", parts);
		}

		// --- HexMap helpers -------------------------------------------------------------------------------------

		// Indices of entries that a later entry with the same coord overrides. HexMap rebuilds its dictionary in
		// list order, so the last duplicate wins and these earlier ones are silently dropped.
		public static HashSet<int> OverriddenIndices (IList<HexCoord> coords) {
			var overridden = new HashSet<int>();
			var lastIndex = new Dictionary<HexCoord, int>();
			for(int i = 0; i < coords.Count; i++) {
				if(lastIndex.TryGetValue(coords[i], out var previous)) overridden.Add(previous);
				lastIndex[coords[i]] = i;
			}
			return overridden;
		}

		// The first coord not in `used`, searching outward ring by ring from the origin. Lets "add cell" produce a
		// fresh, nearby key rather than a duplicate that would be dropped on save.
		public static HexCoord NextFreeCoord (ICollection<HexCoord> used) {
			if(!used.Contains(HexCoord.zero)) return HexCoord.zero;
			for(int ring = 1; ring < 1000; ring++) {
				foreach(var coord in HexCoord.GetPointsOnRing(ring)) {
					if(!used.Contains(coord)) return coord;
				}
			}
			return HexCoord.zero;
		}

		// GUI-space centre of `coord` in a canonical pointy layout with unit circumradius (y flipped to point down).
		public static Vector2 CoordToDiagram (HexCoord coord) {
			var p = coord.Position();
			return new Vector2(p.x, -p.y);
		}

		// Inverse of placing cells at `origin + CoordToDiagram(coord) * scale`: the cell under a GUI-space point.
		public static HexCoord DiagramToCoord (Vector2 point, Vector2 origin, float scale) {
			if(scale <= 0f) return HexCoord.zero;
			var p = (point - origin) / scale;
			return HexCoord.AtPosition(new Vector2(p.x, -p.y));
		}

		// Scale and offset that fit a set of cells (unit circumradius) into `rect`. Returns the circumradius in
		// pixels; `origin` is where HexCoord.zero lands.
		public static float FitCoords (IList<HexCoord> coords, Rect rect, float maxRadius, out Vector2 origin) {
			if(coords.Count == 0) {
				origin = rect.center;
				return maxRadius;
			}
			var min = new Vector2(float.MaxValue, float.MaxValue);
			var max = new Vector2(float.MinValue, float.MinValue);
			foreach(var coord in coords) {
				var p = CoordToDiagram(coord);
				min = Vector2.Min(min, p);
				max = Vector2.Max(max, p);
			}
			// Each cell reaches cos30 sideways and 1 vertically beyond its centre.
			min -= new Vector2(Cos30, 1f);
			max += new Vector2(Cos30, 1f);
			var size = max - min;
			float scale = Mathf.Min(maxRadius, rect.width / size.x, rect.height / size.y);
			origin = rect.center - (min + size * 0.5f) * scale;
			return scale;
		}

		// --- AngleArc helpers -----------------------------------------------------------------------------------

		// A range is well formed when it lies within 0..360 and doesn't run backwards.
		public static bool IsValidAngleRange (Vector2 range) {
			return range.x >= 0f && range.y <= 360f && range.x <= range.y;
		}

		// Total degrees spanned by the valid ranges (overlaps are counted twice; ranges normally don't overlap).
		public static float TotalDegrees (IList<Vector2> ranges) {
			float total = 0f;
			foreach(var range in ranges) if(IsValidAngleRange(range)) total += range.y - range.x;
			return total;
		}
	}
}
