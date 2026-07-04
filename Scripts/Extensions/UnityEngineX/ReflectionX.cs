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
				if(i >= parts.Length) break; // Malformed/short path: no collection element part to parse.
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
				if(partIndex+2 >= parts.Length) return default; // Malformed/short path: no collection element part to parse.
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
				if(partIndex+2 >= parts.Length) return null; // Malformed/short path: no collection element part to parse.
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
				if(partIndex+2 >= parts.Length) return null; // Malformed/short path: no collection element part to parse.
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
	
	// Sets the value at a serialized-property path (e.g. "a.b", "myList.Array.data[2].c") on obj.
	// Walks down the path and writes each value-type (struct) intermediate back up the chain: reflection
	// returns a boxed *copy* of a struct, so without the write-back a set on a nested struct field is lost.
	// (obj should be a reference type — a Unity component/asset, as with SerializedObject — so the leaf
	//  and any struct intermediates propagate to it. A struct passed as obj is itself a boxed copy.)
	public static void SetValueFromObject<T>(object obj, string propertyPath, T val) {
		Debug.Assert(obj != null);
		if(obj == null) return;
		SetValueRecursive(obj, propertyPath.Split('.'), 0, val);
	}

	// Applies the set at parts[index..] within target and returns target (possibly a mutated boxed struct)
	// so the caller can re-assign it into its own parent.
	static object SetValueRecursive(object target, string[] parts, int index, object val) {
		if(target == null || index >= parts.Length) return target;

		MemberInfo member = target.GetType().GetMember(parts[index], bindingAttr).FirstOrDefault();
		if(member == null) return target; // path doesn't resolve — nothing to set

		object current = GetMemberValue(member, target);

		// Array/List element: Unity serialized paths look like "<field>.Array.data[<i>]".
		if(current is IList list && index + 2 < parts.Length && parts[index + 1] == "Array") {
			int elementIndex = ParseArrayElementIndex(parts[index + 2]);
			if(elementIndex < 0 || elementIndex >= list.Count) return target;
			if(index + 2 == parts.Length - 1) list[elementIndex] = val;
			else list[elementIndex] = SetValueRecursive(list[elementIndex], parts, index + 3, val);
			return target; // the list is shared by reference with target's field, so no write-back needed
		}

		if(index == parts.Length - 1) {
			SetMemberValue(member, target, val);
		} else {
			object child = SetValueRecursive(current, parts, index + 1, val);
			SetMemberValue(member, target, child); // re-assign in case child is a boxed struct
		}
		return target;
	}

	static object GetMemberValue(MemberInfo member, object target) {
		if(member is FieldInfo fi) return fi.GetValue(target);
		if(member is PropertyInfo pi && pi.CanRead) return pi.GetValue(target, null);
		return null;
	}

	static void SetMemberValue(MemberInfo member, object target, object value) {
		if(member is FieldInfo fi) fi.SetValue(target, value);
		else if(member is PropertyInfo pi && pi.CanWrite) pi.SetValue(target, value, null);
	}

	static int ParseArrayElementIndex(string dataPart) {
		int start = dataPart.IndexOf("[", StringComparison.Ordinal) + 1;
		if(start <= 0 || dataPart.Length - start - 1 <= 0) return -1;
		return int.TryParse(dataPart.Substring(start, dataPart.Length - start - 1), out int i) ? i : -1;
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