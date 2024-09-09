using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EditorApplicationX {

	static bool? _isRetina;
	public static bool IsRetina () {
		if(_isRetina == null) _isRetina = Application.platform == RuntimePlatform.OSXEditor && float.Parse(Application.unityVersion.Substring(0,3)) >= 5.4;
		return (bool)_isRetina;
	}


	// Returns a sanitized version of the supplied string by:
	//    - swapping MS Windows-style file separators with Unix/Mac style file separators.
	//
	// If null is provided, null is returned.
	public static string SanitizePathString(string path) {
		if (path == null) {
			return null;
		}
		return path.Replace('\\', '/');
	}
	
	// Combines two file paths and returns that path.  Unlike C#'s native Paths.Combine, regardless of operating 
	// system this method will always return a path which uses forward slashes ('/' characters) exclusively to ensure
	// equality checks on path strings return equalities as expected.
	public static string CombinePaths(string firstPath, string secondPath) {
		return SanitizePathString(Path.Combine(firstPath, secondPath));
	}

	#region Conversion
	/// <summary>
	/// Returns an asset path from an absolute path. Does not include "Assets/".
	/// </summary>
	/// <returns>The to unity relative path.</returns>
	/// <param name="absolutePath">Absolute path.</param>
	public static string AbsoluteToUnityRelativePath(string absolutePath) {
		return SanitizePathString(absolutePath.Substring(Application.dataPath.Length-6));
	}

	public static string UnityRelativeToAbsolutePath(string localPath) {
		if(localPath.Length < 7) return Application.dataPath;
		return CombinePaths(Application.dataPath, localPath.Substring(7));
	}

	/// <summary>
	/// Returns a project path from an absolute path. Includes "Assets/". Can be used by AssetDatabase functions.
	/// </summary>
	/// <returns>The to project path.</returns>
	/// <param name="absolutePath">Absolute path.</param>
	public static string AbsoluteToProjectPath(string absolutePath) {
		return SanitizePathString(absolutePath.Substring(Application.dataPath.Length+1));
	}

	public static string ProjectToAbsolutePath(string localPath) {
		return CombinePaths(Application.dataPath, localPath);
	}

    /// <summary>
	/// Returns a resources path from an absolute path, using the last instance of the path "Resources". Can be used by Resources functions.
	/// </summary>
	/// <returns>The resource path.</returns>
	/// <param name="absolutePath">Absolute path.</param>
	public static string AbsoluteToResourcesPath(string absolutePath) {
		if(!absolutePath.Contains("Resources/")) return "";
		var relativePath = After(absolutePath, "Resources/");
        string After(string value, string a) {
            return value.Substring(value.LastIndexOf(a) + a.Length);
        }
        var directoryName = Path.GetDirectoryName(relativePath);
		relativePath = Path.Combine(directoryName, Path.GetFileNameWithoutExtension(relativePath));
		return relativePath;
	}

	#if UNITY_EDITOR
	public static string ResourcesToAbsolutePath(string localPath) {
		var asset = Resources.Load(localPath);
		if(asset == null) return "";
		var assetPath = AssetDatabase.GetAssetPath(asset);
		// This seems not to let it work consistently?
//		Resources.UnloadAsset(asset);
		return UnityRelativeToAbsolutePath(assetPath);
	}
	#endif

	public static string AbsoluteToPersistentDataPath(string absolutePath) {
		return absolutePath.Substring(Application.persistentDataPath.Length+1);
	}

	public static string PersistentDataPathToAbsolutePath(string localPath) {
		return Path.Combine(Application.persistentDataPath, localPath);
	}

	#endregion

	public static bool IsAbsolutePath(string filePath) {
		return Path.IsPathRooted(filePath) && !Path.GetPathRoot(filePath).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
	}
}
