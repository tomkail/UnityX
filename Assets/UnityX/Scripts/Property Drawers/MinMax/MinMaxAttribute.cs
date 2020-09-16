using UnityEngine;

public class MinMaxAttribute : PropertyAttribute {

	public float min;
	public float max;
	public float step;

	public MinMaxAttribute(float min, float max, float step = -1) {
		this.min = min;
		this.max = max;
		this.step = step;
	}
}