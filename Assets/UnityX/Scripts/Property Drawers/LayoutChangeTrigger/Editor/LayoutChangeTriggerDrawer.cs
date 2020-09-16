using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Use attribute on a field that you use in a custom ILayoutController to drive the layout, so that
/// when it's changed in the editor, it automatically triggers a layout. For example:
/// 
///     [LayoutChangeTrigger]
///     public float myCustomMargin;
/// 
///     public void SetLayoutHorizontal() {
///         ... something that uses myCustomMargin ...
///     }
/// 
/// </summary>
[CustomPropertyDrawer(typeof(LayoutChangeTriggerAttribute))]
public class LayoutChangeTriggerDrawer : BaseAttributePropertyDrawer<LayoutChangeTriggerAttribute> {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginChangeCheck();
		EditorGUI.PropertyField(position, property, label);
		if (EditorGUI.EndChangeCheck()) {

			var component = property.serializedObject.targetObject as Component;
			if( !component ) return;

			var rectTransform = component.transform as RectTransform;
			if( !rectTransform ) return;

			LayoutRebuilder.MarkLayoutForRebuild (rectTransform);
		}
	}

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}
}