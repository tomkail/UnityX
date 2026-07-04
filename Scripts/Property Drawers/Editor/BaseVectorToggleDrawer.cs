using UnityEditor;
using UnityEngine;

// Shared base for the per-axis 0/1 toggle drawers (Vector2Toggle / Vector3Toggle):
// draws N ToggleLeft controls after the prefix label and writes 0/1 back per axis.
public abstract class BaseVectorToggleDrawer<TAttribute> : BaseAttributePropertyDrawer<TAttribute> where TAttribute : PropertyAttribute {
	static readonly string[] AxisLabels = { "X", "Y", "Z", "W" };

	protected abstract float[] GetAxes(SerializedProperty property);
	protected abstract void SetAxes(SerializedProperty property, float[] axes);

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		position = EditorGUI.PrefixLabel(position, label);
		var axes = GetAxes(property);
		var oneThird = Mathf.FloorToInt(position.width / 3);
		for (int i = 0; i < axes.Length; i++) {
			var rect = new Rect(position.x + i * oneThird, position.y, oneThird, position.height);
			axes[i] = EditorGUI.ToggleLeft(rect, AxisLabels[i], axes[i] == 1) ? 1 : 0;
		}
		SetAxes(property, axes);
	}
}
