using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SetCaseAttribute))]
public class SetCaseDrawer : BaseAttributePropertyDrawer<SetCaseAttribute> {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		string value = property.stringValue;
		value = EditorGUI.TextField(position, label, value);
		property.stringValue = SetCase(value);
    }

	string SetCase(string myString) {
		if(attribute.caseType == SetCaseAttribute.CaseType.Upper) {
			return myString.ToUpper();
		} else if(attribute.caseType == SetCaseAttribute.CaseType.Lower) {
			return myString.ToLower();
		} else {
			return myString;
		}
	}
	
	protected override bool IsSupported(SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String;
	}
}