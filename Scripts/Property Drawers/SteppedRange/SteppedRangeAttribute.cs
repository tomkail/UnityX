using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class SteppedRangeAttribute : PropertyAttribute {

	public float min;
	public float max;
	public float step;

	public SteppedRangeAttribute(float min, float max, float step) {
        this.min = min;
        this.max = max;
		this.step = step;
    }
}
