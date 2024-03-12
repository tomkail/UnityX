using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DisableIfAttribute))]
public class DisableIfDrawer : BaseIfAttributeDrawer<DisableIfAttribute> {
	public override void OnGUITrue (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginDisabledGroup(true);
		base.OnGUITrue(position, property, label);
		EditorGUI.EndDisabledGroup();
	}   
}