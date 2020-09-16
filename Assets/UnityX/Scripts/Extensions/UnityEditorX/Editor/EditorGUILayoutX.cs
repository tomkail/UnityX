using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class EditorGUILayoutX {

	public static T ObjectField<T>(T val, bool allowSceneObjects = true) where T : Object {
		return EditorGUILayout.ObjectField(val, typeof(T), allowSceneObjects) as T;
	}

	public static T ObjectField<T>(string label, T val, bool allowSceneObjects = true) where T : Object {
		return EditorGUILayout.ObjectField(label, val, typeof(T), allowSceneObjects) as T;
	}

	public static T ObjectField<T>(GUIContent guiContent, T val, bool allowSceneObjects = true) where T : Object {
		return EditorGUILayout.ObjectField(guiContent, val, typeof(T), allowSceneObjects) as T;
	}

	/// <summary>
	/// Draws a serialized property (including children) fully, even if it's an instance of a custom serializable class.
	/// Supersedes EditorGUILayout.PropertyField(serializedProperty, true);
	/// </summary>
	/// <param name="_serializedProperty">Serialized property.</param>
	public static void DrawSerializedProperty (SerializedProperty _serializedProperty) {
		if(_serializedProperty == null) {
			EditorGUILayout.HelpBox("SerializedProperty was null!", MessageType.Error);
			return;
		}
		var serializedProperty = _serializedProperty.Copy();
		int startingDepth = serializedProperty.depth;
		EditorGUI.indentLevel = startingDepth;
		DrawPropertyField(serializedProperty);
		while (serializedProperty.NextVisible(serializedProperty.isExpanded && !EditorGUIX.PropertyTypeHasDefaultCustomDrawer(serializedProperty.propertyType)) && serializedProperty.depth > startingDepth) {
			EditorGUI.indentLevel = serializedProperty.depth;
			DrawPropertyField(serializedProperty);
		}
		EditorGUI.indentLevel = startingDepth;
	}

	public static void DrawPropertyField (SerializedProperty serializedProperty) {
		if(serializedProperty.propertyType == SerializedPropertyType.Generic) {
			serializedProperty.isExpanded = EditorGUILayout.Foldout(serializedProperty.isExpanded, serializedProperty.displayName, true);
		} else {
			EditorGUILayout.PropertyField(serializedProperty);
		}
	}

	/// <summary>
	/// A text field that allows a placeholder. Unlike EditorGUIX's version, this placeholder is used as default text when the box is selected.
	/// </summary>
	/// <returns>The field.</returns>
	/// <param name="label">Label.</param>
	/// <param name="text">Text.</param>
	/// <param name="placeholderText">Placeholder text.</param>
	public static string TextField (GUIContent label, string text, string placeholderText) {
		string uniqueControlName = "TextFieldControlName_"+label+"_"+placeholderText;
		GUI.SetNextControlName(uniqueControlName);

		if(GUI.GetNameOfFocusedControl() != uniqueControlName && text == string.Empty) {
			GUIStyle style = new GUIStyle(GUI.skin.textField);
			style.fontStyle = FontStyle.Italic;
			style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
			// Have to add a space to make this work, for some reason
			EditorGUILayout.TextField(label.text+" ", placeholderText, style);
		} else {
			text = EditorGUILayout.TextField(label, text);
		}
		return text;
    }

	public static void ProgressBar (string label, float value) {
		Rect r = EditorGUILayout.BeginVertical();
		EditorGUI.ProgressBar(r, value, label);
		GUILayout.Space(EditorGUIUtility.singleLineHeight);
		EditorGUILayout.EndVertical();
		GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
	}

	
	public static T Popup<T> (string label, T current) where T : struct {
		return Popup(new GUIContent(label), current, EnumX.ToArray<T>(), EnumX.ToStringArray<T>().Select(x => new GUIContent(x)).ToArray(), false);
	}
	public static T Popup<T> (GUIContent label, T current) where T : struct {
		return Popup(label, current, EnumX.ToArray<T>(), EnumX.ToStringArray<T>().Select(x => new GUIContent(x)).ToArray(), false);
	}

	public static T Popup<T> (GUIContent label, T current, Dictionary<T, GUIContent> valuesAndLabels, params GUILayoutOption[] options) {
		return Popup(label, current, valuesAndLabels.Keys.ToArray(), valuesAndLabels.Values.ToArray(), true, "CUSTOM", options);
	}

	public static T Popup<T> (GUIContent label, T current, Dictionary<T, GUIContent> valuesAndLabels, bool allowCustom = true, string customLabel = "CUSTOM", params GUILayoutOption[] options) {
		return Popup(label, current, valuesAndLabels.Keys.ToArray(), valuesAndLabels.Values.ToArray(), allowCustom, customLabel, options);
	}

	public static T Popup<T> (GUIContent label, T current, T[] values, GUIContent[] labels, bool allowCustom = true, string customLabel = "CUSTOM", params GUILayoutOption[] options) {
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
		index = EditorGUILayout.Popup(label, index, labels, options);
		return values[index];
    }

	

	public static float VariableSlider (GUIContent label, float val, ref float min, ref float max) {
		EditorGUILayout.BeginHorizontal();
		val = EditorGUILayout.Slider(label, val, min, max);
		min = EditorGUILayout.FloatField(min, GUILayout.Width(40));
		max = EditorGUILayout.FloatField(max, GUILayout.Width(40));
		EditorGUILayout.EndHorizontal();
		return val;
	}
}
