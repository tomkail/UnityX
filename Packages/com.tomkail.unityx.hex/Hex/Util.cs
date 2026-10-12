using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityX.HexGrid {
	// Small self-contained math / vector / collection helpers used across the hex core, so the package depends
	// on nothing but Unity itself. Plain static (not extension) methods: Util.X(a) reads as a local helper
	// rather than decorating built-in types. Internal, so it never widens the package's public API.
	static class Util {
		// True modulo: the result takes the sign of n (unlike C#'s %, which keeps the sign of a).
		public static int Mod (int a, int n) {
			if (n == 0) throw new ArgumentOutOfRangeException(nameof(n), "(a mod 0) is undefined.");
			var remainder = a % n;
			if ((n > 0 && remainder < 0) || (n < 0 && remainder > 0)) return remainder + n;
			return remainder;
		}

		// Sign of f as an int. With allowZero, exactly 0 maps to 0; otherwise 0 counts as positive.
		public static int Sign (float f, bool allowZero = false) {
			if (allowZero && f == 0f) return 0;
			return f >= 0 ? 1 : -1;
		}

		// Absolute difference between a and b.
		public static float Difference (float a, float b) {
			return Mathf.Abs(a - b);
		}

		// True when a and b are equal within a tolerance.
		public static bool NearlyEqual (float a, float b, float maxDifference = 0.001f) {
			if (a == b) return true;
			return Difference(a, b) < maxDifference;
		}

		// Angle of direction a in degrees, where 0 is up (+Y) and 90 is right (+X).
		public static float Degrees (Vector2 a) {
			return Vector2.SignedAngle(a, Vector2.up);
		}

		// Unit vector pointing from a to b.
		public static Vector2 NormalizedDirection (Vector2 a, Vector2 b) {
			return (b - a).normalized;
		}

		// Squared distance between a and b.
		public static float SqrDistance (Vector2 a, Vector2 b) {
			return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
		}

		// Cyclic rotation: shift items by `places`, wrapping around.
		public static T[] GetShiftedRepeating<T> (IList<T> items, int places) {
			int count = items.Count;
			places %= count;
			var shifted = new T[count];
			for (int i = 0; i < count; i++) {
				int index = (((i - places) % count) + count) % count;
				shifted[i] = items[index];
			}
			return shifted;
		}

		// Indices of the elements matching the predicate.
		public static IEnumerable<int> IndexesWhere<T> (IEnumerable<T> source, Func<T, bool> predicate) {
			int i = 0;
			foreach (T element in source) {
				if (predicate(element)) yield return i;
				i++;
			}
		}

		static readonly string[] _headings = { "E", "NE", "N", "NW", "W", "SW", "S", "SE" };
		// Compass-style name of a direction (8-way), empty for the zero vector.
		public static string DirectionName (Vector2 dir) {
			if (dir == Vector2.zero) return string.Empty;
			float angle = Mathf.Atan2(dir.y, dir.x);
			int octant = Mathf.RoundToInt(8 * angle / (2 * Mathf.PI) + 8) % 8;
			return _headings[octant];
		}

		// Human-readable dump of a sequence, one item per line by default.
		public static string ListAsString<T> (IEnumerable<T> list, Func<T, string> toString = null, bool showTypeAndCount = true, bool lineSeparated = true) {
			if (list == null) return "NULL";
			int count = 0;
			foreach (var item in list) count++;
			var sb = new StringBuilder();
			if (showTypeAndCount) sb.AppendLine("Displaying list of " + typeof(T).Name + " with " + count + " values:");
			bool first = true;
			foreach (var item in list) {
				var itemStr = item == null ? "NULL" : toString == null ? item.ToString() : toString(item);
				if (lineSeparated) sb.AppendLine(itemStr);
				else {
					if (!first) sb.Append(", ");
					sb.Append(itemStr);
				}
				first = false;
			}
			return sb.ToString();
		}
	}
}
