using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

#if UNITY_WINRT
using UnityEngine.Windows;
#endif

public static class TypeX {
	
	#if !UNITY_WINRT
	/// <summary>
	/// Determines if the type of an object inherits from the specified type. Includes interfaces.
	/// </summary>
	/// <returns><c>true</c> if is type of the specified obj; otherwise, <c>false</c>.</returns>
	/// <param name="obj">Object.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static bool IsAssignableToGenericType(this Type givenType, Type genericType) {
		var interfaceTypes = givenType.GetInterfaces();

		foreach (var it in interfaceTypes)
		{
			if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
				return true;
		}
		
		if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
			return true;
		
		Type baseType = givenType.BaseType;
		if (baseType == null) return false;
		
		return IsAssignableToGenericType(baseType, genericType);
	}
	#endif
}