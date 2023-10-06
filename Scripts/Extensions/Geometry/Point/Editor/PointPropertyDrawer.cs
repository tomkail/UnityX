#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (Point))]
public class PointPropertyDrawer : PropertyDrawer {
	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty (position, label, property);
		SerializedProperty serializedProperty = property.Copy();
		serializedProperty.NextVisible(true);
		EditorGUI.MultiPropertyField(position, new GUIContent[] {
			new GUIContent("X"),
			new GUIContent("Y")
		}, serializedProperty, label);
		EditorGUI.EndProperty ();
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		if(EditorGUIUtility.wideMode) {
			return base.GetPropertyHeight (property, label);
		} else {
			return base.GetPropertyHeight (property, label) + EditorGUIUtility.singleLineHeight;
		}
	}

	public static Point Draw (Rect position, Point coord) {
		return Draw(position, GUIContent.none, coord);
	}
	public static Point Draw (Rect position, string label, Point coord) {
		return Draw(position, new GUIContent(label), coord);
	}
	public static Point Draw (Rect position, GUIContent label, Point coord) {
		EditorGUI.BeginChangeCheck();
		
		position = EditorGUI.PrefixLabel(position, label);
		var values = new int[] {coord.x,coord.y};
		EditorGUI.MultiIntField(position, new GUIContent[] {
			new GUIContent("X"),
			new GUIContent("Y")
		}, values);
		
		if(EditorGUI.EndChangeCheck()) coord = new Point(values[0], values[1]);
		return coord;
	}

	public static Point DrawLayout (string label, Point coord) {
		return DrawLayout(new GUIContent(label), coord);
	}
	public static Point DrawLayout (GUIContent label, Point coord) {
		Rect r = EditorGUILayout.BeginVertical();
		coord = Draw(r, label, coord);
		GUILayout.Space(EditorGUIUtility.singleLineHeight);
		EditorGUILayout.EndVertical();
		GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
		return coord;
	}

	public static Point DrawLayout (Point coord) {
		Rect r = EditorGUILayout.BeginVertical();
		coord = Draw(r, coord);
		GUILayout.Space(EditorGUIUtility.singleLineHeight);
		EditorGUILayout.EndVertical();
		GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
		return coord;
	}
}

#endif