using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field)]
public class PlaceholderAttribute : PropertyAttribute {
	public string placeholder;
	
	public PlaceholderAttribute (string placeholder) {
		this.placeholder = placeholder;
	}
}