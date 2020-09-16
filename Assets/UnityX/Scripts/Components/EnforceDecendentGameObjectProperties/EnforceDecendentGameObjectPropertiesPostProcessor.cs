#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class EnforceDecendentGameObjectPropertiesPostProcessor : UnityEditor.AssetModificationProcessor {
	static string[] OnWillSaveAssets (string[] paths) {
		EnforceDecendentGameObjectProperties.EnforcePropertiesAll();
		return paths;
	}
}
#endif