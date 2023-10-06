using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;

namespace UnityEditor.UI {
	[CustomEditor(typeof(ExtendedScrollRect), true)]
	[CanEditMultipleObjects]
	public class ExtendedScrollRectEditor : ScrollRectEditor {
		SerializedProperty parentScrollRectProp;
		GUIContent parentScrollRectGUIContent = new GUIContent("Reroute Scroll Events:", "Stop this scroll view from handling scroll events and route them to the parent instead.");
		
		SerializedProperty onScrollProperty;
		SerializedProperty onBeginDragProperty;
		SerializedProperty onEndDragProperty;
		SerializedProperty onDragProperty;
				
		protected override void OnEnable() {
			base.OnEnable();
			parentScrollRectProp = serializedObject.FindProperty("routeScrollEventsToParent");

			onScrollProperty = serializedObject.FindProperty("onScroll");
			onBeginDragProperty = serializedObject.FindProperty("onBeginDrag");
			onEndDragProperty = serializedObject.FindProperty("onEndDrag");
			onDragProperty = serializedObject.FindProperty("onDrag");
		}
		
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.Space();
			
			serializedObject.Update();
       		EditorGUILayout.PropertyField(parentScrollRectProp, parentScrollRectGUIContent);
			EditorGUILayout.PropertyField(onScrollProperty);
			EditorGUILayout.PropertyField(onBeginDragProperty);
			EditorGUILayout.PropertyField(onEndDragProperty);
			EditorGUILayout.PropertyField(onDragProperty);
			serializedObject.ApplyModifiedProperties();
		}
	}
}