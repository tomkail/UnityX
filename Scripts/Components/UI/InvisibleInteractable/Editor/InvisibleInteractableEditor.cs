using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InvisibleInteractable))]
public class InvisibleInteractableEditor : BaseEditor<InvisibleInteractable> {
	public override void OnInspectorGUI () {}
}
