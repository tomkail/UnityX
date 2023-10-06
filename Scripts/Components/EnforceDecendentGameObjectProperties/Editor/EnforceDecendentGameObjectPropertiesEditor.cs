using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnforceDecendentGameObjectProperties)), CanEditMultipleObjects]
public class EnforceDecendentGameObjectPropertiesEditor : BaseEditor<EnforceDecendentGameObjectProperties> {

	public override void OnEnable () {
		base.OnEnable ();
		data.EnforceProperties();
	}

	public override void OnInspectorGUI () {
		base.OnInspectorGUI ();
		data.EnforceProperties();
	}
}