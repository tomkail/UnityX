
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#nullable enable

/// <summary>
/// Mono singleton Class. Extend this class to make singleton component.
/// Example: 
/// <code>
/// public class Foo : MonoSingleton<Foo>
/// </code>. To get the instance of Foo class, use <code>Foo.instance</code>
/// Override <code>Init()</code> method instead of using <code>Awake()</code>
/// from this class.
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> {
	public static bool isInitialized {
		get {
			return _instance == null;
		}
	}
	static T? _instance = null;
	public static T Instance { 
		get {
			// Instance requiered for the first time, we look for it
			if( _instance == null ) {
				_instance = (T)Object.FindObjectOfType(typeof(T));
			}
			return _instance;
		}
	}

	// THIS DOESN'T RUN!
	// #if UNITY_EDITOR
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    // static void CompileReset() {
	//     _instance = null;
    // }
    // #endif
	
	// If no other monobehaviour request the instance in an awake function
	// executing before this one, no need to search the object.
	protected virtual void Awake() {
		if (_instance == null) {
			_instance = this as T;
		} else if (_instance != this) {
			Debug.LogError ("Another instance of " + GetType () + " already exists!");
			return;
		}
	}
}