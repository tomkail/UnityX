using UnityEngine;
using UnityEditor;

namespace UnityX.SceneManagement.Editor {

	// Draws a SceneReference as a single SceneAsset object field, caching the asset path into the
	// reference whenever the asset is assigned or changed.
	[CustomPropertyDrawer(typeof(SceneReference))]
	public class SceneReferenceDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			var sceneAsset = property.FindPropertyRelative("sceneAsset");
			var scenePath = property.FindPropertyRelative("scenePath");

			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.BeginChangeCheck();
			var picked = EditorGUI.ObjectField(position, label, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
			if (EditorGUI.EndChangeCheck()) {
				sceneAsset.objectReferenceValue = picked;
				scenePath.stringValue = picked != null ? AssetDatabase.GetAssetPath(picked) : string.Empty;
			}
			EditorGUI.EndProperty();
		}
	}
}
