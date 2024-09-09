using System.Globalization;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof (Spring))]
public class SpringPropertyDrawer : PropertyDrawer {
	const float curveHeight = 40;
	static bool allowOverdamping = false;
	
	static Texture _swapPropertiesIcon;
	static Texture swapPropertiesIcon {
		get {
			if (_swapPropertiesIcon == null) _swapPropertiesIcon = EditorGUIUtility.IconContent("Preset.Context").image;
			return _swapPropertiesIcon;
		}
	}

	static bool showPhysicalProperties {
		get => EditorPrefs.GetBool($"{nameof(SpringPropertyDrawer)}.showPhysicalProperties", false);
		set => EditorPrefs.SetBool($"{nameof(SpringPropertyDrawer)}.showPhysicalProperties", value);
	}
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty (position, label, property);
		if (property.isExpanded) EditorGUI.indentLevel++;
        
		var showingProperties = EditorGUIUtility.wideMode || property.isExpanded;
		if (showingProperties) {
			var y = DrawProperties(position, property, label) + EditorGUIUtility.standardVerticalSpacing;
			if(property.isExpanded) {
				var curveRect = new Rect(position.x, y, position.width, curveHeight);
				
				var mass = property.FindPropertyRelative("_mass");
				var stiffness = property.FindPropertyRelative("_stiffness");
				var damping = property.FindPropertyRelative("_damping");
				DrawSpringGraph(curveRect, 1, 0, 0, mass.floatValue, stiffness.floatValue, damping.floatValue, null);
			}
		}
		if (property.isExpanded) EditorGUI.indentLevel--;
		property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, property.displayName, true);
		EditorGUI.EndProperty ();
	}

	public void Draw(Rect position, SerializedProperty springProperty, GUIContent label, float startValue, float endValue, float initialVelocity, float? time) {
        if (springProperty.isExpanded) EditorGUI.indentLevel++;
        
		var showingProperties = EditorGUIUtility.wideMode || springProperty.isExpanded;
		if (showingProperties) {
			var y = DrawProperties(position, springProperty, label) + EditorGUIUtility.standardVerticalSpacing;
			if(springProperty.isExpanded) {
				var curveRect = new Rect(position.x, y, position.width, curveHeight);
				
				var mass = springProperty.FindPropertyRelative("_mass");
				var stiffness = springProperty.FindPropertyRelative("_stiffness");
				var damping = springProperty.FindPropertyRelative("_damping");
				DrawSpringGraph(curveRect, startValue, endValue, initialVelocity, mass.floatValue, stiffness.floatValue, damping.floatValue, time);
			}
		}
		if (springProperty.isExpanded) EditorGUI.indentLevel--;
		springProperty.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), springProperty.isExpanded, springProperty.displayName, true);
	}

	float DrawProperties(Rect position, SerializedProperty property, GUIContent label) {
		var cachedLabelWidth = EditorGUIUtility.labelWidth;

		static float CalculateItemSize(float containerSize, int numItems, float spacing, Vector2 margin) {
			return numItems == 0 ? 0 : (containerSize - (spacing * (numItems - 1)) - (margin.x + margin.y)) / numItems;
		}
		Rect massRect = Rect.zero;
		Rect stiffnessRect = Rect.zero;
		Rect dampingRect = Rect.zero;
		Rect responseRect = Rect.zero;
		Rect dampingRatioRect = Rect.zero;
		Rect showPhysicalPropertiesRect = Rect.zero;
		var showPhysicalPropertiesWidth = 20;
		var spacing = 4;
		if (EditorGUIUtility.wideMode) {
			var currentRect = new Rect(position.x+EditorGUIUtility.labelWidth, position.y, position.width-EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
			if (showPhysicalProperties) {
				EditorGUIUtility.labelWidth = 37;
				var width = CalculateItemSize(currentRect.width, 3, spacing, Vector2.zero);
				massRect = new Rect(currentRect.x, currentRect.y, width, currentRect.height);
				stiffnessRect = new Rect(currentRect.x + (width + spacing) * 1, currentRect.y, width, currentRect.height);
				dampingRect = new Rect(currentRect.x + (width + spacing) * 2, currentRect.y, width, currentRect.height);
				showPhysicalPropertiesRect = new Rect(currentRect.x-(showPhysicalPropertiesWidth+2), currentRect.y, showPhysicalPropertiesWidth, currentRect.height);
			} else {
				EditorGUIUtility.labelWidth = 58;
				var width = CalculateItemSize(currentRect.width, 2, spacing, Vector2.zero);
				responseRect = new Rect(currentRect.x, currentRect.y, width, currentRect.height);
				dampingRatioRect = new Rect(currentRect.x + (width + spacing) * 1, currentRect.y, width, currentRect.height);
				showPhysicalPropertiesRect = new Rect(currentRect.x-(showPhysicalPropertiesWidth+2), currentRect.y, showPhysicalPropertiesWidth, currentRect.height);
			}
			
		} else {
			var indentedRect = EditorGUI.IndentedRect(position);
			if (showPhysicalProperties) {
				var showPhysicalPropertiesHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;
				massRect = new Rect(indentedRect.x, indentedRect.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 1, indentedRect.width, EditorGUIUtility.singleLineHeight);
				stiffnessRect = new Rect(indentedRect.x, indentedRect.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2, indentedRect.width, EditorGUIUtility.singleLineHeight);
				dampingRect = new Rect(indentedRect.x, indentedRect.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3, indentedRect.width, EditorGUIUtility.singleLineHeight);
				showPhysicalPropertiesRect = new Rect(indentedRect.x-(showPhysicalPropertiesWidth+2), position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 1, showPhysicalPropertiesWidth, showPhysicalPropertiesHeight);
			} else {
				var showPhysicalPropertiesHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
				responseRect = new Rect(indentedRect.x, indentedRect.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 1, indentedRect.width, EditorGUIUtility.singleLineHeight);
				dampingRatioRect = new Rect(indentedRect.x, indentedRect.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2, indentedRect.width, EditorGUIUtility.singleLineHeight);
				showPhysicalPropertiesRect = new Rect(indentedRect.x-(showPhysicalPropertiesWidth+2), position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 1, showPhysicalPropertiesWidth, showPhysicalPropertiesHeight);
			}
		}

		var cachedIndentLevel = EditorGUI.indentLevel; 
		EditorGUI.indentLevel = 0;
		if(GUI.Button(showPhysicalPropertiesRect, new GUIContent(swapPropertiesIcon, showPhysicalProperties ? "Switch to Frequency/Damping Properties" : "Switch to Physical Properties"))){
			showPhysicalProperties = !showPhysicalProperties;
		}
		var mass = property.FindPropertyRelative("_mass");
		var stiffness = property.FindPropertyRelative("_stiffness");
		var damping = property.FindPropertyRelative("_damping");
		var response = property.FindPropertyRelative("_response");
		var dampingRatio = property.FindPropertyRelative("_dampingRatio");
		if (showPhysicalProperties) {
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(massRect, mass, new GUIContent("Mass", "The mass of the object attached to the end of the spring"));
			EditorGUI.PropertyField(stiffnessRect, stiffness, new GUIContent(EditorGUIUtility.wideMode ? "Stiff." : "Stiffness", "The spring stiffness coefficient"));
			EditorGUI.PropertyField(dampingRect, damping, new GUIContent(EditorGUIUtility.wideMode ? "Damp." : "Damping", "Defines how the spring’s motion should be damped due to the forces of friction"));
			if (EditorGUI.EndChangeCheck()) {
				var responseDampingProperties = Spring.PhysicalToResponseDamping(mass.floatValue, stiffness.floatValue, damping.floatValue);
				response.floatValue = responseDampingProperties.response;
				dampingRatio.floatValue = responseDampingProperties.dampingRatio;
			}
		} else {
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(responseRect, response, new GUIContent("Response", "The stiffness of the spring, defined as an approximate duration in seconds"));
			response.floatValue = Mathf.Max(response.floatValue, 0.001f);
			var bounce = EditorGUI.FloatField(dampingRatioRect, new GUIContent("Bounce", "How bouncy the spring is"), 1f - dampingRatio.floatValue);
			if (EditorGUI.EndChangeCheck()) {
				if(allowOverdamping) bounce = Mathf.Min(bounce, 0.999f);
				else bounce = Mathf.Clamp(bounce, 0, 0.999f);
				dampingRatio.floatValue = 1f - bounce;
				// EditorGUI.PropertyField(dampingRatioRect, dampingRatio, new GUIContent(EditorGUIUtility.wideMode ? "Damp Ratio" : "Damping Ratio"));
				var physicalProperties = Spring.ResponseDampingToPhysical(response.floatValue, dampingRatio.floatValue, mass.floatValue == 0 ? 1 : mass.floatValue);
				mass.floatValue = physicalProperties.mass;
				stiffness.floatValue = physicalProperties.stiffness;
				damping.floatValue = physicalProperties.damping;
			}
		}
		EditorGUI.indentLevel = cachedIndentLevel;

		
		EditorGUIUtility.labelWidth = cachedLabelWidth;
		
		return showPhysicalPropertiesRect.yMax;
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		var graphSize = property.isExpanded ? curveHeight + EditorGUIUtility.standardVerticalSpacing : 0;
		if (EditorGUIUtility.wideMode) {
			return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + graphSize;
		} else {
			if (property.isExpanded) {
				if(showPhysicalProperties) {
					return EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 4 + graphSize;
				} else {
					return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 3 + graphSize;
				}
			} else {
				return EditorGUIUtility.singleLineHeight;
			}
		}
	}
	


	
	// static Texture _settlingDurationMarkerIcon;
	// static Texture settlingDurationMarkerIcon {
	// 	get {
	// 		if (_settlingDurationMarkerIcon == null) _settlingDurationMarkerIcon = EditorGUIUtility.IconContent("curvekeyframe").image;
	// 		return _settlingDurationMarkerIcon;
	// 	}
	// }
	static Texture _currentTimeMarkerIcon;
	static Texture currentTimeMarkerIcon {
		get {
			if (_currentTimeMarkerIcon == null) _currentTimeMarkerIcon = EditorGUIUtility.IconContent("d_curvekeyframeselected").image;
			return _currentTimeMarkerIcon;
		}
	}

	
	public static void DrawSpringGraph(Rect rect, float startValue, float endValue, float initialVelocity, float mass, float stiffness, float damping, float? time) {
		// Draw graph
		float settlingDuration = Spring.SettlingDuration(mass, stiffness, damping);
		float graphMaxTime = settlingDuration;
		float samplePixelDistance = 3;
		int numKeys = Mathf.Max(1, Mathf.FloorToInt(rect.width / samplePixelDistance));
		Keyframe[] keys = new Keyframe[numKeys];
		var r = 1f/(numKeys-1);
		for (int i = 0; i < numKeys; i++) {
			var sampleTime = (r * i) * graphMaxTime;
			var val = Spring.Value(startValue, endValue, initialVelocity, sampleTime, mass, stiffness, damping);
			keys[i] = new Keyframe(sampleTime, val);
		}
		AnimationCurve curve = new AnimationCurve(keys);
		for (int i = 0; i < curve.keys.Length; i++) curve.SmoothTangents(i, 0);
		
		var graphGUI = new GraphGUI(curve);
		graphGUI.showHoverTooltip = true;
		EditorGUI.BeginDisabledGroup(true);
		graphGUI.DrawSpringGraph(rect, (indentedGraphRect) => {
			if (time != null) {
				var valueAtTime = Spring.Value(startValue, endValue, initialVelocity, time.Value, mass, stiffness, damping);
				var pos = graphGUI.TimeAndValueToGUIRectPosition(indentedGraphRect, time.Value, valueAtTime);
				GUI.DrawTexture(GraphGUI.CreateRectFromCenter(pos.x, pos.y+1, 16, 16), currentTimeMarkerIcon);
			}
			graphGUI.DrawXAxisLabel(indentedGraphRect, "Time");
			graphGUI.DrawXMinMaxScaleLabels(indentedGraphRect);
			var minMaxScaleLabels = GraphGUI.RoundToSignificantDigits(2, graphGUI.curveValueRanges.yMin, graphGUI.curveValueRanges.yMax);
			graphGUI.DrawYScaleLabel(indentedGraphRect, graphGUI.curveValueRanges.yMin, minMaxScaleLabels[0].ToString(CultureInfo.InvariantCulture));
			graphGUI.DrawYScaleLabel(indentedGraphRect, graphGUI.curveValueRanges.yMax, minMaxScaleLabels[1].ToString(CultureInfo.InvariantCulture));
		});
		EditorGUI.EndDisabledGroup();
	}
	
	public class GraphGUI {
    public AnimationCurve curve;
    public Rect curveValueRanges;
    public Rect viewValueRanges;
    
    GUIStyle labelStyle => EditorStyles.centeredGreyMiniLabel;

    static int scaleHeight = 14;
    static int scaleMargin = 2;
    public bool showHoverTooltip;

    // Create a new graph GUI for the given curve and sets the rect of the graph to the range of the curve
    public GraphGUI(AnimationCurve curve) {
        this.curve = curve;

        curveValueRanges = viewValueRanges = Rect.MinMaxRect(curve.keys.Min(x => x.time), curve.keys.Min(x => x.value), curve.keys.Max(x => x.time), curve.keys.Max(x => x.value));

        if (viewValueRanges.height == 0) {
            viewValueRanges.yMin -= 1;
            viewValueRanges.yMax += 1;
        } else {
            viewValueRanges.yMin -= (viewValueRanges.yMax - viewValueRanges.yMin) * 0.35f;
            viewValueRanges.yMax += (viewValueRanges.yMax - viewValueRanges.yMin) * 0.35f;
        }
    }
    
    public delegate void OnDrawIndentedGraphGUI(Rect indentedGraphRect);
    // Draw the graph. Provides callback to draw additional GUI elements inside the graph area
    public void DrawSpringGraph(Rect rect, OnDrawIndentedGraphGUI onDrawIndentedGraphGUI) {
        var graphRect = new Rect(rect.x, rect.y, rect.width, rect.height-scaleHeight);
        EditorGUI.CurveField(graphRect, GUIContent.none, curve, Color.white, viewValueRanges);
        var indentedGraphRect = EditorGUI.IndentedRect(graphRect);
        onDrawIndentedGraphGUI?.Invoke(indentedGraphRect);
        
        if (showHoverTooltip && indentedGraphRect.Contains(Event.current.mousePosition)) {
            var normalizedTime = Rect.PointToNormalized(indentedGraphRect, Event.current.mousePosition).x;
            var time = Mathf.Lerp(viewValueRanges.xMin, viewValueRanges.xMax, normalizedTime);
            var value = curve.Evaluate(time);
            var label = new GUIContent($"Time: {RoundToSignificantDigits(2, time).ToString(CultureInfo.CurrentCulture)}\nValue: {RoundToSignificantDigits(2, value).ToString(CultureInfo.CurrentCulture)}");
            var labelSize = EditorStyles.helpBox.CalcSize(label);
            var tooltipPivotPosition = TimeAndValueToGUIRectPosition(indentedGraphRect, time, value);
            var tooltipRect = new Rect(tooltipPivotPosition.x - labelSize.x * 0.5f, tooltipPivotPosition.y-labelSize.y, labelSize.x, labelSize.y);
            GUI.Box(tooltipRect, label, EditorStyles.helpBox);
        }
    }
    
    public Vector2 TimeAndValueToNormalized(float time, float value) {
        return new Vector2(Mathf.InverseLerp(viewValueRanges.xMin, viewValueRanges.xMax, time), Mathf.InverseLerp(viewValueRanges.yMin, viewValueRanges.yMax, value));
    }
	
    public Vector2 TimeAndValueToGUIRectPosition(Rect indentedGraphRect, float time, float value) {
        var normalizedTimeAndValue = TimeAndValueToNormalized(time, value);
        normalizedTimeAndValue.y = 1 - normalizedTimeAndValue.y;
        return Rect.NormalizedToPoint(indentedGraphRect, normalizedTimeAndValue);
    }

    public void DrawXAxisLabel(Rect indentedGraphRect, string labelStr) => DrawXAxisLabel(indentedGraphRect, new GUIContent(labelStr));
    public void DrawXAxisLabel(Rect indentedGraphRect, GUIContent label) {
        var labelSize = labelStyle.CalcSize(label);
        GUI.Label(CreateRectFromCenter(indentedGraphRect.center.x, indentedGraphRect.yMax + scaleMargin + labelSize.y * 0.5f, labelSize.x, labelSize.y), label, labelStyle);
    }
    
    public void DrawYAxisLabel(Rect indentedGraphRect, string labelStr) => DrawYAxisLabel(indentedGraphRect, new GUIContent(labelStr));
    public void DrawYAxisLabel(Rect indentedGraphRect, GUIContent label) {
        var labelSize = labelStyle.CalcSize(label);
        GUI.Label(CreateRectFromCenter(indentedGraphRect.xMin - scaleMargin - labelSize.x * 0.5f, indentedGraphRect.center.y, labelSize.x, labelSize.y), label, labelStyle);
    }
    
    
    public void DrawXScaleLabel(Rect indentedGraphRect, float time, string label) {
        var minTimeLabel = new GUIContent(label);
        var minTimeLabelSize = labelStyle.CalcSize(minTimeLabel);
        var position = TimeAndValueToGUIRectPosition(indentedGraphRect, time, viewValueRanges.yMin);
        // DrawLine(new Vector2(position.x, position.y), new Vector2(position.x, position.y + scaleMargin), labelStyle.normal.textColor, 1);
        GUI.Label(CreateRectFromCenter(position.x, position.y + scaleMargin + minTimeLabelSize.y * 0.5f, minTimeLabelSize.x, minTimeLabelSize.y), minTimeLabel, labelStyle);
    }
    
    // Displays the values of the view value ranges on the graph. Offsets them so they don't overshoot the graph rect.
    public void DrawXMinMaxScaleLabels(Rect indentedGraphRect) {
        var minMaxScaleLabels = RoundToSignificantDigits(2, curveValueRanges.xMin, curveValueRanges.xMax);
        {
            var label = new GUIContent(minMaxScaleLabels[0].ToString(CultureInfo.CurrentCulture));
            var labelSize = labelStyle.CalcSize(label);
            var position = TimeAndValueToGUIRectPosition(indentedGraphRect, curveValueRanges.xMin, viewValueRanges.yMin);
            DrawLine(new Vector2(position.x, position.y), new Vector2(position.x, position.y + scaleMargin), labelStyle.normal.textColor, 1);
            GUI.Label(CreateRectFromCenter(position.x + labelSize.x*0.5f, position.y + scaleMargin + labelSize.y * 0.5f, labelSize.x, labelSize.y), label, labelStyle);
        }
        {
            var label = new GUIContent(minMaxScaleLabels[1].ToString(CultureInfo.CurrentCulture));
            var labelSize = labelStyle.CalcSize(label);
            var position = TimeAndValueToGUIRectPosition(indentedGraphRect, curveValueRanges.xMax, viewValueRanges.yMin);
            DrawLine(new Vector2(position.x, position.y), new Vector2(position.x, position.y + scaleMargin), labelStyle.normal.textColor, 1);
            GUI.Label(CreateRectFromCenter(position.x - labelSize.x * 0.5f, position.y + scaleMargin + labelSize.y * 0.5f, labelSize.x, labelSize.y), label, labelStyle);
        }
    }

    
    
    public void DrawYScaleLabel(Rect indentedGraphRect, float value, string label) {
        var minTimeLabel = new GUIContent(label);
        var minTimeLabelSize = labelStyle.CalcSize(minTimeLabel);
        var position = TimeAndValueToGUIRectPosition(indentedGraphRect, viewValueRanges.xMin, value);
        DrawLine(new Vector2(position.x, position.y), new Vector2(position.x - scaleMargin, position.y), labelStyle.normal.textColor, 1);
        GUI.Label(CreateRectFromCenter(position.x - scaleMargin - minTimeLabelSize.x * 0.5f, position.y, minTimeLabelSize.x, minTimeLabelSize.y), minTimeLabel, labelStyle);
    }
    
    // Displays the values of the view value ranges on the graph. Offsets them so they don't overshoot the graph rect.
    public void DrawYMinMaxScaleLabels(Rect indentedGraphRect) {
        var minMaxScaleLabels = RoundToSignificantDigits(2, viewValueRanges.yMin, viewValueRanges.yMax);
        // DrawYScaleLabel(indentedGraphRect, curveValueRanges.yMin, minMaxScaleLabels[0].ToString(CultureInfo.CurrentCulture));
        // DrawYScaleLabel(indentedGraphRect, curveValueRanges.yMax, minMaxScaleLabels[1].ToString(CultureInfo.CurrentCulture));
        {
            var label = new GUIContent(minMaxScaleLabels[0].ToString(CultureInfo.CurrentCulture));
            var labelSize = labelStyle.CalcSize(label);
            var position = TimeAndValueToGUIRectPosition(indentedGraphRect, curveValueRanges.xMin, viewValueRanges.yMin);
            DrawLine(new Vector2(position.x, position.y), new Vector2(position.x - scaleMargin, position.y), labelStyle.normal.textColor, 1);
            GUI.Label(CreateRectFromCenter(position.x - scaleMargin - labelSize.x*0.5f, position.y - labelSize.y*0.5f, labelSize.x, labelSize.y), label, labelStyle);
        }
        {
            var label = new GUIContent(minMaxScaleLabels[1].ToString(CultureInfo.CurrentCulture));
            var labelSize = labelStyle.CalcSize(label);
            var position = TimeAndValueToGUIRectPosition(indentedGraphRect, curveValueRanges.xMin, viewValueRanges.yMax);
            DrawLine(new Vector2(position.x, position.y), new Vector2(position.x - scaleMargin, position.y), labelStyle.normal.textColor, 1);
            GUI.Label(CreateRectFromCenter(position.x - scaleMargin - labelSize.x * 0.5f, position.y + labelSize.y*0.5f, labelSize.x, labelSize.y), label, labelStyle);
        }
    }
	
    public static Rect CreateRectFromCenter (Vector2 centerPosition, Vector2 size) {
        return CreateRectFromCenter(centerPosition.x, centerPosition.y, size.x, size.y);
    }

    public static Rect CreateRectFromCenter (float centerX, float centerY, float sizeX, float sizeY) {
        return new Rect(centerX - sizeX * 0.5f, centerY - sizeY * 0.5f, sizeX, sizeY);
    }
	
    
    static Vector3 offset = new(0, -0.5f, 0); // Compensate for line width	
    static Matrix4x4 guiTransMat = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
    static Matrix4x4 guiTransMatInv = Matrix4x4.TRS(-offset, Quaternion.identity, Vector3.one);
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width) {
        if(width <= 0 || pointA == pointB || color.a == 0) return;

        Matrix4x4 matrix = GUI.matrix;
        Color savedColor = GUI.color;
        GUI.color = color;

        var delta = (Vector3)(pointB-pointA);
        Quaternion guiRot = Quaternion.FromToRotation(Vector2.right, delta);
        Matrix4x4 guiRotMat = Matrix4x4.TRS(pointA, guiRot, new Vector3(delta.magnitude, width, 1));
        GUI.matrix = guiTransMatInv * guiRotMat * guiTransMat;
        
        GUI.DrawTexture(new Rect(0, 0, 1, 1), Texture2D.whiteTexture);
        GUI.matrix = matrix;
        GUI.color = savedColor;
    }
    
    // Rounds several numbers to the same factor, using the largest number and a fixed num sig.digits by absolute value to determine the scale
    public static float RoundToSignificantDigits(int significantDigits, float value) {
        if(value == 0) return 0;
        float scale = Mathf.Pow(10, Mathf.Floor(Mathf.Log10(Mathf.Abs(value))) + 1);
        return scale * (value / scale).RoundTo(significantDigits);
    }
    public static float[] RoundToSignificantDigits(int significantDigits, params float[] values) {
        // Find the largest number by absolute value to determine the scale
        float maxNum = 0;
        foreach (float num in values) if (Mathf.Abs(num) > Mathf.Abs(maxNum)) maxNum = num;
        // Calculate the scale factor based on the largest number
        float scale = Mathf.Pow(10, (int)Mathf.Floor(Mathf.Log10(Mathf.Abs(maxNum))) - (significantDigits - 1));
        // Round all numbers using the calculated scale
        for (int i = 0; i < values.Length; i++) values[i] = Mathf.Round(values[i] / scale) * scale;

        return values;
    }
}
}