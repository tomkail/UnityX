using System.Collections.Generic;
using UnityEngine;

namespace UnityX.HexGrid {
	// A set of angle ranges in degrees (0 = up/+Y, 90 = right/+X, as Util.Degrees), each within 0..360. An arc that
	// crosses 0/360 is stored as two ranges.
	[System.Serializable]
	public class AngleArc {
		public List<Vector2> ranges = new List<Vector2>();

		// An arc `fieldOfView` degrees wide, centred on `direction`.
		public static AngleArc DirectionFieldOfView (Vector2 direction, float fieldOfView) {
			return AngleFieldOfView(Util.Degrees(direction), fieldOfView);
		}

		// An arc `fieldOfView` degrees wide, centred on `degrees`.
		public static AngleArc AngleFieldOfView (float degrees, float fieldOfView) {
			degrees = Mathf.Repeat(degrees, 360);
			return new AngleArc(degrees - fieldOfView * 0.5f, degrees + fieldOfView * 0.5f);
		}

		public AngleArc () {}

		// `start` may be below 0 and `end` above 360 (by at most one turn); the arc is split at the wrap.
		public AngleArc (float start, float end) {
			Set(start, end);
		}

		public void Set (float start, float end) {
			ranges.Clear();
			if (start >= 0 && end <= 360) {
				ranges.Add(new Vector2(start, end));
			} else if (start < 0 && end == 0) {
				ranges.Add(new Vector2(start + 360, 360));
			} else if (start < 0 && end > 0) {
				ranges.Add(new Vector2(start + 360, 360));
				ranges.Add(new Vector2(0, end));
			} else if (end > 360) {
				ranges.Add(new Vector2(start, 360));
				ranges.Add(new Vector2(0, end - 360));
			}
		}

		public bool Overlaps (AngleArc arc) {
			for (int i = 0; i < arc.ranges.Count; i++)
				if (OverlapsRange(arc.ranges[i])) return true;
			return false;
		}

		// True if `range` touches any of this arc's ranges (with a tiny tolerance, so zero-width ranges register).
		public bool OverlapsRange (Vector2 range) {
			var start = range.x - 0.0001f;
			var end = range.y + 0.0001f;
			for (int i = 0; i < ranges.Count; i++) {
				var other = ranges[i];
				if (start >= other.x && start <= other.y ||
					end >= other.x && end <= other.y ||
					start <= other.x && end >= other.y) return true;
			}
			return false;
		}
	}
}
