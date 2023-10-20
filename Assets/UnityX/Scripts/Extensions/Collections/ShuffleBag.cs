using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ShuffleBag<T> {
	[SerializeField] List<T> _sourceItems = new List<T>();
	public ReadOnlyCollection<T> sourceItems => _sourceItems.AsReadOnly();

	[SerializeField] List<T> _items = new List<T>();
	public ReadOnlyCollection<T> items => _items.AsReadOnly();

	public bool shuffle = true;

	ShuffleBag () {}
	
	public ShuffleBag (List<T> sourceItems, bool shuffle = true) {
		Debug.Assert(sourceItems != null && sourceItems.Count > 0);
		_sourceItems = sourceItems;
		this.shuffle = shuffle;
		RefreshBag(true, shuffle);
	}
	public ShuffleBag (List<T> sourceItems, List<T> items) {
		Debug.Assert(sourceItems != null && sourceItems.Count > 0);
		_sourceItems = sourceItems;
		_items = items;
	}

	public ShuffleBag (ShuffleBag<T> otherBag) {
		Debug.Assert(otherBag != null);
		_sourceItems = new List<T>(otherBag.sourceItems);
		_items = new List<T>(otherBag.items);
	}

	public void RefreshBag (bool clearBeforeAdding = true, bool shuffle = true) {
        if(clearBeforeAdding) _items.Clear();
		foreach(var item in _sourceItems) {
			_items.Add(item);
		}
		if(shuffle)
			Shuffle(_items);
	}

	public T PeekAhead () {
		return _items[0];
	}

	public T TakeNext () {
		T item = _items[0];
		Remove(item);
		return item;
	}

	public bool Remove (T item) {
		int index = _items.IndexOf(item);
		return RemoveAt(index);
	}

	public bool RemoveAt (int index) {
		bool success = _items.ContainsIndex(index);
		if(success) {
			_items.RemoveAt(index);
			if(_items.Count == 0) {
				RefreshBag(true, shuffle);
			}
		}
		return success;
	}

	/// <summary>
    /// Shuffles the specified list.
    /// </summary>
    /// <param name="list">List.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static void Shuffle(IList<T> list) {  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = Random.Range (0, n + 1);  
			(list [k], list [n]) = (list [n], list [k]);
		}  
	}

	public static void Shuffle(IList<T> list, int seed) {  
		var oldState = Random.state;
		Random.InitState(seed);
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = Random.Range (0, n + 1);  
			(list [k], list [n]) = (list [n], list [k]);
		}
		Random.state = oldState;
	}
}