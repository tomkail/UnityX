using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

// Cribbed a lot from https://gamedev.stackexchange.com/questions/154696/picking-multiple-choices-from-an-enum/154699
[CustomPropertyDrawer(typeof (EnumButtonGroupAttribute))]
class EnumButtonGroupDrawer : PropertyDrawer {
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		if (_properties == null)
            Initialize(property);
		
		EditorGUI.BeginProperty (position, label, property);
		var containerRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label);
		
		var numButtons = _entries.Count;
		var width = containerRect.width/numButtons;

		foreach (var prop in _properties)
            prop.serializedObject.Update();

        int newIndex = -1;
		for(int i = 0; i < _entries.Count; i++) {
            var entry = _entries[i];
			var rect = new Rect(containerRect.x + width * i, containerRect.y, width, containerRect.height);
            EditorGUI.BeginChangeCheck();
            bool pressed = entry.currentValue == TriBool.True;
			GUI.Toggle(rect, pressed, entry.label, GetGUIStyle(i, numButtons, entry.currentValue == TriBool.Both));
            if (EditorGUI.EndChangeCheck()) {
                newIndex = i;
            }
        }
        if(newIndex != -1) {
            var entry = _entries[newIndex];
            foreach (var prop in _properties)
                prop.intValue = entry.enumValue;
            for(int i = 0; i < _entries.Count; i++) {
                entry = _entries[i];
                entry.currentValue = i == newIndex ? TriBool.True : TriBool.False;
                _entries[i] = entry;
            }
        }


		foreach (var prop in _properties)
            prop.serializedObject.ApplyModifiedProperties();
		
		EditorGUI.EndProperty ();
	}


    public static void DrawLayout (SerializedProperty property) {
        DrawLayout(property, new GUIContent(property.displayName));
    }
    // TODO - remove entries with a value of 0, or else have them set the entire thing. We might do the same with a -1 "everything" value.
    public static void DrawLayout (SerializedProperty property, GUIContent label) {
        var position = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        Draw(position, property, label);
    }
    public static void Draw (Rect position, SerializedProperty property, GUIContent label) {
		// EditorGUI.BeginProperty (position, label, property);
		var containerRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label);
		
        var parentType = property.serializedObject.targetObject.GetType();
        var fieldInfo = parentType.GetField(property.propertyPath);
        var enumType = fieldInfo.FieldType;
        var trueNames = System.Enum.GetNames(enumType);

        var typedValues = GetTypedValues(property, enumType);
        var display = property.enumDisplayNames;
        var names = property.enumNames;

        // for (int i = 0; i < names.Length; i++) {
        //     int sortedIndex = System.Array.IndexOf(trueNames, names[i]);
        //     int value = typedValues[sortedIndex];
        //     int bitCount = 0;  

        //     for (int temp = value; (temp != 0 && bitCount <= 1); temp >>= 1)
        //         bitCount += temp & 1;
                
        //     if (bitCount != 1)
        //         continue;
        // }

		var numButtons = names.Length;
		var width = containerRect.width/numButtons;

		for(int i = 0; i < names.Length; i++) {
            int sortedIndex = System.Array.IndexOf(trueNames, names[i]);
            int value = typedValues[sortedIndex];

            var entry = names[i];
			var rect = new Rect(containerRect.x + width * i, containerRect.y, width, containerRect.height);
            EditorGUI.BeginChangeCheck();
			bool pressed = (property.intValue & value) != 0;
			pressed = GUI.Toggle(rect, pressed, display[i], GetGUIStyle(i, numButtons, false));
            if (EditorGUI.EndChangeCheck()) {
                if (pressed) {
                    property.intValue = value;
                } else {
                    property.intValue = ~value;
                }
                names[i] = entry;
            }
        }
		// EditorGUI.EndProperty ();
	}

	
	[System.Flags]
    enum TriBool {
        Unset = 0,
        False =  1,
        True = 2,
        Both = 3
    }
    struct Entry {
        public string label;
        public int enumValue;
        public TriBool currentValue;
    }

    List<SerializedProperty> _properties;
    List<Entry> _entries;
    int _rowCount;
    int _columnCount;
	void Initialize(SerializedProperty property) {
        var allTargetObjects = property.serializedObject.targetObjects;
        _properties = new List<SerializedProperty>(allTargetObjects.Length);
        foreach (var targetObject in allTargetObjects) {
            SerializedObject iteratedObject = new SerializedObject(targetObject);
            SerializedProperty iteratedProperty = iteratedObject.FindProperty(property.propertyPath);
            if (iteratedProperty != null) _properties.Add(iteratedProperty);
        }

        var parentType = property.serializedObject.targetObject.GetType();
        var fieldInfo = parentType.GetField(property.propertyPath);
        var enumType = fieldInfo.FieldType;
        var trueNames = System.Enum.GetNames(enumType);

        var typedValues = GetTypedValues(property, enumType);
        var display = property.enumDisplayNames;
        var names = property.enumNames;

        _entries = new List<Entry>();

        for (int i = 0; i < names.Length; i++) {
            int sortedIndex = System.Array.IndexOf(trueNames, names[i]);
            int value = typedValues[sortedIndex];

            TriBool consensus = TriBool.Unset;
            foreach (var prop in _properties) {
                if (prop.intValue == value) consensus |= TriBool.True;
                else consensus |= TriBool.False;
            }

            _entries.Add(new Entry { label = display[i], enumValue = value, currentValue = consensus });
        }
    }

    static int[] GetTypedValues(SerializedProperty property, System.Type enumType) {
        var values = System.Enum.GetValues(enumType);
        var underlying = System.Enum.GetUnderlyingType(enumType);

        if (underlying == typeof(int))
            return ConvertFrom<int>(values);
        else if (underlying == typeof(uint))
            return ConvertFrom<uint>(values);
        else if (underlying == typeof(short))
            return ConvertFrom<short>(values);
        else if (underlying == typeof(ushort))
            return ConvertFrom<ushort>(values);
        else if (underlying == typeof(sbyte))
            return ConvertFrom<sbyte>(values);
        else if (underlying == typeof(byte))
            return ConvertFrom<byte>(values);
        else
            throw new System.InvalidCastException("Cannot use enum backing types other than byte, sbyte, ushort, short, uint, or int.");
    }

    static int[] ConvertFrom<T>(System.Array untyped) where T : System.IConvertible {
        var typedValues = new int[untyped.Length];

        for (int i = 0; i < typedValues.Length; i++)
            typedValues[i] = System.Convert.ToInt32((T)untyped.GetValue(i));

        return typedValues;
    }



	static GUIStyle GetGUIStyle (int index, int numButtons, bool mixed) {
		var style = mixed ? miniButtonMixed : EditorStyles.miniButton;
		if(numButtons > 1) {
			if(index == 0 && numButtons > 1) style = mixed ? miniButtonLeftMixed : EditorStyles.miniButtonLeft;
			else if(index == numButtons-1 && numButtons > 1) style = mixed ? miniButtonRightMixed : EditorStyles.miniButtonRight;
			else style = mixed ? miniButtonMidMixed : EditorStyles.miniButtonMid;
		}
		return style;
	}

	static GUIStyle _miniButtonMixed;
	public static GUIStyle miniButtonMixed {
		get {
			if(_miniButtonMixed == null) {
				_miniButtonMixed = new GUIStyle(EditorStyles.miniButton);
				_miniButtonMixed.normal.textColor = Color.grey;
			}
			return _miniButtonMixed;
		}
	}
	static GUIStyle _miniButtonLeftMixed;
	public static GUIStyle miniButtonLeftMixed {
		get {
			if(_miniButtonLeftMixed == null) {
				_miniButtonLeftMixed = new GUIStyle(EditorStyles.miniButtonLeft);
				_miniButtonLeftMixed.normal.textColor = Color.grey;
			}
			return _miniButtonLeftMixed;
		}
	}
	static GUIStyle _miniButtonRightMixed;
	public static GUIStyle miniButtonRightMixed {
		get {
			if(_miniButtonRightMixed == null) {
				_miniButtonRightMixed = new GUIStyle(EditorStyles.miniButtonRight);
				_miniButtonRightMixed.normal.textColor = Color.grey;
			}
			return _miniButtonRightMixed;
		}
	}
	static GUIStyle _miniButtonMidMixed;
	public static GUIStyle miniButtonMidMixed {
		get {
			if(_miniButtonMidMixed == null) {
				_miniButtonMidMixed = new GUIStyle(EditorStyles.miniButtonMid);
				_miniButtonMidMixed.normal.textColor = Color.grey;
			}
			return _miniButtonMidMixed;
		}
	}
}