using UnityEditor;
using UnityEngine;

namespace FlexLayout.Editor {
    [CustomPropertyDrawer(typeof(Container))]
    public class ContainerDrawer : LayoutElementDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            base.OnGUI(position, property, label);

            if (property.isExpanded) {
                EditorGUI.indentLevel++;

                SerializedProperty paddingMin = property.FindPropertyRelative("paddingMin");
                SerializedProperty paddingMax = property.FindPropertyRelative("paddingMax");
                SerializedProperty spacing = property.FindPropertyRelative("spacing");
                SerializedProperty surplusMode = property.FindPropertyRelative("surplusMode");
                SerializedProperty surplusOffsetPivot = property.FindPropertyRelative("surplusOffsetPivot");
                SerializedProperty surplusSpacePaddingRatio = property.FindPropertyRelative("surplusSpacePaddingRatio");
                SerializedProperty reversed = property.FindPropertyRelative("reversed");

                position.y += base.GetPropertyHeight(property, label);

                DrawMinMaxField(position, paddingMin, paddingMax, new GUIContent("Padding"));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), spacing);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), surplusMode);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                if (surplusMode.enumValueIndex == (int)Container.SurplusMode.Offset) {
                    EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), surplusOffsetPivot, new GUIContent("Surplus Offset Pivot"));
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                else if (surplusMode.enumValueIndex == (int)Container.SurplusMode.Space) {
                    EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), surplusSpacePaddingRatio, new GUIContent("Surplus Space Padding Ratio"));
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), reversed);

                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!property.isExpanded) return base.GetPropertyHeight(property, label);
            float baseHeight = base.GetPropertyHeight(property, label);
            return baseHeight + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 5;
        }
    }
}