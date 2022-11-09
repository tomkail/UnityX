using System;
using System.Text.RegularExpressions;
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
		case SerializedPropertyType.Enum:
			return ReflectionX.GetTypeFromObject(property.serializedObject.targetObject, property.propertyPath);
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






	/// (Extension) Get the value of the serialized property.
    public static object GetValue(this SerializedProperty property)
    {
        string propertyPath = property.propertyPath;
        object value = property.serializedObject.targetObject;
        int i = 0;
        while (NextPathComponent(propertyPath, ref i, out var token))
            value = GetPathComponentValue(value, token);
        return value;
    }
    
    /// (Extension) Set the value of the serialized property.
    public static void SetValue(this SerializedProperty property, object value)
    {
        Undo.RecordObject(property.serializedObject.targetObject, $"Set {property.name}");

        SetValueNoRecord(property, value);

        EditorUtility.SetDirty(property.serializedObject.targetObject);
        property.serializedObject.ApplyModifiedProperties();
    }

    /// (Extension) Set the value of the serialized property, but do not record the change.
    /// The change will not be persisted unless you call SetDirty and ApplyModifiedProperties.
    public static void SetValueNoRecord(this SerializedProperty property, object value)
    {
        string propertyPath = property.propertyPath;
        object container = property.serializedObject.targetObject;

        int i = 0;
        NextPathComponent(propertyPath, ref i, out var deferredToken);
        while (NextPathComponent(propertyPath, ref i, out var token))
        {
            container = GetPathComponentValue(container, deferredToken);
            deferredToken = token;
        }
        Debug.Assert(!container.GetType().IsValueType, $"Cannot use SerializedObject.SetValue on a struct object, as the result will be set on a temporary.  Either change {container.GetType().Name} to a class, or use SetValue with a parent member.");
        SetPathComponentValue(container, deferredToken, value);
    }

    // Union type representing either a property name or array element index.  The element
    // index is valid only if propertyName is null.
    struct PropertyPathComponent
    {
        public string propertyName;
        public int elementIndex;
    }

    static Regex arrayElementRegex = new Regex(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);

    // Parse the next path component from a SerializedProperty.propertyPath.  For simple field/property access,
    // this is just tokenizing on '.' and returning each field/property name.  Array/list access is via
    // the pseudo-property "Array.data[N]", so this method parses that and returns just the array/list index N.
    //
    // Call this method repeatedly to access all path components.  For example:
    //
    //      string propertyPath = "quests.Array.data[0].goal";
    //      int i = 0;
    //      NextPropertyPathToken(propertyPath, ref i, out var component);
    //          => component = { propertyName = "quests" };
    //      NextPropertyPathToken(propertyPath, ref i, out var component) 
    //          => component = { elementIndex = 0 };
    //      NextPropertyPathToken(propertyPath, ref i, out var component) 
    //          => component = { propertyName = "goal" };
    //      NextPropertyPathToken(propertyPath, ref i, out var component) 
    //          => returns false
    static bool NextPathComponent(string propertyPath, ref int index, out PropertyPathComponent component)
    {
        component = new PropertyPathComponent();

        if (index >= propertyPath.Length)
            return false;

        var arrayElementMatch = arrayElementRegex.Match(propertyPath, index);
        if (arrayElementMatch.Success)
        {
            index += arrayElementMatch.Length + 1; // Skip past next '.'
            component.elementIndex = int.Parse(arrayElementMatch.Groups[1].Value);
            return true;
        }

        int dot = propertyPath.IndexOf('.', index);
        if (dot == -1)
        {
            component.propertyName = propertyPath.Substring(index);
            index = propertyPath.Length;
        }
        else
        {
            component.propertyName = propertyPath.Substring(index, dot - index);
            index = dot + 1; // Skip past next '.'
        }

        return true;
    }

    static object GetPathComponentValue(object container, PropertyPathComponent component)
    {
        if (component.propertyName == null)
            return ((IList)container)[component.elementIndex];
        else
            return GetMemberValue(container, component.propertyName);
    }
    
    static void SetPathComponentValue(object container, PropertyPathComponent component, object value)
    {
        if (component.propertyName == null)
            ((IList)container)[component.elementIndex] = value;
        else
            SetMemberValue(container, component.propertyName, value);
    }

    static object GetMemberValue(object container, string name)
    {
        if (container == null)
            return null;
        var type = container.GetType();
        var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < members.Length; ++i)
        {
            if (members[i] is FieldInfo field)
                return field.GetValue(container);
            else if (members[i] is PropertyInfo property)
                return property.GetValue(container);
        }
        return null;
    }

    static void SetMemberValue(object container, string name, object value)
    {
        var type = container.GetType();
        var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < members.Length; ++i)
        {
            if (members[i] is FieldInfo field)
            {
                field.SetValue(container, value);
                return;
            }
            else if (members[i] is PropertyInfo property)
            {
                property.SetValue(container, value);
                return;
            }
        }
        Debug.Assert(false, $"Failed to set member {container}.{name} via reflection");
    }
}