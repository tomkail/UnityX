using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AudioAssetSaverAttribute))]
public class AudioAssetSaverDrawer : BaseAttributePropertyDrawer<AudioAssetSaverAttribute> {

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.ObjectReference;
    }

	const int buttonWidth = 80;
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		position.height = base.GetPropertyHeight(property, label);
		
		var propertyRect = position;
		propertyRect.width -= buttonWidth;
		EditorGUI.PropertyField(propertyRect, property, label);

		var buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);
		if(GUI.Button(buttonRect, "Save WAV")) {
			var clip = property.objectReferenceValue as AudioClip;
			var selectedAssetPath = GetPath(property);
			SavWav.SaveAssetWithPrompt(clip, selectedAssetPath);
		}
		

		EditorGUI.EndProperty();
	}

	public string GetPath (SerializedProperty property) {
		string selectedAssetPath = "Assets";
		if(property.serializedObject.targetObject is MonoBehaviour) {
			MonoScript ms = MonoScript.FromMonoBehaviour((MonoBehaviour)property.serializedObject.targetObject);
			selectedAssetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath( ms ));
		}
		return selectedAssetPath;
	}
}