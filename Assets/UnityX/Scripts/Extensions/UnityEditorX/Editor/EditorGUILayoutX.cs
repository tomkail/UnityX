using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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



	public static void FakeEnumButtonGroup (SerializedProperty serializedProperty, Dictionary<int, string> fakeEnum) {
        EditorGUI.showMixedValue = serializedProperty.hasMultipleDifferentValues;
        var currentValue = serializedProperty.intValue;
        if(EditorGUI.showMixedValue) currentValue = -1;
        var newValue = EditorGUILayoutX.FakeEnumButtonGroup(new GUIContent(serializedProperty.displayName), currentValue, fakeEnum);
        if(currentValue != newValue) serializedProperty.intValue = newValue;
        EditorGUI.showMixedValue = false;
    }
    public static int FakeEnumButtonGroup (GUIContent label, int currentInt, Dictionary<int, string> fakeEnum) {
        var newInt = currentInt;
        var names = fakeEnum.Values;
        var values = fakeEnum.Keys;

        if(EditorGUI.showMixedValue) {
            currentInt = int.MinValue;
            label.text += " (Mixed)";
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        var length = fakeEnum.Count;
        int i = 0;
        foreach(var val in fakeEnum) {
            var style = EditorStyles.miniButton;
            if(length > 1) {
                if(i == 0 && length > 1) style = EditorStyles.miniButtonLeft;
                else if(i == length-1 && length > 1) style = EditorStyles.miniButtonRight;
                else style = EditorStyles.miniButtonMid;
            }
            
            var valInt = val.Key;
            var isCurrent = currentInt == valInt;
            var newIsCurrent = GUILayout.Toggle(isCurrent, val.Value, style);
            if(isCurrent != newIsCurrent) {
                newInt = valInt;
            }
            i++;
        }
		EditorGUILayout.EndHorizontal();
        return newInt;
    }

    // Draws all the values of an enum in a horizontal button group, highlighting the selected value
    public static void EnumButtonGroup<T> (SerializedProperty serializedProperty) where T : System.Enum {
        EditorGUI.showMixedValue = serializedProperty.hasMultipleDifferentValues;
        var currentValue = serializedProperty.intValue;
        if(EditorGUI.showMixedValue) currentValue = -1;
        var newValue = EditorGUILayoutX.EnumButtonGroup<T>(new GUIContent(serializedProperty.displayName), currentValue);
        if(currentValue != newValue) serializedProperty.intValue = newValue;
        EditorGUI.showMixedValue = false;
    }
    public static int EnumButtonGroup<T> (GUIContent label, int currentInt) where T : System.Enum {
        var newInt = currentInt;
        var type = typeof(T);
        var names = System.Enum.GetNames(type);
        var values = System.Enum.GetValues(type);

        if(EditorGUI.showMixedValue) {
            currentInt = -1;
            label.text += " (Mixed)";
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        var length = values.Length;
        for(int i = 0; i < length; i++) {
            var style = EditorStyles.miniButton;
            if(length > 1) {
                if(i == 0 && length > 1) style = EditorStyles.miniButtonLeft;
                else if(i == length-1 && length > 1) style = EditorStyles.miniButtonRight;
                else style = EditorStyles.miniButtonMid;
            }
            
            var valInt = (int)values.GetValue(i);
            var isCurrent = currentInt == valInt;
            var newIsCurrent = GUILayout.Toggle(isCurrent, names[i], style);
            if(isCurrent != newIsCurrent) {
                newInt = valInt;
            }
        }
		EditorGUILayout.EndHorizontal();
        return newInt;
    }
    public static T EnumButtonGroup<T> (GUIContent label, T current) where T : System.Enum {
        return (T)(object)EnumButtonGroup<T>(label, (int)(object)current);
    }


    
    // Draws all the values of an enum in a horizontal button group, highlighting the selected values
    public static int FlagToggleGroup<T> (GUIContent label, int currentInt) where T : System.Enum {
        return (int)(object)FlagToggleGroup<T>(label, (T)System.Enum.ToObject(typeof(T), currentInt));
    }
    public static T FlagToggleGroup<T> (GUIContent label, T current) where T : System.Enum {
		var currentInt = (int)(object)current;
        var newInt = currentInt;
        var type = typeof(T);
        var names = System.Enum.GetNames(type);
        var values = System.Enum.GetValues(type);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        var length = values.Length;
        for(int i = 0; i < length; i++) {
            var style = EditorStyles.miniButton;
            if(length > 1) {
                if(i == 0 && length > 1) style = EditorStyles.miniButtonLeft;
                else if(i == length-1 && length > 1) style = EditorStyles.miniButtonRight;
                else style = EditorStyles.miniButtonMid;
            }
            
            var val = (System.Enum)values.GetValue(i);
            var valInt = (int)(object)val;
            var included = current.HasFlag(val);
            var newIncluded = GUILayout.Toggle(included, names[i], style);
            if(included != newIncluded) {
                newInt = (newInt ^ valInt);
            }
        }
		EditorGUILayout.EndHorizontal();
        if(currentInt != newInt) {
            current = (T)System.Enum.ToObject(typeof(T), newInt);
        }
        return current;
    }
}
