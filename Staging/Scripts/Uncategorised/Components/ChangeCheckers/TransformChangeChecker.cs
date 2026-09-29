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
		if(!Application.isPlaying && !useInEditMode) {
			enabled = false;
			return;
		}
		#endif

		// Per-field checks stay inline to preserve the type-specific != (Transform reference; Unity's
		// approximate Vector3/Quaternion comparison); FireChange handles the shared event/SendMessage tail.
		if(transform.parent != lastParent) {
			lastParent = transform.parent;
			FireChange(OnParentChanged, "Parent");
		}
		if(transform.rotation != lastTransform.rotation) {
			lastTransform.rotation = transform.rotation;
			FireChange(OnRotationChanged, "Rotation");
		}
		if(transform.localScale != lastTransform.localScale) {
			lastTransform.localScale = transform.localScale;
			FireChange(OnScaleChanged, "Scale");
		}
		if(transform.position != lastTransform.position) {
			lastTransform.position = transform.position;
			FireChange(OnPositionChanged, "Position");
		}
	}

	void FireChange (TransformDelegate specificChange, string suffix) {
		specificChange?.Invoke();
		OnTransformChanged?.Invoke();
		gameObject.BetterSendMessage("OnChangedTransform");
		gameObject.BetterSendMessage("OnChanged" + suffix);
	}
}