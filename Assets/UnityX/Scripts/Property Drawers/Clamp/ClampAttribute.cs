using UnityEngine;
using System;

public class ClampAttribute : PropertyAttribute {		
	public int minInt = 0;
	public int maxInt = 0;
	public float minFloat = 0;
	public float maxFloat = 0;
	
	public ClampAttribute(int _min, int _max) {
		minInt = _min;
		maxInt = _max;
	}
	
	public ClampAttribute(float _min, float _max) {
		minFloat = _min;
		maxFloat = _max;
	}
}