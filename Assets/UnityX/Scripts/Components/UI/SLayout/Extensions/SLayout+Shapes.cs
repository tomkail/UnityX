using System;
using ThisOtherThing.UI;
using ThisOtherThing.UI.Shapes;
using ThisOtherThing.UI.ShapeUtils;
using UnityEngine;
using Polygon = ThisOtherThing.UI.Shapes.Polygon;
using Line = ThisOtherThing.UI.Shapes.Line;

/// <summary>
/// Shortcut to get a TextMeshPro from an SLayout. Don't want to include it
/// directly in SLayout directly since we don't want a dependency on TMPro.
/// </summary>
public partial class SLayout
{
	public IShape shape => graphic as IShape;

	public Rectangle rectangle => graphic as Rectangle;

	public Arc arc => graphic as Arc;

	public EdgeGradient edgeGradient => graphic as EdgeGradient;

	public Ellipse ellipse => graphic as Ellipse;

	public Polygon polygon => graphic as Polygon;

	public Line line => graphic as Line;


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



	SLayoutColorProperty _fillColor;
	SLayoutColorProperty InitFillColor() {
		_fillColor ??= new SLayoutColorProperty {
			layout = this,
			getter = GetFillColor,
			setter = SetFillColor,
			isValid = GetIsValidForAnimation
		};
		return _fillColor;
	}
	public Color fillColor {
		get => InitFillColor().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitFillColor().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public Color startFillColor {
		get => InitFillColor().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitFillColor().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public Color targetFillColor {
		get => InitFillColor().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitFillColor().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
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
	

	SLayoutFloatProperty _borderRadius;
	SLayoutFloatProperty InitBorderRadius() {
		_borderRadius ??= new SLayoutFloatProperty {
			layout = this,
			getter = GetBorderRadius,
			setter = SetBorderRadius,
			isValid = GetIsValidForAnimation
		};
		return _borderRadius;
	}
	public float borderRadius {
		get => InitBorderRadius().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitBorderRadius().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public float startBorderRadius {
		get => InitBorderRadius().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitBorderRadius().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public float targetBorderRadius {
		get => InitBorderRadius().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitBorderRadius().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
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
	
	



	SLayoutFloatProperty _outlineWeight;
	SLayoutFloatProperty InitOutlineWeight() {
		_outlineWeight ??= new SLayoutFloatProperty {
			layout = this,
			getter = GetOutlineWeight,
			setter = SetOutlineWeight,
			isValid = GetIsValidForAnimation
		};
		return _outlineWeight;
	}
	public float outlineWeight {
		get => InitOutlineWeight().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitOutlineWeight().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public float startOutlineWeight {
		get => InitOutlineWeight().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitOutlineWeight().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public float targetOutlineWeight {
		get => InitOutlineWeight().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitOutlineWeight().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
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
	
	

	SLayoutColorProperty _outlineColor;
	SLayoutColorProperty InitOutlineColor() {
		_outlineColor ??= new SLayoutColorProperty {
			layout = this,
			getter = GetOutlineColor,
			setter = SetOutlineColor,
			isValid = GetIsValidForAnimation
		};
		return _outlineColor;
	}
	public Color outlineColor {
		get => InitOutlineColor().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitOutlineColor().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public Color startOutlineColor {
		get => InitOutlineColor().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitOutlineColor().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public Color targetOutlineColor {
		get => InitOutlineColor().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitOutlineColor().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
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
	
	
	
	SLayoutColorProperty _shadowColor;
	SLayoutColorProperty InitShadowColor() {
		_shadowColor ??= new SLayoutColorProperty {
			layout = this,
			getter = GetShadowColor,
			setter = SetShadowColor,
			isValid = GetIsValidForAnimation
		};
		return _shadowColor;
	}
	public Color shadowColor {
		get => InitShadowColor().GetProperty(SLayoutProperty.GetMode.Current);
		set => InitShadowColor().SetProperty(value, SLayoutProperty.SetMode.Auto);
	}
	public Color startShadowColor {
		get => InitShadowColor().GetProperty(SLayoutProperty.GetMode.AnimStart);
		set => InitShadowColor().SetProperty(value, SLayoutProperty.SetMode.AnimStart);
	}
	public Color targetShadowColor {
		get => InitShadowColor().GetProperty(SLayoutProperty.GetMode.AnimEnd);
		set => InitShadowColor().SetProperty(value, SLayoutProperty.SetMode.AnimEnd);
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

	public void SetShadowPropertiesOverwrite(GeoUtils.ShadowProperties shadow) {
		var properties = GetShadowsProperties();
		if (properties != null) {
			var dirty = false;
			if (properties.Shadows == null) {
				properties.Shadows = new GeoUtils.ShadowProperties[1];
				dirty = true;
			}

			if (properties.Shadows.Length == 0) {
				Array.Resize(ref properties.Shadows, 1);
				dirty = true;
			}

			if (properties.Shadows[0] == null) properties.Shadows[0] = new GeoUtils.ShadowProperties();
			if (properties.Shadows[0].Color.r != shadow.Color.r || properties.Shadows[0].Color.g != shadow.Color.g || properties.Shadows[0].Color.b != shadow.Color.b || properties.Shadows[0].Color.a != shadow.Color.a) {
				properties.Shadows[0].Color = shadow.Color;
				dirty = true;
			}

			if (properties.Shadows[0].Offset != shadow.Offset) {
				properties.Shadows[0].Offset = shadow.Offset;
				dirty = true;
			}

			if (properties.Shadows[0].Size != shadow.Size) {
				properties.Shadows[0].Size = shadow.Size;
				dirty = true;
			}

			if (properties.Shadows[0].Softness != shadow.Softness) {
				properties.Shadows[0].Softness = shadow.Softness;
				dirty = true;
			}

			if (dirty) shape.ForceMeshUpdate();
		}
	}
}