using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TransformerEditorWindow : EditorWindow {
	private const string windowTitle = "Transformer Window";
	Vector3 positionOffset;

	[MenuItem("Tools/"+windowTitle, false, 0)]
	public static TransformerEditorWindow GetWindow () {
		return GetWindow<TransformerEditorWindow>(windowTitle, true);
	}
	private void OnGUI () {
		EditorGUILayout.BeginHorizontal();
		positionOffset = EditorGUILayout.Vector3Field("Translation", positionOffset);
		if(GUILayout.Button("Translate")) {
			Undo.RecordObjects(Selection.transforms, "Translated selection");
			foreach(var transform in Selection.transforms) {
				transform.Translate(positionOffset, Space.World);
			}
		}
		EditorGUILayout.EndHorizontal();
	}
}