using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Add this attribute to a string-based asset path and it will draw a proper object
/// field to allow you to pick the asset. You need to also specify whether it's a resource
/// or a normal asset. Most often you probably want it to be a resource. e.g.:
/// 
///     [AssetPath(typeof(Sprite), resourcePath:true)]
///	    public string spritePath;
/// 
/// An extra little feature is that when the (non-empty) path can't be found, it will show
/// the path in red in a normal string field, so you can check/edit it manually if necessary.
/// </summary>
[CustomPropertyDrawer (typeof(AssetPathAttribute))]
class AssetPathDrawer : BaseAttributePropertyDrawer<AssetPathAttribute> {

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		var currentPath = property.stringValue;
		if(Application.isPlaying && attribute.isResourcePath && attribute.onlyLoadResourcePathsInEditMode) {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUI.TextField(position, property.displayName, currentPath);
			EditorGUI.EndDisabledGroup();
			return;
		}

		Object obj;
		Object newObj = null;

		if( attribute.isResourcePath ) {
			obj = Resources.Load(currentPath, attribute.assetType);
		} else {
			obj = AssetDatabase.LoadAssetAtPath(currentPath, attribute.assetType);
		}

		bool pathExistsButNotFound = !string.IsNullOrWhiteSpace(currentPath) && obj == null;

		// Not found? Show the path that's there in red
		if( pathExistsButNotFound ) {
			OnGUIX.BeginBackgroundColor(Color.red);
			property.stringValue = EditorGUI.TextField(new Rect(position.x,position.y, position.width-100, position.height), property.displayName +" (not found)", currentPath);
			OnGUIX.EndBackgroundColor();
			newObj = EditorGUI.ObjectField(new Rect(position.xMax-100,position.y, 100, position.height), obj, attribute.assetType, allowSceneObjects:false);
		}

		// Show object picker
		else {
			newObj = EditorGUI.ObjectField(position, property.displayName, obj, attribute.assetType, allowSceneObjects:false);
		}
			
		if( newObj != obj ) {
			
			var newPath = AssetDatabase.GetAssetPath (newObj);

			// Resource paths are relative to a resources directory rather than project
			// They also don't have an extension
			if( attribute.isResourcePath ) {
				const string resourcesStr = "/Resources/";
				var resourcesIdx = newPath.IndexOf(resourcesStr);
				if( resourcesIdx == -1 ) {
					Debug.LogError("Asset must be in a resources folder");
					return;
				}

				var resourcesRelativePath = newPath.Substring(resourcesIdx + resourcesStr.Length);

				// Resource paths don't have extensions either
				newPath = Path.Combine(
					Path.GetDirectoryName(resourcesRelativePath), 
					Path.GetFileNameWithoutExtension(resourcesRelativePath));
			}
			
			property.stringValue = newPath;
			property.serializedObject.ApplyModifiedProperties();
		}
	}

	protected override bool IsSupported(SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String;
	}
}