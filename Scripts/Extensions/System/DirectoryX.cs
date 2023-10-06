using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class DirectoryX {
	public static void DeleteAllContents (this DirectoryInfo directoryInfo, bool alsoDeleteFolder = true) {
        if(!directoryInfo.Exists) return;
		foreach(System.IO.FileInfo file in directoryInfo.GetFiles()) file.Delete();
    	foreach(System.IO.DirectoryInfo subDirectory in directoryInfo.GetDirectories()) subDirectory.Delete(true);
		if(alsoDeleteFolder) directoryInfo.Delete(true);
	}

	public static string GetRelativePath(string filespec, string folder) {
		Uri pathUri = new Uri(filespec);
		// Folders must end in a slash
		if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString())) 
			folder += Path.DirectorySeparatorChar;
		Uri folderUri = new Uri(folder);
		return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
	}
}
