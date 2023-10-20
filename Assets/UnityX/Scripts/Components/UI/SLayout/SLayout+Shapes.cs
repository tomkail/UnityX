using System;
using UnityEngine;
using ThisOtherThing.UI;
using ThisOtherThing.UI.Shapes;
using ThisOtherThing.UI.ShapeUtils;

/// <summary>
/// Shortcut to get a TextMeshPro from an SLayout. Don't want to include it
/// directly in SLayout directly since we don't want a dependency on TMPro.
/// </summary>
public partial class SLayout
{
	public IShape shape {
		get {
			return graphic as IShape;
		}
	}
	public Rectangle rectangle {
		get {
			return graphic as Rectangle;
		}
	}
	public Arc arc {
		get {
			return graphic as Arc;
		}
	}
	public EdgeGradient edgeGradient {
		get {
			return graphic as EdgeGradient;
		}
	}
	public Ellipse ellipse {
		get {
			return graphic as Ellipse;
		}
	}
	public ThisOtherThing.UI.Shapes.Polygon polygon {
		get {
			return graphic as ThisOtherThing.UI.Shapes.Polygon;
		}
	}
	public ThisOtherThing.UI.Shapes.Line line {
		get {
			return graphic as ThisOtherThing.UI.Shapes.Line;
		}
	}


	GeoUtils.ShapeProperties GetShapeProperties() {
		GeoUtils.ShapeProperties shapeProperties = null;
		if(arc != null) shapeProperties = arc.ShapeProperties;
		else if(ellipse != null) shapeProperties = ellipse.ShapeProperties;
		else if(line != null) shapeProperties = line.ShapeProperties;
		else if(rectangle != null) shapeProperties = rectangle.ShapeProperties;
		return shapeProperties;
	}
	GeoUtils.OutlineProperties GetOutlineProperties() {
		GeoUtils.OutlineProperties outlineProperties = null;
		if(arc != null) outlineProperties = arc.OutlineProperties;
		else if(ellipse != null) outlineProperties = ellipse.OutlineProperties;
		else if(line != null) outlineProperties = line.OutlineProperties;
		else if(rectangle != null) outlineProperties = rectangle.OutlineProperties;
		return outlineProperties;
	}
	GeoUtils.OutlineShapeProperties GetOutlineShapeProperties() {
		GeoUtils.OutlineShapeProperties outlineShapeProperties = null;
		if(rectangle != null) outlineShapeProperties = rectangle.ShapeProperties;
		else if(ellipse != null) outlineShapeProperties = ellipse.ShapeProperties;
		return outlineShapeProperties;
	}
	GeoUtils.ShadowsProperties GetShadowsProperties() {
		GeoUtils.ShadowsProperties shadowProperties = null;
		if(arc != null) shadowProperties = arc.ShadowProperties;
		else if(ellipse != null) shadowProperties = ellipse.ShadowProperties;
		else if(line != null) shadowProperties = line.ShadowProperties;
		else if(rectangle != null) shadowProperties = rectangle.ShadowProperties;
		return shadowProperties;
	}
	RoundedRects.RoundedProperties GetRoundedProperties() {
		RoundedRects.RoundedProperties properties = null;
		if(rectangle != null) properties = rectangle.RoundedProperties;
		return properties;
	}




	public Color fillColor {
		get {
			InitFillColor();
			return _fillColor.value;
		}
		set {
			InitFillColor();
			_fillColor.value = value;
		}
	}

	public Color targetFillColor {
		get {
			InitFillColor();
			return _fillColor.animatedProperty != null ? _fillColor.animatedProperty.end : _fillColor.value;
		}
	}

	void InitFillColor() {
		if( _fillColor == null ) {
			_fillColor = new SLayoutColorProperty {
				getter = GetFillColor,
				setter = SetFillColor,
				isValid = GetIsValidForAnimation
			};
		}
	}

	Color GetFillColor() {
		var properties = GetShapeProperties();
		if(properties != null) return properties.FillColor;
		return Color.clear;
	}
	void SetFillColor(Color fillColor) {
		var properties = GetShapeProperties();
		if(properties != null) {
			if(properties.FillColor != fillColor) {
				properties.FillColor = fillColor;
				shape.ForceMeshUpdate();
			}
		}
	}
	



	public float borderRadius {
		get {
			InitBorderRadiusWeight();
			return _borderRadius.value;
		}
		set {
			InitBorderRadiusWeight();
			_borderRadius.value = value;
		}
	}

	public float targetBorderRadius {
		get {
			InitBorderRadiusWeight();
			return _borderRadius.animatedProperty != null ? _borderRadius.animatedProperty.end : _borderRadius.value;
		}
	}

	void InitBorderRadiusWeight() {
		if( _borderRadius == null ) {
			_borderRadius = new SLayoutFloatProperty {
				getter = GetBorderRadius,
				setter = SetBorderRadius,
				isValid = GetIsValidForAnimation
			};
		}
	}

	float GetBorderRadius() {
		var properties = GetRoundedProperties();
		if(properties != null) return properties.ClampedUniformRadius;
		return 0;
	}
	void SetBorderRadius(float borderRadius) {
		var properties = GetRoundedProperties();
		if(properties != null) {
			var dirty = false;
			if(properties.Type != RoundedRects.RoundedProperties.RoundedType.Uniform) {
				properties.Type = RoundedRects.RoundedProperties.RoundedType.Uniform;
				dirty = true;
			}
			if(properties.UseMaxRadius) {
				properties.UseMaxRadius = false;
				dirty = true;
			}
			if(properties.UniformRadius != borderRadius || properties.ClampedUniformRadius != borderRadius) {
				properties.UniformRadius = properties.ClampedUniformRadius = borderRadius;
				dirty = true;
			}
			if(dirty) shape.ForceMeshUpdate();
		}
	}
	
	




	
	public float outlineWeight {
		get {
			InitOutlineWeight();
			return _outlineWeight.value;
		}
		set {
			InitOutlineWeight();
			_outlineWeight.value = value;
		}
	}

	public float targetOutlineWeight {
		get {
			InitOutlineWeight();
			return _outlineWeight.animatedProperty != null ? _outlineWeight.animatedProperty.end : _outlineWeight.value;
		}
	}

	void InitOutlineWeight() {
		if( _outlineWeight == null ) {
			_outlineWeight = new SLayoutFloatProperty {
				getter = GetOutlineWeight,
				setter = SetOutlineWeight,
				isValid = GetIsValidForAnimation
			};
		}
	}

	float GetOutlineWeight() {
		var properties = GetOutlineProperties();
		if(properties != null) return properties.LineWeight;
		return 0;
	}
	void SetOutlineWeight(float lineWeight) {
		var properties = GetOutlineProperties();
		if(properties != null) {
			var dirty = false;
			var shapeProperties = GetOutlineShapeProperties();
			if(shapeProperties != null) {
				if(!shapeProperties.DrawOutline) {
					shapeProperties.DrawOutline = true;
					dirty = true;
				}
			}
			if(properties.LineWeight != lineWeight) {
				properties.LineWeight = lineWeight;
				dirty = true;
			}
			if(dirty) shape.ForceMeshUpdate();
		}
	}
	
	


	public Color outlineColor {
		get {
			InitOutlineColor();
			return _outlineColor.value;
		}
		set {
			InitOutlineColor();
			_outlineColor.value = value;
		}
	}

	public Color targetOutlineColor {
		get {
			InitOutlineColor();
			return _outlineColor.animatedProperty != null ? _outlineColor.animatedProperty.end : _outlineColor.value;
		}
	}

	void InitOutlineColor() {
		if( _outlineColor == null ) {
			_outlineColor = new SLayoutColorProperty {
				getter = GetOutlineColor,
				setter = SetOutlineColor,
				isValid = GetIsValidForAnimation
			};
		}
	}

	Color GetOutlineColor() {
		var properties = GetOutlineShapeProperties();
		if(properties != null) return properties.OutlineColor;
		return Color.clear;
	}
	void SetOutlineColor(Color outlineColor) {
		var properties = GetOutlineShapeProperties();
		if(properties != null) {
			if(properties.OutlineColor != outlineColor) {
				properties.OutlineColor = outlineColor;
				shape.ForceMeshUpdate();
			} 
		}
	}
	
	
	

	public Color shadowColor {
		get {
			InitShadowColor();
			return _shadowColor.value;
		}
		set {
			InitShadowColor();
			_shadowColor.value = value;
		}
	}

	public Color targetShadowColor {
		get {
			InitShadowColor();
			return _shadowColor.animatedProperty != null ? _shadowColor.animatedProperty.end : _shadowColor.value;
		}
	}

	void InitShadowColor() {
		if( _shadowColor == null ) {
			_shadowColor = new SLayoutColorProperty {
				getter = GetShadowColor,
				setter = SetShadowColor,
				isValid = GetIsValidForAnimation
			};
		}
	}

	Color GetShadowColor() {
		var properties = GetShadowsProperties();
		if(properties != null) {
			foreach(var shadow in properties.Shadows) {
				if(shadow == null) continue;
				return shadow.Color;
			}
		}
		return Color.clear;
	}
	void SetShadowColor(Color ShadowColor) {
		var properties = GetShadowsProperties();
		if(properties != null) {
			var dirty = false;
			if(properties.Shadows == null) {
				properties.Shadows = new GeoUtils.ShadowProperties[1];
				dirty = true;
			}
			if(properties.Shadows.Length == 0) {
				Array.Resize(ref properties.Shadows, 1);
				dirty = true;
			}
            for (int i = 0; i < properties.Shadows.Length; i++) {
                GeoUtils.ShadowProperties shadow = properties.Shadows[i];
                if (shadow == null) shadow = new GeoUtils.ShadowProperties();
				if(shadow.Color != ShadowColor) {
					shadow.Color = ShadowColor;
					dirty = true;
				}
			}
			if(dirty) shape.ForceMeshUpdate();
		}
	}

	public void SetShadowPropertiesOverwrite (GeoUtils.ShadowProperties shadow) {
		var properties = GetShadowsProperties();
		if(properties != null) {
			var dirty = false;
			if(properties.Shadows == null) {
				properties.Shadows = new GeoUtils.ShadowProperties[1];
				dirty = true;
			}
			if(properties.Shadows.Length == 0) {
				Array.Resize(ref properties.Shadows, 1);
				dirty = true;
			}
			if(properties.Shadows[0] == null) properties.Shadows[0] = new GeoUtils.ShadowProperties();
			if(properties.Shadows[0].Color.r != shadow.Color.r || properties.Shadows[0].Color.g != shadow.Color.g || properties.Shadows[0].Color.b != shadow.Color.b || properties.Shadows[0].Color.a != shadow.Color.a) {
				properties.Shadows[0].Color = shadow.Color;
				dirty = true;
			}
			if(properties.Shadows[0].Offset != shadow.Offset) {
				properties.Shadows[0].Offset = shadow.Offset;
				dirty = true;
			}
			if(properties.Shadows[0].Size != shadow.Size) {
				properties.Shadows[0].Size = shadow.Size;
				dirty = true;
			}
			if(properties.Shadows[0].Softness != shadow.Softness) {
				properties.Shadows[0].Softness = shadow.Softness;
				dirty = true;
			}
			if(dirty) shape.ForceMeshUpdate();
		}
	}


	SLayoutColorProperty _fillColor;
	SLayoutFloatProperty _borderRadius;
	SLayoutFloatProperty _outlineWeight;
	SLayoutColorProperty _outlineColor;
	SLayoutColorProperty _shadowColor;
}