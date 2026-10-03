using UnityEngine;
using UnityEditor;

public class SmartEditorWindow : EditorWindow {
	// Per window: these used to be static, so every window (of every subclass) shared one flag and only the first subscribed.
	bool _subscribed;
	public bool subscribed {get{return _subscribed;}}

	bool _visible;
	public bool visible {get{return _visible;}}
	
	// Called on window create/recompile. Not subscribing from the constructor: it runs during deserialization,
	// where Unity APIs aren't allowed.
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