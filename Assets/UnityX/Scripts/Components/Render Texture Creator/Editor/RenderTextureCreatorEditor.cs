using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RenderTextureCreator), true), CanEditMultipleObjects]
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

	public override bool RequiresConstantRepaint() => true;
	public override bool HasPreviewGUI() => true;

    public override void OnPreviewGUI(Rect r, GUIStyle background) {
		if(Event.current.type == EventType.Repaint && _renderTextureProperty.objectReferenceValue != null) {
			EditorGUI.DrawTextureTransparent(r, _renderTextureProperty.objectReferenceValue as RenderTexture, ScaleMode.ScaleToFit);
		}
    }
    
    public override void OnPreviewSettings() {
	    var rt = _renderTextureProperty.objectReferenceValue as RenderTexture;
	    EditorGUI.BeginDisabledGroup(true);
	    EditorGUILayout.LabelField(new GUIContent("Size"), GUILayout.Width(40));
	    EditorGUILayout.Vector2IntField(GUIContent.none, new Vector2Int(rt.width, rt.height), GUILayout.Width(120));
	    EditorGUI.EndDisabledGroup();
    }
}