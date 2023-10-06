using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AssetSaverAttribute))]
public class AssetSaverDrawer : BaseAttributePropertyDrawer<AssetSaverAttribute> {

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.ObjectReference;
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return base.GetPropertyHeight(property, label);
    }

	const int buttonWidth = 66;
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		position.height = base.GetPropertyHeight(property, label);
		
		var propertyRect = position;
		propertyRect.width -= buttonWidth;
		EditorGUI.PropertyField(propertyRect, property, label);

		var buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);
		if(GUI.Button(buttonRect, "Save")) {
			string selectedAssetPath = "Assets";
			if(property.serializedObject.targetObject is MonoBehaviour) {
				MonoScript ms = MonoScript.FromMonoBehaviour((MonoBehaviour)property.serializedObject.targetObject);
				selectedAssetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath( ms ));
			}
			Type type = fieldInfo.FieldType;
			if(type.IsArray) type = type.GetElementType();
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) type = type.GetGenericArguments()[0];
			CreateAssetWithSavePrompt(property.objectReferenceValue, type, selectedAssetPath);
		}
		

		EditorGUI.EndProperty();
	}

	static T CreateAssetWithSavePrompt<T> (T obj, Type type, string path) where T : UnityEngine.Object {
		if (obj == null) return null;
		path = EditorUtility.SaveFilePanelInProject("Save "+type, type.Name+".asset", "asset", "Enter a file name for the "+type.Name.ToString()+".", path);
		if (path == "") return null;
		AssetDatabase.DeleteAsset (path);
		AssetDatabase.CreateAsset (obj, path);
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh();
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		EditorGUIUtility.PingObject(obj);
		return obj;
	}
}