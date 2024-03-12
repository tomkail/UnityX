using System;
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
	public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate){
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
	public static T Best<T>(this IEnumerable<T> source, GetScore<T> selector, ChooseBest comparer, float defaultBestScore = 0, T defaultValue = default){
		int index = source.BestIndex (selector, comparer, defaultBestScore);
		if(index == -1) return defaultValue;
		else return source.ElementAt(index);
	}
	public static T Best<T>(this IEnumerable<T> source, GetScore<T> selector, ChooseBest comparer, ref float defaultBestScore, T defaultValue = default){
		int index = source.BestIndex (selector, comparer, defaultBestScore);
		if(index == -1) return defaultValue;
		else return source.ElementAt(index);
	}
	
	public static float Multiply<T>(this IEnumerable<T> source, Func<T, float> selector)
	{
		if (source == null) throw new Exception("Source is null!");
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
		if(!source.Any()) return default;
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
    public static bool GetChanges<T>(IEnumerable<T> oldList, IEnumerable<T> newList, out List<T> itemsRemoved, out List<T> itemsAdded, IEqualityComparer<T> comparer = default) {
	    itemsRemoved = new List<T>();
	    itemsAdded = new List<T>();
	    return GetChanges(oldList, newList, itemsRemoved, itemsAdded, comparer);
    }
    public static bool GetChanges<T> (IEnumerable<T> oldList, IEnumerable<T> newList, List<T> itemsRemoved, List<T> itemsAdded, IEqualityComparer<T> comparer = default) {
		if(itemsRemoved == null) itemsRemoved = new List<T>();
		if(itemsAdded == null) itemsAdded = new List<T>();
		
		GetRemovedNonAlloc(oldList, newList, itemsRemoved, comparer);
		GetAddedNonAlloc(oldList, newList, itemsAdded, comparer);

        return itemsRemoved.Count > 0 || itemsAdded.Count > 0;
	}
    

	public static bool GetChanges<T>(IEnumerable<T> oldList, IEnumerable<T> newList, out List<T> itemsRemoved, out List<T> itemsAdded, out List<T> itemsUnchanged, IEqualityComparer<T> comparer = default) {
		itemsRemoved = new List<T>();
		itemsAdded = new List<T>();
		itemsUnchanged = new List<T>();
		return GetChanges(oldList, newList, itemsRemoved, itemsAdded, itemsUnchanged, comparer);
	}
	public static bool GetChanges<T> (IEnumerable<T> oldList, IEnumerable<T> newList, List<T> itemsRemoved, List<T> itemsAdded, List<T> itemsUnchanged, IEqualityComparer<T> comparer = default) {
		if(itemsRemoved == null) itemsRemoved = new List<T>();
		if(itemsAdded == null) itemsAdded = new List<T>();
		if(itemsUnchanged == null) itemsUnchanged = new List<T>();
		
		GetRemovedNonAlloc(oldList, newList, itemsRemoved, comparer);
		GetAddedNonAlloc(oldList, newList, itemsAdded, comparer);
		GetInBothNonAlloc(oldList, newList, itemsUnchanged, comparer);

        return itemsRemoved.Count > 0 || itemsAdded.Count > 0;
	}

	public static IEnumerable<T> GetInBoth<T> (IEnumerable<T> oldList, IEnumerable<T> newList, IEqualityComparer<T> comparer = default) {
        if(oldList == null) yield break;
        foreach(var oldItem in oldList) {
            if(newList != null && newList.Contains(oldItem, comparer)) {
                yield return oldItem;
            }
        }
	}

	public static void GetInBothNonAlloc<T> (IEnumerable<T> oldList, IEnumerable<T> newList, List<T> unchangedListToFill, IEqualityComparer<T> comparer = default) {
        unchangedListToFill.Clear();
		if(oldList == null) return;
        foreach(var oldItem in oldList) {
            if(newList != null && newList.Contains(oldItem, comparer)) {
                unchangedListToFill.Add(oldItem);
            }
        }
	}

	public static IEnumerable<T> GetRemoved<T> (IEnumerable<T> oldList, IEnumerable<T> newList, IEqualityComparer<T> comparer = default) {
        if(oldList == null) yield break;
        foreach(var oldItem in oldList) {
            if(newList == null || !newList.Contains(oldItem, comparer)) {
                yield return oldItem;
            }
        }
	}

	public static void GetRemovedNonAlloc<T> (IEnumerable<T> oldList, IEnumerable<T> newList, List<T> removedListToFill, IEqualityComparer<T> comparer = default) {
        removedListToFill.Clear();
        if(oldList == null) return;
        foreach(var oldItem in oldList) {
            if(newList == null || !newList.Contains(oldItem, comparer)) {
                removedListToFill.Add(oldItem);
            }
        }
	}

	public static IEnumerable<T> GetAdded<T> (IEnumerable<T> oldList, IEnumerable<T> newList, IEqualityComparer<T> comparer = default) {
        if(newList == null) yield break;
        foreach(var newItem in newList) {
            if(oldList == null || !oldList.Contains(newItem, comparer)) {
                yield return newItem;
            }
        }
	}
	
	public static void GetAddedNonAlloc<T> (IEnumerable<T> oldList, IEnumerable<T> newList, List<T> addedListToFill, IEqualityComparer<T> comparer = default) {
        addedListToFill.Clear();
		if(newList == null) return;
        foreach(var newItem in newList) {
            if(oldList == null || !oldList.Contains(newItem, comparer)) {
                addedListToFill.Add(newItem);
            }
        }
	}
	


	// Same as above, but for dictionaries.
	public static bool GetChanges<T, Q> (IDictionary<T, Q> oldList, IDictionary<T, Q> newList, out Dictionary<T, Q> itemsRemoved, out Dictionary<T, Q> itemsAdded, out Dictionary<T, Tuple<Q,Q>> itemsChanged) {
		itemsRemoved = new Dictionary<T, Q>();
		itemsAdded = new Dictionary<T, Q>();
		itemsChanged = new Dictionary<T, Tuple<Q,Q>>();
		return GetChanges(oldList, newList, itemsRemoved, itemsAdded, itemsChanged);
	}
	public static bool GetChanges<T, Q> (IDictionary<T, Q> oldList, IDictionary<T, Q> newList, Dictionary<T, Q> itemsRemoved, Dictionary<T, Q> itemsAdded, Dictionary<T, Tuple<Q,Q>> itemsChanged) {
		if(itemsRemoved == null) itemsRemoved = new Dictionary<T, Q>();
		if(itemsAdded == null) itemsAdded = new Dictionary<T, Q>();
		
		GetRemovedNonAlloc(oldList, newList, itemsRemoved);
		GetAddedNonAlloc(oldList, newList, itemsAdded);

		var changedKeys = itemsRemoved.Select(x => x.Key).Intersect(itemsAdded.Select(x => x.Key)).ToArray();
		foreach(var changed in changedKeys) {
			itemsChanged.Add(changed, new Tuple<Q,Q>(itemsRemoved[changed], itemsAdded[changed]));
			itemsRemoved.Remove(changed);
			itemsAdded.Remove(changed);
		}
		

        return itemsRemoved.Count > 0 || itemsAdded.Count > 0 || itemsChanged.Count > 0;
	}
	public static void GetRemovedNonAlloc<T, Q> (IDictionary<T, Q> oldList, IDictionary<T, Q> newList, Dictionary<T, Q> removedListToFill) {
        removedListToFill.Clear();
        if(oldList == null) return;
        foreach(var oldItem in oldList) {
            if(newList == null || !newList.Contains(oldItem)) {
                removedListToFill.Add(oldItem.Key, oldItem.Value);
            }
        }
	}
	public static void GetAddedNonAlloc<T, Q> (IDictionary<T, Q> oldList, IDictionary<T, Q> newList, Dictionary<T, Q> addedListToFill) {
        addedListToFill.Clear();
		if(newList == null) return;
        foreach(var newItem in newList) {
            if(oldList == null || !oldList.Contains(newItem)) {
                addedListToFill.Add(newItem.Key, newItem.Value);
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
	
	
	// Remove this when Unity upgrades to .NET 6!
	public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
	{
		if (chunkSize <= 0)
		{
			throw new ArgumentException("Chunk size must be greater than 0.", nameof(chunkSize));
		}

		using (var enumerator = source.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				yield return GetChunk(enumerator, chunkSize);
			}
		}
		
		static IEnumerable<T> GetChunk(IEnumerator<T> enumerator, int chunkSize)
		{
			do
			{
				yield return enumerator.Current;
			} while (--chunkSize > 0 && enumerator.MoveNext());
		}
	}
}