using System;
using UnityEngine;
using UnityEditor;

// Can be thought of as a singleton reference a scriptable object, loaded and saved to playerprefs rather than serialized to the inspector.
// This is commonly useful for per-user settings files, especially for development debug settings.
public class SerializedScriptableSingleton<T> : ScriptableObject where T : ScriptableObject {
	static string _settingsPrefsKey;
	public static string settingsPrefsKey {
		get {
			if(_settingsPrefsKey == null)
				_settingsPrefsKey = string.Format("{0} Settings ({1})", typeof(T).Name, Application.productName);
			return _settingsPrefsKey;
		} set {
			_settingsPrefsKey = value;
		}
	}
	public static event Action OnCreateOrLoad;

	static T _Instance;
	public static T Instance {
		get {
			if(_Instance == null) LoadOrCreateAndSave();
			return _Instance;
		}
	}

	static T LoadOrCreateAndSave () {
		Load();
		if(_Instance == null) CreateAndSave();
		return _Instance;
	}

	public static void CreateAndSave () {
		_Instance = ScriptableObject.CreateInstance<T>();
		Save(_Instance);
        if(OnCreateOrLoad != null) OnCreateOrLoad();
	}
	
	public static void Save () {
		Save(_Instance);
    }

	public static void Save (T settings) {
		string data = JsonUtility.ToJson(settings);
		if(!Application.isEditor) PlayerPrefs.SetString(settingsPrefsKey, data);
		#if UNITY_EDITOR
		else EditorPrefs.SetString(settingsPrefsKey, data);
		#endif
		
    }

	static void Load () {
		string data = null;
		if(!Application.isEditor) {
			if(!PlayerPrefs.HasKey(settingsPrefsKey)) return;
			data = PlayerPrefs.GetString(settingsPrefsKey);
		}
		#if UNITY_EDITOR
		else {
			if(!EditorPrefs.HasKey(settingsPrefsKey)) return;
			data = EditorPrefs.GetString(settingsPrefsKey);
		}
		#endif
		_Instance = ScriptableObject.CreateInstance<T>();
		try {
			JsonUtility.FromJsonOverwrite(data, _Instance);
            if(_Instance != null) if(OnCreateOrLoad != null) OnCreateOrLoad();
		} catch {
			Debug.LogError("Save Data was corrupt and could not be parsed. New data created. Old data was:\n"+data);
			CreateAndSave();
		}
    }

	public static void Delete () {
		if(!Application.isEditor) PlayerPrefs.DeleteKey(settingsPrefsKey);
		#if UNITY_EDITOR
		else EditorPrefs.DeleteKey(settingsPrefsKey);
		#endif
		if(Application.isPlaying)
			Destroy(_Instance);
		else 
			DestroyImmediate(_Instance);
	}
}