using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

public static class ReflectionX {
    static BindingFlags bindingAttr => BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy|BindingFlags.Static|BindingFlags.Instance;

    public static Type GetTypeFromObject(object obj, string propertyPath) {
		Debug.Assert(obj != null);
		string[] parts = propertyPath.Split('.');
        FieldInfo fieldInfo = null;
		PropertyInfo propertyInfo = null;
		MemberInfo memberInfo = null;
		Type type = null;
		for (int i = 0; i < parts.Length; i++) {
			fieldInfo = null;
			propertyInfo = null;
			memberInfo = obj.GetType().GetMember(parts[i], bindingAttr).FirstOrDefault();
			if(memberInfo is FieldInfo) {
				fieldInfo = (FieldInfo)memberInfo;
				obj = fieldInfo.GetValue(obj);
				type = fieldInfo.FieldType;
			} else if(memberInfo is PropertyInfo) {
				propertyInfo = (PropertyInfo)memberInfo;
				obj = propertyInfo.GetValue(obj, null);
				type = propertyInfo.PropertyType;
			}
			bool isArray = type != null && (type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>));
			if (i != parts.Length-1 && isArray) {
				i+=2;
				int indexStart = parts[i].IndexOf("[", StringComparison.Ordinal)+1;
				int collectionElementIndex = Int32.Parse(parts[i].Substring(indexStart, parts[i].Length-indexStart-1));
				if(obj != null) {
					IList list = obj as IList;
					if(MathX.IsBetweenInclusive(collectionElementIndex, 0, list.Count-1)) {
						obj = list[collectionElementIndex];
						if(obj == null) {
							if(i == parts.Length-1) {
								break;
							} else {
								return null;
							}
						}
					}
				}
			}
		}
		
		if(type == null) return null;
		else if(type.IsArray) return type.GetElementType();
		else if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return type.GetGenericArguments()[0];
		else return type;
	}

	// If this goes wrong again, try some of the suggestions here - http://stackoverflow.com/questions/23181307/parse-field-property-path
	public static T GetValueFromObject<T>(object obj, string propertyPath) {
		Debug.Assert(obj != null);
        MemberInfo memberInfo = null;
//		PropertyInfo propertyInfo = null;
		string[] parts = propertyPath.Split('.');
		int partIndex = -1;
		foreach (string part in parts) {
			partIndex++;
			if(obj is T) return (T)obj;
			memberInfo = obj.GetType().GetMember(part, bindingAttr).FirstOrDefault();
			if(memberInfo == null)continue;

//			propertyInfo = obj.GetType().GetProperty(part, bindingAttr);
//			if(propertyInfo == null)continue;
			object x = null;
			if(memberInfo is FieldInfo) x = ((FieldInfo)memberInfo).GetValue(obj);
			if(memberInfo is PropertyInfo) x = ((PropertyInfo)memberInfo).GetValue(obj, null);
//			((PropertyInfo)fieldInfo).
			
			if (x is IList) {
				int indexStart = parts[partIndex+2].IndexOf("[", StringComparison.Ordinal)+1;
				int collectionElementIndex = Int32.Parse(parts[partIndex+2].Substring(indexStart, parts[partIndex+2].Length-indexStart-1));
				IList list = x as IList;
				if(MathX.IsBetweenInclusive(collectionElementIndex, 0, list.Count-1)) {
					obj = (x as IList)[collectionElementIndex];
//					type = obj.GetType();
				} else {
					DebugX.LogWarning ("Index: "+collectionElementIndex+", List Count: "+list.Count+", Current Path Part: "+part+", Full Path: "+propertyPath);
					return default;
				}
				continue;
			} else {
//				type = fieldInfo.GetType();
			}

			if(memberInfo is FieldInfo) obj = ((FieldInfo)memberInfo).GetValue(obj);
			if(memberInfo is PropertyInfo) obj = ((PropertyInfo)memberInfo).GetValue(obj, null);
//			obj = fieldInfo.GetValue(obj);
		}
			
		if(!(obj is T)) return default;
		return (T)obj;
	}



public static object GetValueFromObject(object obj, string propertyPath, Type t) {
		Debug.Assert(obj != null);
		MemberInfo memberInfo = null;
//		PropertyInfo propertyInfo = null;
		string[] parts = propertyPath.Split('.');
		int partIndex = -1;
		foreach (string part in parts) {
			partIndex++;
			if(obj.GetType() == t) return obj;
			memberInfo = obj.GetType().GetMember(part, bindingAttr).FirstOrDefault();
			if(memberInfo == null)continue;

//			propertyInfo = obj.GetType().GetProperty(part, bindingAttr);
//			if(propertyInfo == null)continue;
			object x = null;
			if(memberInfo is FieldInfo fieldInfo) x = fieldInfo.GetValue(obj);
			if(memberInfo is PropertyInfo propertyInfo) x = propertyInfo.GetValue(obj, null);
//			((PropertyInfo)fieldInfo).
			
			if (x is IList list) {
				int indexStart = parts[partIndex+2].IndexOf("[", StringComparison.Ordinal)+1;
				int collectionElementIndex = Int32.Parse(parts[partIndex+2].Substring(indexStart, parts[partIndex+2].Length-indexStart-1));
				if(MathX.IsBetweenInclusive(collectionElementIndex, 0, list.Count-1)) {
					obj = list[collectionElementIndex];
//					type = obj.GetType();
				} else {
					DebugX.LogWarning ("Index: "+collectionElementIndex+", List Count: "+list.Count+", Current Path Part: "+part+", Full Path: "+propertyPath);
					return null;
				}
				continue;
			} else {
//				type = fieldInfo.GetType();
			}

			if(memberInfo is FieldInfo) obj = ((FieldInfo)memberInfo).GetValue(obj);
			if(memberInfo is PropertyInfo) obj = ((PropertyInfo)memberInfo).GetValue(obj, null);
//			obj = fieldInfo.GetValue(obj);
		}
			
		if(obj.GetType() != t) return null;
		return obj;
	}


	public static Object GetValueFromObject(object obj, string propertyPath) {
		Debug.Assert(obj != null);
		MemberInfo fieldInfo = null;
//		PropertyInfo propertyInfo = null;
		string[] parts = propertyPath.Split('.');
		int partIndex = -1;
		foreach (string part in parts) {
			partIndex++;
			fieldInfo = obj.GetType().GetMember(part, bindingAttr).FirstOrDefault();
			if(fieldInfo == null)continue;
			object x = null;
			if(fieldInfo is FieldInfo) x = ((FieldInfo)fieldInfo).GetValue(obj);
			if(fieldInfo is PropertyInfo) x = ((PropertyInfo)fieldInfo).GetValue(obj, null);
			if (x is IList) {
				int indexStart = parts[partIndex+2].IndexOf("[", StringComparison.Ordinal)+1;
				int collectionElementIndex = Int32.Parse(parts[partIndex+2].Substring(indexStart, parts[partIndex+2].Length-indexStart-1));
				IList list = x as IList;
				if(MathX.IsBetweenInclusive(collectionElementIndex, 0, list.Count-1)) {
					obj = (x as IList)[collectionElementIndex];
//					type = obj.GetType();
				} else {
					DebugX.LogWarning ("Index: "+collectionElementIndex+", List Count: "+list.Count+", Current Path Part: "+part+", Full Path: "+propertyPath);
					return null;
				}
				continue;
			} else {
				// obj = x;
			}

			if(fieldInfo is FieldInfo) obj = ((FieldInfo)fieldInfo).GetValue(obj);
			if(fieldInfo is PropertyInfo) obj = ((PropertyInfo)fieldInfo).GetValue(obj, null);
		}
			
		return obj;
	}
	
	// TODO - This doesn't work on structs that aren't in arrays. 
	// There's some code below that does. fieldInfo.SetValue might be the answer, although not having it in the first setter doesn't make a difference. Must be related to the setter at the bottom?
	// Weirdly
	public static void SetValueFromObject<T>(object obj, string propertyPath, T val) {
		Debug.Assert(obj != null);
//		Type type = obj.GetType();
		FieldInfo fieldInfo = null;
		string[] parts = propertyPath.Split('.');
		object value = obj;
		int partIndex = -1;
		foreach (string part in parts) {
			partIndex++;
			
			if(value is T) {
//				value = val;
				fieldInfo.SetValue(obj, val);
				return;
			}
			fieldInfo = value.GetType().GetField(part, bindingAttr);
			if(fieldInfo == null)continue;
			object x = fieldInfo.GetValue(value);
			
			if (x is IList) {
//				int indexStart = parts[partIndex+2].IndexOf("[")+1;
//				string collectionPropertyName = parts[partIndex+2].Substring(0, indexStart-1);
//				int collectionElementIndex = Int32.Parse(parts[partIndex+2].Substring(indexStart, parts[partIndex+2].Length-indexStart-1));
//				type = value.GetType();
				continue;
			}
			else {
//				type = fieldInfo.GetType();
			}
			
			value = fieldInfo.GetValue(value);
		}
		
		value = val;
	}


	



	// Nabbed from ReflectionUtils that comes with Unity ImageEffects. I'd like to unify this in with the code above sometime
	static Dictionary<KeyValuePair<object, string>, FieldInfo> s_FieldInfoFromPaths = new();

	public static FieldInfo GetFieldInfoFromPath(object source, string path)
	{
		FieldInfo field = null;
		var kvp = new KeyValuePair<object, string>(source, path);

		if (!s_FieldInfoFromPaths.TryGetValue(kvp, out field))
		{
			var splittedPath = path.Split('.');
			var type = source.GetType();

			foreach (var t in splittedPath)
			{
				field = type.GetField(t, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

				if (field == null)
					break;

				type = field.FieldType;
			}

			s_FieldInfoFromPaths.Add(kvp, field);
		}

		return field;
	}
	
	public static object GetFieldValue(object source, string name)
	{
		var type = source.GetType();

		while (type != null)
		{
			var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (f != null)
				return f.GetValue(source);

			type = type.BaseType;
		}

		return null;
	}

	public static object GetFieldValueFromPath(object source, ref Type baseType, string path)
	{
		var splittedPath = path.Split('.');
		object srcObject = source;

		foreach (var t in splittedPath)
		{
			var fieldInfo = baseType.GetField(t, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

			if (fieldInfo == null)
			{
				baseType = null;
				break;
			}

			baseType = fieldInfo.FieldType;
			srcObject = GetFieldValue(srcObject, t);
		}

		return baseType == null
				? null
				: srcObject;
	}

	public static object GetParentObject(string path, object obj)
	{
		var fields = path.Split('.');

		if (fields.Length == 1)
			return obj;

		var info = obj.GetType().GetField(fields[0], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		obj = info.GetValue(obj);

		return GetParentObject(string.Join(".", fields, 1, fields.Length - 1), obj);
	}
}