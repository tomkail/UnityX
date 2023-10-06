using UnityEngine;

public class QuaternionPropertyCurve : PropertyCurve<Quaternion> {

	public QuaternionPropertyCurve(QuaternionPropertyCurve curve) : base (curve) {}
	public QuaternionPropertyCurve(params PropertyCurveKeyframe<Quaternion>[] keys) : base (keys) {}
	
	protected override Quaternion GetSmoothedValue(Quaternion key1, Quaternion key2, float time) {
		return Quaternion.Slerp(key1, key2, time);
	}
}
