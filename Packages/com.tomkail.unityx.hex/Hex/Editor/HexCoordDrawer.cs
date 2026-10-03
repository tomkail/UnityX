#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

[CustomPropertyDrawer(typeof (HexCoord))]
public class HexCoordDrawer : PropertyDrawer {
	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty (position, label, property);
		SerializedProperty serializedProperty = property.Copy();
		serializedProperty.NextVisible(true);
		EditorGUI.MultiPropertyField(position, new GUIContent[] {
			new GUIContent("Q"),
			new GUIContent("R")
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

	public static HexCoord Draw (Rect position, HexCoord coord) {
		return Draw(position, GUIContent.none, coord);
	}
	public static HexCoord Draw (Rect position, string label, HexCoord coord) {
		return Draw(position, new GUIContent(label), coord);
	}
	public static HexCoord Draw (Rect position, GUIContent label, HexCoord coord) {
		EditorGUI.BeginChangeCheck();
		
		position = EditorGUI.PrefixLabel(position, label);
		var values = new int[] {coord.q,coord.r};
		EditorGUI.MultiIntField(position, new GUIContent[] {
			new GUIContent("Q"),
			new GUIContent("R")
		}, values);
		
		if(EditorGUI.EndChangeCheck()) coord = new HexCoord(values[0], values[1]);
		return coord;
	}

	public static HexCoord DrawLayout (string label, HexCoord coord) {
		return DrawLayout(new GUIContent(label), coord);
	}
	public static HexCoord DrawLayout (GUIContent label, HexCoord coord) {
		Rect r = EditorGUILayout.BeginVertical();
		coord = Draw(r, label, coord);
		GUILayout.Space(EditorGUIUtility.singleLineHeight);
		EditorGUILayout.EndVertical();
		GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
		return coord;
	}

	public static HexCoord DrawLayout (HexCoord coord) {
		Rect r = EditorGUILayout.BeginVertical();
		coord = Draw(r, coord);
		GUILayout.Space(EditorGUIUtility.singleLineHeight);
		EditorGUILayout.EndVertical();
		GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
		return coord;
	}
}

#endif