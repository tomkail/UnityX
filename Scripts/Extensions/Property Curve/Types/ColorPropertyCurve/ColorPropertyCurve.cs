using UnityEngine;

public class ColorPropertyCurve : PropertyCurve<Color> {

	public ColorPropertyCurve(ColorPropertyCurve curve) : base (curve) {}
	public ColorPropertyCurve(params PropertyCurveKeyframe<Color>[] keys) : base (keys) {}
	
	protected override Color GetSmoothedValue(Color key1, Color key2, float time) {
		return Color.Lerp(key1, key2, time);
	}
}
