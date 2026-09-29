using UnityEngine;
using UnityEditor;

public class SmartEditorWindow : EditorWindow {
	static bool _subscribed;
	public static bool subscribed {get{return _subscribed;}}

	static bool _visible;
	public static bool visible {get{return _visible;}}
	
	
	// Called on window create/recompile
	protected SmartEditorWindow () {
		TrySubscribe();
	}
	// Called on window create/recompile
	void OnEnable () {
		TrySubscribe();
	}

	void OnDisable () {
		TryUnsubscribe();
	}

	private void OnBecameVisible() {
		_visible = true;
	}
 
	private void OnBecameInvisible() {
		_visible = false;
	}

	void TrySubscribe () {
		if(_subscribed) return;
		_subscribed = true;
		Subscribe();
	}

	void TryUnsubscribe () {
		if(!_subscribed) return;
		_subscribed = false;
		Unsubscribe();
	}

	protected virtual void Subscribe () {}

	protected virtual void Unsubscribe () {}
}