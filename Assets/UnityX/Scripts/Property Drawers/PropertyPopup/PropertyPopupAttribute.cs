using UnityEngine;

// Shows a popup with values from another property.

// Usage:

// public string[] list;

// [PropertyPopup("list")]
// public string popupItem;

public class PropertyPopupAttribute : PropertyAttribute {
    public string relativePropertyPath;
	public bool addDefault;

	public PropertyPopupAttribute (string relativePropertyPath, bool addDefault = false) {
		this.relativePropertyPath = relativePropertyPath;
		this.addDefault = addDefault;
    }
}