using UnityEditor;
using UnityEngine;

namespace FlexLayout.Editor {
    [CustomPropertyDrawer(typeof(LayoutElement))]
    public class LayoutElementDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);

            if (property.isExpanded) {
                EditorGUI.indentLevel++;
                position.y += EditorGUIUtility.singleLineHeight;

                SerializedProperty flexible = property.FindPropertyRelative("flexible");
                SerializedProperty fixedSize = property.FindPropertyRelative("fixedSize");
                SerializedProperty minSize = property.FindPropertyRelative("minSize");
                SerializedProperty maxSize = property.FindPropertyRelative("maxSize");

                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), flexible);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                if (flexible.boolValue) DrawMinMaxField(position, minSize, maxSize, new GUIContent("Size"));
                else EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), fixedSize);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public void DrawMinMaxField(Rect position, SerializedProperty min, SerializedProperty max, GUIContent label) {
            EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label);
            Rect minFieldPosition = new Rect(position.x+EditorGUIUtility.labelWidth, position.y, (position.width - EditorGUIUtility.labelWidth) / 2 - 5, EditorGUIUtility.singleLineHeight);
            Rect maxFieldPosition = new Rect(minFieldPosition.x + minFieldPosition.width + 5, position.y, minFieldPosition.width, EditorGUIUtility.singleLineHeight);

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 32;
            EditorGUI.BeginChangeCheck();
            float newMin = EditorGUI.FloatField(minFieldPosition, new GUIContent("Min"), min.floatValue);
            float newMax = EditorGUI.FloatField(maxFieldPosition, new GUIContent("Max"), max.floatValue);
            if (EditorGUI.EndChangeCheck()) {
                min.floatValue = newMin;
                max.floatValue = newMax;
            }
            EditorGUIUtility.labelWidth = labelWidth;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel = indentLevel;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3 - EditorGUIUtility.standardVerticalSpacing;
        }
    }
}