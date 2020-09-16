using UnityEngine;

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
public class LayoutChangeTriggerAttribute : PropertyAttribute {
	public LayoutChangeTriggerAttribute() { }
}