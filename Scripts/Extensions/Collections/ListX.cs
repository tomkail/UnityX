using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ListX {

	public static List<T> ToList<T>(this IList<T> IList) {
		return new List<T>(IList);
	}

	public static void Fill<T>(this IList<T> array, T myVar) {
        for(int i = 0; i < array.Count; i++){
			array[i] = myVar;
		}
    }

	public static bool ContainsIndex<T>(this IList<T> list, int index) {
		if(list == null || list.Count == 0) return false;
        return index >= 0 && index <= list.Count-1;
    }

	/// <summary>
	/// First object in the specified list.
	/// </summary>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T First<T>(this IList<T> list) {
		return list[0];
	}
	
	/// <summary>
	/// Last object in the specified list.
	/// </summary>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T Last<T>(this IList<T> list) {
		return list[list.Count - 1];
	}
	
	/// <summary>
	/// Returns true of a list contains a type
	/// </summary>
	/// <param name="list">List.</param>
	/// <param name="type">Type.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static bool Contains<T>(this IList<T> list, Type type) {
		for(int i = list.Count - 1; i >= 0; i--) {
			if(list[i].GetType() == type) return true;
		}
		return false;
	}

	/// <summary>
	/// Returns true of a list contains a list, in order, contiguously
	/// </summary>
	/// <param name="list">List.</param>
	/// <param name="subList">List.</param>
	public static bool ContainsSubList<T>(this IList<T> list, IList<T> subList) {
		var found = false;
		for(int i = list.Count - subList.Count; !found && i >= 0; i--) {
			found = list [i].Equals (subList [0]);

			for (int j = 1; found && j < subList.Count; j++) {
				found = list [i + j].Equals(subList [j]);
			}
		}
		return found;
	}
    
	public static int GetRepeatingIndex<T>(this IList<T> list, int index) {
		return index.Mod(list.Count);
    }

	/// <summary>
	/// Gets the list element looping with an index that repeats around the length of the array.
	/// </summary>
	/// <returns>The looping.</returns>
	/// <param name="list">List.</param>
	/// <param name="index">Index.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
    public static T GetRepeating<T>(this IList<T> list, int index) {
		return list[list.GetRepeatingIndex(index)];
    }

	public static int GetClampedIndex<T>(this IList<T> list, int index) {
		return Mathf.Clamp(index, 0, list.Count-1);
    }

	/// <summary>
	/// Gets the list element clamp with an index that repeats around the length of the array.
	/// </summary>
	/// <returns>The looping.</returns>
	/// <param name="list">List.</param>
	/// <param name="index">Index.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T GetClamped<T>(this IList<T> list, int index) {
		return list[list.GetClampedIndex(index)];
	}
	public static T TryGetValue<T>(this IList<T> list, int index) {
        if(list.ContainsIndex(index)) return list[index];
        else return default;
	}

	
	/// <summary>
	/// Checks to see if a list contains a particular item.
	/// </summary>
	/// <param name="list">List.</param>
	/// <param name="item">Item.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
    public static bool Contains<T>(this IList<T> list, T item) {
		if(list.IsNullOrEmpty()) return false;
		for(int i = list.Count - 1; i >= 0; i--) {
			if(list[i].Equals(item)) return true;
		}
		return false;
    }
    
    /// <summary>
    /// Finds the index of a particular item. Returns -1 if the item could not be found.
    /// </summary>
    /// <returns>The index.</returns>
    /// <param name="list">List.</param>
    /// <param name="item">Item.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static int IndexOf<T>(this IList<T> list, T item) {
        for(int i = list.Count - 1; i >= 0; i--) {
			if(list[i] == null) continue;
            if(list[i].Equals(item)) return i;
        }
        return -1;
    }

    /// <summary>
    /// Returns the specified item in the list, or returns default(T).
    /// </summary>
    /// <param name="list">List.</param>
    /// <param name="item">Item.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static T Find<T>(this IList<T> list, T item) {
        int index = list.IndexOf(item);
        if(index < 0) return default;
        else return list[index];
    }
    
	/// <summary>
	/// G
	/// </summary>
	/// <returns>The all of type.</returns>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	/// <typeparam name="Q">The 2nd type parameter.</typeparam>
	public static List<T> GetAllOfType<T>(this List<T> list, Type removeType) {
		List<T> newList = new List<T>();
		for (int i = list.Count-1; i >= 0; i--) {
			if(list[i].GetType() == removeType) newList.Add(list[i]);
		}
		return newList;
	}


	public static void RemoveNull<T>(this List<T> list) {
        for (int i = list.Count-1; i >= 0; i--) {
			// Equals check is a workaround for weird Unity Object behaviour. http://forum.unity3d.com/threads/fun-with-null.148090/
			if(list[i] == null || list[i].Equals(null)) {
            	list.RemoveAt(i);
            }
        }
    }

    public static T[] GetShiftedRepeating<T>(IList<T> items, int places) {
		places %= items.Count;
		T[] shiftedItems = new T[items.Count];
		for (int i = 0; i < items.Count; i++)
			shiftedItems[i] = items.GetRepeating(i-places);
		return shiftedItems;
	}
    
	/// <summary>
	/// Swaps the elements at the two given indices within the list
	/// </summary>
	/// <param name="list">The list we want to swap the elements within.</param>
	/// <param name="indexA">The index of the first element we wish to swap.</param>
	/// <param name="indexB">The index of the second element we wish to swap.</param>
	/// <typeparam name="T">The type of the elements within the list.</typeparam>
	public static void Swap<T>(this IList<T> list, int indexA, int indexB) {
		T tmp = list[indexA];
		list[indexA] = list[indexB];
		list[indexB] = tmp;
	}
    
    /// <summary>
    /// Shuffles the specified list.
    /// </summary>
    /// <param name="list">List.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static void Shuffle<T>(this IList<T> list) {  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = Random.Range (0, n + 1);  
			T value = list [k];  
			list [k] = list [n];  
			list [n] = value;  
		}  
	}

	public static void Shuffle<T>(this IList<T> list, int seed) {  
		var oldState = Random.state;
		Random.InitState(seed);
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = Random.Range (0, n + 1);  
			T value = list [k];  
			list [k] = list [n];  
			list [n] = value;  
		}
		Random.state = oldState;
	}
}