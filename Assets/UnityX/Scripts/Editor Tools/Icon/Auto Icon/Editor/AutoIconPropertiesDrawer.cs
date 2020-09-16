using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using UnityX.Editor.Icon;

[CustomPropertyDrawer(typeof(AutoIcon.IconProperties))]
public class AutoIconPropertiesDrawer : PropertyDrawer {
	
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (property.isExpanded ? 3 : 1);
	}
	
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		Rect r = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, label, true);
		if(property.isExpanded) {
			EditorGUI.indentLevel++;
			
			r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(r, property.FindPropertyRelative("iconType"));
			
			r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			AutoIcon.IconType type = (AutoIcon.IconType)property.FindPropertyRelative("iconType").enumValueIndex;
			if(type == AutoIcon.IconType.Dot) {
				EditorGUI.PropertyField(r, property.FindPropertyRelative("icon"));
			} else if(type == AutoIcon.IconType.Label) {
				EditorGUI.PropertyField(r, property.FindPropertyRelative("labelIcon"));
			} else if(type == AutoIcon.IconType.Custom) {
				EditorGUI.PropertyField(r, property.FindPropertyRelative("texture"));
			}
			EditorGUI.indentLevel--;
		}
		EditorGUI.EndProperty();
    }
}
