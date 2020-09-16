using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DontAllowSceneObjectsAttribute))]
public class DontAllowSceneObjectsDrawer : BaseAttributePropertyDrawer<DontAllowSceneObjectsAttribute> {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty (position, label, property);

		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, property.GetActualType(), false);

		EditorGUI.EndProperty ();
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label);
	}

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.ObjectReference;
	}
}