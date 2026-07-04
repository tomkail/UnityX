using System;
using System.Collections.Generic;

public class Island<Coord> where Coord : IEquatable<Coord> {
	public List<Coord> points;
	public Island () {
		this.points = new List<Coord>();
	}
	public Island (List<Coord> islandPoints) {
		this.points = islandPoints;
	}

	public override string ToString() {
		return string.Format("[{0}] List={1}", GetType().Name, DebugX.ListAsString(points));
	}
}