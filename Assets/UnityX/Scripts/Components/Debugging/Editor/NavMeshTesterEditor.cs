using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEditor.AI;

[CustomEditor(typeof(NavMeshTester))]
public class NavMeshTesterEditor : BaseEditor<NavMeshTester>{
	public override void OnInspectorGUI () {

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("currentTool"));
		var toolChanged = EditorGUI.EndChangeCheck();

		if( toolChanged)
			serializedObject.ApplyModifiedProperties();

		if( data.currentTool == NavMeshTester.Tool.Raycast || data.currentTool == NavMeshTester.Tool.PathFind ) {
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();
		}
	}
}