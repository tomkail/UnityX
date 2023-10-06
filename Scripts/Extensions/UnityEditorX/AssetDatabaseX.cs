using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class AssetDatabaseX {
	#if UNITY_EDITOR

	/// <summary>
	/// Given an absolute path, return a path rooted at the Assets folder.
	/// </summary>
	/// <remarks>
	/// Asset relative paths can only be used in the editor. They will break in builds.
	/// </remarks>
	/// <example>
	/// /Folder/UnityProject/Assets/resources/music returns Assets/resources/music
	/// </example>
	public static string AssetsRelativePath (string absolutePath) {
		if (absolutePath.StartsWith(Application.dataPath)) {
			return "Assets" + absolutePath.Substring(Application.dataPath.Length);
		} else {
			throw new System.ArgumentException("Full path does not contain the current project's Assets folder", "absolutePath");
		}
	}

	/// <summary>
	/// Loads all assets at path.
	/// Replaces AssetDatabase.LoadAllAssetsAtPath, which for some reason requires a file path rather than a folder path.
	/// </summary>
	/// <returns>The all assets at path.</returns>
	/// <param name="path">Path.</param>
	public static Object[] LoadAllAssetsAtPath(string path) {
		if(path.EndsWith("/")) {
			path = path.TrimEnd('/');
		}
		string[] GUIDs = AssetDatabase.FindAssets("", new string[] {path}).Distinct().ToArray();

		Object[] objectList = new Object[GUIDs.Length];
		for (int index = 0; index < GUIDs.Length; index++)
		{
			string guid = GUIDs[index];
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)) as Object;
			objectList[index] = asset;
		}
		
		return objectList;
	}
	
	
	public static T[] LoadAllAssetsAtPath<T> (string path) {
		Object[] assets = AssetDatabaseX.LoadAllAssetsAtPath(path);
		T[] castAssets = assets.Where(asset => asset.GetType() == typeof(T)).Cast<T>().ToArray ();
		return castAssets;
	}
	
	/// <summary>
	/// Loads all assets of type T from anywhere in the project.
	/// </summary>
	/// <returns>The all assets of type.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T[] LoadAllAssetsOfType<T>(string optionalPath = "") where T : Object {
		string[] GUIDs;
		if(optionalPath != "") {
			if(optionalPath.EndsWith("/")) {
				optionalPath = optionalPath.TrimEnd('/');
			}
			GUIDs = AssetDatabase.FindAssets("t:" + typeof (T).ToString(),new string[] { optionalPath });
		} else {
			GUIDs = AssetDatabase.FindAssets("t:" + typeof (T).ToString());
		}
		T[] objectList = new T[GUIDs.Length];
		
		for (int index = 0; index < GUIDs.Length; index++) {
			string guid = GUIDs[index];
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			T asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
			objectList[index] = asset;
		}
		
		return objectList;
	}

	public static T LoadAssetOfType<T>(string optionalPath = "") where T : Object {
		var all = LoadAllAssetsOfType<T>(optionalPath);
		if(all.IsEmpty()) return null;
		else return all.First();
	}

	public static T LoadNamedAssetOfType<T>(string name) where T : Object
	{
		return LoadNamedAsset<T>("t:" + typeof (T).ToString()+" "+name);
	}

	public static T LoadNamedAsset<T>(string name) where T : Object
	{
		var GUIDs = AssetDatabase.FindAssets(name);
		if( GUIDs.Length == 0 ) return null;

		string assetPath = AssetDatabase.GUIDToAssetPath(GUIDs[0]);
		return AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
	}

	/// <summary>
	/// Destroys all sub assets. If used on a subasset, will also destroy the subasset and all family members up to the main asset.
	/// </summary>
	/// <param name="parent">Parent.</param>
	public static void DestroyAllSubAssets (Object parent) {
		string path = AssetDatabase.GetAssetPath(parent);
		Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
		foreach(Object asset in assets) {
			if (UnityEditor.AssetDatabase.IsMainAsset(asset) || asset is GameObject || asset is Component) continue;
			else Object.DestroyImmediate(asset, true);
		}
	}
	
	public static bool IsSubAsset (SerializedProperty prop) {
		if(prop.propertyType != SerializedPropertyType.ObjectReference || prop.objectReferenceValue == null) return false;
		return AssetDatabase.IsSubAsset(prop.objectReferenceValue);
	}
    
	public static bool IsSubAssetOf(SerializedProperty prop, Object parentAsset) {
        return (IsSubAsset(prop) && IsSubAssetOf(prop.objectReferenceValue, parentAsset));
	}

	public static bool IsSubAssetOf(Object asset, Object parentAsset) {
        if(!AssetDatabase.IsSubAsset(asset)) return false;
		Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(parentAsset));
        foreach(Object o in objs){
            if(o == asset){
                return true;
            }
        }
		return false;
	}


	public static bool CanBeSubAsset (SerializedProperty prop) {
		if(prop.propertyType != SerializedPropertyType.ObjectReference || prop.objectReferenceValue == null) return false;
		// These all need testing
		// else if(prop.objectReferenceValue is AnimationClip) return true;
		// else if(prop.objectReferenceValue is AudioClip) return true;
		// else if(prop.objectReferenceValue is GameObject) return (PrefabUtility.GetPrefabParent(prop.objectReferenceValue) == null && PrefabUtility.GetPrefabObject(prop.objectReferenceValue) != null);			
		else if(prop.objectReferenceValue is ScriptableObject) return true;
		// else if(prop.objectReferenceValue is TextAsset) return true;
		// else if(prop.objectReferenceValue is Texture) return true;
		// else if(prop.objectReferenceValue is Texture2D) return true;
		else return false;
	}

// Texture2D.CreateExternalTexture()
	public static void UnSetAsSubAsset<T>(ref T asset) where T : Object {
		var isSubAsset = AssetDatabase.IsSubAsset(asset);
		if(!isSubAsset) return;

		// This returns the parent path, not the child path (since it has none)
		var parentAssetPath = AssetDatabase.GetAssetPath(asset);
		var newAssetPath = PathX.GetFullPathWithNewFileName(parentAssetPath, asset.name);

		// Clone the asset since you can't add assets that already have a file.
		Object objectCopy = Object.Instantiate(asset);
        objectCopy.name = asset.name;
        AssetDatabase.CreateAsset(objectCopy, newAssetPath);
		UnityEngine.Object.DestroyImmediate(asset, true);
        AssetDatabase.ImportAsset(newAssetPath);
		AssetDatabase.SaveAssets ();
		asset = objectCopy as T;
	}
	public static void SetAsSubAsset<T>(ref T asset, Object parentAsset) where T : Object {
		Debug.Assert(parentAsset != null);
		// Clone the asset since you can't add assets that already have a file.
		Object objectCopy = Object.Instantiate(asset);
        objectCopy.name = asset.name;

		var assetPath = AssetDatabase.GetAssetPath(asset);
		var parentAssetPath = AssetDatabase.GetAssetPath(parentAsset);
        AssetDatabase.AddObjectToAsset(objectCopy, parentAssetPath);
        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.ImportAsset(parentAssetPath);
		AssetDatabase.SaveAssets ();
		asset = objectCopy as T;
	}

	public static List<T> GetSubObjectsOfType<T>(Object asset) where T : Object{
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
        List<T> ofType = new List<T>();
        foreach(Object o in objs){
            if(o is T){
                ofType.Add(o as T);
            }
        }
        return ofType;
    }
 
    public static List<ScriptableObject> GetSubObjectsOfTypeAsScriptableObjects<T>(ScriptableObject asset) where T : ScriptableObject{
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
        List<ScriptableObject> ofType = new List<ScriptableObject>();
        foreach(Object o in objs){
            if(o is T){
                ofType.Add(o as ScriptableObject);
            }
        }
        return ofType;
    }
 
    public static List<T2> GetSubObjectsOfTypeAsType<T1, T2> (Object asset) where T1 : Object where T2 : T1{
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
        List<T2> ofType = new List<T2>();
        foreach(Object o in objs){
            if(o is T1){
                ofType.Add(o as T2);
            }
        }
        return ofType;
    }



	
	// Loads subassets using ::
	// FROM https://answers.unity.com/questions/1377941/getassetpath-returning-incomplete-path-for-default.html
	// Allows finding default assets
	// Eg: AssetDatabaseHelper.LoadAssetFromUniqueAssetPath<Mesh> ( "Library/unity default resources::Cube");
	public static T LoadAssetFromUniqueAssetPath<T>(string aAssetPath) where T : UnityEngine.Object {
		if (aAssetPath.Contains("::")) {
			string[] parts = aAssetPath.Split(new string[] { "::" },System.StringSplitOptions.RemoveEmptyEntries);
			aAssetPath = parts[0];
			if (parts.Length > 1) {
				string assetName = parts[1];
				System.Type t = typeof(T);
				var assets = AssetDatabase.LoadAllAssetsAtPath(aAssetPath)
				.Where(i => t.IsAssignableFrom(i.GetType())).Cast<T>();
				var obj = assets.Where(i => i.name == assetName).FirstOrDefault();
				if (obj == null) {
					int id;
					if (int.TryParse(parts[1], out id))
						obj = assets.Where(i => i.GetInstanceID() == id).FirstOrDefault();
					}
					if (obj != null)
						return obj;
				}
			}
		return AssetDatabase.LoadAssetAtPath<T>(aAssetPath);
	}
	public static string GetUniqueAssetPath(UnityEngine.Object aObj) {
		if (!aObj) return "";
		string path = AssetDatabase.GetAssetPath(aObj);
		if (!string.IsNullOrEmpty(aObj.name))
		path += "::" + aObj.name;
		else
		path += "::" + aObj.GetInstanceID();
		return path;
	}
#endif
}
