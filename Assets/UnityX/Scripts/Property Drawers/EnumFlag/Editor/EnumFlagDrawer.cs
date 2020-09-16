using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagAttribute))]
public class EnumFlagDrawer : BaseAttributePropertyDrawer<EnumFlagAttribute> {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		Enum targetEnum = property.GetBaseProperty<Enum>();

		EditorGUI.BeginProperty(position, label, property);
        Enum enumNew = EditorGUI.EnumFlagsField(position, ObjectNames.NicifyVariableName(property.name), targetEnum);
		property.intValue = (int) Convert.ChangeType(enumNew, targetEnum.GetType());
		EditorGUI.EndProperty();
	}
	
	protected override bool IsSupported(SerializedProperty property) {
		Enum targetEnum = property.GetBaseProperty<Enum>();
		return targetEnum != null;
	}
}