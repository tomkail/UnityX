using System;
using System.Collections.Generic;

namespace UnityX.HexGrid {
	// Searches over hex cells. Self-contained (no pathfinding package), like the rest of this assembly.
	//
	// The hex plane is unbounded, so a search only ends when it runs out of passable cells or hits its limit: make
	// `passable` return false off the map, or pass a step/cost limit, or an unreachable goal searches forever.
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

		/// <summary>
		/// Every cell reachable from <paramref name="start"/> for a total cost of at most <paramref name="maxCost"/>,
		/// with its cheapest cost (Dijkstra). <paramref name="cost"/> is the cost of stepping between two neighbours;
		/// return <see cref="float.PositiveInfinity"/> for a step that isn't allowed. Costs must not be negative.
		/// </summary>
		public static Dictionary<HexCoord, float> ReachableWithinCost (HexCoord start, float maxCost, Func<HexCoord, HexCoord, float> cost) {
			var best = new Dictionary<HexCoord, float> { [start] = 0 };
			var settled = new HashSet<HexCoord>();
			var open = new MinHeap();
			open.Push(start, 0);
			while (open.Count > 0) {
				var (coord, coordCost) = open.Pop();
				if (!settled.Add(coord)) continue;
				for (int i = 0; i < 6; i++) {
					var neighbor = coord.Neighbor(i);
					if (settled.Contains(neighbor)) continue;
					var neighborCost = coordCost + cost(coord, neighbor);
					if (float.IsPositiveInfinity(neighborCost) || neighborCost > maxCost || (best.TryGetValue(neighbor, out var known) && known <= neighborCost)) continue;
					best[neighbor] = neighborCost;
					open.Push(neighbor, neighborCost);
				}
			}
			return best;
		}

		/// <summary>
		/// Cheapest path from <paramref name="start"/> to <paramref name="goal"/> (A*), including both ends, or null if
		/// there isn't one within <paramref name="maxCost"/>.
		/// </summary>
		/// <param name="passable">Whether a cell can be entered. The start isn't checked; the goal is.</param>
		/// <param name="cost">Cost of stepping between two neighbours; 1 per step if null. Must not be negative;
		/// <see cref="float.PositiveInfinity"/> forbids the step.</param>
		/// <param name="minStepCost">The least any step can cost, which scales the distance heuristic. Leave at 1 for
		/// the default cost; lower it if steps can cost less, or the path may not be the cheapest. 0 gives Dijkstra.</param>
		public static List<HexCoord> FindPath (HexCoord start, HexCoord goal, Func<HexCoord, bool> passable, Func<HexCoord, HexCoord, float> cost = null, float maxCost = float.PositiveInfinity, float minStepCost = 1) {
			if (start == goal) return new List<HexCoord> { start };
			if (!passable(goal)) return null;

			var cameFrom = new Dictionary<HexCoord, HexCoord>();
			var best = new Dictionary<HexCoord, float> { [start] = 0 };
			var closed = new HashSet<HexCoord>();
			var open = new MinHeap();
			open.Push(start, HexCoord.Distance(start, goal) * minStepCost);
			while (open.Count > 0) {
				var (coord, _) = open.Pop();
				if (coord == goal) return BuildPath(cameFrom, start, goal);
				if (!closed.Add(coord)) continue;
				var coordCost = best[coord];
				for (int i = 0; i < 6; i++) {
					var neighbor = coord.Neighbor(i);
					if (closed.Contains(neighbor) || !passable(neighbor)) continue;
					var neighborCost = coordCost + (cost == null ? 1 : cost(coord, neighbor));
					if (float.IsPositiveInfinity(neighborCost) || neighborCost > maxCost || (best.TryGetValue(neighbor, out var known) && known <= neighborCost)) continue;
					best[neighbor] = neighborCost;
					cameFrom[neighbor] = coord;
					open.Push(neighbor, neighborCost + HexCoord.Distance(neighbor, goal) * minStepCost);
				}
			}
			return null;
		}

		static List<HexCoord> BuildPath (Dictionary<HexCoord, HexCoord> cameFrom, HexCoord start, HexCoord goal) {
			var path = new List<HexCoord> { goal };
			var coord = goal;
			while (coord != start) {
				coord = cameFrom[coord];
				path.Add(coord);
			}
			path.Reverse();
			return path;
		}

		// Binary min-heap of cells by priority. Duplicates are allowed; callers skip stale entries when popped.
		class MinHeap {
			readonly List<(HexCoord coord, float priority)> items = new List<(HexCoord, float)>();
			public int Count => items.Count;

			public void Push (HexCoord coord, float priority) {
				items.Add((coord, priority));
				int i = items.Count - 1;
				while (i > 0) {
					int parent = (i - 1) / 2;
					if (items[parent].priority <= items[i].priority) break;
					(items[parent], items[i]) = (items[i], items[parent]);
					i = parent;
				}
			}

			public (HexCoord coord, float priority) Pop () {
				var top = items[0];
				int last = items.Count - 1;
				items[0] = items[last];
				items.RemoveAt(last);
				int i = 0;
				while (true) {
					int left = i * 2 + 1, right = left + 1, smallest = i;
					if (left < items.Count && items[left].priority < items[smallest].priority) smallest = left;
					if (right < items.Count && items[right].priority < items[smallest].priority) smallest = right;
					if (smallest == i) break;
					(items[smallest], items[i]) = (items[i], items[smallest]);
					i = smallest;
				}
				return top;
			}
		}
	}
}
