using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using System.IO;
#endif

public static class SystemX {
	// Intentionally duplicates part of EditorUtility.RevealInFinder: that API is editor-only,
	// whereas this works at RUNTIME (in built players) by shelling out to the platform file browser.
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
		// mac finder doesn't like backward slashes, and wants "-R <quoted path>" to reveal a file.
		return RunFileBrowserProcess(path, "\\", "/", "open", (cleanPath, openInsidesOfFolder) => {
			string quotedPath = cleanPath;
			if ( !quotedPath.StartsWith("\"") ) {
				quotedPath = "\"" + quotedPath;
			}
			if ( !quotedPath.EndsWith("\"") ) {
				quotedPath = quotedPath + "\"";
			}
			return (openInsidesOfFolder ? "" : "-R ") + quotedPath;
		});
	}

	private static bool OpenInWinFileBrowser(string path) {
		// windows explorer doesn't like forward slashes, and wants "/select, <path>" to reveal a file.
		return RunFileBrowserProcess(path, "/", "\\", "explorer.exe",
			(cleanPath, openInsidesOfFolder) => (openInsidesOfFolder ? "" : "/select, \"") + cleanPath + "\"");
	}

	// Shared body for the platform-specific browsers: clean the path separators, detect whether the
	// path is a folder (so we open its insides), build the OS-specific arguments, then Process.Start.
	private static bool RunFileBrowserProcess(string path, string fromSeparator, string toSeparator, string executable, System.Func<string, bool, string> buildArguments) {
		bool openInsidesOfFolder = false;

		string cleanPath = path.Replace(fromSeparator, toSeparator);
		#if UNITY_EDITOR
		// if path requested is a folder, automatically open insides of that folder
		if ( Directory.Exists(cleanPath) ) {
			openInsidesOfFolder = true;
		}
		#endif

		try {
			Process.Start(executable, buildArguments(cleanPath, openInsidesOfFolder));
		} catch ( Win32Exception ) {
			// Deliberate fallback: we tried to launch the file browser for the wrong OS
			// (e.g. mac 'open' while running on Windows). We have no platform define for the
			// current OS, so we just silently skip.
			return false;
		}
		return true;
	}
}