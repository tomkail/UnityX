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

	// https://chrisbitting.com/2014/04/14/fixing-removing-invalid-characters-from-a-file-path-name-c/
	public static string ReplaceIllegalCharacters (string toCleanPath, string replaceWith = "_") {
		//get just the filename - can't use Path.GetFileName since the path might be bad!  
		string[] pathParts = toCleanPath.Split(new[] { '\\' });  
		string newFileName = pathParts[pathParts.Length - 1];  
		//get just the path  
		string newPath = toCleanPath.Substring(0, toCleanPath.Length - newFileName.Length);   
		//clean bad path chars  
		foreach (char badChar in Path.GetInvalidPathChars())  
		{  
			newPath = newPath.Replace(badChar.ToString(), replaceWith);  
		}  
		//clean bad filename chars  
		foreach (char badChar in Path.GetInvalidFileNameChars())  
		{  
			newFileName = newFileName.Replace(badChar.ToString(), replaceWith);  
		}  
		//remove duplicate "replaceWith" characters. ie: change "test-----file.txt" to "test-file.txt"  
		if (string.IsNullOrWhiteSpace(replaceWith) == false)  
		{  
			newPath = newPath.Replace(replaceWith + replaceWith, replaceWith);  
			newFileName = newFileName.Replace(replaceWith + replaceWith, replaceWith);  
		}  
		//return new, clean path:  
		return newPath + newFileName;  
	}
}
