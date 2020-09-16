using UnityEngine;

// Shows a popup with values from another property.

// Usage:

// public string[] list;

// [ArrayPropertyRange("list")]
// public string popupItem;

public class ArrayIndexSliderAttribute : PropertyAttribute {
    public string relativePropertyPath;

	public ArrayIndexSliderAttribute (string relativePropertyPath) {
		this.relativePropertyPath = relativePropertyPath;
    }
}