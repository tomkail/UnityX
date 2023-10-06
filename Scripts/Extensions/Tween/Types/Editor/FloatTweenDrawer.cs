using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer (typeof (FloatTween))]
public class FloatTweenDrawer : PropertyDrawer {

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		if(property.isExpanded) {
			return EditorGUIUtility.standardVerticalSpacing * 3 + EditorGUIUtility.singleLineHeight * 4;
		} else {
			return base.GetPropertyHeight (property, label);
		}
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty (position, label, property);

		Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
		property.isExpanded = EditorGUI.Foldout (foldoutRect, property.isExpanded, label, true);

		var progressBarRect = new Rect (position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
		string text = "";
		float progress = 0;

		if (property.FindPropertyRelative ("tweening").boolValue) {
			text = "Playing";
		} else {
			text = "Stopped";
		}
		if(property.FindPropertyRelative("tweenTimer").FindPropertyRelative("_targetTime").floatValue > 0) {
			progress = Mathf.Clamp01(property.FindPropertyRelative("tweenTimer").FindPropertyRelative("currentTime").floatValue/property.FindPropertyRelative("tweenTimer").FindPropertyRelative("_targetTime").floatValue);
		}
		EditorGUI.ProgressBar(progressBarRect, progress, text);

		if(property.isExpanded) {
			EditorGUI.indentLevel++;
			Rect currentValueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
			Rect targetValueRect = new Rect(position.x, position.y + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight * 2, position.width, EditorGUIUtility.singleLineHeight);
			Rect easingCurveRect = new Rect(position.x, position.y + EditorGUIUtility.standardVerticalSpacing * 2 + EditorGUIUtility.singleLineHeight * 3, position.width, EditorGUIUtility.singleLineHeight);
			
			EditorGUI.PropertyField(currentValueRect, property.FindPropertyRelative ("_currentValue"));
			EditorGUI.PropertyField(targetValueRect, property.FindPropertyRelative ("targetValue"));
			EditorGUI.PropertyField(easingCurveRect, property.FindPropertyRelative ("_easingCurve"));
			EditorGUI.indentLevel--;
		}
		/*
		FloatTween data = SerializedPropertyX.GetBaseProperty<FloatTween>(property);

		string text = "";
		float progress = 0;

		Debug.Log(data);
		if(data != null && data.lerpFunction != null) {
//			progress = property.FindPropertyRelative ("_currentValue").floatValue/property.FindPropertyRelative("targetValue").floatValue;
			progress = Mathf.InverseLerp(data.GetValueAtNormalizedTime(0), data.GetValueAtNormalizedTime(1), data.currentValue);
		}

		if (property.FindPropertyRelative ("tweening").boolValue) {
			text = "Playing " + property.FindPropertyRelative ("_currentValue").floatValue.ToString()+"/"+property.FindPropertyRelative ("targetValue").floatValue.ToString();
		} else {
			text = "Stopped " + property.FindPropertyRelative ("_currentValue").floatValue.ToString();
		}


		var progressBarRect = new Rect (indentedPosition.x, indentedPosition.y, indentedPosition.width - 75, EditorGUIUtility.singleLineHeight);
		var easingRect = new Rect (indentedPosition.x + indentedPosition.width - 70, indentedPosition.y, 70, EditorGUIUtility.singleLineHeight);
		EditorGUI.ProgressBar(progressBarRect, progress, text);
		Color c = GUI.backgroundColor;
		GUI.backgroundColor = c.WithAlpha(0.5f);
		EditorGUI.PropertyField (progressBarRect, property.FindPropertyRelative ("_easingCurve"), GUIContent.none);
		GUI.backgroundColor = c;
		EditorGUI.EndProperty ();
		*/
	}
}
