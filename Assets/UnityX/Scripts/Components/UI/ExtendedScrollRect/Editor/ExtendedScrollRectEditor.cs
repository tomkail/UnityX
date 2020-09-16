using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;

namespace UnityEditor.UI {
	[CustomEditor(typeof(ExtendedScrollRect), true)]
	[CanEditMultipleObjects]
	public class ExtendedScrollRectEditor : ScrollRectEditor {
		
		SerializedProperty onScrollProperty;
		SerializedProperty onBeginDragProperty;
		SerializedProperty onEndDragProperty;
		SerializedProperty onDragProperty;
		
		protected override void OnEnable()
		{
			base.OnEnable();
			onScrollProperty = serializedObject.FindProperty("onScroll");
			onBeginDragProperty = serializedObject.FindProperty("onBeginDrag");
			onEndDragProperty = serializedObject.FindProperty("onEndDrag");
			onDragProperty = serializedObject.FindProperty("onDrag");
		}
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.Space();
			
			serializedObject.Update();
			EditorGUILayout.PropertyField(onScrollProperty);
			EditorGUILayout.PropertyField(onBeginDragProperty);
			EditorGUILayout.PropertyField(onEndDragProperty);
			EditorGUILayout.PropertyField(onDragProperty);
			serializedObject.ApplyModifiedProperties();
		}
	}
}