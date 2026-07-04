using System;
using System.IO;

public static class DirectoryX {
	public static void DeleteAllContents (this DirectoryInfo directoryInfo, bool alsoDeleteFolder = true) {
        if(!directoryInfo.Exists) return;
		foreach(FileInfo file in directoryInfo.GetFiles()) file.Delete();
    	foreach(DirectoryInfo subDirectory in directoryInfo.GetDirectories()) subDirectory.Delete(true);
		if(alsoDeleteFolder) directoryInfo.Delete(true);
	}

	// Kept as a custom impl (not Path.GetRelativePath) because that BCL method only exists on .NET Standard 2.1+, not .NET Framework.
	// Computes the relative path from 'folder' to 'filespec' by comparing absolute path segments,
	// avoiding the old Uri-based approach (which threw UriFormatException on relative input and
	// mis-parsed '#' as a URI fragment).
	public static string GetRelativePath(string filespec, string folder) {
		// Resolve to absolute paths first so relative inputs are valid and comparable.
		string fullFile = Path.GetFullPath(filespec);
		string fullFolder = Path.GetFullPath(folder);

		char sep = Path.DirectorySeparatorChar;
		char[] separators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
		string[] fileParts = fullFile.Split(separators, StringSplitOptions.RemoveEmptyEntries);
		string[] folderParts = fullFolder.Split(separators, StringSplitOptions.RemoveEmptyEntries);

		// Find the length of the shared leading directory prefix.
		// Ordinal (case-sensitive) matches the old Uri behaviour and is correct on
		// case-sensitive filesystems.
		int common = 0;
		int max = Math.Min(fileParts.Length, folderParts.Length);
		while (common < max && string.Equals(fileParts[common], folderParts[common], StringComparison.Ordinal))
			common++;

		var relativeParts = new System.Collections.Generic.List<string>();
		// Step up out of each folder segment not shared with the file.
		for (int i = common; i < folderParts.Length; i++)
			relativeParts.Add("..");
		// Step down into the file's remaining segments.
		for (int i = common; i < fileParts.Length; i++)
			relativeParts.Add(fileParts[i]);

		return string.Join(sep.ToString(), relativeParts);
	}
}
