using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

// Finds islands "owned" by a specific property of the coord, such as the land type
public class OwnedIslandDetector<Coord, Owner> : IslandDetector<Coord> where Coord : IEquatable<Coord> {
	static new List<OwnedIsland<Coord, Owner>> islands = new List<OwnedIsland<Coord, Owner>>();

	public Func<Coord, Owner> GetPointOwner;

	public OwnedIslandDetector (IEnumerable<Coord> startPoints, Func<Coord, IEnumerable<Coord>> GetAdjacentPoints, Func<Coord, bool> GetPointIsValid, Func<Coord, Owner> GetPointOwner) : base (startPoints, GetAdjacentPoints, GetPointIsValid) {
		this.GetPointOwner = GetPointOwner;
		Debug.Assert(GetAdjacentPoints != null);
		Debug.Assert(GetPointOwner != null);
	}

	public new List<OwnedIsland<Coord, Owner>> FindIslands () {
		islands.Clear();
		testedPoints.Clear();

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
				point => GetPointIsValid(point) && GetPointOwner(point).Equals(owner),
				island.points.Add,
				seedQueue.Enqueue
			);
			islands.Add(island);
		}
		return islands;
	}
}
