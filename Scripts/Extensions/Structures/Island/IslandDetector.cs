using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Finds contiguous "islands" from a point cloud
public class IslandDetector<Coord> where Coord : IEquatable<Coord> {

	// Instance (not static) so two detectors — or a re-entrant call — don't clobber each other's shared state.
	// testedPoints is a field because FloodFill (a separate method) shares it; the result list is a local in
	// FindIslands so each call returns a fresh, caller-owned list (no aliasing of a previously-returned result).
	protected HashSet<Coord> testedPoints = new HashSet<Coord>();

	public IEnumerable<Coord> startPoints;
	public Func<Coord, IEnumerable<Coord>> GetAdjacentPoints;
	public Func<Coord, bool> GetPointIsValid;

	public IslandDetector (IEnumerable<Coord> startPoints, Func<Coord, IEnumerable<Coord>> GetAdjacentPoints, Func<Coord, bool> GetPointIsValid) {
		this.startPoints = startPoints;
		this.GetAdjacentPoints = GetAdjacentPoints;
		this.GetPointIsValid = GetPointIsValid;
	}

	public List<Island<Coord>> FindIslands () {
		List<Island<Coord>> islands = new List<Island<Coord>>();
		testedPoints.Clear();

		// Walk a fixed collection (startPoints) and flood-fill the valid region reachable from each
		// not-yet-visited seed. `testedPoints` (a HashSet) marks everything already assigned to an
		// island, so each point is visited once — O(n) overall, and termination is guaranteed.
		foreach(Coord seed in startPoints) {
			if(testedPoints.Contains(seed) || !GetPointIsValid(seed)) continue;
			Island<Coord> island = new Island<Coord>();
			FloodFill(seed, GetPointIsValid, island.points.Add);
			islands.Add(island);
		}
		return islands;
	}

	// Iterative (non-recursive) flood fill from `seed`. Every point reachable through GetAdjacentPoints
	// for which canJoin(point) is true is added to the island via addPoint and recorded in testedPoints
	// so it is never visited twice. An explicit Stack replaces the old mutual recursion (no stack-overflow
	// risk) and, combined with the HashSet membership test, keeps this O(n).
	// onValidSkip (optional) receives points that are valid but rejected by canJoin (e.g. a neighbour that
	// belongs to a different owner) so a caller can queue them as future seeds.
	protected void FloodFill (Coord seed, Func<Coord, bool> canJoin, Action<Coord> addPoint, Action<Coord> onValidSkip = null) {
		Stack<Coord> frontier = new Stack<Coord>();
		frontier.Push(seed);
		while(frontier.Count > 0) {
			Coord point = frontier.Pop();
			if(testedPoints.Contains(point)) continue;
			if(!canJoin(point)) {
				if(onValidSkip != null && GetPointIsValid(point)) onValidSkip(point);
				continue;
			}
			testedPoints.Add(point);
			addPoint(point);
			foreach(Coord adjacentPoint in GetAdjacentPoints(point)) {
				if(!testedPoints.Contains(adjacentPoint)) frontier.Push(adjacentPoint);
			}
		}
	}
}
