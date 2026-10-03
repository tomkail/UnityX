using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityX.HexGrid {
	[Flags]
	public enum HexSightOcclusion {
		None = 0,
		// Sight can't pass through this cell; it shadows the whole hexagon's angular extent beyond it.
		Block = 1 << 0,
		// This cell can't be seen itself, but doesn't stop sight passing through.
		Mask = 1 << 1,
	}

	// Hex field of view by ring-by-ring shadowcasting over angle ranges. Starting with the viewing arc, each ring out
	// from the origin removes the angles covered by its blocking cells; a cell is visible if the angle to its centre
	// is still open when its ring is reached. Blockers shadow their full corner-to-corner extent, but visibility only
	// needs the centre, so grazing a cell's edge doesn't count as seeing it.
	//
	// Ported from ArcadeTactics' HexViewCone, with the board lookup replaced by an occlusion callback.
	public static class HexFieldOfView {
		/// <summary>
		/// Cells visible from <paramref name="origin"/> within <paramref name="arc"/>, between
		/// <paramref name="nearClip"/> and <paramref name="farClip"/> rings (inclusive).
		/// </summary>
		/// <param name="occlusion">How each cell affects sight. Return Block for cells off the map if sight
		/// shouldn't leak past its edge.</param>
		/// <param name="includeBlockingCells">Also return blocking cells whose centre is in view (walls you can see),
		/// unless they're masked.</param>
		/// <remarks>Stops when <paramref name="farClip"/> is reached or every angle is blocked, so farClip should
		/// be finite unless blockers fully enclose the origin.</remarks>
		public static HashSet<HexCoord> GetVisibleCells (HexCoord origin, AngleArc arc, int nearClip, int farClip, Func<HexCoord, HexSightOcclusion> occlusion, bool includeBlockingCells = false) {
			var visible = new HashSet<HexCoord>();
			if (farClip < 0 || farClip < nearClip) return visible;
			if (nearClip <= 0) visible.Add(origin);

			var originPosition = origin.Position();
			var openRanges = new List<Vector2>(arc.ranges);
			var cornerVectors = HexCoord.CornerVectors().ToArray();
			var cornersArc = new AngleArc();
			var centreArc = new AngleArc();
			var angles = new List<float>(6);

			for (int radius = 1; radius <= farClip && openRanges.Count > 0; radius++) {
				bool inClipRange = radius >= nearClip;
				foreach (var coord in HexCoord.GetPointsOnRing(origin, radius)) {
					var coordPosition = coord.Position();
					var cornersAngles = AngleHexagon(originPosition, coordPosition, cornerVectors, 1, angles);
					cornersArc.Set(cornersAngles.x, cornersAngles.y);
					var centreAngles = AngleHexagon(originPosition, coordPosition, cornerVectors, 0, angles);
					centreArc.Set(centreAngles.x, centreAngles.y);

					bool cornersInView = Overlaps(openRanges, cornersArc);
					bool centreInView = inClipRange && Overlaps(openRanges, centreArc);
					if (!cornersInView && !centreInView) continue;

					var cellOcclusion = occlusion(coord);
					bool blocks = (cellOcclusion & HexSightOcclusion.Block) != 0;
					bool masks = (cellOcclusion & HexSightOcclusion.Mask) != 0;

					if (centreInView && !masks && (!blocks || includeBlockingCells)) visible.Add(coord);
					if (cornersInView && blocks) RemoveArcFromRanges(openRanges, cornersArc);
				}
			}
			return visible;
		}

		/// <summary>
		/// Cells visible within a cone <paramref name="fieldOfView"/> degrees wide, facing <paramref name="direction"/>.
		/// </summary>
		public static HashSet<HexCoord> GetVisibleCells (HexCoord origin, Vector2 direction, float fieldOfView, int nearClip, int farClip, Func<HexCoord, HexSightOcclusion> occlusion, bool includeBlockingCells = false) {
			return GetVisibleCells(origin, AngleArc.DirectionFieldOfView(direction, fieldOfView), nearClip, farClip, occlusion, includeBlockingCells);
		}

		/// <summary>
		/// True if a straight line from <paramref name="from"/>'s centre to <paramref name="to"/>'s centre isn't blocked
		/// by any cell in between. <paramref name="to"/>'s own occlusion is ignored. A line passing exactly through a
		/// corner is blocked if either cell at that corner blocks.
		/// </summary>
		public static bool HasLineOfSight (HexCoord from, HexCoord to, Func<HexCoord, HexSightOcclusion> occlusion) {
			if (from == to) return true;
			var direction = to.Position() - from.Position();
			var visible = GetVisibleCells(from, AngleArc.DirectionFieldOfView(direction, 0), 0, HexCoord.Distance(from, to),
				coord => coord == to ? HexSightOcclusion.None : occlusion(coord));
			return visible.Contains(to);
		}

		// True if any open range overlaps the arc.
		public static bool Overlaps (List<Vector2> ranges, AngleArc arc) {
			for (int i = 0; i < ranges.Count; i++)
				if (arc.OverlapsRange(ranges[i])) return true;
			return false;
		}

		// Remove an arc (a hexagon's angular extent) from the open ranges.
		public static void RemoveArcFromRanges (List<Vector2> ranges, AngleArc arc) {
			for (int i = 0; i < arc.ranges.Count; i++)
				RemoveRangeFromRanges(ranges, arc.ranges[i]);
		}

		static void RemoveRangeFromRanges (List<Vector2> ranges, Vector2 range) {
			var removeStart = range.x;
			var removeEnd = range.y;
			for (int i = ranges.Count - 1; i >= 0; i--) {
				var start = ranges[i].x;
				var end = ranges[i].y;
				if (removeStart > end || removeEnd < start) continue;
				ranges.RemoveAt(i);
				// Keep whatever survives either side of the removed span, dropping slivers.
				if (removeStart > start) AddIfValid(ranges, i, new Vector2(start, removeStart));
				if (removeEnd < end) AddIfValid(ranges, i, new Vector2(removeEnd, end));
			}
		}

		// Ranges under a degree wide are dropped, so near-touching blockers close the gap between them.
		static void AddIfValid (List<Vector2> ranges, int index, Vector2 range) {
			if (range.y - range.x > 1) ranges.Insert(index, range);
		}

		// Min and max angle of a hexagon (scaled by `hexSize`; 0 means just its centre) seen from `originPosition`,
		// unwrapped so a hexagon straddling 0/360 gives e.g. (-10, 20) rather than (20, 350).
		public static Vector2 AngleHexagon (Vector2 originPosition, Vector2 coordPosition, Vector2[] cornerVectors, float hexSize, List<float> scratch = null) {
			var angles = scratch ?? new List<float>(6);
			angles.Clear();
			if (hexSize == 0) {
				angles.Add(Mathf.Repeat(Util.Degrees(coordPosition - originPosition), 360));
			} else {
				for (int i = 0; i < cornerVectors.Length; i++)
					angles.Add(Mathf.Repeat(Util.Degrees(coordPosition + cornerVectors[i] * hexSize - originPosition), 360));
			}

			float min = angles.Min(), max = angles.Max();
			if (max - min > 180) {
				for (int i = 0; i < angles.Count; i++)
					if (angles[i] > 180) angles[i] -= 360;
				min = angles.Min();
				max = angles.Max();
			}
			return new Vector2(min, max);
		}
	}
}
