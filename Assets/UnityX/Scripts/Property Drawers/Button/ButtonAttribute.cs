#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Reflection;

public class ButtonAttribute : PropertyAttribute {
	public string methodName;
	public string buttonName;
	public bool useValue;
	public BindingFlags flags;
	
	public ButtonAttribute(string methodName, string buttonName, bool useValue, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) {
		this.methodName = methodName;
		this.buttonName = buttonName;
		this.useValue = useValue;
		this.flags = flags;
	}
	public ButtonAttribute(string methodName, bool useValue, BindingFlags flags) : this (methodName, methodName, useValue, flags) {}
	public ButtonAttribute(string methodName, bool useValue) : this (methodName, methodName, useValue) {}
	public ButtonAttribute(string methodName, string buttonName, BindingFlags flags) : this (methodName, buttonName, false, flags) {}
	public ButtonAttribute(string methodName, string buttonName) : this (methodName, buttonName, false) {}
	public ButtonAttribute(string methodName, BindingFlags flags) : this (methodName, methodName, false, flags) {}
	public ButtonAttribute(string methodName) : this (methodName, 
		#if UNITY_EDITOR 
		ObjectNames.NicifyVariableName(methodName)
		#else 
	 	methodName
	 	#endif
	, false) {}
}
