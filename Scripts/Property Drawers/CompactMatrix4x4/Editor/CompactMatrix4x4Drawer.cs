using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(CompactMatrix4x4Attribute))]
public class CompactMatrix4x4Drawer : PropertyDrawer {
	bool showing = false;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		
        // When the property is not Matrix4x4
        if (property.type != "Matrix4x4f")
        {
//            position.height -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
//            EditorGUI.PropertyField(position, property, label, true);
			EditorGUI.LabelField(position, label);
            position.x += 80;
//			position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.HelpBox(position, "Attribute must be of type Matrix4x4.", MessageType.Warning);
            return;
        }

        // Label
        
        //EditorGUI.LabelField(position, label);
	
		showing = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), showing, label);
		if(showing) {
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
        if (property.type == "Matrix4x4f")
            if(showing)return EditorGUIUtility.singleLineHeight * 5;
			else return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
        else
            return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
    }
}