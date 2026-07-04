using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;

public class Structure : Shape {
	// Convenience wrapper over Enumerable.Any (kept as named API).
	public bool Contains (System.Func<Point,bool> checker) {
		return points.Any(checker);
	}

	public override string ToString () {
		return string.Format ("[Structure] gridPoints={0}", DebugX.ListAsString(points));
	}
}
