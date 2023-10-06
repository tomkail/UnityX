using UnityEngine;
using System.Collections;
using UnityX.Geometry;

[System.Serializable]
public class HeightMap : TypeMap<float> {
	public float averageHeight;
	public float minHeight;
	public float maxHeight;
	public float deltaHeight;
	public float totalHeight;

	public HeightMap (Point _size) : base (_size) {}
	public HeightMap (Point _size, float _value) : base (_size, _value) {}
	public HeightMap (Point _size, float[] _mapArray) : base (_size, _mapArray) {}
	public HeightMap (TypeMap<float> typeMap) : base (typeMap.size, typeMap.values) {}
	public HeightMap (HeightMap _map) : base (_map) {
		averageHeight = _map.averageHeight;
		minHeight = _map.minHeight;
		maxHeight = _map.maxHeight;
		deltaHeight = _map.deltaHeight;
		totalHeight = _map.totalHeight;
	}
	
	public override void CalculateMapProperties(){
		CalculateTotalHeight();
		CalculateAverageHeight(totalHeight);
		CalculateMinHeight();
		CalculateMaxHeight();
		CalculateDeltaHeight();
	}

	public virtual void CalculateTotalHeight () {
		totalHeight = values.Sum();
	}
	
	public virtual void CalculateAverageHeight () {
		averageHeight = values.Average();
	}

	public virtual void CalculateAverageHeight (float _totalHeight) {
		averageHeight = valuesLengthReciprocal * _totalHeight;
	}

	public virtual void CalculateMinHeight () {
		minHeight = Mathf.Min(values);
	}

	public virtual void CalculateMaxHeight () {
		maxHeight = Mathf.Max(values);
	}

	public virtual void CalculateDeltaHeight () {
		deltaHeight = maxHeight - minHeight;
	}

	


	//Utility Functions
	protected override float Lerp (float a, float b, float l) {
		return Mathf.Lerp(a,b,l);
	}
}