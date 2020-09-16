using UnityEngine;

// Creates a popup GUI field out of a list of elements for String, Float and Int types.

// Usage example:

// [Popup("one","two","three","wolf")]
// public string item;
public class PopupAttribute : PropertyAttribute {
    public object[] list;
    
    public PopupAttribute (params object[] list) {
        this.list = list;
    }
}