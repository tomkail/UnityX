using UnityEngine;

// Allows you to create inspectable nullable types, when given the path to a serializable bool storing if the variable is null.
/*
[SerializeField, HideInInspector]
bool _distanceFromFloorSet;
[FakeNullable("_distanceFromFloorSet")]
public float _distanceFromFloor;
*/
public class FakeNullableAttribute : PropertyAttribute {
    public string boolBackingName;

	public FakeNullableAttribute (string relativePropertyPath) {
		this.boolBackingName = relativePropertyPath;
    }
}