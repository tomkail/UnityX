using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityX.Islands {
	// Generates a random contiguous island — the mirror of IslandDetector, which *finds* islands in
	// existing data. Grows a connected set of `count` coordinates outward from a seed, one random
	// neighbour at a time (think a tetromino / random-blob generator).
	//
	// Generic and geometry-agnostic like the detectors: it only asks for a neighbour function, so it
	// works for square grids, hex grids, 3D, etc.
	//
	// Usage (square grid):
	//   Island<Vector2Int> island = IslandGenerator.CreateRandomIsland(
	//       Vector2Int.zero,
	//       c => new[] { c + Vector2Int.up, c + Vector2Int.down, c + Vector2Int.left, c + Vector2Int.right },
	//       count: 4);
	public static class IslandGenerator {

		// Grows a random connected island of up to `count` cells from `seed`. Stops early (returning
		// fewer cells) if the shape gets boxed in and has no free neighbour left to grow into.
		public static Island<Coord> CreateRandomIsland<Coord> (Coord seed, Func<Coord, IEnumerable<Coord>> neighbours, int count) where Coord : IEquatable<Coord> {
			var chosen = new List<Coord>();
			if(count < 1) return new Island<Coord>(chosen);

			var chosenSet = new HashSet<Coord> { seed };
			chosen.Add(seed);

			var candidates = new List<Coord>();
			while(chosen.Count < count) {
				// Collect every not-yet-chosen neighbour of the current cells, then pick one at random.
				// Simple and allocation-light for the small shapes this is used for; stops if boxed in.
				candidates.Clear();
				for(int i = 0; i < chosen.Count; i++)
					foreach(var n in neighbours(chosen[i]))
						if(!chosenSet.Contains(n)) candidates.Add(n);
				if(candidates.Count == 0) break;

				var pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
				chosen.Add(pick);
				chosenSet.Add(pick);
			}
			return new Island<Coord>(chosen);
		}
	}
}
