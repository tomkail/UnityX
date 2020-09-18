#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityX.Geometry {

	[CustomPropertyDrawer(typeof (Polygon))]
	public class PolygonPropertyDrawer : PropertyDrawer {
		private bool initialized = false;
		ReorderableList list;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
 			if (!initialized) Initialize(property);
			EditorGUI.BeginProperty (position, label, property);
			var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, property.displayName, true);
			if(property.isExpanded) {
				var listRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, list.GetHeight());
				list.DoList(listRect);
			}
			SerializedPropertyX.AddCopyPasteMenu(position, property);
			EditorGUI.EndProperty ();
		}

		private void Initialize(SerializedProperty property) {
			initialized = true;
			var listProperty = property.FindPropertyRelative("_vertices");
            list = new ReorderableList(property.serializedObject, listProperty, true, true, true, true);
			list.drawHeaderCallback = (Rect rect) => {
				EditorGUI.LabelField(rect, list.serializedProperty.arraySize +" "+ list.serializedProperty.displayName);
			};
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
				EditorGUI.PropertyField(rect, listProperty.GetArrayElementAtIndex(index), new GUIContent("Index "+index));
            };
            list.elementHeightCallback = DefaultElementHeightCallback(list);
			list.onCanRemoveCallback = (ReorderableList list) => {
				return list.serializedProperty.arraySize > 3;
			};

			ReorderableList.ElementHeightCallbackDelegate DefaultElementHeightCallback(ReorderableList list, float extraHeight = 0) {
				return (int index) => {
					if(list.serializedProperty == null) {
						return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + extraHeight;
					} else {
						var element = list.serializedProperty.GetArrayElementAtIndex(index);
						return EditorGUI.GetPropertyHeight(element) + EditorGUIUtility.standardVerticalSpacing + extraHeight;
					}
				};
			}
		}

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			if (!initialized) Initialize(property);
			var height = 0f;
			height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			if(property.isExpanded) height += list.GetHeight();
			return height;
		}
	}
}

#endif