using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class PathX {
	public static string GetFullPathWithNewFileName(string fullPath, string newFileName) {
		var ext = Path.GetExtension(fullPath);
		var dirPath = Path.GetDirectoryName(fullPath);
		return Path.Combine(dirPath, newFileName)+ext;
    }
	public static string GetFullPathWithoutExtension(string path) {
        return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
    }

	/// <summary>
	/// Determine whether a given path is a directory.
	/// </summary>
	public static bool PathIsDirectory (string absolutePath) {
		FileAttributes attr = File.GetAttributes(absolutePath);
		if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
			return true;
		else
			return false;
	}

	public static bool Compare (string pathA, string pathB) {
		var fullPathA = Path.GetFullPath(pathA);
		var fullPathB = Path.GetFullPath(pathB);
		return fullPathA == fullPathB;
	}
}
