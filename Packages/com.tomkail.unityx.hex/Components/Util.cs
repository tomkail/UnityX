using UnityEngine;

namespace UnityX.HexGrid {
	// Small self-contained vector / quaternion / colour helpers for the grid-integration layer, so this
	// assembly depends on nothing but Unity itself. Plain static and internal - same rationale as the core Util.
	static class Util {
		// Drop the Y component, mapping a world-space vector onto a 2D (x, z) plane.
		public static Vector2 XZ (Vector3 v) {
			return new Vector2(v.x, v.z);
		}

		// Angle of direction a in degrees, where 0 is up (+Y) and 90 is right (+X).
		public static float Degrees (Vector2 a) {
			return Vector2.SignedAngle(a, Vector2.up);
		}

		// Apply an additional local rotation (Space.Self) from euler angles.
		public static Quaternion Rotate (Quaternion q, Vector3 eulerAngles) {
			return q * Quaternion.Euler(eulerAngles);
		}

		// The same colour with a replaced alpha.
		public static Color WithAlpha (Color c, float a) {
			return new Color(c.r, c.g, c.b, a);
		}
	}
}
