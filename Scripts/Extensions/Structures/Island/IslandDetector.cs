using System;
using System.Collections.Generic;

namespace UnityX.Islands {
	// Finds the contiguous "islands" (connected groups of coordinates) within a set of points, by
	// flood-filling outward from each unvisited point through its neighbours.
	//
	// It knows nothing about your grid's geometry — you supply three things:
	//   • startPoints        — the coords to consider (e.g. every cell in your map).
	//   • GetAdjacentPoints  — given a coord, the coords touching it (4-/8-neighbours, hex neighbours…).
	//   • GetPointIsValid    — whether a coord may belong to an island at all (e.g. "is this land?").
	// so it works for square grids, hex grids, 3D grids, etc.
	//
	// Usage:
	//   var detector = new IslandDetector<Vector2Int>(
	//       allCells,                          // start points
	//       c => c.CardinalDirections(),       // neighbours
	//       c => map[c] == Terrain.Land);      // valid?
	//   List<Island<Vector2Int>> islands = detector.FindIslands();
	//
	// Runs in O(n): each point is visited at most once (tracked in a HashSet).
	public class IslandDetector<Coord> where Coord : IEquatable<Coord> {

		public IEnumerable<Coord> startPoints;
		public Func<Coord, IEnumerable<Coord>> GetAdjacentPoints;
		public Func<Coord, bool> GetPointIsValid;

		public IslandDetector (IEnumerable<Coord> startPoints, Func<Coord, IEnumerable<Coord>> GetAdjacentPoints, Func<Coord, bool> GetPointIsValid) {
			this.startPoints = startPoints;
			this.GetAdjacentPoints = GetAdjacentPoints;
			this.GetPointIsValid = GetPointIsValid;
		}

		// Returns one Island per connected valid region reachable from the start points.
		public List<Island<Coord>> FindIslands () {
			List<Island<Coord>> islands = new List<Island<Coord>>();
			// Local (not a field) so each call — including a re-entrant one — gets its own visited set.
			HashSet<Coord> testedPoints = new HashSet<Coord>();

			// Walk a fixed collection (startPoints) and flood-fill the valid region reachable from each
			// not-yet-visited seed. `testedPoints` (a HashSet) marks everything already assigned to an
			// island, so each point is visited once — O(n) overall, and termination is guaranteed.
			foreach(Coord seed in startPoints) {
				if(testedPoints.Contains(seed) || !GetPointIsValid(seed)) continue;
				Island<Coord> island = new Island<Coord>();
				FloodFill(seed, testedPoints, GetAdjacentPoints, GetPointIsValid, GetPointIsValid, island.points.Add);
				islands.Add(island);
			}
			return islands;
		}

		// Iterative (non-recursive) flood fill from `seed`. Static: all shared state — the visited-set and the
		// adjacency/validity predicates — is passed in, so it holds no instance state; but it stays a member
		// (not a free function) and `protected` so subclasses like OwnedIslandDetector can still reuse it.
		// Every point reachable through getAdjacentPoints for which canJoin(point) is true is added via addPoint
		// and recorded in testedPoints so it is never visited twice. An explicit Stack replaces the old mutual
		// recursion (no stack-overflow risk) and, with the HashSet membership test, keeps this O(n).
		// onValidSkip (optional) receives points that are valid but rejected by canJoin (e.g. a neighbour that
		// belongs to a different owner) so a caller can queue them as future seeds.
		protected static void FloodFill (Coord seed, HashSet<Coord> testedPoints, Func<Coord, IEnumerable<Coord>> getAdjacentPoints, Func<Coord, bool> getPointIsValid, Func<Coord, bool> canJoin, Action<Coord> addPoint, Action<Coord> onValidSkip = null) {
			Stack<Coord> frontier = new Stack<Coord>();
			frontier.Push(seed);
			while(frontier.Count > 0) {
				Coord point = frontier.Pop();
				if(testedPoints.Contains(point)) continue;
				if(!canJoin(point)) {
					if(onValidSkip != null && getPointIsValid(point)) onValidSkip(point);
					continue;
				}
				testedPoints.Add(point);
				addPoint(point);
				foreach(Coord adjacentPoint in getAdjacentPoints(point)) {
					if(!testedPoints.Contains(adjacentPoint)) frontier.Push(adjacentPoint);
				}
			}
		}
	}
}
