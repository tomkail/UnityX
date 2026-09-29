using UnityEngine;

public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>{
	public static T FindInResources(string assetName) {
		return Resources.Load<T>(assetName);
	}
	static T _Instance;
	public static T Instance {
		get {
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
			if(_Instance == null) _Instance = FindInResources(typeof(T).Name);
			return _Instance;
		}
	}

	// Should use OnEnable and OnDisable rather than OnDestroy
	// http://answers.unity3d.com/questions/639852/does-unity-call-destroy-on-a-scriptableobject-that.html
	protected virtual void OnEnable() {
		if( _Instance == null )
			_Instance = (T)this;
	}

	protected virtual void OnDisable () {
		if( _Instance == this )
			_Instance = null;
	}
}