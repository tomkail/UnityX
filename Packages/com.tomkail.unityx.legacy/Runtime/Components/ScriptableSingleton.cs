using UnityEngine;

public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>{
	public static T FindInResources(string assetName) {
		return Resources.Load<T>(assetName);
	}
	static T _Instance;

	// Without domain reload a default instance made by CreateInstance (and its runtime changes) would survive into the
	// next play session and hide an asset added since. Generic classes can't use [RuntimeInitializeOnLoadMethod], so
	// compare session ids; a Resources asset is simply loaded again.
	static int _session;
	static void ResetIfNewSession () {
		if(_session == PlaySession.id) return;
		_session = PlaySession.id;
		_Instance = null;
	}

	public static T Instance {
		get {
			ResetIfNewSession();
			if(_Instance == null) _Instance = FindInResources(typeof(T).Name);
// #if UNITY_EDITOR
// 			if(_Instance == null) _Instance = AssetDatabaseX.LoadAssetOfType<T>();
// #else
			if(_Instance == null){
				Debug.LogWarning("No instance of " + typeof(T).Name + " found, using default values");
				_Instance = CreateInstance<T>();
			}
// #endif
            return _Instance;
		}
	}

	// Like Instance, but returns null instead of creating a default (and logging a warning)
	// when no asset exists — useful during OnValidate / asset import when the singleton may not be loaded yet.
	public static T InstanceIfExists {
		get {
			ResetIfNewSession();
			if(_Instance == null) _Instance = FindInResources(typeof(T).Name);
			return _Instance;
		}
	}

	// Should use OnEnable and OnDisable rather than OnDestroy
	// http://answers.unity3d.com/questions/639852/does-unity-call-destroy-on-a-scriptableobject-that.html
	protected virtual void OnEnable() {
		ResetIfNewSession();
		if( _Instance == null )
			_Instance = (T)this;
	}

	protected virtual void OnDisable () {
		if( _Instance == this )
			_Instance = null;
	}
}