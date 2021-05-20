using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CommentComponent))]
public class CommentComponentEditor : BaseEditor<CommentComponent> {
	private bool editing = false;
	private string unsavedText;
	
	public override void OnInspectorGUI() {
		serializedObject.Update();
		if(editing) {
			GUIStyle wordWrapTextAreaStyle = EditorStyles.textArea;
//			wordWrapTextAreaStyle.wordWrap = true;
			unsavedText = EditorGUILayout.TextArea(unsavedText, wordWrapTextAreaStyle);
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Save")) {
				Save();
			}
			if(GUILayout.Button("Cancel")) {
				Cancel ();
			}
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty("messageType"));
			// data.messageType = ;
			GUILayout.EndHorizontal();
		} else {
            MessageType messageType  = MessageType.None;
            System.Enum.TryParse<MessageType>(data.messageType.ToString(), true, out messageType);
			EditorGUILayout.HelpBox(data.text, messageType);
			if(GUILayout.Button("Edit")) {
				Edit();
			}
		}
		serializedObject.ApplyModifiedProperties();
	}
	
	public void OnDisable () {
		if(editing && unsavedText != data.text) {
			if(EditorUtility.DisplayDialog("Save comment?", "New comment:\n"+unsavedText, "Save", "Cancel")) {
				Save();
			} else {
				Cancel();
			}
		}
	}
	
	private void Edit () {
		editing = true;
		unsavedText = data.text;
	}
	
	private void Save () {
		data.text = unsavedText;
		CompleteEdit();
	}
	
	private void Cancel () {
		CompleteEdit();
	}
	
	private void CompleteEdit () {
		unsavedText = "";
		editing = false;
		GUIUtility.hotControl = 0;
		GUIUtility.keyboardControl = 0;
	}
}
