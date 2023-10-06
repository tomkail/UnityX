using System.Linq;  
using UnityEngine;

public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>{
	private static T _Instance;
	public static T Instance {
		get {
			if(_Instance == null) _Instance = Resources.Load<T>(typeof(T).Name);
#if UNITY_EDITOR
			if(_Instance == null) _Instance = AssetDatabaseX.LoadAssetOfType<T>();

            //			_instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
            //			if(_Instance == null) _Instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
#else
			if(_Instance == null){
				Debug.LogWarning("No instance of " + typeof(T).Name + " found, using default values");
				_Instance = ScriptableObject.CreateInstance<T>();
			}
#endif
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