using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SetCaseAttribute))]
public class SetCaseDrawer : BaseAttributePropertyDrawer<SetCaseAttribute> {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		EditorGUI.BeginProperty(position, label, property);
		EditorGUI.BeginChangeCheck();
		string value = EditorGUI.TextField(position, label, property.stringValue);
		// Only write on edit — an unconditional write would dirty the object every repaint and clobber
		// multi-object selections. ToUpper/ToLower are the correct built-ins.
		if (EditorGUI.EndChangeCheck())
			property.stringValue = SetCase(value);
		EditorGUI.EndProperty();
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