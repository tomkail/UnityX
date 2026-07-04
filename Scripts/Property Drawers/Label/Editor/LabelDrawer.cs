using UnityEngine;
using UnityEditor;

// Kept intentionally: [Label] relabels the inspector label of ANY serialized field. Unity's built-in [InspectorName] only relabels ENUM VALUES, so it is NOT a substitute — don't remove this in favour of InspectorName.
[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelAttributeDrawer : BaseAttributePropertyDrawer<LabelAttribute> {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if(attribute.label == null) {
            EditorGUI.PropertyField(position, property, GUIContent.none, true);
        } else {
            label = new GUIContent(attribute.label);
            EditorGUI.PropertyField(position, property, label, true);
        }
	}

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}
}