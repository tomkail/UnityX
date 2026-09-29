using UnityEngine;
using UnityX.Geometry;
using UnityX.PropertyCurves;

// Bridges PropertyCurve (UnityX.PropertyCurves) with Polygon (UnityX.Geometry). Because it depends on BOTH
// modules it can't live inside either portable assembly, so it sits in Assembly-CSharp. Move it into a
// dedicated bridge/module if Geometry + PropertyCurves ever ship together.
[System.Serializable]
public class PolygonPropertyCurve : PropertyCurve<Polygon> {

	public PolygonPropertyCurve(PolygonPropertyCurve curve) : base (curve) {}
	public PolygonPropertyCurve(params PropertyCurveKeyframe<Polygon>[] keys) : base (keys) {}
	
	protected override Polygon GetSmoothedValue(Polygon key1, Polygon key2, float time) {
		return Polygon.LerpAuto(key1, key2, time);
	}
}
