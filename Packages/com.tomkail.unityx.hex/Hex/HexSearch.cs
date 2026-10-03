using System;
using System.Collections.Generic;

namespace UnityX.HexGrid {
	public static class HexSearch {
		/// <summary>
		/// Every cell reachable from <paramref name="start"/> in at most <paramref name="maxSteps"/> moves through
		/// passable cells, with its step count. Breadth-first, so each count is the shortest walk. The start is always
		/// included at 0 and its own passability isn't checked.
		/// </summary>
		public static Dictionary<HexCoord, int> Reachable (HexCoord start, int maxSteps, Func<HexCoord, bool> passable) {
			var steps = new Dictionary<HexCoord, int> { [start] = 0 };
			var frontier = new List<HexCoord> { start };
			var next = new List<HexCoord>();
			for (int step = 1; step <= maxSteps && frontier.Count > 0; step++) {
				next.Clear();
				foreach (var coord in frontier) {
					for (int i = 0; i < 6; i++) {
						var neighbor = coord.Neighbor(i);
						if (steps.ContainsKey(neighbor) || !passable(neighbor)) continue;
						steps[neighbor] = step;
						next.Add(neighbor);
					}
				}
				(frontier, next) = (next, frontier);
			}
			return steps;
		}
	}
}
