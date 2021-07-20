﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides additional functionality to IEnumerables
/// </summary>
public static class IEnumerableX {
	public static HashSet<T> ToHashSet<T>(
        this IEnumerable<T> source,
        IEqualityComparer<T> comparer = null)
    {
        return new HashSet<T>(source, comparer);
    }
	
	public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
		HashSet<TKey> knownKeys = new HashSet<TKey>();
		foreach (TSource element in source) {
			if (knownKeys.Add(keySelector(element))) {
				yield return element;
			}
		}
	}

	public static IEnumerable<int> IndexesWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
	    int i = 0;
	    foreach (T element in source) {
			if(predicate(element)) yield return i;
	        i++;
	    }
	}
	public static int IndexOf<T>(this IEnumerable<T> source, System.Func<T, bool> predicate){
		int i = 0;
		foreach(var element in source) {
			if(predicate(element)) return i;
			i++;
		}
		return -1;
	}

	public static int Closest(this IEnumerable<int> source, int target){
		var minDistance = source.Min(n => Math.Abs(target - n));
		var closest = source.First(n => Math.Abs(target - n) == minDistance);
		return closest;
	}
	public static int ClosestIndex(this IList<int> source, int target){
		return source.IndexOf(Closest(source, target));
	}

	public static float Closest(this IEnumerable<float> source, float target){
		var minDistance = source.Min(n => Math.Abs(target - n));
		var closest = source.First(n => Math.Abs(target - n) == minDistance);
		return closest;
	}
	public static int ClosestIndex(this IList<float> source, float target){
		return source.IndexOf(Closest(source, target));
	}

	public delegate float GetScore<T> (T value);
	public delegate bool ChooseBest (float other, float currentBest);
	public static int BestIndex<T>(this IEnumerable<T> source, GetScore<T> selector, ChooseBest comparer, float defaultBestScore = 0){
		int index = 0;
		int bestIndex = -1;
		float bestScore = defaultBestScore;
		foreach(var element in source) {
			float score = selector(element);
			if (comparer(score, bestScore)) {
				bestScore = score;
				bestIndex = index;
			}
			index++;
		}
		return bestIndex;
	}

	// Used as so:
	// return transforms.Best(x => x.distance, (other, currentBest) => other < currentBest, Mathf.Infinity, null);
	public static T Best<T>(this IEnumerable<T> source, GetScore<T> selector, ChooseBest comparer, float defaultBestScore = 0, T defaultValue = default(T)){
		int index = source.BestIndex (selector, comparer, defaultBestScore);
		if(index == -1) return defaultValue;
		else return source.ElementAt(index);
	}
	public static T Best<T>(this IEnumerable<T> source, GetScore<T> selector, ChooseBest comparer, ref float defaultBestScore, T defaultValue = default(T)){
		int index = source.BestIndex (selector, comparer, defaultBestScore);
		if(index == -1) return defaultValue;
		else return source.ElementAt(index);
	}

	public static int Sum<T>(this IEnumerable<T> source, Func<T, int> selector) {
		if (source == null) throw new System.Exception("Source is null!");
        int sum = 0;
		foreach (T v in source) {
            sum += selector(v);
        }
        return sum;
    }

	public static float Sum<T>(this IEnumerable<T> source, Func<T, float> selector) {
		if (source == null) throw new System.Exception("Source is null!");
		float sum = 0;
		foreach (T v in source) {
            sum += selector(v);
        }
        return sum;
    }

	public static float Multiply<T>(this IEnumerable<T> source, Func<T, float> selector)
	{
		if (source == null) throw new System.Exception("Source is null!");
		float mult = 1.0f;
		foreach (T v in source)
		{
			mult *= selector(v);
		}
		return mult;
	}

	public static int Min<T>(this IEnumerable<T> source, Func<T, int> selector) {
		int value = 0;
	    bool hasValue = false;
	    foreach (T _x in source) {
			int x = selector(_x);
	        if (hasValue) {
	            if (x < value) value = x;
	        }
	        else {
	            value = x;
	            hasValue = true;
	        }
	    }
	    if (hasValue) return value;
	    return -1;
    }

	public static float Min<T>(this IEnumerable<T> source, Func<T, float> selector) {
		float value = 0;
	    bool hasValue = false;
	    foreach (T _x in source) {
			float x = selector(_x);
	        if (hasValue) {
	            if (x < value) value = x;
	        }
	        else {
	            value = x;
	            hasValue = true;
	        }
	    }
	    if (hasValue) return value;
	    return -1;
    }

	public static int Max<T>(this IEnumerable<T> source, Func<T, int> selector) {
		int value = 0;
	    bool hasValue = false;
	    foreach (T _x in source) {
			int x = selector(_x);
	        if (hasValue) {
	            if (x > value) value = x;
	        }
	        else {
	            value = x;
	            hasValue = true;
	        }
	    }
	    if (hasValue) return value;
	    return -1;
    }

	public static float Max<T>(this IEnumerable<T> source, Func<T, float> selector) {
		float value = 0;
	    bool hasValue = false;
	    foreach (T _x in source) {
			float x = selector(_x);
	        if (hasValue) {
	            if (x > value) value = x;
	        }
	        else {
	            value = x;
	            hasValue = true;
	        }
	    }
	    if (hasValue) return value;
	    return -1;
    }

    public static T Random<T>(this IEnumerable<T> source) {
		if(source.Count() == 0) return default(T);
		return source.ElementAt(source.RandomIndex());
    }
    
	public static int RandomIndex<T>(this IEnumerable<T> source) {
		return UnityEngine.Random.Range(0, source.Count());
	}

	public static bool IsEmpty<T>(this ICollection<T> coll) {
		return coll.Count == 0;
	}

	/// <summary>
	/// Returns true if larger or equal to the specified size
	/// </summary>
	/// <param name="list"></param>
	/// <typeparam name="T"></typeparam>
	public static bool CompareSize<T>(this IEnumerable<T> list, int targetSize) {
		int count = 0;
		foreach(var element in list) {
			if(targetSize >= count) return true;
			count++;
		}
		return false;
	}

	public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection) {
		return collection == null || !collection.Any();
	}

	public static IEnumerable<TSource> NonNull<TSource>(this IEnumerable<TSource> enumerable) where TSource : class {
		return enumerable.Where(item => item != null);
	}

    /// <summary>
    /// Synonym for Where, since Filter is the standard functional programming name.
    /// </summary>
    public static IEnumerable<T> Filter<T> (this IEnumerable<T> list, Func<T, bool> pred) {
        return list.Where (pred);
    }

    /// <summary>
    /// Synonym for Select, since Map is the standard functional programming name.
    /// </summary>
    public static IEnumerable<U> Map<T, U> (this IEnumerable<T> list, Func<T, U> mapFunc)
    {
        return list.Select (mapFunc);
    }

	/// <summary>
	/// Garbage free version of list.Any(x => x == searchedObj).
	/// (Can't be a struct or you get boxing during Equals so you would always get garbage.)
	/// </summary>
	public static bool ContainsSelected<T, U>(this IEnumerable<T> list, U searchedObj, Func<T, U> selectFunc) where T : class where U : class {
		foreach(T t in list) {
			var u = selectFunc(t);
			if( u.Equals(searchedObj) )
				return true;
		}
		return false;
	}

	

	// Finds the differences between items in two lists of the same type. Only checks if items exist, not their positions.
	// A faster version of 
	// var entered = newChunkPoints.Except(chunkPoints).ToList();
	// var exited = chunkPoints.Except(newChunkPoints).ToList();	
    // Returns true if any changes were found.
	public static bool GetChanges<T> (IEnumerable<T> oldList, IEnumerable<T> newList, ref List<T> itemsRemoved, ref List<T> itemsAdded) {
		if(itemsRemoved == null) itemsRemoved = new List<T>();
		else itemsRemoved.Clear();
		if(itemsAdded == null) itemsAdded = new List<T>();
		else itemsAdded.Clear();
		
		foreach(var item in GetRemoved(oldList, newList)) itemsRemoved.Add(item);
		foreach(var item in GetAdded(oldList, newList)) itemsAdded.Add(item);

        return itemsRemoved.Count > 0 || itemsAdded.Count > 0;
	}
	public static bool GetChanges<T> (IEnumerable<T> oldList, IEnumerable<T> newList, ref List<T> itemsRemoved, ref List<T> itemsAdded, ref List<T> itemsUnchanged) {
		if(itemsRemoved == null) itemsRemoved = new List<T>();
		else itemsRemoved.Clear();
		if(itemsAdded == null) itemsAdded = new List<T>();
		else itemsAdded.Clear();
		if(itemsUnchanged == null) itemsUnchanged = new List<T>();
		else itemsUnchanged.Clear();
		
		foreach(var item in GetRemoved(oldList, newList)) itemsRemoved.Add(item);
		foreach(var item in GetAdded(oldList, newList)) itemsAdded.Add(item);
		foreach(var item in GetInBoth(oldList, newList)) itemsUnchanged.Add(item);

        return itemsRemoved.Count > 0 || itemsAdded.Count > 0;
	}

	public static IEnumerable<T> GetInBoth<T> (IEnumerable<T> oldList, IEnumerable<T> newList) {
        if(oldList == null) yield break;
        foreach(var oldItem in oldList) {
            if(newList != null && newList.Contains(oldItem)) {
                yield return oldItem;
            }
        }
	}

	public static IEnumerable<T> GetRemoved<T> (IEnumerable<T> oldList, IEnumerable<T> newList) {
        if(oldList == null) yield break;
        foreach(var oldItem in oldList) {
            if(newList == null || !newList.Contains(oldItem)) {
                yield return oldItem;
            }
        }
	}

	public static IEnumerable<T> GetAdded<T> (IEnumerable<T> oldList, IEnumerable<T> newList) {
        if(newList == null) yield break;
        foreach(var newItem in newList) {
            if(oldList == null || !oldList.Contains(newItem)) {
                yield return newItem;
            }
        }
	}



	public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2) {
		var cnt = new Dictionary<T, int>();
		foreach (T s in list1) {
			if (cnt.ContainsKey(s)) {
				cnt[s]++;
			} else {
				cnt.Add(s, 1);
			}
		}
		foreach (T s in list2) {
			if (cnt.ContainsKey(s)) {
				cnt[s]--;
			} else {
				return false;
			}
		}
		return cnt.Values.All(c => c == 0);
	}
}