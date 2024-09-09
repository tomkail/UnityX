using System;
using System.Collections.Generic;

/// <summary>
/// Provides additional functionality to Enums.
/// </summary>
public static class EnumX {
	
	/// <summary>
	/// Gets the length of the enum
	/// </summary>
	/// <param name="src">Source.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static int Length<T>(T src) where T : Enum {
		return Length<T>();
	}
	
	/// <summary>
	/// Gets the length of the enum
	/// </summary>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static int Length<T>() where T : Enum {
		#if !UNITY_WINRT
		if (!typeof(T).IsEnum) throw new Exception("Argument {0} is not an Enum "+typeof(T).FullName);
		#endif
		return Enum.GetNames(typeof(T)).Length;
	}

	/// <summary>
	/// Gets the index of the specified enum value.
	/// </summary>
	/// <returns>The index of the value in the enum.</returns>
	/// <param name="src">Source.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static int IndexOf<T>(T src) where T : Enum {
		#if !UNITY_WINRT
		if (!typeof(T).IsEnum) throw new ("Argument {0} is not an Enum "+typeof(T).FullName);
		#endif
		var values = GetValues<T>();
		return Array.IndexOf(values, src);
	}
	
	/// <summary>
	/// Gets a random enum.
	/// </summary>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T Random<T>() where T : Enum {
		#if !UNITY_WINRT
		if (!typeof(T).IsEnum) throw new ("Argument {0} is not an Enum "+typeof(T).FullName);
		#endif
		return (T)Enum.ToObject(typeof(T), UnityEngine.Random.Range(0, Length<T>()));
	}
	
	/// <summary>
	/// Gets the next member in the enum. Repeats if the current member is the last in the sequence.
	/// </summary>
	/// <param name="src">Source.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T Next<T>(T src) where T : Enum {
		#if !UNITY_WINRT
		if (!typeof(T).IsEnum) throw new ("Argument {0} is not an Enum "+typeof(T).FullName);
		#endif
		T[] Arr = (T[])GetValues<T>();
		int j = Array.IndexOf(Arr, src) + 1;
		return (j == Arr.Length) ? Arr[0] : Arr[j];            
	}
	
	/// <summary>
	/// Gets the previous member in the enum. Repeats if the current member is the first in the sequence.
	/// </summary>
	/// <param name="src">Source.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T Previous<T>(T src) where T : Enum {
		#if !UNITY_WINRT
		if (!typeof(T).IsEnum) throw new ("Argument {0} is not an Enum "+typeof(T).FullName);
		#endif
		T[] Arr = (T[])GetValues<T>();
		int j = Array.IndexOf(Arr, src) - 1;
		return (j == -1) ? Arr[Arr.Length-1] : Arr[j];            
	}

	/// <summary>
	/// Returns an array of all the values of the enum.
	/// </summary>
	/// <returns>The array.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T[] ToArray<T>() where T : Enum {
		var values = GetValues<T>();
		return (T[])values;
	}

	/// <summary>
	/// Returns an array of all the values of the enum.
	/// </summary>
	/// <returns>The array.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static IEnumerable<T> GetEnumerable<T>() where T : Enum {
		var values = GetValues<T>();
		for(int i = 0; i < values.Length; i++)
			yield return (T)values.GetValue(i);
	}

	/// <summary>
	/// Returns an array of all the names of the enum values.
	/// </summary>
	/// <returns>The array.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	static Dictionary<Type, string[]> stringValuesCache = new();
	public static string[] ToStringArray<T>() where T : Enum {
		var type = typeof(T);
		if(!stringValuesCache.TryGetValue(type, out var stringValues)) {
        	var values = GetValues<T>();
			stringValues = stringValuesCache[type] = new string[values.Length];
			for(int i = 0; i < values.Length; i++) stringValues[i] = ((T)values.GetValue(i)).ToString();
        }
        return stringValues;
	}

	public static string GetString<T>(T val) where T : Enum {
		var array = ToStringArray<T>();
		var index = IndexOf(val);
		return array[index];
	}
	
	/// <summary>
	/// Determines if the value is first in the specified enum.
	/// </summary>
	/// <returns><c>true</c> if is first the specified src; otherwise, <c>false</c>.</returns>
	/// <param name="src">Source.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static bool IsFirst<T>(T src) where T : Enum {
		return IndexOf(src) == 0;
	}

	/// <summary>
	/// Determines if the value is last in the specified enum.
	/// </summary>
	/// <returns><c>true</c> if is last the specified src; otherwise, <c>false</c>.</returns>
	/// <param name="src">Source.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static bool IsLast<T>(T src) where T : Enum {
		return IndexOf(src) == Length<T>()-1;
	}

	public static bool IsValid<T>(T src) where T : Enum {
		return Enum.IsDefined(typeof(T), src);
	}

	static Dictionary<Type, Array> valuesCache = new();
	public static Array GetValues<T>() {
        var type = typeof(T);
        Array values = null;
        if(!valuesCache.TryGetValue(type, out values)) {
            values = valuesCache[type] = Enum.GetValues(type);
        }
        return values;
    }
}