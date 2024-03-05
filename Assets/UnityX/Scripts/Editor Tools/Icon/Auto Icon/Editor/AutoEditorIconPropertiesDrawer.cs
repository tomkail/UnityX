using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using UnityX.Editor.Icon;

[CustomPropertyDrawer(typeof(AutoEditorIcon.IconProperties))]
public class AutoEditorIconPropertiesDrawer : PropertyDrawer {
	
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
			AutoEditorIcon.IconType type = (AutoEditorIcon.IconType)property.FindPropertyRelative("iconType").enumValueIndex;
			if(type == AutoEditorIcon.IconType.Dot) {
				EditorGUI.PropertyField(r, property.FindPropertyRelative("icon"));
			} else if(type == AutoEditorIcon.IconType.Label) {
				EditorGUI.PropertyField(r, property.FindPropertyRelative("labelIcon"));
			} else if(type == AutoEditorIcon.IconType.Custom) {
				EditorGUI.PropertyField(r, property.FindPropertyRelative("texture"));
			}
			EditorGUI.indentLevel--;
		}
		EditorGUI.EndProperty();
    }
}
