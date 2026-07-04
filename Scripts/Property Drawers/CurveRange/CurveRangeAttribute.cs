using System;
using UnityEngine;

// Constrains an AnimationCurve field's editor to a fixed range (both the visible window and the editable key bounds).
// e.g. [CurveRange(0, 0, 1, 1)] locks the curve to x in [0,1], y in [0,1]. Args are (x, y, width, height) of the
// range rect, so allow amplification with e.g. [CurveRange(0, 0, 1, 2)] (y up to 2).
[AttributeUsage(AttributeTargets.Field)]
public class CurveRangeAttribute : PropertyAttribute {
	public readonly Rect ranges;
	public readonly Color color;

	public CurveRangeAttribute(float x, float y, float width, float height) {
		ranges = new Rect(x, y, width, height);
		color = Color.green;
	}
}
