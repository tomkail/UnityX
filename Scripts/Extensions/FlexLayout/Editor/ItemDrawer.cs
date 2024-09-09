using UnityEditor;
using UnityEngine;

namespace FlexLayout.Editor {
    [CustomPropertyDrawer(typeof(Item))]
    public class ItemDrawer : LayoutElementDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            base.OnGUI(position, property, label); // Draw LayoutElement properties

            if (property.isExpanded) {
                EditorGUI.indentLevel++;

                SerializedProperty weight = property.FindPropertyRelative("weight");
                SerializedProperty marginMin = property.FindPropertyRelative("marginMin");
                SerializedProperty marginMax = property.FindPropertyRelative("marginMax");

                position.y += base.GetPropertyHeight(property, label);
                
                DrawMinMaxField(position, marginMin, marginMax, new GUIContent("Margin"));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), weight, new GUIContent("Weight"));

                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!property.isExpanded) return base.GetPropertyHeight(property, label);

            return base.GetPropertyHeight(property, label) + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
        }
    }
}