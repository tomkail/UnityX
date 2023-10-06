using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class EditorGUIX {

	public const float windowHeaderHeight = 14;
	public const float scrollBarSize = 15;

	public static Stack<float> labelWidths = new Stack<float>();

	public static void BeginLabelWidth (float width) {
		labelWidths.Push(EditorGUIUtility.labelWidth);
		EditorGUIUtility.labelWidth = width;
	}

	public static void EndLabelWidth () {
		EditorGUIUtility.labelWidth = labelWidths.Pop();
	}

	public static bool PropertyTypeHasDefaultCustomDrawer(SerializedPropertyType type) {
		return 
		type == SerializedPropertyType.AnimationCurve ||
		type == SerializedPropertyType.Bounds || 
		type == SerializedPropertyType.Color || 
		type == SerializedPropertyType.Gradient ||
		type == SerializedPropertyType.LayerMask ||
		type == SerializedPropertyType.ObjectReference || 
		type == SerializedPropertyType.Rect || 
		type == SerializedPropertyType.Vector2 || 
		type == SerializedPropertyType.Vector3;
	}

	public static float GetPropertyHeight (SerializedProperty _serializedProperty) {
		if(_serializedProperty == null) {
			EditorGUILayout.HelpBox("SerializedProperty was null!", MessageType.Error);
			return 0;
		}
		float height = 0f;
		var serializedProperty = _serializedProperty.Copy();
		int startingDepth = serializedProperty.depth;
		EditorGUI.indentLevel = serializedProperty.depth;
		height += EditorGUIX.GetPropertyFieldHeight(serializedProperty);
		while (serializedProperty.NextVisible(serializedProperty.isExpanded && !EditorGUIX.PropertyTypeHasDefaultCustomDrawer(serializedProperty.propertyType)) && serializedProperty.depth > startingDepth) {
			EditorGUI.indentLevel = serializedProperty.depth;
			height += EditorGUIUtility.standardVerticalSpacing;
			height += EditorGUIX.GetPropertyFieldHeight(serializedProperty);
		}
		EditorGUI.indentLevel = startingDepth;
		return height;
	}
	public static float DrawSerializedProperty (Rect rect, SerializedProperty _serializedProperty) {
		if(_serializedProperty == null) {
			EditorGUILayout.HelpBox("SerializedProperty was null!", MessageType.Error);
			return rect.y;
		}
		var serializedProperty = _serializedProperty.Copy();
		int startingDepth = serializedProperty.depth;
		EditorGUI.indentLevel = serializedProperty.depth;
		rect.y = EditorGUIX.DrawPropertyField(rect, serializedProperty);
		while (serializedProperty.NextVisible(serializedProperty.isExpanded && !EditorGUIX.PropertyTypeHasDefaultCustomDrawer(serializedProperty.propertyType)) && serializedProperty.depth > startingDepth) {
			EditorGUI.indentLevel = serializedProperty.depth;
			rect.y += EditorGUIUtility.standardVerticalSpacing;
			rect.y = EditorGUIX.DrawPropertyField(rect, serializedProperty);
		}
		EditorGUI.indentLevel = startingDepth;
		return rect.y;
	}

	public static float DrawPropertyField (Rect rect, SerializedProperty serializedProperty) {
		if(serializedProperty.propertyType == SerializedPropertyType.Generic) {
			serializedProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), serializedProperty.isExpanded, serializedProperty.displayName, true);
			return rect.y + EditorGUIUtility.singleLineHeight;
		} else {
			EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(serializedProperty)), serializedProperty);
			return rect.y + EditorGUI.GetPropertyHeight(serializedProperty);
		}
	}

	public static float DrawPropertyField (Rect rect, SerializedProperty serializedProperty, GUIContent label) {
		if(serializedProperty.propertyType == SerializedPropertyType.Generic) {
			serializedProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), serializedProperty.isExpanded, serializedProperty.displayName, true);
			return rect.y + EditorGUIUtility.singleLineHeight;
		} else {
			EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(serializedProperty, label)), serializedProperty, label);
			return rect.y + EditorGUI.GetPropertyHeight(serializedProperty);
		}
	}

	public static float GetPropertyFieldHeight (SerializedProperty serializedProperty) {
		if(serializedProperty.propertyType == SerializedPropertyType.Generic) {
			return EditorGUIUtility.singleLineHeight;
		} else {
			return EditorGUI.GetPropertyHeight(serializedProperty);
		}
	}

	public static T ObjectField<T>(Rect rect, T val, bool allowSceneObjects = true) where T : Object {
		return EditorGUI.ObjectField(rect, val, typeof(T), allowSceneObjects) as T;
	}

	public static T ObjectField<T>(Rect rect, string label, T val, bool allowSceneObjects = true) where T : Object {
		return EditorGUI.ObjectField(rect, label, val, typeof(T), allowSceneObjects) as T;
	}

	public static T ObjectField<T>(Rect rect, GUIContent guiContent, T val, bool allowSceneObjects = true) where T : Object {
		return EditorGUI.ObjectField(rect, guiContent, val, typeof(T), allowSceneObjects) as T;
	}

	public static T Popup<T> (Rect rect, GUIContent label, T current, Dictionary<T, GUIContent> valuesAndLabels) {
		return Popup(rect, label, current, valuesAndLabels.Keys.ToArray(), valuesAndLabels.Values.ToArray(), true, "CUSTOM");
	}

	public static T Popup<T> (Rect rect, GUIContent label, T current, Dictionary<T, GUIContent> valuesAndLabels, bool allowCustom = true, string customLabel = "CUSTOM") {
		return Popup(rect, label, current, valuesAndLabels.Keys.ToArray(), valuesAndLabels.Values.ToArray(), allowCustom, customLabel);
	}

	public static T Popup<T> (Rect rect, GUIContent label, T current, T[] values, GUIContent[] labels, bool allowCustom = true, string customLabel = "CUSTOM") {
		if(values.Length != labels.Length) Debug.LogError("Not the same size.");
		int index = 0;
		if(allowCustom && !values.Contains(current)) {
			var valuesList = values.ToList();
			valuesList.Insert(0, current);
			values = valuesList.ToArray();

			var labelsList = labels.ToList();
			labelsList.Insert(0, new GUIContent(customLabel));
			labels = labelsList.ToArray();
		} else {
			index = values.IndexOf(current);
		}
		index = EditorGUI.Popup(rect, label, index, labels);
		return values[index];
    }

	/// <summary>
	/// Textfield with placeholder.
	/// </summary>
	/// <returns>The field.</returns>
	/// <param name="position">Position.</param>
	/// <param name="label">Label.</param>
	/// <param name="text">Text.</param>
	/// <param name="placeholderText">Placeholder text.</param>
	public static string TextField (Rect position, GUIContent label, string text, string placeholderText) {
		string uniqueControlName = "TextFieldControlName_"+label+"_"+placeholderText;
		GUI.SetNextControlName(uniqueControlName);
		text = EditorGUI.TextField(position, label, text);
		if(GUI.GetNameOfFocusedControl() != uniqueControlName && text == string.Empty) {
			GUIStyle style = new GUIStyle(GUI.skin.textField);
			style.fontStyle = FontStyle.Italic;
			style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
			// Have to add a space to make this work, for some reason
			EditorGUI.TextField(position, label.text+" ", placeholderText, style);
		}
		return text;
	}
}
