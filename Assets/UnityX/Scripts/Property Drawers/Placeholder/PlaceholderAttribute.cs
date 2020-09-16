using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlaceholderAttribute : PropertyAttribute {
	public string placeholder;
	
	public PlaceholderAttribute (string placeholder) {
		this.placeholder = placeholder;
	}
}