using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnforceDecendentGameObjectProperties)), CanEditMultipleObjects]
public class EnforceDecendentGameObjectPropertiesEditor : BaseEditor<EnforceDecendentGameObjectProperties> {

	// Enforcement runs on select (OnEnable) and every inspector repaint (OnInspectorGUI) on purpose: it's how an
	// edit to THIS object's tag/layer/isStatic (made via the inspector header, which isn't a serialized field so
	// OnValidate wouldn't catch it) propagates to descendants live. It's cheap despite running per-repaint because
	// EnforceProperties only *writes* a descendant property when it actually differs (see EnforceOnOther) — there's
	// no dirtying storm. Kept intentionally; don't switch it to a change-only trigger (it would miss cases).
	public override void OnEnable () {
		base.OnEnable ();
		data.EnforceProperties();
	}

	public override void OnInspectorGUI () {
		base.OnInspectorGUI ();
		data.EnforceProperties();
	}
}