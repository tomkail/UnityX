using UnityEngine;
using System.Collections;
using UnityX.Geometry;

[System.Serializable]
public class ColorMap : TypeMap<Color> {
	public Color averageColor;
	
	public ColorMap (Point _size) : base (_size) {}
	public ColorMap (Point _size, Color _value) : base (_size, _value) {}
	public ColorMap (Point _size, Color[] _mapArray) : base (_size, _mapArray) {}
	public ColorMap (TypeMap<Color> typeMap) : base (typeMap.size, typeMap.values) {}

	public override void CalculateMapProperties() {
		CalculateAverageColor();
	}

	public virtual void CalculateAverageColor () {
		averageColor = values.Average();
	}

	protected override Color Lerp (Color a, Color b, float l) {
		return Color.Lerp(a,b,l);
	}
}
