using System.Globalization;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof (Spring))]
public class SpringPropertyDrawer : PropertyDrawer {
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
				var curveRect = new Rect(position.x, y, position.width, 30);
				
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
				var curveRect = new Rect(position.x, y, position.width, 30);
				
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
				EditorGUIUtility.labelWidth = 34;
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
			var indentedWidth = position.x-indentedRect.x;
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
			EditorGUI.PropertyField(massRect, mass, new GUIContent(new GUIContent("Mass", "The mass of the object attached to the end of the spring")));
			EditorGUI.PropertyField(stiffnessRect, stiffness, new GUIContent(new GUIContent(EditorGUIUtility.wideMode ? "Stiff." : "Stiffness", "The spring stiffness coefficient")));
			EditorGUI.PropertyField(dampingRect, damping, new GUIContent(new GUIContent(EditorGUIUtility.wideMode ? "Damp." : "Damping", "Defines how the spring’s motion should be damped due to the forces of friction")));
			var responseDampingProperties = Spring.PhysicalToResponseDamping(mass.floatValue, stiffness.floatValue, damping.floatValue);
			response.floatValue = responseDampingProperties.response;
			dampingRatio.floatValue = responseDampingProperties.dampingRatio;
		} else {
			EditorGUI.PropertyField(responseRect, response, new GUIContent("Response", "The stiffness of the spring, defined as an approximate duration in seconds"));
			response.floatValue = Mathf.Max(response.floatValue, 0.001f);
			var bounce = EditorGUI.FloatField(dampingRatioRect, new GUIContent("Bounce", "How bouncy the spring is"), 1f - dampingRatio.floatValue);
			dampingRatio.floatValue = 1f-Mathf.Clamp(bounce, -1, 0.999f);
			// EditorGUI.PropertyField(dampingRatioRect, dampingRatio, new GUIContent(EditorGUIUtility.wideMode ? "Damp Ratio" : "Damping Ratio"));
			var physicalProperties = Spring.ResponseDampingToPhysical(response.floatValue, dampingRatio.floatValue, mass.floatValue == 0 ? 1 : mass.floatValue);
			mass.floatValue = physicalProperties.mass;
			stiffness.floatValue = physicalProperties.stiffness;
			damping.floatValue = physicalProperties.damping;
		}
		EditorGUI.indentLevel = cachedIndentLevel;

		
		EditorGUIUtility.labelWidth = cachedLabelWidth;
		
		return showPhysicalPropertiesRect.yMax;
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		var graphSize = property.isExpanded ? (40 + EditorGUIUtility.standardVerticalSpacing) : 0;
		if (EditorGUIUtility.wideMode) {
			return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + graphSize;
		} else {
			if (property.isExpanded) {
				if(showPhysicalProperties) {
					return EditorGUIUtility.singleLineHeight * 5 + EditorGUIUtility.standardVerticalSpacing * 5 + graphSize;
				} else {
					return EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 4 + graphSize;
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
		EditorGUI.BeginDisabledGroup(true);
		
		// Draw graph
		float settlingDuration = Spring.SettlingDuration(mass, stiffness, damping);
		float graphMaxTime = settlingDuration;
		float samplePixelDistance = 2;
		int numKeys = Mathf.Max(1, Mathf.FloorToInt(rect.width / samplePixelDistance));
		Keyframe[] keys = new Keyframe[numKeys];
		var r = 1f/(numKeys-1);
		for (int i = 0; i < numKeys; i++) {
			var sampleTime = (r * i) * graphMaxTime;
			var val = Spring.Value(startValue, endValue, initialVelocity, sampleTime, mass, stiffness, damping);
			keys[i] = new Keyframe(sampleTime, val);
		}
		float minVal = keys.Min(x => x.value);
		float maxVal = keys.Max(x => x.value);
		float minGraphHeight = minVal - (maxVal - minVal) * 0.35f;
		float maxGraphHeight = maxVal + (maxVal - minVal) * 0.35f;
		
		AnimationCurve curve = new AnimationCurve(keys);
		EditorGUI.CurveField(rect, GUIContent.none, curve, Color.white, new Rect(0f, minGraphHeight, graphMaxTime, maxGraphHeight - minGraphHeight));
				
		rect = EditorGUI.IndentedRect(rect);
		
		// Draw current time marker
		if (time != null) {
			var valueAtTime = Spring.Value(startValue, endValue, initialVelocity, time.Value, mass, stiffness, damping);
			var normalizedRectCoordinates = new Vector2(time.Value / graphMaxTime, 1-Mathf.InverseLerp(minGraphHeight, maxGraphHeight, valueAtTime));
			var pos = Rect.NormalizedToPoint(rect, normalizedRectCoordinates);
			GUI.DrawTexture(CreateFromCenter(pos.x, pos.y+1, 16, 16), currentTimeMarkerIcon);
		}

		var minMaxGraphHeightStrings = RoundToSignificantDigits(2, minGraphHeight, maxGraphHeight);
		// Draw axis labels
		var labelStyle = EditorStyles.centeredGreyMiniLabel;
		{
			var maxHeightLabel = new GUIContent(minMaxGraphHeightStrings[1].ToString(CultureInfo.InvariantCulture));
			var maxHeightLabelSize = labelStyle.CalcSize(maxHeightLabel);
			GUI.Label(CreateFromCenter(rect.xMin - maxHeightLabelSize.x * 0.5f, rect.yMin + maxHeightLabelSize.y * 0.5f, maxHeightLabelSize.x, maxHeightLabelSize.y), maxHeightLabel, labelStyle);
		}
		{
			var minHeightLabel = new GUIContent(minMaxGraphHeightStrings[0].ToString(CultureInfo.InvariantCulture));
			var minHeightLabelSize = labelStyle.CalcSize(minHeightLabel);
			GUI.Label(CreateFromCenter(rect.xMin-minHeightLabelSize.x*0.5f, rect.yMax-minHeightLabelSize.y*0.5f, minHeightLabelSize.x, minHeightLabelSize.y), minHeightLabel, labelStyle);
		}
		{
			var minTimeLabel = new GUIContent(0.ToString());
			var minTimeLabelSize = labelStyle.CalcSize(minTimeLabel);
			GUI.Label(CreateFromCenter(rect.xMin + minTimeLabelSize.x * 0.5f, rect.yMax + minTimeLabelSize.y * 0.5f, minTimeLabelSize.x, minTimeLabelSize.y), minTimeLabel, labelStyle);
		}
		{
			var timeLabel = new GUIContent("Time");
			var maxTimeLabelSize = labelStyle.CalcSize(timeLabel);
			GUI.Label(CreateFromCenter(rect.center.x, rect.yMax + maxTimeLabelSize.y * 0.5f, maxTimeLabelSize.x, maxTimeLabelSize.y), timeLabel, labelStyle);
		}
		{
			var maxTimeLabel = new GUIContent(graphMaxTime.ToString("G2"));
			var maxTimeLabelSize = labelStyle.CalcSize(maxTimeLabel);
			GUI.Label(CreateFromCenter(rect.xMax - maxTimeLabelSize.x * 0.5f, rect.yMax + maxTimeLabelSize.y * 0.5f, maxTimeLabelSize.x, maxTimeLabelSize.y), maxTimeLabel, labelStyle);
		}
		EditorGUI.EndDisabledGroup();
		
	}
	static Rect CreateFromCenter (Vector2 centerPosition, Vector2 size) {
		return CreateFromCenter(centerPosition.x, centerPosition.y, size.x, size.y);
	}

	static Rect CreateFromCenter (float centerX, float centerY, float sizeX, float sizeY) {
		return new Rect(centerX - sizeX * 0.5f, centerY - sizeY * 0.5f, sizeX, sizeY);
	}
	
	// Rounds several numbers to the same factor, using the largest number and a fixed num sig.digits by absolute value to determine the scale
	static float[] RoundToSignificantDigits(int significantDigits, params float[] nums) {
		// Find the largest number by absolute value to determine the scale
		float maxNum = 0;
		foreach (float num in nums) if (Mathf.Abs(num) > Mathf.Abs(maxNum)) maxNum = num;
		// Calculate the scale factor based on the largest number
		float scale = Mathf.Pow(10, (int)Mathf.Floor(Mathf.Log10(Mathf.Abs(maxNum))) - (significantDigits - 1));
		// Round all numbers using the calculated scale
		for (int i = 0; i < nums.Length; i++) nums[i] = Mathf.Round(nums[i] / scale) * scale;

		return nums;
	}
}
