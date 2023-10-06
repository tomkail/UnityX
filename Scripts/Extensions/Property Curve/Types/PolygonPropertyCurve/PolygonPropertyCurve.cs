using UnityEngine;
using UnityX.Geometry;

[System.Serializable]
public class PolygonPropertyCurve : PropertyCurve<Polygon> {

	public PolygonPropertyCurve(PolygonPropertyCurve curve) : base (curve) {}
	public PolygonPropertyCurve(params PropertyCurveKeyframe<Polygon>[] keys) : base (keys) {}
	
	protected override Polygon GetSmoothedValue(Polygon key1, Polygon key2, float time) {
		return Polygon.LerpAuto(key1, key2, time);
	}
}
