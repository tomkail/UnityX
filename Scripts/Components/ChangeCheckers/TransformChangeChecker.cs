using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
#endif

[ExecuteInEditMode, DisallowMultipleComponent]
public class TransformChangeChecker : MonoBehaviour {
	[SerializeField, HideInInspector]
	Transform lastParent;
	[SerializeField, HideInInspector]
	SerializableTransform lastTransform;
	public bool useInPlayMode = true;
	#if UNITY_EDITOR
	public bool useInEditMode = true;
	#endif

	public delegate void TransformDelegate ();
	public event TransformDelegate OnTransformChanged;
	public event TransformDelegate OnParentChanged;
	public event TransformDelegate OnPositionChanged;
	public event TransformDelegate OnRotationChanged;
	public event TransformDelegate OnScaleChanged;

	public void Clear () {
		lastParent = transform.parent;
		lastTransform.rotation = transform.rotation;
		lastTransform.localScale = transform.localScale;
		lastTransform.position = transform.position;
	}

	void Update () {
		if(Application.isPlaying && !useInPlayMode) {
			enabled = false;
			return;
		}
		#if UNITY_EDITOR
		if(!Application.isPlaying && !useInEditMode) return;
		#endif

		if(transform.parent != lastParent) {
			lastParent = transform.parent;
			if(OnParentChanged != null) OnParentChanged();
			if(OnTransformChanged != null) OnTransformChanged();
			gameObject.BetterSendMessage("OnChangedTransform");
			gameObject.BetterSendMessage("OnChangedParent");
		}

		if(transform.rotation != lastTransform.rotation) {
			lastTransform.rotation = transform.rotation;
			if(OnRotationChanged != null) OnRotationChanged();
			if(OnTransformChanged != null) OnTransformChanged();
			gameObject.BetterSendMessage("OnChangedTransform");
			gameObject.BetterSendMessage("OnChangedRotation");
		}
		if(transform.localScale != lastTransform.localScale) {
			lastTransform.localScale = transform.localScale;
			if(OnScaleChanged != null) OnScaleChanged();
			if(OnTransformChanged != null) OnTransformChanged();
			gameObject.BetterSendMessage("OnChangedTransform");
			gameObject.BetterSendMessage("OnChangedScale");
		}

		if(transform.position != lastTransform.position) {
			lastTransform.position = transform.position;
			if(OnPositionChanged != null) OnPositionChanged();
			if(OnTransformChanged != null) OnTransformChanged();
			gameObject.BetterSendMessage("OnChangedTransform");
			gameObject.BetterSendMessage("OnChangedPosition");
		}
	}
}