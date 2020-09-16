using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

public class Structure : Shape {
	public bool Contains (System.Func<Point,bool> checker) {
		foreach(Point point in points) {
			if(checker(point)) return true;
		}
		return false;
	}

	public override string ToString () {
		return string.Format ("[Structure] gridPoints={0}", DebugX.ListAsString(points));
	}
}
