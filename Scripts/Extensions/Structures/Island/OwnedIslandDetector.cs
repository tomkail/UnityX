using UnityEngine;
using System;
using System.Collections.Generic;

namespace UnityX.Islands {
	// Like IslandDetector, but partitions the result by an "owner" property of each cell — so instead
	// of "all connected land", you get separate islands for "connected grassland", "connected desert",
	// etc. Two adjacent cells only join the same island if they share the same owner.
	//
	// Adds one callback to IslandDetector's three:
	//   • GetPointOwner — the owner value for a coord (the land type, team, region id, ...).
	//
	// Usage:
	//   var detector = new OwnedIslandDetector<Vector2Int, Terrain>(
	//       allCells,
	//       c => c.CardinalDirections(),   // neighbours
	//       c => map[c] != Terrain.Water,  // valid?
	//       c => map[c]);                  // owner
	//   List<OwnedIsland<Vector2Int, Terrain>> islands = detector.FindIslands();
	public class OwnedIslandDetector<Coord, Owner> : IslandDetector<Coord> where Coord : IEquatable<Coord> {
		public Func<Coord, Owner> GetPointOwner;

		public OwnedIslandDetector (IEnumerable<Coord> startPoints, Func<Coord, IEnumerable<Coord>> GetAdjacentPoints, Func<Coord, bool> GetPointIsValid, Func<Coord, Owner> GetPointOwner) : base (startPoints, GetAdjacentPoints, GetPointIsValid) {
			this.GetPointOwner = GetPointOwner;
			Debug.Assert(GetAdjacentPoints != null);
			Debug.Assert(GetPointOwner != null);
		}

		// Returns one OwnedIsland per connected same-owner region. Hides the base FindIslands (`new`)
		// because it returns the richer OwnedIsland type.
		public new List<OwnedIsland<Coord, Owner>> FindIslands () {
			List<OwnedIsland<Coord, Owner>> islands = new List<OwnedIsland<Coord, Owner>>();
			// Local (not a field) so each call — including a re-entrant one — gets its own visited set.
			HashSet<Coord> testedPoints = new HashSet<Coord>();

			// Seeds are processed from a queue (Dequeue = pop, never peek → cannot hang). Flood-filling a
			// region hands its valid-but-differently-owned boundary neighbours back onto the queue, so a
			// single start point still discovers every connected owner-region (partitioned by owner).
			// `testedPoints` dedupes: a point does real work only the first time it is dequeued/filled.
			Queue<Coord> seedQueue = new Queue<Coord>(startPoints);
			while(seedQueue.Count > 0) {
				Coord seed = seedQueue.Dequeue();
				if(testedPoints.Contains(seed) || !GetPointIsValid(seed)) continue;
				Owner owner = GetPointOwner(seed);
				OwnedIsland<Coord, Owner> island = new OwnedIsland<Coord, Owner>(owner, new List<Coord>());
				FloodFill(
					seed,
					testedPoints,
					GetAdjacentPoints,
					GetPointIsValid,
					point => GetPointIsValid(point) && GetPointOwner(point).Equals(owner),
					island.points.Add,
					seedQueue.Enqueue
				);
				islands.Add(island);
			}
			return islands;
		}
	}
}
