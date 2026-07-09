using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CompactMatrix4x4Attribute))]
public class CompactMatrix4x4Drawer : PropertyDrawer {
	// Matrix4x4 serializes as a Generic property, so the type string is the only cheap identity check.
	const string matrixTypeName = "Matrix4x4f";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (property.type != matrixTypeName) {
			EditorGUI.LabelField(position, label);
            position.x += 80;
            EditorGUI.HelpBox(position, "Attribute must be of type Matrix4x4.", MessageType.Warning);
            return;
        }

		// Foldout state lives on the property, not the drawer — drawer instances are reused across list elements.
		property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);
		if(property.isExpanded) {
			position.height /= 5;
			var attr = attribute as CompactMatrix4x4Attribute;
			position.y += position.height;
			position.xMin += EditorGUIUtility.singleLineHeight;
			position.width /= 4;
			var empty = new GUIContent("");
			for (int i = 0; i < 4; i++)
			{
				bool enabledBackup = GUI.enabled;
				if (attr.IsAffine && i == 3) {
					GUI.enabled = false;
				}
				for (int j = 0; j < 4; j++) {
					var elem = property.FindPropertyRelative(("e" + i) + j);
					EditorGUI.PropertyField(position, elem, empty, false);
					if (attr.IsAffine && i == 3)
					{
						var ideal = i == j ? 1f : 0f;
						if (elem.floatValue != ideal)
						{
							elem.floatValue = ideal;
						}
					}
					position.x += position.width;
				}
				GUI.enabled = enabledBackup;
				position.x -= position.width * 4;
				position.y += position.height;
			}
		}
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (property.type == matrixTypeName && property.isExpanded)
            return EditorGUIUtility.singleLineHeight * 5;
        return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
    }
}
