using System.Globalization;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (NoiseSamplerProperties))]
public class NoiseSamplerPropertiesPropertyDrawer : PropertyDrawer {
    const float curveHeight = 40;
    static float graphXRange = 20;
	
    static Texture _currentPositionMarkerIcon;
    static Texture currentPositionMarkerIcon {
        get {
            if (_currentPositionMarkerIcon == null) _currentPositionMarkerIcon = EditorGUIUtility.IconContent("d_curvekeyframeselected").image;
            return _currentPositionMarkerIcon;
        }
    }
	
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty (position, label, property);

        if(property.isExpanded) {
            EditorGUI.indentLevel++;
            var y = DrawProperties(position, property, label) + EditorGUIUtility.standardVerticalSpacing;
			
            var curveRect = new Rect(position.x, y, position.width, curveHeight);

            var frequency = property.FindPropertyRelative("frequency");
            var octaves = property.FindPropertyRelative("octaves");
            var lacunarity = property.FindPropertyRelative("lacunarity");
            var persistence = property.FindPropertyRelative("persistence");
            DrawNoiseGraph(curveRect, Noise.Perlin3D, frequency.floatValue, octaves.intValue, lacunarity.floatValue, persistence.floatValue);
            EditorGUI.indentLevel--;
        }
        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, property.displayName, true);
        EditorGUI.EndProperty ();
    }

    public void Draw(Rect position, SerializedProperty noiseProperties, GUIContent label, Vector3 positionOffset) {
        
        var showingProperties = noiseProperties.isExpanded;
        if (showingProperties) {
            EditorGUI.indentLevel++;
            var y = DrawProperties(position, noiseProperties, label) + EditorGUIUtility.standardVerticalSpacing;
            if(noiseProperties.isExpanded) {
                var curveRect = new Rect(position.x, y, position.width, curveHeight);
				
                var frequency = noiseProperties.FindPropertyRelative("frequency");
                var octaves = noiseProperties.FindPropertyRelative("octaves");
                var lacunarity = noiseProperties.FindPropertyRelative("lacunarity");
                var persistence = noiseProperties.FindPropertyRelative("persistence");
                DrawNoiseGraph(curveRect, Noise.Perlin3D, frequency.floatValue, octaves.intValue, lacunarity.floatValue, persistence.floatValue, positionOffset);
            }
            EditorGUI.indentLevel--;
        }
        noiseProperties.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), noiseProperties.isExpanded, noiseProperties.displayName, true);
    }

    float DrawProperties(Rect position, SerializedProperty property, GUIContent label) {
        var cachedLabelWidth = EditorGUIUtility.labelWidth;

        var indentedRect = EditorGUI.IndentedRect(position);

        var y = indentedRect.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 1;
        Rect frequencyRect = new Rect(indentedRect.x, y, indentedRect.width, EditorGUIUtility.singleLineHeight);
        var frequency = property.FindPropertyRelative("frequency");
        y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        Rect octavesRect = new Rect(indentedRect.x, y, indentedRect.width, EditorGUIUtility.singleLineHeight);
        var octaves = property.FindPropertyRelative("octaves");
        y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		
        var cachedIndentLevel = EditorGUI.indentLevel; 
        EditorGUI.indentLevel = 0;
        
        EditorGUI.PropertyField(frequencyRect, frequency, new GUIContent("Frequency"));
        EditorGUI.PropertyField(octavesRect, octaves, new GUIContent("Octaves"));
        octaves.intValue = Mathf.Max(octaves.intValue, 1);
        if (octaves.intValue > 1) {
            Rect lacunarityRect = new Rect(indentedRect.x, y, indentedRect.width, EditorGUIUtility.singleLineHeight);
            var lacunarity = property.FindPropertyRelative("lacunarity");
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            Rect persistenceRect = new Rect(indentedRect.x, y, indentedRect.width, EditorGUIUtility.singleLineHeight);
            var persistence = property.FindPropertyRelative("persistence");
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                
            EditorGUI.PropertyField(lacunarityRect, lacunarity, new GUIContent("Lacunarity"));
            EditorGUI.PropertyField(persistenceRect, persistence, new GUIContent("Persistence"));
        }
		
        EditorGUI.indentLevel = cachedIndentLevel;

		
        EditorGUIUtility.labelWidth = cachedLabelWidth;

        return y;
    }

	
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        var graphSize = property.isExpanded ? curveHeight + EditorGUIUtility.standardVerticalSpacing : 0;
        if (property.isExpanded) {
            var octaves = property.FindPropertyRelative("octaves");
            if(octaves.intValue > 1) return EditorGUIUtility.singleLineHeight * 5 + EditorGUIUtility.standardVerticalSpacing * 5 + graphSize;
            else return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 3 + graphSize;
        }
        else return EditorGUIUtility.singleLineHeight;
    }

    public static void DrawNoiseGraph(Rect rect, float frequency, int octaves, float lacunarity, float persistence) {
        DrawNoiseGraph(rect, Noise.Perlin3D, frequency, octaves, lacunarity, persistence, Vector3.zero);
    }
    public static void DrawNoiseGraph(Rect rect, NoiseMethod noiseMethod, float frequency, int octaves, float lacunarity, float persistence) {
        DrawNoiseGraph(rect, noiseMethod, frequency, octaves, lacunarity, persistence, Vector3.zero);
    }

    public static void DrawNoiseGraph(Rect rect, NoiseMethod noiseMethod, float frequency, int octaves, float lacunarity, float persistence, Vector3 offsetPosition) {
        // Draw graph
        float minGraphTime = offsetPosition.x + -graphXRange * 0.5f;
        float maxGraphTime = offsetPosition.x + graphXRange * 0.5f;
        
        float samplePixelDistance = 3;
        int numKeys = Mathf.Max(1, Mathf.FloorToInt(rect.width / samplePixelDistance));
        Keyframe[] keys = new Keyframe[numKeys];
        var r = 1f/(numKeys-1);
        for (int i = 0; i < numKeys; i++) {
            var sampleTime = Mathf.Lerp(minGraphTime, maxGraphTime, r * i);
            var position = new Vector3(sampleTime, offsetPosition.y, offsetPosition.z);
            var val = Noise.Sum(noiseMethod, position, frequency, octaves, lacunarity, persistence);
            keys[i] = new Keyframe(sampleTime, val.value);
        }
        AnimationCurve curve = new AnimationCurve(keys);
        for (int i = 0; i < curve.keys.Length; i++) curve.SmoothTangents(i, 0);
        
        
        var graphGUI = new GraphGUI(curve);
        graphGUI.showHoverTooltip = true;
        EditorGUI.BeginDisabledGroup(true);
        graphGUI.DrawSpringGraph(rect, (indentedGraphRect) => {
            {
                var pointerPosition = 0.5f;
                var normalizedRectCoordinates = new Vector2(pointerPosition, 1);
                var pos = Rect.NormalizedToPoint(indentedGraphRect, normalizedRectCoordinates);
                // GUI.DrawTexture(CreateFromCenter(pos.x, pos.y+1, 16, 16), currentPositionMarkerIcon);
                Color savedColor = GUI.color;
                GUI.color = Color.gray;
                GUI.DrawTexture(new Rect(indentedGraphRect.x+indentedGraphRect.width*0.5f, indentedGraphRect.y, 1, indentedGraphRect.height), Texture2D.whiteTexture);
                GUI.color = savedColor;
            }
            graphGUI.DrawXAxisLabel(indentedGraphRect, "Position");
            graphGUI.DrawXMinMaxScaleLabels(indentedGraphRect);
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