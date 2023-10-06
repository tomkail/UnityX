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
	public Vector2Map (Point _size) : base (_size) {}
	public Vector2Map (Point _size, Vector2 _value) : base (_size, _value) {}
	public Vector2Map (Point _size, Vector2[] _mapArray) : base (_size, _mapArray) {}
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

	/*
	public static Point operator +(Vector2Map left, Vector2Map right) {
		return Add(left, right);
	}
	
	public static Point operator -(Vector2Map left, Vector2Map right) {
		return Subtract(left, right);
	}
	
	public static Point operator *(Vector2Map left, Vector2Map right) {
		return Multiply(left, right);
	}
	
	public static Point operator /(Vector2Map left, Vector2Map right) {
		return Divide(left, right);
	}
	
	public static Vector2Map operator -(Vector2 left, Vector2Map right) {
		return Subtract(left, right);
	}
	
	public static Vector2Map operator -(Vector2Map left, Vector2 right) {
		return Subtract(left, right);
	}
	*/

	/*
	/// <summary>
	/// Returns the total of the specified values.
	/// </summary>
	/// <param name="values">Values.</param>
	public static float[] Divide(float divider){
		for(int i = 0; i < newValues.Length; i++)
			newValues[i] = values[i]/divider;
		return newValues;
	}
	
	public static IList<float> Multiply(float multiplier){
		for(int i = 0; i < values.Count; i++)
			values[i] = values[i] * multiplier;
		return values;
	}


	 public virtual void Add(IList<T> _mapArray) {
		if(values.Count != _mapArray.Count) Debug.LogWarning("Map arrays are of different length");
	 	for(int i = 0; i < mapArray.Length; i++) {
			values[i] += _mapArray[i];
	 	}
	 }

	 public virtual void Subtract(IList<T> _mapArray) {
	 	if(mapArray.Count != _mapArray.Count) Debug.LogWarning("Map arrays are of different length");
	 	for(int i = 0; i < mapArray.Length; i++) {
	 		mapArray[i] -= _mapArray[i];
	 	}
	 }

	 public virtual void Multiply(IList<T> _mapArray) {
	 	if(mapArray.Count != _mapArray.Count) Debug.LogWarning("Map arrays are of different length");
	 	for(int i = 0; i < mapArray.Length; i++) {
	 		mapArray[i] *= _mapArray[i];
	 	}
	 }

	 public virtual void Divide(IList<T> _mapArray) {
	 	if(mapArray.Count != _mapArray.Count) Debug.LogWarning("Map arrays are of different length");
	 	for(int i = 0; i < mapArray.Length; i++) {
	 		mapArray[i] /= _mapArray[i];
	 	}
	 }
	 */
}
