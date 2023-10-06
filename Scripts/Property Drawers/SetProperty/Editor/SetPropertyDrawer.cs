#if UNITY_EDITOR
// https://github.com/LMNRY/SetProperty
// Copyright (c) 2014 Luminary LLC
// Licensed under The MIT License (See LICENSE for full text)
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;

[CustomPropertyDrawer(typeof(SetPropertyAttribute))]
public class SetPropertyDrawer : BaseAttributePropertyDrawer<SetPropertyAttribute>
{

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		// Rely on the default inspector GUI
		EditorGUI.BeginChangeCheck ();
		EditorGUI.PropertyField(position, property, label, true);
		
//		setProperty.IsDirty = true;
		if (EditorGUI.EndChangeCheck()) {
			// When a SerializedProperty is modified the actual field does not have the current value set (i.e.  
			// FieldInfo.GetValue() will return the prior value that was set) until after this OnGUI call has completed. 
			// Therefore, we need to mark this property as dirty, so that it can be updated with a subsequent OnGUI event 
			// (e.g. Repaint)
			attribute.IsDirty = true;
		} 
		if (attribute.IsDirty) {
			// The propertyPath may reference something that is a child field of a field on this Object, so it is necessary
			// to find which object is the actual parent before attempting to set the property with the current value.
			object parent = GetParentObjectOfProperty(property.propertyPath, property.serializedObject.targetObject);
			Type type = parent.GetType();
			PropertyInfo pi = type.GetProperty(attribute.Name);
			if (pi == null)
			{
				Debug.LogError("Invalid property name: " + attribute.Name + "\nCheck your [SetProperty] attribute");
			}
			else
			{
				// Use FieldInfo instead of the SerializedProperty accessors as we'd have to deal with every 
				// SerializedPropertyType and use the correct accessor
				pi.SetValue(parent, fieldInfo.GetValue(parent), null);
			}
			attribute.IsDirty = false;
		}		
	}
	
	private object GetParentObjectOfProperty(string path, object obj) {
		string[] fields = path.Split('.');
		// We've finally arrived at the final object that contains the property
		if (fields.Length == 1) return obj;
		// We may have to walk public or private fields along the chain to finding our container object, so we have to allow for both
		FieldInfo fi = obj.GetType().GetField(fields[0], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		obj = fi.GetValue(obj);
		
		// Keep searching for our object that contains the property
		return GetParentObjectOfProperty(string.Join(".", fields, 1, fields.Length - 1), obj);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property);
	}
}
#endif