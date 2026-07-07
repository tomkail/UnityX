using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

public struct TypeMap3DCellInfo<T> {
	public int index {get; private set;}
	public Vector3Int point {get; private set;}
	public T value {get; private set;}

	public TypeMap3DCellInfo (int index, Vector3Int point, T value) {
		this.index = index;
		this.point = point;
		this.value = value;
	}

	public void Set (int index, Vector3Int point, T value) {
		this.index = index;
		this.point = point;
		this.value = value;
	}

	public override string ToString () {
		return string.Format ("[TypeMapCellInfo] index={0} point={1} value={2}", index, point, value);
	}
}