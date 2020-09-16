using UnityEngine;
using System.Collections;
using UnityX.Geometry;

[System.Serializable]
public class FloatMap3D : TypeMap3D<float> {

	public FloatMap3D (Point3 _size) : base (_size) {}
	public FloatMap3D (Point3 _size, float _value) : base (_size, _value) {}
	public FloatMap3D (Point3 _size, float[] _mapArray) : base (_size, _mapArray) {}
	public FloatMap3D (TypeMap3D<float> typeMap) : base (typeMap.size, typeMap.values) {}
	public FloatMap3D (FloatMap3D _map) : base (_map) {}
	
	public override void CalculateMapProperties(){}

	//Utility Functions
	protected override float Lerp (float a, float b, float l) {
		return Mathf.Lerp(a,b,l);
	}
}