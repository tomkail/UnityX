using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (Vector2ToggleAttribute))]
public class Vector2ToggleDrawer : BaseVectorToggleDrawer<Vector2ToggleAttribute> {
	protected override bool IsSupported (SerializedProperty property) => property.propertyType == SerializedPropertyType.Vector2;
	protected override float[] GetAxes(SerializedProperty property) { var v = property.vector2Value; return new[] { v.x, v.y }; }
	protected override void SetAxes(SerializedProperty property, float[] axes) => property.vector2Value = new Vector2(axes[0], axes[1]);
}