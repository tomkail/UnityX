using UnityEngine;
using UnityEditor;
using System.IO;

[CustomPropertyDrawer (typeof(FolderPathAttribute))]
class FolderPathDrawer : BaseAttributePropertyDrawer<FolderPathAttribute> {
	const int buttonWidth = 22;

	public static string FolderPathLayout (string path, string label, FolderPathAttribute.RelativeTo relativeTo, bool editable = true, bool removePrefixSlash = false) {
		return FolderPathLayout(path, new GUIContent(label), relativeTo, editable, removePrefixSlash);
	}

	public static string FolderPathLayout (string path, GUIContent label, FolderPathAttribute.RelativeTo relativeTo, bool editable = true, bool removePrefixSlash = false) {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(label);
		
		if(path == null) path = "";
		EditorGUI.BeginDisabledGroup(!editable);
		path = EditorGUILayout.TextField(path);
		EditorGUI.EndDisabledGroup();
		if(editable && GUILayout.Button("...", GUILayout.Width(buttonWidth))) path = GetPath(path, relativeTo, removePrefixSlash);
		EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(path));
		if(GUILayout.Button(">", GUILayout.Width(buttonWidth))) RevealPathInFinder(path, relativeTo);
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.EndHorizontal();
		return path;
	}

	public static string FolderPath (Rect position, string path, string label, FolderPathAttribute.RelativeTo relativeTo, bool editable = true, bool removePrefixSlash = false) {
		return FolderPath(position, path, new GUIContent(label), relativeTo, editable, removePrefixSlash);
	}
	
	public static string FolderPath (Rect position, string path, GUIContent label, FolderPathAttribute.RelativeTo relativeTo, bool editable = true, bool removePrefixSlash = false) {
		var contentRect = EditorGUI.PrefixLabel(position, label);
		
		var textRect = contentRect;
		textRect.width -= (buttonWidth + EditorGUIUtility.standardVerticalSpacing) * 2;
		
		var buttonRect = contentRect;
		buttonRect.width = buttonWidth;
		buttonRect.x = textRect.xMax + EditorGUIUtility.standardVerticalSpacing;

		var buttonRect2 = buttonRect;
		buttonRect.x = buttonRect.xMax + EditorGUIUtility.standardVerticalSpacing;

		if(path == null) path = "";
		EditorGUI.BeginDisabledGroup(!editable);
        path = EditorGUI.TextField(textRect, path);
		EditorGUI.EndDisabledGroup();
		if(editable && GUI.Button(buttonRect, "...")) path = GetPath(path, relativeTo, removePrefixSlash);
		EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(path));
		if(GUI.Button(buttonRect2, ">")) RevealPathInFinder(path, relativeTo);
		EditorGUI.EndDisabledGroup();
		return path;
	}

	static string GetPath (string unityRelativePath, FolderPathAttribute.RelativeTo relativeTo, bool removePrefixSlash = false) {
		var absolutePath = ToAbsolutePath(unityRelativePath, relativeTo);
		absolutePath = EditorUtility.OpenFolderPanel("Select Folder", absolutePath, "");
		if(string.IsNullOrWhiteSpace(absolutePath)) return unityRelativePath;
		
		unityRelativePath = absolutePath.Split(new string[]{GetAbsolutePathPrefix(relativeTo)}, System.StringSplitOptions.RemoveEmptyEntries)[0];
		if(removePrefixSlash && unityRelativePath.IndexOf('/') == 0) unityRelativePath = unityRelativePath.Remove(0,1);
		return unityRelativePath;
	}

	static void RevealPathInFinder (string unityRelativePath, FolderPathAttribute.RelativeTo relativeTo) {
		var absolutePath = ToAbsolutePath(unityRelativePath, relativeTo);
		OpenInFileBrowser(Path.GetFullPath(absolutePath));
	}

	static string ToAbsolutePath (string localPath, FolderPathAttribute.RelativeTo relativeTo) {
		return Path.Combine(GetAbsolutePathPrefix(relativeTo), localPath);
	}

	static string GetAbsolutePathPrefix (FolderPathAttribute.RelativeTo relativeTo) {
		if(relativeTo == FolderPathAttribute.RelativeTo.Assets) {
			return Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets"));
		} else if(relativeTo == FolderPathAttribute.RelativeTo.Project) {
			return Application.dataPath;
		} else if(relativeTo == FolderPathAttribute.RelativeTo.PersistentDataPath) {
			return Application.persistentDataPath;
		} else if(relativeTo == FolderPathAttribute.RelativeTo.Desktop) {
			return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
		}
		return "";
	}
	
	// Draw the property inside the given rect
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}
		property.stringValue = FolderPath(position, property.stringValue, label, attribute.relativeTo);
	}

	protected override bool IsSupported(SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String;
	}









	static void OpenInFileBrowser(string path) {
		#if UNITY_EDITOR_OSX
		OpenInMacFileBrowser(path);	
		#elif UNITY_EDITOR_WIN
		OpenInWinFileBrowser(path);
		#else
		EditorUtility.RevealInFinder(path);
		#endif
	}
	
	#if UNITY_EDITOR_OSX
	static void OpenInMacFileBrowser(string path) {
		bool openInsidesOfFolder = false;
		string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes
		if (Directory.Exists(macPath)) openInsidesOfFolder = true;
		if (!macPath.StartsWith("\"")) macPath = "\"" + macPath;
		if (!macPath.EndsWith("\"")) macPath = macPath + "\"";
		
		string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;
		System.Diagnostics.Process.Start("open", arguments);
	}
	#endif
	
	#if UNITY_EDITOR_WIN
	static void OpenInWinFileBrowser(string path) {
		bool openInsidesOfFolder = false;
		string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes
		if (Directory.Exists(winPath)) openInsidesOfFolder = true;
		System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "" : "/select,") + winPath);
	}
	#endif
}