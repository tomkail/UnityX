using UnityEngine;

// Allows you to create inspectable nullable types, when given the path to a serializable bool storing if the variable is null.
/*
[SerializeField, HideInInspector]
bool _distanceFromFloorSet;
[FakeNullable("_distanceFromFloorSet")]
public float _distanceFromFloor;
*/
// TODO - Add option to use multiple paths and simple boolean logic for if both/if either
public abstract class BaseIfAttribute : PropertyAttribute {
	public enum Options {
		PlayMode,
		EditMode
	}
	public Options option;
    public string relativeBoolPath;
	public bool invert;

	public BaseIfAttribute (string relativePropertyPath) {
		this.relativeBoolPath = relativePropertyPath;
    }

	public BaseIfAttribute (string relativePropertyPath, bool invert) {
		this.relativeBoolPath = relativePropertyPath;
		this.invert = invert;
    }

	public BaseIfAttribute (Options option) {
		this.option = option;
    }

	public BaseIfAttribute (Options option, bool invert) {
		this.option = option;
		this.invert = invert;
    }
}