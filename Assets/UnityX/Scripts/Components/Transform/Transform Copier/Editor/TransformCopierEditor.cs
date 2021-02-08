using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(TransformCopier)), CanEditMultipleObjects]
public class TransformCopierEditor : BaseEditor<TransformCopier> {
	
	public override void OnEnable() {
		base.OnEnable();
	}
	
	void OnDisable() {
		Tools.hidden = false;
	}
    protected override void OnMultiEditSceneGUI () {
		Tools.hidden = data.enabled && (Application.isPlaying && data.playMode || !Application.isPlaying && data.editMode) && ((data.position && Tools.current == Tool.Move) || (data.rotation && Tools.current == Tool.Rotate));
		var color = Handles.color;
		Handles.color = Color.blue;
		if(data.position)
			Handles.SphereHandleCap(0, data.target.position, data.target.rotation, HandleUtility.GetHandleSize(data.target.position) * 0.25f, Event.current.type);
		if(data.rotation)
			Handles.ArrowHandleCap(0, data.target.position, data.target.rotation, HandleUtility.GetHandleSize(data.target.position) * 0.8f, Event.current.type);
		Handles.color = color;
	}
}