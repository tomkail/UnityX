using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RenderTextureCreator)), CanEditMultipleObjects]
public class RenderTextureCreatorEditor : Editor {
    SerializedProperty _renderTextureProperty;
	void OnEnable() {
		_renderTextureProperty = serializedObject.FindProperty("_renderTexture");
	}

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		if(serializedObject.FindProperty("fullScreen").boolValue) {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.Vector2IntField("Size", RenderTextureCreator.screenSize);
			EditorGUI.EndDisabledGroup();
		}

		serializedObject.ApplyModifiedProperties();
	}

	public override bool RequiresConstantRepaint() {
		return true;
	}

	public override bool HasPreviewGUI() {return true;}

    public override void OnPreviewGUI(Rect r, GUIStyle background) {
		if(Event.current.type == EventType.Repaint && _renderTextureProperty.objectReferenceValue != null) {
			EditorGUI.DrawPreviewTexture(r, _renderTextureProperty.objectReferenceValue as RenderTexture, null, ScaleMode.ScaleToFit);
		}
    }
}