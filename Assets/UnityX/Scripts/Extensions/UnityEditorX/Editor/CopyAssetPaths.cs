using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CopyAssetPaths {
	[MenuItem("Assets/Copy/Asset Path", true)]
	public static bool CopyAssetPathValidator () {
		return Selection.objects.Length == 1;
	}
	[MenuItem("Assets/Copy/Asset Path")]
	public static void CopyAssetPath () {
		GUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(Selection.activeObject);
	}

	[MenuItem("Assets/Copy/Absolute Path", true)]
	public static bool CopyAbsolutePathValidator () {
		return Selection.objects.Length == 1;
	}
	[MenuItem("Assets/Copy/Absolute Path")]
	public static void CopyAbsolutePath () {
		GUIUtility.systemCopyBuffer = EditorApplicationX.UnityRelativeToAbsolutePath(AssetDatabase.GetAssetPath(Selection.activeObject));
	}

	[MenuItem("Assets/Copy/Resources Path", true)]
	public static bool CopyResourcesPathValidator () {
		return Selection.objects.Length == 1 && AssetDatabase.GetAssetPath(Selection.activeObject).Contains("Resources/");
	}
	[MenuItem("Assets/Copy/Resources Path")]
	public static void CopyResourcesPath () {
		GUIUtility.systemCopyBuffer = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(Selection.activeObject).After("Resources/"));
	}
}
