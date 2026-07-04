using System;
using System.Collections.Generic;

public class OwnedIsland<Coord, T> : Island<Coord> where Coord : IEquatable<Coord> {
	public T owner;
	public OwnedIsland (T owner, List<Coord> islandPoints) : base (islandPoints) {
		this.owner = owner;
	}

    public override string ToString() {
		return string.Format("[{0}] Owner={1} List={2}", GetType().Name, owner, DebugX.ListAsString(points));
	}
	// (An earlier inner OutlineSolver that traced island points into an outline polygon was removed —
	// it never compiled and is superseded by the live, generic OutlineDetector.GetOutlinePoly.)
}
