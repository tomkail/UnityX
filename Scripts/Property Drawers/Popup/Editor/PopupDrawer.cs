using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(PopupAttribute))]
public class PopupDrawer : BaseAttributePropertyDrawer<PopupAttribute> {

    private Action<int> setValue;
	private Func<string> getValue;
    private Func<int, int> validateValue;

    private string[] _list = null;
    private string[] list {
		get {
			if (_list == null) {
				_list = new string[attribute.list.Length];
				for (int i = 0; i < attribute.list.Length; i++) {
					_list[i] = attribute.list[i].ToString();
				}
			}
			return _list;
		}
	}

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String || property.propertyType == SerializedPropertyType.Float || property.propertyType == SerializedPropertyType.Integer;
	}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		//This line allows validate and setvalue functions to be cached, which is probably great for performance, but copies the attribute when used on the same object
		if (validateValue == null && setValue == null && getValue == null)
			SetUp(property);

		if (validateValue == null && setValue == null && getValue == null) {
			EditorGUI.HelpBox(position, "Popup drawer error.", MessageType.Error);
            return;
        }

        int selectedIndex = list.IndexOf(getValue());
        if(selectedIndex == -1) {
            selectedIndex = 0;
            setValue(selectedIndex);
        }

        for (int i = 0; i < list.Length; i++) {
            selectedIndex = validateValue(i);
            if (selectedIndex != 0)
                break;
        }

		EditorGUI.BeginChangeCheck();
		selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, list);
		if (EditorGUI.EndChangeCheck()) {
			setValue(selectedIndex);
		}
	}

	void SetUp(SerializedProperty property) {
		if (property.propertyType == SerializedPropertyType.String) {
			validateValue = (index) => {
				return property.stringValue == list[index] ? index : 0;
			};
			setValue = (index) => {
				property.stringValue = list[index];
			};
			getValue = () => {
				return property.stringValue;
			};
		} else if (property.propertyType == SerializedPropertyType.Integer) {
			validateValue = (index) => {
				return property.intValue == Convert.ToInt32(list[index]) ? index : 0;
			};
			setValue = (index) => {
				property.intValue = Convert.ToInt32(list[index]);
			};
			getValue = () => {
				return property.intValue.ToString();
			};
		} else if (property.propertyType == SerializedPropertyType.Float) {
			validateValue = (index) => {
				return property.floatValue == Convert.ToSingle(list[index]) ? index : 0;
			};
			setValue = (index) => {
				property.floatValue = Convert.ToSingle(list[index]);
			};
			getValue = () => {
				return property.floatValue.ToString();
			};
        }
    }

    private Type variableType {
        get {
            return attribute.list[0].GetType();
        }
    }
}