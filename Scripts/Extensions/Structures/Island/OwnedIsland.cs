using System;
using System.Collections.Generic;

namespace UnityX.Islands {
	// An Island that also remembers an "owner" — some property shared by all its cells, such as the
	// land type, team, or region id it was grouped by. Produced by OwnedIslandDetector.
	//   Coord — the coordinate type (see Island).
	//   T     — the owner type (an enum, id, colour, ...).
	public class OwnedIsland<Coord, T> : Island<Coord> where Coord : IEquatable<Coord> {
		// The property value shared by every cell in this island.
		public T owner;

		public OwnedIsland (T owner, List<Coord> islandPoints) : base(islandPoints) {
			this.owner = owner;
		}

		public override string ToString () {
			return string.Format("[{0}] Owner={1} points={2}", GetType().Name, owner, string.Join(", ", points));
		}
	}
}
