using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// Cribbed a lot from https://gamedev.stackexchange.com/questions/154696/picking-multiple-choices-from-an-enum/154699
[CustomPropertyDrawer(typeof (EnumFlagsButtonGroupAttribute))]
class EnumFlagsButtonGroupDrawer : PropertyDrawer {
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		if (_properties == null)
            Initialize(property);
		
		EditorGUI.BeginProperty (position, label, property);
		var containerRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label);
		
		var numButtons = _entries.Count;
		var width = containerRect.width/numButtons;

		foreach (var prop in _properties)
            prop.serializedObject.Update();

		for(int i = 0; i < _entries.Count; i++) {
            var entry = _entries[i];
			var rect = new Rect(containerRect.x + width * i, containerRect.y, width, containerRect.height);
            EditorGUI.BeginChangeCheck();
            bool pressed = entry.currentValue == TriBool.True;
			pressed = GUI.Toggle(rect, pressed, entry.label, GetGUIStyle(i, numButtons, entry.currentValue == TriBool.Both));
            if (EditorGUI.EndChangeCheck()) {
                if (pressed) {
                    foreach (var prop in _properties)
                        prop.intValue |= entry.mask;
                    entry.currentValue = TriBool.True;
                } else {
                    foreach (var prop in _properties)
                        prop.intValue &= ~entry.mask;
                    entry.currentValue = TriBool.False;
                }
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

        var typedValues = GetTypedValues(enumType);
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
                    property.intValue |= value;
                } else {
                    property.intValue &= ~value;
                }
                names[i] = entry;
            }
        }
		// EditorGUI.EndProperty ();
	}



    public static T DrawLayout<T> (T enumm, GUIContent label) where T : Enum {
        var position = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        return Draw(position, enumm, label);
    }
    public static T Draw<T> (Rect position, T enumm, GUIContent label) where T : Enum {
		var containerRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label);
		
        var enumType = typeof(T);
        var trueNames = System.Enum.GetNames(enumType);

        var typedValues = GetTypedValues(enumType);

		var numButtons = trueNames.Length;
		var width = containerRect.width/numButtons;

        var enummAsInt = (int)(object)enumm;
		for(int i = 0; i < trueNames.Length; i++) {
            int value = typedValues[i];

			var rect = new Rect(containerRect.x + width * i, containerRect.y, width, containerRect.height);
            EditorGUI.BeginChangeCheck();
			bool pressed = (enummAsInt & value) != 0;
			pressed = GUI.Toggle(rect, pressed, trueNames[i], GetGUIStyle(i, numButtons, false));
            if (EditorGUI.EndChangeCheck()) {
                if (pressed) {
                    enummAsInt |= value;
                } else {
                    enummAsInt &= ~value;
                }
            }
        }
        return (T)(object)enummAsInt;
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
        public int mask;
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

        var enumType = GetTypeFromObject(property.serializedObject.targetObject, property.propertyPath);
        var trueNames = System.Enum.GetNames(enumType);

        var typedValues = GetTypedValues(enumType);
        var display = property.enumDisplayNames;
        var names = property.enumNames;

        _entries = new List<Entry>();

        for (int i = 0; i < names.Length; i++) {
            int sortedIndex = System.Array.IndexOf(trueNames, names[i]);
            int value = typedValues[sortedIndex];
            int bitCount = 0;  

            for (int temp = value; (temp != 0 && bitCount <= 1); temp >>= 1)
                bitCount += temp & 1;

            //Debug.Log(names[i] + ": " + value + " ~ " + bitCount);

            if (bitCount != 1)
                continue;

            TriBool consensus = TriBool.Unset;
            foreach (var prop in _properties) {
                if ((prop.intValue & value) == 0)
                    consensus |= TriBool.False;
                else
                    consensus |= TriBool.True;
            }

            _entries.Add(new Entry { label = display[i], mask = value, currentValue = consensus });
        }
    }

    static int[] GetTypedValues(System.Type enumType) {
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

    public static Type GetTypeFromObject(object obj, string propertyPath) {
		Debug.Assert(obj != null);
		string[] parts = propertyPath.Split('.');
        FieldInfo fieldInfo = null;
		PropertyInfo propertyInfo = null;
		MemberInfo memberInfo = null;
		Type type = null;
		for (int i = 0; i < parts.Length; i++) {
			fieldInfo = null;
			propertyInfo = null;
			memberInfo = obj.GetType().GetMember(parts[i], BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy|BindingFlags.Instance).FirstOrDefault();
			if(memberInfo is FieldInfo) {
				fieldInfo = (FieldInfo)memberInfo;
				obj = fieldInfo.GetValue(obj);
				type = fieldInfo.FieldType;
			} else if(memberInfo is PropertyInfo) {
				propertyInfo = (PropertyInfo)memberInfo;
				obj = propertyInfo.GetValue(obj, null);
				type = propertyInfo.PropertyType;
			}
			bool isArray = type != null && (type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>));
			if (i != parts.Length-1 && isArray) {
				i+=2;
				int indexStart = parts[i].IndexOf("[")+1;
				int collectionElementIndex = Int32.Parse(parts[i].Substring(indexStart, parts[i].Length-indexStart-1));
				if(obj != null) {
					IList list = obj as IList;
					if(collectionElementIndex >= 0 && collectionElementIndex < list.Count) {
						obj = list[collectionElementIndex];
						if(obj == null) {
							if(i == parts.Length-1) {
								break;
							} else {
								return null;
							}
						}
					}
				}
			}
		}
		
		if(type == null) return null;
		else if(type.IsArray) return type.GetElementType();
		else if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return type.GetGenericArguments()[0];
		else return type;
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