#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace UnityX.Geometry {

	[CustomPropertyDrawer(typeof (PointRect))]
	public class PointRectPropertyDrawer : PropertyDrawer {
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty (position, label, property);
			SerializedProperty serializedProperty = property.Copy();
			serializedProperty.NextVisible(true);
			EditorGUI.MultiPropertyField(position, new GUIContent[] {
				new GUIContent("X"),
				new GUIContent("Y")
			}, serializedProperty, label);
			position.y += EditorGUIUtility.singleLineHeight;
			EditorGUI.MultiPropertyField(position, new GUIContent[] {
				new GUIContent("W"),
				new GUIContent("H")
			}, serializedProperty, new GUIContent(" "));
			EditorGUI.EndProperty ();
		}

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			if(EditorGUIUtility.wideMode) {
				return base.GetPropertyHeight (property, label) + EditorGUIUtility.singleLineHeight;
			} else {
				return base.GetPropertyHeight (property, label) + EditorGUIUtility.singleLineHeight * 2;
			}
		}
	}
}

#endif