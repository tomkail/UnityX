using UnityEngine;

// Emulates an inspectable nullable value using a companion serializable bool that marks whether the value is "null".
// Note: the value field itself is never cleared — it keeps its last/default value; only the bool represents the null state.
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