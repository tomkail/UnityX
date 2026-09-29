using System;
using System.Collections.Generic;

namespace UnityX.Islands {
	// An "island" is a connected group of coordinates — the region you get by flood-filling outward
	// from a cell through its like/adjacent neighbours. It's a thin wrapper around the list of
	// coordinates that make it up.
	//
	// `Coord` is whatever coordinate type your grid uses — Vector2Int for a square grid, a hex coord,
	// a 3D cell, etc. It only has to be IEquatable so the detectors can dedupe visited cells; the
	// islands themselves carry no geometry.
	//
	// You don't usually construct these by hand — an IslandDetector produces them from a set of
	// points, and IslandGenerator can build a random one. See IslandDetector for typical usage.
	public class Island<Coord> where Coord : IEquatable<Coord> {
		// The coordinates belonging to this island.
		public List<Coord> points;

		public Island () {
			points = new List<Coord>();
		}

		public Island (List<Coord> islandPoints) {
			points = islandPoints;
		}

		public override string ToString () {
			return string.Format("[{0}] points={1}", GetType().Name, string.Join(", ", points));
		}
	}
}
