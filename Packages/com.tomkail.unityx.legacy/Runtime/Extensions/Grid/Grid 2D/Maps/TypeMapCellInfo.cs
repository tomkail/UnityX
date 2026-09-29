using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

public struct TypeMapCellInfo<T> {
	public int index {get; private set;}
	public Vector2Int point {get; private set;}
	public T value {get; private set;}

	public TypeMapCellInfo (int index, Vector2Int point, T value) {
		this.index = index;
		this.point = point;
		this.value = value;
	}

	public void Set (int index, Vector2Int point, T value) {
		this.index = index;
		this.point = point;
		this.value = value;
	}

	public override string ToString () {
		return string.Format ("[TypeMapCellInfo] index={0} point={1} value={2}", index, point, value);
	}
}