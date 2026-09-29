using System.IO;

public static class PathX {
	public static string GetFullPathWithNewFileName(string fullPath, string newFileName) {
		var ext = Path.GetExtension(fullPath);
		var dirPath = Path.GetDirectoryName(fullPath);
		return Path.Combine(dirPath, newFileName)+ext;
    }
	public static string GetFullPathWithoutExtension(string path) {
        // Thin convenience wrapper: passing null to ChangeExtension strips the extension.
        return System.IO.Path.ChangeExtension(path, null);
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
		if (toCleanPath == null) return toCleanPath;
		// Sanitise each filename SEGMENT independently while preserving the directory
		// separators that delimit them. We must handle BOTH separators ('/' and '\'),
		// otherwise on macOS/Linux (forward-slash paths) the whole path would be treated
		// as one filename and every '/' would be replaced.
		char primarySeparator = Path.DirectorySeparatorChar;
		char altSeparator = Path.AltDirectorySeparatorChar;
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		var result = new System.Text.StringBuilder(toCleanPath.Length);
		var segment = new System.Text.StringBuilder();
		for (int i = 0; i < toCleanPath.Length; i++) {
			char c = toCleanPath[i];
			if (c == primarySeparator || c == altSeparator) {
				result.Append(SanitiseFileNameSegment(segment.ToString(), invalidFileNameChars, replaceWith));
				result.Append(c); // keep the separator as-is
				segment.Length = 0;
			} else {
				segment.Append(c);
			}
		}
		result.Append(SanitiseFileNameSegment(segment.ToString(), invalidFileNameChars, replaceWith));
		return result.ToString();
	}

	static string SanitiseFileNameSegment (string segment, char[] invalidFileNameChars, string replaceWith) {
		//clean bad filename chars
		foreach (char badChar in invalidFileNameChars)
			segment = segment.Replace(badChar.ToString(), replaceWith);
		//collapse runs of "replaceWith". ie: change "test-----file.txt" to "test-file.txt"
		if (string.IsNullOrWhiteSpace(replaceWith) == false) {
			string doubled = replaceWith + replaceWith;
			while (segment.Contains(doubled))
				segment = segment.Replace(doubled, replaceWith);
		}
		return segment;
	}
}
