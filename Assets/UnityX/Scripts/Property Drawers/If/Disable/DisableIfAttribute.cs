using UnityEngine;

// Allows you to create inspectable nullable types, when given the path to a serializable bool storing if the variable is null.
/*
[SerializeField, HideInInspector]
bool _distanceFromFloorSet;
[VisibleIf("_distanceFromFloorSet")]
public float _distanceFromFloor;
*/

/// <summary>
/// Disables the field if the target property is true
/// </summary>
public class DisableIfAttribute : BaseIfAttribute {
	public DisableIfAttribute (string relativePropertyPath) : base (relativePropertyPath) {}
	public DisableIfAttribute (string relativePropertyPath, bool invert) : base (relativePropertyPath, invert) {}
	public DisableIfAttribute (Options option) : base (option) {}
	public DisableIfAttribute (Options option, bool invert) : base (option, invert) {}
}