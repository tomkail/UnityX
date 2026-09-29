using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

public static class ReorderableListX {
	public static ReorderableList.HeaderCallbackDelegate DefaultDrawHeaderCallback(ReorderableList list) {
		return (Rect rect) => {
			
			if(list.serializedProperty != null) EditorGUI.LabelField(rect, list.serializedProperty.displayName);
			if(list.list != null) EditorGUI.LabelField(rect, "List of "+list.list.GetType().Name);
		};
	}

	public static ReorderableList.ElementHeightCallbackDelegate DefaultElementHeightCallback(ReorderableList list, float extraHeight = 0) {
		return (int index) => {
			if(list.serializedProperty == null) {
				return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + extraHeight;
			} else {
				var element = list.serializedProperty.GetArrayElementAtIndex(index);
				return EditorGUI.GetPropertyHeight(element) + EditorGUIUtility.standardVerticalSpacing + extraHeight;
			}
		};
	}
	
	public static ReorderableList.ElementCallbackDelegate DefaultDrawElementCallback(ReorderableList list, System.Func<SerializedProperty, GUIContent> GetName = null) {
		return (Rect rect, int index, bool isActive, bool isFocused) => {
			var element = list.serializedProperty.GetArrayElementAtIndex(index);
			rect.x += ReorderableList.Defaults.dragHandleWidth;
			rect.width -= ReorderableList.Defaults.dragHandleWidth;
			if(GetName != null) {
				var label = GetName(element);
				EditorGUI.PropertyField(rect, element, label, true);
			} else {
				EditorGUI.PropertyField(rect, element, true);
			}
   	 	};
	}
}
