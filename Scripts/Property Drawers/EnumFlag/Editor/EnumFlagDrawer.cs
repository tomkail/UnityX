using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagAttribute))]
public class EnumFlagDrawer : BaseAttributePropertyDrawer<EnumFlagAttribute> {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		Enum targetEnum = GetBaseProperty<Enum>(property);

		EditorGUI.BeginProperty(position, label, property);
        Enum enumNew = EditorGUI.EnumFlagsField(position, ObjectNames.NicifyVariableName(property.name), targetEnum);
		property.intValue = (int) Convert.ChangeType(enumNew, targetEnum.GetType());
		EditorGUI.EndProperty();
	}
	
	protected override bool IsSupported(SerializedProperty property) {
		Enum targetEnum = GetBaseProperty<Enum>(property);
		return targetEnum != null;
	}


	/// <summary>
	/// Gets the actual value of a serialized property using reflection. A bit expensive. Doesn't work in every case.
	/// </summary>
	/// <returns>The base property.</returns>
	/// <param name="prop">Property.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T GetBaseProperty<T>(SerializedProperty prop) {
		return GetValueFromObject<T>(prop.serializedObject.targetObject as object, prop.propertyPath);
	}

	// public static System.Object GetBaseProperty(this SerializedProperty prop) {
	// 	return GetValueFromObject(prop.serializedObject.targetObject as object, prop.propertyPath);
	// }


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
			var members = obj.GetType().GetMember(part, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance);
			memberInfo = members.Length > 0 ? members[0] : null;
			if(memberInfo == null)continue;

//			propertyInfo = obj.GetType().GetProperty(part, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance);
//			if(propertyInfo == null)continue;
			object x = null;
			if(memberInfo is FieldInfo) x = ((FieldInfo)memberInfo).GetValue(obj);
			if(memberInfo is PropertyInfo) x = ((PropertyInfo)memberInfo).GetValue(obj, null);
//			((PropertyInfo)fieldInfo).
			
			if (x is IList) {
				int indexStart = parts[partIndex+2].IndexOf("[")+1;
				int collectionElementIndex = Int32.Parse(parts[partIndex+2].Substring(indexStart, parts[partIndex+2].Length-indexStart-1));
				IList list = x as IList;
				if(collectionElementIndex >= 0 && collectionElementIndex < list.Count) {
					obj = (x as IList)[collectionElementIndex];
//					type = obj.GetType();
				} else {
					Debug.LogWarning ("Index: "+collectionElementIndex+", List Count: "+list.Count+", Current Path Part: "+part+", Full Path: "+propertyPath);
					return default(T);
				}
				continue;
			} else {
//				type = fieldInfo.GetType();
			}

			if(memberInfo is FieldInfo) obj = ((FieldInfo)memberInfo).GetValue(obj);
			if(memberInfo is PropertyInfo) obj = ((PropertyInfo)memberInfo).GetValue(obj, null);
//			obj = fieldInfo.GetValue(obj);
		}
			
		if(!(obj is T)) return default(T);
		return (T)obj;
	}

}