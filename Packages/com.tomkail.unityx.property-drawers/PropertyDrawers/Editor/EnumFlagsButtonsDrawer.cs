using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Draws a [Flags] enum as a strip of independent toggle buttons, one per single-bit flag.
// Zero and composite members (e.g. Everything = A | B) are not given buttons — they read and
// write implicitly through the bits they're made of. Values are masked to the enum's defined
// bits on write, so undefined bits and ~0-style "Everything" values can't linger.
[CustomPropertyDrawer(typeof(EnumFlagsButtonsAttribute))]
class EnumFlagsButtonsDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // For a List<T>/T[] field, fieldInfo is the collection; unwrap to the element type.
        var type = fieldInfo.FieldType;
        if (type.IsArray) type = type.GetElementType();
        else if (type.IsGenericType) type = type.GetGenericArguments()[0];

        if (property.propertyType != SerializedPropertyType.Enum || !type.IsEnum) {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var displayNames = property.enumDisplayNames; // same order as Enum.GetValues
        var values = Enum.GetValues(type);
        var labels = new List<string>(values.Length);
        var masks = new List<int>(values.Length);
        int definedMask = 0;
        for (int i = 0; i < values.Length; i++) {
            int v = unchecked((int)Convert.ToInt64(values.GetValue(i)));
            if (v == 0 || (v & (v - 1)) != 0) continue; // zero or composite
            labels.Add(displayNames[i]);
            masks.Add(v);
            definedMask |= v;
        }
        if (masks.Count == 0) {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        label = EditorGUI.BeginProperty(position, label, property);
        var buttonsRect = EditorGUI.PrefixLabel(position, label);

        var prevContentColor = GUI.contentColor;
        if (property.hasMultipleDifferentValues) GUI.contentColor *= new Color(1f, 1f, 1f, 0.5f);

        int value = property.intValue & definedMask;
        float width = buttonsRect.width / masks.Count;
        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < masks.Count; i++) {
            var rect = new Rect(buttonsRect.x + width * i, buttonsRect.y, width, buttonsRect.height);
            bool on = GUI.Toggle(rect, (value & masks[i]) != 0, labels[i], StyleFor(i, masks.Count));
            value = on ? value | masks[i] : value & ~masks[i];
        }
        if (EditorGUI.EndChangeCheck()) property.intValue = value;

        GUI.contentColor = prevContentColor;
        EditorGUI.EndProperty();
    }

    static GUIStyle StyleFor(int index, int count) {
        if (count == 1) return EditorStyles.miniButton;
        if (index == 0) return EditorStyles.miniButtonLeft;
        if (index == count - 1) return EditorStyles.miniButtonRight;
        return EditorStyles.miniButtonMid;
    }
}
