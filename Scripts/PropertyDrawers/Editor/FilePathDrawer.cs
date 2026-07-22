using UnityEngine;
using UnityEditor;
using System.IO;

[CustomPropertyDrawer (typeof(FilePathFieldAttribute))]
class FilePathDrawer : BaseAttributePropertyDrawer<FilePathFieldAttribute> {
	const int buttonWidth = 22;

	public static string FilePathLayout (string path, string label, FilePathFieldAttribute.RelativeTo relativeTo, bool editable = true, bool removePrefixSlash = false, bool showPrevNextFileControls = false) {
		return FilePathLayout(path, new GUIContent(label, null, Path.GetFileName(path)), relativeTo, editable, removePrefixSlash, showPrevNextFileControls);
	}

	public static string FilePathLayout (string path, GUIContent label, FilePathFieldAttribute.RelativeTo relativeTo, bool editable = true, bool removePrefixSlash = false, bool showPrevNextFileControls = false) {
		bool exists = FileExistsOrPathEmpty(path, relativeTo);
		Color previousColor = GUI.backgroundColor;
        if(!exists) GUI.backgroundColor = Color.red;
		label.text = string.Format("{0} ({1})", label.text, RelativeToLabelText(relativeTo));

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(label);
		
		if(path == null) path = "";
		EditorGUI.BeginDisabledGroup(!editable);
		path = EditorGUILayout.TextField(path);
		EditorGUI.EndDisabledGroup();
		if(showPrevNextFileControls) {
			string[] filesInDir = null;
            int numFilesInDir;
            int indexOfFile;
            SetFilesInDir(path, ref filesInDir, out numFilesInDir, out indexOfFile);
			EditorGUI.BeginDisabledGroup(indexOfFile == -1 || indexOfFile == 0);
			if(GUILayout.Button(new GUIContent("<", null, GUI.enabled ? Path.GetFileName(filesInDir[indexOfFile-1]) : string.Empty), GUILayout.Width(buttonWidth))) path = filesInDir[indexOfFile-1];
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(indexOfFile == -1 || indexOfFile == numFilesInDir-1);
			if(GUILayout.Button(new GUIContent(">", null, GUI.enabled ? Path.GetFileName(filesInDir[indexOfFile+1]) : string.Empty), GUILayout.Width(buttonWidth))) path = filesInDir[indexOfFile+1];
			EditorGUI.EndDisabledGroup();
		}
		if(editable && GUILayout.Button(new GUIContent("...", null, "Show file picker"), GUILayout.Width(buttonWidth))) path = GetPath(path, relativeTo, removePrefixSlash);
		EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(path) || !exists);
		if(GUILayout.Button(new GUIContent(">", null, "Show in finder"), GUILayout.Width(buttonWidth))) RevealPathInFinder(path, relativeTo);
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.EndHorizontal();
		if(!exists) GUI.backgroundColor = previousColor;
		return path;
	}

	public static string FilePath (Rect position, string path, string label, FilePathFieldAttribute.RelativeTo relativeTo, bool editable = true, bool removePrefixSlash = false, bool showPrevNextFileControls = false) {
		return FilePath(position, path, new GUIContent(label, null, Path.GetFileName(path)), relativeTo, editable, removePrefixSlash, showPrevNextFileControls);
	}
	
	public static string FilePath (Rect position, string path, GUIContent label, FilePathFieldAttribute.RelativeTo relativeTo, bool editable = true, bool removePrefixSlash = false, bool showPrevNextFileControls = false) {
		bool exists = FileExistsOrPathEmpty(path, relativeTo);
		Color previousColor = GUI.backgroundColor;
        if(!exists) GUI.backgroundColor = Color.red;
		label.text = string.Format("{0} ({1})", label.text, RelativeToLabelText(relativeTo));
		
		var contentRect = EditorGUI.PrefixLabel(position, label);
		
		int numButtons = showPrevNextFileControls ? 4 : 2;
		var textRect = contentRect;
		textRect.width -= (buttonWidth + EditorGUIUtility.standardVerticalSpacing) * numButtons;

		var buttonRect = textRect;
		buttonRect.width = buttonWidth;
		buttonRect.x = textRect.xMax + EditorGUIUtility.standardVerticalSpacing;

		var buttonRect2 = buttonRect;
		buttonRect2.x = buttonRect.xMax + EditorGUIUtility.standardVerticalSpacing;

		var buttonRect3 = buttonRect;
		var buttonRect4 = buttonRect2;

		if(showPrevNextFileControls) {
			buttonRect3 = buttonRect2;
			buttonRect3.x = buttonRect2.xMax + EditorGUIUtility.standardVerticalSpacing;
			buttonRect4 = buttonRect3;
			buttonRect4.x = buttonRect3.xMax + EditorGUIUtility.standardVerticalSpacing;
		}

		if(showPrevNextFileControls) {
			string dirName = null;
			if(!string.IsNullOrWhiteSpace(path)) dirName = Path.GetDirectoryName(path);
			string[] filesInDir = null;
            int numFilesInDir;
            int indexOfFile;
            SetFilesInDir(path, ref filesInDir, out numFilesInDir, out indexOfFile);
			EditorGUI.BeginDisabledGroup(indexOfFile == -1 || indexOfFile == 0);
			if(GUI.Button(buttonRect, new GUIContent("<", null, GUI.enabled ? Path.GetFileName(filesInDir[indexOfFile-1]) : string.Empty))) path = filesInDir[indexOfFile-1];
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(indexOfFile == -1 || indexOfFile == numFilesInDir-1);
			if(GUI.Button(buttonRect2, new GUIContent(">", null, GUI.enabled ? Path.GetFileName(filesInDir[indexOfFile+1]) : string.Empty))) path = filesInDir[indexOfFile+1];
			EditorGUI.EndDisabledGroup();
		}

		if(path == null) path = "";
		EditorGUI.BeginDisabledGroup(!editable);
		path = EditorGUI.TextField(textRect, path);
		EditorGUI.EndDisabledGroup();
		if(editable && GUI.Button(buttonRect3, new GUIContent("...", null, "Show file picker") )) path = GetPath(path, relativeTo, removePrefixSlash);
		EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(path) || !exists);
		if(GUI.Button(buttonRect4, new GUIContent(">", null, "Show in finder"))) RevealPathInFinder(path, relativeTo);
		EditorGUI.EndDisabledGroup();
		if(!exists) GUI.backgroundColor = previousColor;
		return path;
	}

    static void SetFilesInDir (string path, ref string[] filesInDir, out int numFilesInDir, out int indexOfFile) {
        string dirName = null;
        if(!string.IsNullOrWhiteSpace(path)) dirName = Path.GetDirectoryName(path);
        if(!string.IsNullOrWhiteSpace(dirName)) filesInDir = Directory.GetFiles(dirName);
        indexOfFile = -1;
        if(filesInDir != null) {
            numFilesInDir = filesInDir.Length;
            for(int i = 0; i < numFilesInDir; i++) {
                if(PathX.Compare(filesInDir[i], path)) {
                    indexOfFile = i;
                    break;
                }
            }
        } else {
            numFilesInDir = 0;
        }
    }

	static string GetPath (string unityRelativePath, FilePathFieldAttribute.RelativeTo relativeTo, bool removePrefixSlash = false) {
		var absolutePath = ToAbsolutePath(unityRelativePath, relativeTo);
		absolutePath = EditorUtility.OpenFilePanel("Select File", absolutePath, "");
		if(string.IsNullOrWhiteSpace(absolutePath)) return unityRelativePath;

		unityRelativePath = FromAbsolutePath(absolutePath, relativeTo);
		if(removePrefixSlash && unityRelativePath.IndexOf('/') == 0) unityRelativePath = unityRelativePath.Remove(0,1);
		return unityRelativePath;
	}

	static void RevealPathInFinder (string unityRelativePath, FilePathFieldAttribute.RelativeTo relativeTo) {
		var absolutePath = ToAbsolutePath(unityRelativePath, relativeTo);
		EditorUtility.RevealInFinder(Path.GetFullPath(absolutePath));
	}

	// Draw the property inside the given rect
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}
		label = EditorGUI.BeginProperty(position, label, property);
		var newValue = FilePath(position, property.stringValue, label, attribute.relativeTo, true, false, attribute.showPrevNextFileControls);
		if(property.stringValue != newValue) {
			property.serializedObject.Update();
			property.stringValue = newValue;
			property.serializedObject.ApplyModifiedProperties();
		}
		EditorGUI.EndProperty();
	}


	static string RelativeToLabelText (FilePathFieldAttribute.RelativeTo relativeTo) {
		switch(relativeTo) {
			case FilePathFieldAttribute.RelativeTo.Assets:
				return "Assets";
			case FilePathFieldAttribute.RelativeTo.Resources:
				return "Resources";
			case FilePathFieldAttribute.RelativeTo.Project:
				return "Project";
			case FilePathFieldAttribute.RelativeTo.PersistentDataPath:
				return "Persistent Data";
			case FilePathFieldAttribute.RelativeTo.Root:
				return "Root";
			default:
				return "?";
		}
	}

	static string ToAbsolutePath (string localPath, FilePathFieldAttribute.RelativeTo relativeTo) {
		switch(relativeTo) {
			case FilePathFieldAttribute.RelativeTo.Assets:
				return EditorApplicationX.UnityRelativeToAbsolutePath(localPath);
			case FilePathFieldAttribute.RelativeTo.Resources:
				return EditorApplicationX.ResourcesToAbsolutePath(localPath);
			case FilePathFieldAttribute.RelativeTo.Project:
				return EditorApplicationX.ProjectToAbsolutePath(localPath);
			case FilePathFieldAttribute.RelativeTo.PersistentDataPath:
				return EditorApplicationX.PersistentDataPathToAbsolutePath(localPath);
			default:
				return localPath;
		}
	}

	static string FromAbsolutePath (string localPath, FilePathFieldAttribute.RelativeTo relativeTo) {
		switch(relativeTo) {
			case FilePathFieldAttribute.RelativeTo.Assets:
				return EditorApplicationX.AbsoluteToUnityRelativePath(localPath);
			case FilePathFieldAttribute.RelativeTo.Resources:
				return EditorApplicationX.AbsoluteToResourcesPath(localPath);
			case FilePathFieldAttribute.RelativeTo.Project:
				return EditorApplicationX.AbsoluteToProjectPath(localPath);
			case FilePathFieldAttribute.RelativeTo.PersistentDataPath:
				return EditorApplicationX.AbsoluteToPersistentDataPath(localPath);
			default:
				return localPath;
		}
	}

	static bool FileExistsOrPathEmpty (string localPath, FilePathFieldAttribute.RelativeTo relativeTo) {
		return string.IsNullOrWhiteSpace(localPath) || File.Exists(ToAbsolutePath(localPath, relativeTo));
	}

	protected override bool IsSupported(SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String;
	}
}