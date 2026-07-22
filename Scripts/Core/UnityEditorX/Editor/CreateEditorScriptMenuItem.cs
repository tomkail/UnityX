using System.IO;
using UnityEngine;
using UnityEditor;
public static class CreateEditorScriptMenuItem {
	const string fileNameTemplate = "{0}Editor.cs";
	const string template = "using UnityEngine;\nusing UnityEditor;\n\n[CustomEditor(typeof({0}))]\npublic class {0}Editor : BaseEditor<{0}> {{\n\tpublic override void OnInspectorGUI () {{\n\t\tbase.OnInspectorGUI();\n\t}}\n}}";

	[MenuItem("Assets/Create/Editor Script", true)]
	public static bool CreateEditorScriptValidator () {
		if (Selection.activeObject == null || !(Selection.activeObject is MonoScript)) return false;
		var typeName = Selection.activeObject.name;
		string fileName = string.Format(fileNameTemplate, typeName);
		var directoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
		string path = directoryPath+"/Editor/"+fileName;
		var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
		return asset == null;
	}

	[MenuItem("Assets/Create/Editor Script", false, 80)]
	public static void CreateEditorScript () {
		var typeName = Selection.activeObject.name;
		var directoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
		var targetDirectoryPath = directoryPath+"/Editor";
		targetDirectoryPath = targetDirectoryPath.Replace('\\', '/');
		if(!AssetDatabase.GetSubFolders(directoryPath).Contains(targetDirectoryPath)) {
			AssetDatabase.CreateFolder(directoryPath, "Editor");
		}
			
		string fileName = string.Format(fileNameTemplate, typeName);
		string contents = string.Format(template, typeName);
		string filePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(targetDirectoryPath, fileName));
	    ScriptAssetCreator.CreateNewFile(filePath, contents);
	}
}
