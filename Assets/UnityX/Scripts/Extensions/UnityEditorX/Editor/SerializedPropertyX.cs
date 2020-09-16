using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;

public static class SerializedPropertyX
{
	public static void AddCopyPasteMenu (Rect position, SerializedProperty property, Action<SerializedProperty> copySettingsFunc = null, Func<SerializedProperty, bool> canPasteSettingsFunc = null, Action<SerializedProperty> pasteSettingsFunc = null) {
		var e = Event.current;
		if (e.type == EventType.MouseDown)
		{
			if (position.Contains(e.mousePosition))
			{
                if(copySettingsFunc == null) copySettingsFunc = SerializedPropertyX.CopySettings;
                if(canPasteSettingsFunc == null) canPasteSettingsFunc = SerializedPropertyX.CanPaste;
                if(pasteSettingsFunc == null) pasteSettingsFunc = SerializedPropertyX.PasteSettings;

				var popup = new GenericMenu();
				popup.AddItem(new GUIContent("Copy"), false, () => copySettingsFunc(property));

				if (canPasteSettingsFunc(property))
					popup.AddItem(new GUIContent("Paste"), false, () => pasteSettingsFunc(property));
				else
					popup.AddDisabledItem(new GUIContent("Paste"));

				popup.ShowAsContext();
			}
		}
	}
	public static void CopySettings(SerializedProperty settings)
	{
		var t = settings.serializedObject.targetObject.GetType();
		var settingsStruct = ReflectionX.GetFieldValueFromPath(settings.serializedObject.targetObject, ref t, settings.propertyPath);
		var serializedString = t.ToString() + '|' + JsonUtility.ToJson(settingsStruct);
		EditorGUIUtility.systemCopyBuffer = serializedString;
	}

	public static bool CanPaste(SerializedProperty settings)
	{
		var data = EditorGUIUtility.systemCopyBuffer;

		if (string.IsNullOrEmpty(data))
			return false;

		var parts = data.Split('|');

		if (string.IsNullOrEmpty(parts[0]))
			return false;

		var field = ReflectionX.GetFieldInfoFromPath(settings.serializedObject.targetObject, settings.propertyPath);
		if(field == null || field.FieldType == null) return false;
		return parts[0] == field.FieldType.ToString();
	}

	public static void PasteSettings(SerializedProperty settings) {
		foreach(var targetObject in settings.serializedObject.targetObjects) {
			Undo.RecordObject(targetObject, "Paste value");
			var field = ReflectionX.GetFieldInfoFromPath(targetObject, settings.propertyPath);
			if(field == null || field.FieldType == null) continue;
			var json = EditorGUIUtility.systemCopyBuffer.Substring(field.FieldType.ToString().Length + 1);
			var obj = JsonUtility.FromJson(json, field.FieldType);
			var parent = ReflectionX.GetParentObject(settings.propertyPath, targetObject);
			field.SetValue(parent, obj, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, CultureInfo.CurrentCulture);
		}
	}



	/// <summary>
	/// Gets the actual value of a serialized property using reflection. A bit expensive. Doesn't work in every case.
	/// </summary>
	/// <returns>The base property.</returns>
	/// <param name="prop">Property.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T GetBaseProperty<T>(this SerializedProperty prop) {
		return ReflectionX.GetValueFromObject<T>(prop.serializedObject.targetObject as object, prop.propertyPath);
	}

	public static System.Object GetBaseProperty(this SerializedProperty prop) {
		return ReflectionX.GetValueFromObject(prop.serializedObject.targetObject as object, prop.propertyPath);
	}
	
	/// <summary>
	/// Gets the actual value of a serialized property using reflection. A bit expensive. Doesn't work in every case.
	/// </summary>
	/// <param name="prop">Property.</param>
	/// <param name="val">Value.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static void SetBaseProperty<T>(this SerializedProperty prop, T val) {
		ReflectionX.SetValueFromObject(prop.serializedObject.targetObject, prop.propertyPath, val);
	}

	public static Type GetActualType(this SerializedProperty property) {
		switch (property.propertyType) {
		case SerializedPropertyType.Integer:
			return typeof(int);
		case SerializedPropertyType.Float:
			return typeof(float);
		case SerializedPropertyType.Boolean:
			return typeof(bool);
		case SerializedPropertyType.String:
			return typeof(string);
		case SerializedPropertyType.Color:
			return typeof(Color);
		case SerializedPropertyType.Bounds:
			return typeof(Bounds);
		case SerializedPropertyType.Rect:
			return typeof(Rect);
		case SerializedPropertyType.Vector2:
			return typeof(Vector2);
		case SerializedPropertyType.Vector3:
			return typeof(Vector3);
		case SerializedPropertyType.ObjectReference:
			if(property.serializedObject.targetObject == null) return typeof(UnityEngine.Object);
			else return ReflectionX.GetTypeFromObject(property.serializedObject.targetObject, property.propertyPath);
		default: throw new Exception ("Invalid type: " + property.propertyType.ToString());
		}
	}

	// Allows . separated paths, and allows traversing into other objects (scriptable objects, components)
	public static SerializedProperty FindPropertyRelative (SerializedProperty property, string attributePath) {
		if(string.IsNullOrWhiteSpace(attributePath)) return property;

		var splitPath = attributePath.Split('.');
		property = property.serializedObject.FindProperty(splitPath.First());

		for(int i = 1; i < splitPath.Length; i++) {
			// if(property.propertyType == SerializedPropertyType.Generic)
			// 	return null;
			if((property.propertyType == SerializedPropertyType.ObjectReference)) {
				if(property.objectReferenceValue != null) 
					property = new SerializedObject(property.objectReferenceValue).FindProperty(splitPath[i]);
				else return null;
			} else {
				property = property.FindPropertyRelative(splitPath[i]);
			}
		}
		return property;
	}

	
	public static void RemoveNullObjects(this SerializedProperty prop) {
		for (int i = 0; i < prop.arraySize; i++) {
			if (prop.GetAt(i).Value() == null) {
				prop.RemoveAt(i);
				i--;
			}
		}
	}
	
	public static bool Contains(this SerializedProperty prop, System.Object value)
	{
		for (int i = 0, size = prop.arraySize; i < size; i++) {
			if (prop.GetAt(i).Value() == value)
				return true;
		}
		return false;
	}
	
	public static void MoveUp(this SerializedProperty prop, int fromIndex)
	{
		AssertArray(prop);
		AssertNotEmpty(prop);
		int previous = fromIndex - 1;
		if (previous < 0) previous = prop.arraySize - 1;
		prop.Swap(fromIndex, previous);
	}
	
	public static void MoveDown(this SerializedProperty prop, int fromIndex)
	{
		AssertArray(prop);
		AssertNotEmpty(prop);
		int next = (fromIndex + 1) % prop.arraySize;
		prop.Swap(fromIndex, next);
	}
	
	public static void Swap(this SerializedProperty prop, int i, int j)
	{
		AssertArray(prop);
		AssertNotEmpty(prop);
		var value1 = prop.GetObjectValueAt(i);
		var value2 = prop.GetObjectValueAt(j);
		System.Object temp = value1;
		prop.SetObjectValueAt(i, value2);
		prop.SetObjectValueAt(j, temp);
	}
	
	public static bool ContainsReferenceTypes(this SerializedProperty prop)
	{
		AssertArray(prop);
		AssertNotEmpty(prop);
		return prop.GetFirst().IsReferenceType();
	}
	
	public static bool IsReferenceType(this SerializedProperty prop)
	{
		return prop.propertyType == SerializedPropertyType.ObjectReference;
	}
	
	public static void Add(this SerializedProperty prop, UnityEngine.Object value)
	{
		AssertArray(prop);
		prop.arraySize++;
		prop.GetAt(prop.arraySize - 1).objectReferenceValue = value;
	}
	
	public static void Add(this SerializedProperty prop)
	{
		AssertArray(prop);
		prop.arraySize++;
		prop.GetLast().SetToDefault();
	}
	
	public static void SetToDefault(this SerializedProperty prop)
	{
		switch (prop.propertyType) {
		case SerializedPropertyType.Integer:
			prop.intValue = default(int);
			break;
		case SerializedPropertyType.Float:
			prop.floatValue = default(float);
			break;
		case SerializedPropertyType.Boolean:
			prop.boolValue = default(bool);
			break;
		case SerializedPropertyType.Color:
			prop.colorValue = default(Color);
			break;
		case SerializedPropertyType.Bounds:
			prop.boundsValue = default(Bounds);
			break;
		case SerializedPropertyType.Rect:
			prop.rectValue = default(Rect);
			break;
		case SerializedPropertyType.Vector2:
			prop.vector2Value = default(Vector2);
			break;
		case SerializedPropertyType.Vector3:
			prop.vector3Value = default(Vector3);
			break;
		case SerializedPropertyType.ObjectReference:
			prop.objectReferenceValue = null;
			break;
		}
	}
	
	public static SerializedProperty GetLast(this SerializedProperty prop)
	{
		AssertArray(prop);
		return prop.GetAt(prop.arraySize - 1);
	}
	
	public static SerializedProperty GetFirst(this SerializedProperty prop)
	{
		AssertArray(prop);
		return prop.GetAt(0);
	}
	
	public static void AssertArray(this SerializedProperty prop)
	{
		if (!prop.isArray)
			throw new UnityException("SerializedProperty `" + prop.name + "` is not an array. Yet you're trying to index it!");
	}
	
	public static void RemoveAt(this SerializedProperty prop, int atIndex)
	{
		AssertArray(prop);
		AssertNotEmpty(prop);
		
		for (int i = atIndex, size = prop.arraySize; i < size - 1; i++) {
			prop.SetObjectValueAt(i, prop.GetObjectValueAt(i + 1));
		}
		prop.arraySize--;
	}
	
	public static void AssertNotEmpty(this SerializedProperty prop)
	{
		if (prop.arraySize <= 0)
			throw new UnityException("Array `" + prop.name + "` is empty. You can't do anything with it!");
	}
	
	public static System.Object GetObjectValueAt(this SerializedProperty prop, int i)
	{
		//AssertArray(prop);
		//AssertNotEmpty(prop);
		return prop.GetAt(i).Value();
	}
	
	public static void SetObjectValueAt(this SerializedProperty prop, int i, System.Object toValue)
	{
		AssertArray(prop);
		AssertNotEmpty(prop);
		prop.GetAt(i).SetValue(toValue);
	}
	
	public static void SetValue(this SerializedProperty prop, System.Object toValue)
	{
		switch (prop.propertyType) {
		case SerializedPropertyType.Boolean:
			prop.boolValue = (bool)toValue;
			break;
		case SerializedPropertyType.Bounds:
			prop.boundsValue = (Bounds)toValue;
			break;
		case SerializedPropertyType.Color:
			prop.colorValue = (Color)toValue;
			break;
		case SerializedPropertyType.Float:
			prop.floatValue = (float)toValue;
			break;
		case SerializedPropertyType.Integer:
			prop.intValue = (int)toValue;
			break;
		case SerializedPropertyType.ObjectReference:
			prop.objectReferenceValue = toValue as UnityEngine.Object;
			break;
		case SerializedPropertyType.Rect:
			prop.rectValue = (Rect)toValue;
			break;
		case SerializedPropertyType.String:
			prop.stringValue = (string)toValue;
			break;
		case SerializedPropertyType.Vector2:
			prop.vector2Value = (Vector2)toValue;
			break;
		case SerializedPropertyType.Vector3:
			prop.vector3Value = (Vector3)toValue;
			break;
		}
	}
	
	public static System.Object GetValue(this SerializedProperty prop)
	{
		switch (prop.propertyType) {
		case SerializedPropertyType.Boolean:
			return prop.boolValue;
		case SerializedPropertyType.Bounds:
			return prop.boundsValue;
		case SerializedPropertyType.Color:
			return prop.colorValue;
		case SerializedPropertyType.Float:
			return prop.floatValue;
		case SerializedPropertyType.Integer:
			return prop.intValue;
		case SerializedPropertyType.ObjectReference:
			return prop.objectReferenceValue;
		case SerializedPropertyType.Rect:
			return prop.rectValue;
		case SerializedPropertyType.String:
			return prop.stringValue;
		case SerializedPropertyType.Vector2:
			return prop.vector2Value;
		case SerializedPropertyType.Vector3:
			return prop.vector3Value;
		default: return null;
		}
	}

//	public static T GetValue<T>(this SerializedProperty prop)
//	{
//		return (T)prop.GetValue();
//	}
	
	public static SerializedProperty GetAt(this SerializedProperty prop, int i)
	{
		//AssertArray(prop);
		return prop.GetArrayElementAtIndex(i);
	}
	
	
	
	
	/// @note: switch/case derived from the decompilation of SerializedProperty's internal SetToValueOfTarget() method.
	public static ValueT Value<ValueT>(this SerializedProperty thisSP)
	{
		Type valueType = typeof(ValueT);
		
		// First, do special Type checks
		if (valueType.IsEnum)
			return (ValueT)Enum.ToObject(valueType, thisSP.enumValueIndex);
		
		// Next, check for literal UnityEngine struct-types
		// @note: ->object->ValueT double-casts because C# is too dumb to realize that that the ValueT in each situation is the exact type needed.
		// 	e.g. `return thisSP.colorValue` spits _error CS0029: Cannot implicitly convert type `UnityEngine.Color' to `ValueT'_
		// 	and `return (ValueT)thisSP.colorValue;` spits _error CS0030: Cannot convert type `UnityEngine.Color' to `ValueT'_
		if (typeof(Color).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.colorValue;
		else if (typeof(LayerMask).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.intValue;
		else if (typeof(Vector2).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.vector2Value;
		else if (typeof(Vector3).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.vector3Value;
		else if (typeof(Rect).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.rectValue;
		else if (typeof(AnimationCurve).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.animationCurveValue;
		else if (typeof(Bounds).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.boundsValue;
		else if (typeof(Gradient).IsAssignableFrom(valueType))
			return (ValueT)(object)SafeGradientValue(thisSP);
		else if (typeof(Quaternion).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.quaternionValue;
					
		// Next, check if derived from UnityEngine.Object base class
		if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.objectReferenceValue;
		
		// Finally, check for native type-families
		if (typeof(int).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.intValue;
		else if (typeof(bool).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.boolValue;
		else if (typeof(float).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.floatValue;
		else if (typeof(string).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.stringValue;
		else if (typeof(char).IsAssignableFrom(valueType))
			return (ValueT)(object)thisSP.intValue;
		
		// And if all fails, throw an exception.
		throw new NotImplementedException("Unimplemented propertyType "+thisSP.propertyType+".");
	}
	
	public static object Value(this SerializedProperty thisSP)
	{
		switch (thisSP.propertyType) {
		case SerializedPropertyType.Integer:
			return thisSP.intValue;
		case SerializedPropertyType.Boolean:
			return thisSP.boolValue;
		case SerializedPropertyType.Float:
			return thisSP.floatValue;
		case SerializedPropertyType.String:
			return thisSP.stringValue;
		case SerializedPropertyType.Color:
			return thisSP.colorValue;
		case SerializedPropertyType.ObjectReference:
			return thisSP.objectReferenceValue;
		case SerializedPropertyType.LayerMask:
			return thisSP.intValue;
		case SerializedPropertyType.Enum:
			int enumI = thisSP.enumValueIndex;
			return new KeyValuePair<int, string>(enumI, thisSP.enumNames[enumI]);
		case SerializedPropertyType.Vector2:
			return thisSP.vector2Value;
		case SerializedPropertyType.Vector3:
			return thisSP.vector3Value;
		case SerializedPropertyType.Rect:
			return thisSP.rectValue;
		case SerializedPropertyType.ArraySize:
			return thisSP.intValue;
		case SerializedPropertyType.Character:
			return (char)thisSP.intValue;
		case SerializedPropertyType.AnimationCurve:
			return thisSP.animationCurveValue;
		case SerializedPropertyType.Bounds:
			return thisSP.boundsValue;
		case SerializedPropertyType.Gradient:
			return SafeGradientValue(thisSP);
		case SerializedPropertyType.Quaternion:
			return thisSP.quaternionValue;
			
		default:
			throw new NotImplementedException("Unimplemented propertyType "+thisSP.propertyType+".");
		}
	}
	
	/// Access to SerializedProperty's internal gradientValue property getter, in a manner that'll only soft break (returning null) if the property changes or disappears in future Unity revs.
	static Gradient SafeGradientValue(SerializedProperty sp)
	{
		BindingFlags instanceAnyPrivacyBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty(
			"gradientValue",
			instanceAnyPrivacyBindingFlags,
			null,
			typeof(Gradient),
			new Type[0],
			null
			);
		if (propertyInfo == null)
			return null;
		
		Gradient gradientValue = propertyInfo.GetValue(sp, null) as Gradient;
		return gradientValue;
	}
}