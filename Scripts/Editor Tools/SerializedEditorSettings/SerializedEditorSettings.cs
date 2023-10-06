using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Can be thought of as a singleton reference for editor settings, saved via editor prefs.   
public class SerializedEditorSettings<T> where T : class, new() {
    #if UNITY_EDITOR
	static string _settingsEditorPrefsKey;
	static string settingsEditorPrefsKey {
		get {
			if(_settingsEditorPrefsKey == null)
				_settingsEditorPrefsKey = string.Format("{0} Settings ({1})", typeof(T).Name, Application.productName);
			return _settingsEditorPrefsKey;
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
		_Instance = new T();
		Save(_Instance);
        if(OnCreateOrLoad != null) OnCreateOrLoad();
	}
	
	public static void Save () {
		Save(_Instance);
    }

	public static void Save (T settings) {
        
		string data = JsonUtility.ToJson(settings);
		EditorPrefs.SetString(settingsEditorPrefsKey, data);
    }

	static void Load () {
		if(!EditorPrefs.HasKey(settingsEditorPrefsKey)) return;
		string data = EditorPrefs.GetString(settingsEditorPrefsKey);
		try {
			_Instance = JsonUtility.FromJson<T>(data);
            if(_Instance != null) if(OnCreateOrLoad != null) OnCreateOrLoad();
		} catch {
			Debug.LogError("Save Data was corrupt and could not be parsed. New data created. Old data was:\n"+data);
			CreateAndSave();
		}
    }
    #endif
}
