using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// Can be thought of as a singleton reference to a scriptable object, loaded/saved to EditorPrefs (PlayerPrefs at runtime) rather than serialized to the inspector.
// This is commonly useful for per-user settings files, especially for development debug settings.
public class SerializedScriptableSingleton<T> : ScriptableObject where T : ScriptableObject {
	static string _settingsPrefsKey;
	public static string settingsPrefsKey {
		get {
			ResetIfNewSession();
			if(_settingsPrefsKey == null)
				_settingsPrefsKey = $"{typeof(T).Name} Settings ({Application.productName})";
			return _settingsPrefsKey;
		}
		set {
			ResetIfNewSession();
			_settingsPrefsKey = value;
		}
	}

	static Action _onCreateOrLoad;
	public static event Action OnCreateOrLoad {
		add { ResetIfNewSession(); _onCreateOrLoad += value; }
		remove { ResetIfNewSession(); _onCreateOrLoad -= value; }
	}

	static T _Instance;
	public static T Instance {
		get {
			ResetIfNewSession();
			if(_Instance == null) LoadOrCreateAndSave();
			return _Instance;
		}
	}

	// Without domain reload these statics survive into the next play session: the instance would keep last session's runtime
	// changes and subscribers would accumulate. Generic classes can't use [RuntimeInitializeOnLoadMethod], so compare session ids.
	static int _session;
	static void ResetIfNewSession () {
		if(_session == PlaySession.id) return;
		_session = PlaySession.id;
		_Instance = null;
		_onCreateOrLoad = null;
		_settingsPrefsKey = null;
	}

	static T LoadOrCreateAndSave () {
		Load();
		if(_Instance == null) CreateAndSave();
		return _Instance;
	}

	public static void CreateAndSave () {
		ResetIfNewSession();
		_Instance = CreateInstance<T>();
		Save(_Instance);
        _onCreateOrLoad?.Invoke();
	}
	
	public static void Save () {
		ResetIfNewSession();
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
		_Instance = CreateInstance<T>();
		try {
			JsonUtility.FromJsonOverwrite(data, _Instance);
            if(_Instance != null) _onCreateOrLoad?.Invoke();
		} catch {
			Debug.LogError("Save Data was corrupt and could not be parsed. New data created. Old data was:\n"+data);
			CreateAndSave();
		}
    }

	public static void Delete () {
		ResetIfNewSession();
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