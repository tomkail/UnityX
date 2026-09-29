using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (Vector3ToggleAttribute))]
public class Vector3ToggleDrawer : BaseVectorToggleDrawer<Vector3ToggleAttribute> {
	protected override bool IsSupported (SerializedProperty property) => property.propertyType == SerializedPropertyType.Vector3;
	protected override float[] GetAxes(SerializedProperty property) { var v = property.vector3Value; return new[] { v.x, v.y, v.z }; }
	protected override void SetAxes(SerializedProperty property, float[] axes) => property.vector3Value = new Vector3(axes[0], axes[1], axes[2]);
}