using UnityEngine;

// Allows you to create inspectable nullable types, when given the path to a serializable bool storing if the variable is null.
/*
[SerializeField, HideInInspector]
bool _distanceFromFloorSet;
[VisibleIf("_distanceFromFloorSet")]
public float _distanceFromFloor;
*/
public class VisibleIfAttribute : BaseIfAttribute {
	public bool disable;
	public VisibleIfAttribute (string relativePropertyPath) : base (relativePropertyPath) {}
	public VisibleIfAttribute (string relativePropertyPath, bool invert) : base (relativePropertyPath, invert) {}

	public VisibleIfAttribute (Options option) : base (option) {}

	public VisibleIfAttribute (Options option, bool invert, bool disable = false) : base (option, invert) {
		this.disable = disable;
	}
}