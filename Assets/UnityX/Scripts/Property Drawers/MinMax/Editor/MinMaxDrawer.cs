using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer (typeof (MinMaxAttribute))]
public class MinMaxDrawer : BaseAttributePropertyDrawer<MinMaxAttribute> {
	
	public override void OnGUI (Rect rect, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(rect, property, label);
			return;
		}

		EditorGUI.BeginChangeCheck();
		Vector2 v2 = property.vector2Value;
        
		EditorGUI.MinMaxSlider(new Rect(rect.x, rect.y, rect.width, 12), new GUIContent(label), ref v2.x, ref v2.y, attribute.min,  attribute.max);
		if(EditorGUIUtility.wideMode) {
			rect.y += EditorGUIUtility.singleLineHeight;
		}
        v2 = EditorGUI.Vector2Field (new Rect (rect.x, rect.y, rect.width-10, rect.height+8), new GUIContent(" "), v2);
        
        if (EditorGUI.EndChangeCheck()) {
			if(attribute.step > 0) {
				v2.x = Mathf.Round(v2.x/attribute.step)*attribute.step;
				v2.y = Mathf.Round(v2.y/attribute.step)*attribute.step;
            }
			property.vector2Value = new Vector2 (Mathf.Clamp(v2.x, attribute.min, v2.y), Mathf.Clamp(v2.y, v2.x, attribute.max));
        }
    }

	protected override bool IsSupported(SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.Vector2;
	}

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label) + GetHeight(property);
    }

    private static float GetHeight(SerializedProperty property) {
		if(EditorGUIUtility.wideMode) {
			return EditorGUIUtility.singleLineHeight * 1 + EditorGUIUtility.standardVerticalSpacing;
		} else {
			return EditorGUIUtility.singleLineHeight * 1;
		}
    }
}