#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ScriptableObjectX {

	/// <summary>
	//	Create new asset from <see cref="ScriptableObject"/> type using the OS save prompt.
	/// </summary>
	public static T CreateAssetWithSavePrompt<T> (string name = "", string path = "Assets") where T : ScriptableObject {
		T asset = ScriptableObject.CreateInstance<T> ();
		if(string.IsNullOrWhiteSpace(name)) 
			name = typeof(T).Name;
 
		path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", name+".asset", "asset", "Enter a file name for the ScriptableObject.", path);
		if (path != "") {
			AssetDatabase.CreateAsset (asset, path);
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow ();
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			Selection.activeObject = asset;
			return asset;
		} else {
			Object.DestroyImmediate(asset);
			return null;
		}
	}

	/// Create new asset from <see cref="ScriptableObject"/> type with unique name at
	/// selected folder in project window. Asset creation can be cancelled by pressing
	/// escape key when asset is initially being named.
	/// </summary>
	/// <typeparam name="T">Type of scriptable object.</typeparam>
	/// <param name="_objectName">_object name.</param>
	public static T CreateAsset<T>(string _objectName = "", string path = "Assets") where T : ScriptableObject {
		if(!Directory.Exists(path)) {
			Debug.LogError("Path '"+path+"' does not exist.");
			return null;
		}
		T asset = ScriptableObject.CreateInstance<T>();
		if(string.IsNullOrWhiteSpace(_objectName)) _objectName = typeof(T).Name;
		else asset.name = _objectName;
		string nameWithExtension = asset.name + ".asset";
		if (path == "") {
			ProjectWindowUtil.CreateAsset (asset, nameWithExtension);
		} else {
			string filePath = path+"/"+nameWithExtension;
			AssetDatabase.CreateAsset (asset, filePath);
			asset = AssetDatabase.LoadAssetAtPath<T>(filePath);
		}
		return asset;
	}
	
	/// Create new asset from <see cref="ScriptableObject"/> type with unique name at
	/// selected folder in project window. Asset creation can be cancelled by pressing
	/// escape key when asset is initially being named. This method uses reflection to find 
	/// </summary>
	/// <param name="_type">Type of scriptable object.</param>
	/// <param name="_objectName">_object name.</param>
	public static void CreateAssetUsingReflection(Type _type, string _objectName) {
		Type ex = typeof(ScriptableObjectX);
		MethodInfo mi = ex.GetMethod("CreateAsset");
		MethodInfo miConstructed = mi.MakeGenericMethod(_type);
		
		object[] args = {_objectName};
		miConstructed.Invoke(null, args);
	}
		
	[MenuItem("Assets/Create Asset From Script", false, 10000)]
	public static void CreateAssetMenuItem ()
	{
		ScriptableObject asset = ScriptableObject.CreateInstance (Selection.activeObject.name);
		var directoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
		var path = Path.Combine(directoryPath, String.Format ("{0}.asset", Selection.activeObject.name));
		AssetDatabase.CreateAsset (asset, path);
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = asset;
		
	}
	
	[MenuItem("Assets/Create Asset From Script", true, 10000)]
	public static bool ValidateCreateAssetMenuItem ()
	{
		if (Selection.activeObject == null || Selection.activeObject.GetType () != typeof(MonoScript))
			return false;
		MonoScript script = (MonoScript)Selection.activeObject;
		var scriptClass = script.GetClass ();
		if (scriptClass == null)
			return false;
//		return typeof(Manager).IsAssignableFrom (scriptClass.BaseType);
		return true;
	}
}
#endif