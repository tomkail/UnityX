using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

public static class ScriptAssetCreator {
	public static void CreateNewFile (string filePath, string text) {
		CreateScriptAssetFromTemplate(filePath, text);
	}
    public static void CreateNewFileAndStartNameEditing (string filePath, string text) {
		ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptAssetAction>(), filePath, null, text);
	}

    public static UnityEngine.Object CreateScriptAssetFromTemplate(string filePath, string text) {
        string fullPath = Path.GetFullPath(filePath);
        UTF8Encoding encoding = new UTF8Encoding(true, false);
        bool append = false;
        StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
        streamWriter.Write(text);
        streamWriter.Close();
        AssetDatabase.ImportAsset(filePath);
        return AssetDatabase.LoadAssetAtPath(filePath, typeof(DefaultAsset));
    }

	class CreateScriptAssetAction : EndNameEditAction {
		public override void Action(int instanceId, string filePath, string text) {
			UnityEngine.Object asset = CreateScriptAssetFromTemplate(filePath, text);
			ProjectWindowUtil.ShowCreatedAsset(asset);
		}	
	}
    
    // Gets a camelCase variable name from a string
    public static string ToCamelCase(string text) {
        List<char> outputList = new List<char>();
        char[] a = text.ToLower().ToCharArray();
        for (int i = 0; i < a.Length; i++) {
            if(a[i] == ' ') continue;
            if(i == 0) outputList.Add(char.ToLower(a[i]));
            else if(a[i-1] == ' ') outputList.Add(char.ToUpper(a[i]));
            else outputList.Add(a[i]);
        }
        return new string(outputList.ToArray());
    }
}
