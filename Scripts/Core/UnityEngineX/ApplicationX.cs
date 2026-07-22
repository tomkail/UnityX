using System.IO;
using UnityEngine;

public class ApplicationX : MonoBehaviour {
    public static bool isApplicationQuitting;


	public static string projectPath => Path.Combine(Application.dataPath, "../../");

	// Cached for access in threads
	static string _streamingAssetsPath;
	public static string streamingAssetsPath {
		get {
			Debug.Assert(_streamingAssetsPath != null, "streamingAssetsPath not set in ApplicationX! Does this component exist?");
			return _streamingAssetsPath;
		}
	}

	void Awake () {
		_streamingAssetsPath = Application.streamingAssetsPath;
	}

	// Returns a sensible path for saving files depending on device, intended for outputting debugging files, or save files/screenshots.
	// When useExecutableFolderOnDesktop is true, desktop platforms will save next to the game executable rather than in the users file system. 
	public static string GetPlatformSpecificFileDirectory (string directoryName, bool useExecutableFolderOnDesktop = true) {
		// In editor, use the folder containing the Unity project
		#if UNITY_EDITOR
		return Path.GetFullPath(Path.Combine(projectPath, directoryName));
		// On desktop use the desktop if useExecutableFolderOnDesktop is true, else uses persistentDataPath
		// OSX: ~/Library/Application Support/CompanyName/ProductName
		// Windows: C:\Users\UserName\AppData\LocalLow\CompanyName\ProductName
		#elif UNITY_STANDALONE
		if(useExecutableFolderOnDesktop) return Path.GetFullPath(Path.Combine(Application.dataPath, "../"+directoryName));
		else return Path.GetFullPath(Path.Combine(Application.persistentDataPath, directoryName));
		// On phones/consoles/misc use the default path
		#else
		return Path.GetFullPath(Path.Combine(Application.persistentDataPath, directoryName));
		#endif
	}
    
    void OnApplicationQuit () {
        isApplicationQuitting = true;
    }
}
