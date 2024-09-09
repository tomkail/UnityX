using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using System.IO;
#endif

public static class SystemX {
	// Can use EditorUtility.RevealInFinder in editor
	public static bool OpenInFileBrowser(string path) {
		switch (Application.platform) {
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
				return OpenInWinFileBrowser(path);
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
				return OpenInMacFileBrowser(path);
			default:
				Debug.LogError ("Could not open in file browser because OS is unrecognized. OS is "+SystemInfo.operatingSystem);
				return false;
		}
	}
	
	private static bool OpenInMacFileBrowser(string path) {
		bool openInsidesOfFolder = false;
		
		// try mac
		string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes
		#if UNITY_EDITOR
		// if path requested is a folder, automatically open insides of that folder
		if ( Directory.Exists(macPath) ) {
			openInsidesOfFolder = true;
		}
		#endif
		
		if ( !macPath.StartsWith("\"") ) {
			macPath = "\"" + macPath;
		}
		
		if ( !macPath.EndsWith("\"") ){
			macPath = macPath + "\"";
		}
		
		string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;
		
		try {
			Process.Start("open", arguments);
		} catch ( Win32Exception e ) {
			// tried to open mac finder in windows
			// just silently skip error
			// we currently have no platform define for the current OS we are in, so we resort to this
			e.HelpLink = ""; // do anything with this variable to silence warning about not using it
			return false;
		}
		return true;
	}
	
	private static bool OpenInWinFileBrowser(string path) {
		bool openInsidesOfFolder = false;
		
		// try windows
		string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes
		#if UNITY_EDITOR
		// if path requested is a folder, automatically open insides of that folder
		if ( Directory.Exists(winPath) ) {
			openInsidesOfFolder = true;
		}
		#endif
		
		try {
			Process.Start("explorer.exe", (openInsidesOfFolder ? "" : "/select, \"") + winPath+"\"");
		} catch ( Win32Exception e ) {
			// tried to open win explorer in mac
			// just silently skip error
			// we currently have no platform define for the current OS we are in, so we resort to this
			e.HelpLink = ""; // do anything with this variable to silence warning about not using it
			return false;
		}
		return true;
	}
}