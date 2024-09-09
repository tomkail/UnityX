using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Forces some of this gameobject's properties on all children. 
// If a child contains this component then none of this gameobject's properties are applied to it or it's descendents
[DisallowMultipleComponent]
public class EnforceDecendentGameObjectProperties : MonoBehaviour {
	// Set as false so adding the component doesn't trigger them all right away!
	public bool enforceTag = false;
	public bool enforceLayer = false;
	public bool enforceIsStatic = false;

	public static void EnforcePropertiesAll () {
		EnforceDecendentGameObjectProperties[] all = FindObjectsByType<EnforceDecendentGameObjectProperties>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach(var enforce in all) enforce.EnforceProperties();
	}

	public void EnforceProperties () {
		RecurseThroughChildren(transform);
	}
	
	void RecurseThroughChildren (Transform transform) {
		EnforceOnOther(transform.gameObject);
		foreach(Transform child in transform) {
			if(child.GetComponent<EnforceDecendentGameObjectProperties>() != null) 
				continue;
			RecurseThroughChildren(child);
		}
	}
	void EnforceOnOther (GameObject other) {
		if(enforceTag && other.tag != gameObject.tag) other.tag = gameObject.tag;
		if(enforceLayer && other.layer != gameObject.layer) other.layer = gameObject.layer;
		if(enforceIsStatic && other.isStatic != gameObject.isStatic) other.isStatic = gameObject.isStatic;
	}
	void OnTransformChildrenChanged () {
		EnforceProperties();
	}
	/*
	#if UNITY_EDITOR
	[UnityEditor.Callbacks.DidReloadScripts]
 	private static void OnScriptsReloaded() {
		EnforcePropertiesAll();
 	}
 	#endif
 	*/
}