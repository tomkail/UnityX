using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

[System.Serializable]
public class Vector2Map : TypeMap<Vector2> {
//	public Vector2 averageVector;
//	public float minMagnitude;
//	public float maxMagnitude;
//	public float deltaMagnitude;
	public Vector2Map (Vector2Int _size) : base (_size) {}
	public Vector2Map (Vector2Int _size, Vector2 _value) : base (_size, _value) {}
	public Vector2Map (Vector2Int _size, Vector2[] _mapArray) : base (_size, _mapArray) {}
	public Vector2Map (TypeMap<Vector2> typeMap) : base (typeMap) {}
//	public Vector2Map (Vector2Map _map) : base (_map) {
//		averageVector = _map.averageVector;
//		minMagnitude = _map.minMagnitude;
//		maxMagnitude = _map.maxMagnitude;
//		deltaMagnitude = _map.deltaMagnitude;
//	}
//	
//	public override void CalculateMapProperties(){
//		averageVector = values.Average();
//		minMagnitude = Vector2X.SmallestMagnitude(values);
//		maxMagnitude = Vector2X.LargestMagnitude(values);
//		deltaMagnitude = maxMagnitude - minMagnitude;
//	}

	protected override Vector2 Lerp (Vector2 a, Vector2 b, float l) {
		return Vector2.Lerp(a,b,l);
	}

	public override TypeMap<Vector2> CloneMap () => new Vector2Map(this);



	//OPERATORS
	public void Add(Vector2 _value) {
		for(int i = 0; i < values.Length; i++) {
			values[i] += _value;
		}
	}
	
	public void Add(IList<Vector2> _mapArray) {
		if(values.Length != _mapArray.Count) Debug.LogWarning("Map arrays are of different length");
		for(int i = 0; i < values.Length; i++) {
			values[i] += _mapArray[i];
	 	}
	}
	
	public void Subtract(Vector2 _value) {
		for(int i = 0; i < values.Length; i++) {
			values[i] -= _value;
		}
	}
	
	public void Subtract(IList<Vector2> _mapArray) {
		if(values.Length != _mapArray.Count) Debug.LogWarning("Map arrays are of different length");
		for(int i = 0; i < values.Length; i++) {
			values[i] -= _mapArray[i];
		}
	}

	public void Multiply(float _value) {
		for(int i = 0; i < values.Length; i++) {
			values[i] *= _value;
		}
	}

	public void Multiply(Vector2 _value) {
		for(int i = 0; i < values.Length; i++) {
			values[i].x *= _value.x;
			values[i].y *= _value.y;
		}
	}

	public void Multiply(IList<Vector2> _mapArray) {
		if(values.Length != _mapArray.Count) Debug.LogWarning("Map arrays are of different length");
		for(int i = 0; i < values.Length; i++) {
			values[i].x *= _mapArray[i].x;
			values[i].y *= _mapArray[i].y;
	 	}
	}

	public void Divide(float _value) {
		for(int i = 0; i < values.Length; i++) {
			values[i] /= _value;
		}
	}

	public void Divide(Vector2 _value) {
		for(int i = 0; i < values.Length; i++) {
			values[i].x /= _value.x;
			values[i].y /= _value.y;
		}
	}
	
	public void Divide(IList<Vector2> _mapArray) {
		if(values.Length != _mapArray.Count) Debug.LogWarning("Map arrays are of different length");
		for(int i = 0; i < values.Length; i++) {
			values[i].x /= _mapArray[i].x;
			values[i].y /= _mapArray[i].y;
		}
	}
	
	public void ClampMagnitude(float maxMagnitude) {
		for(int i = 0; i < values.Length; i++) {
			values[i] = Vector2.ClampMagnitude(values[i], maxMagnitude);
		}
	}
}
