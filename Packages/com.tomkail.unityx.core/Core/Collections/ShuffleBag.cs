using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

// Serialization: Unity uses [SerializeField]. For other serializers the BCL [DataContract]/[DataMember] attributes
// (no serializer dependency) persist {"_sourceItems":[..], "_items":[..], "shuffle":bool}; Newtonsoft honours them.
[System.Serializable, DataContract]
public class ShuffleBag<T> {
	[SerializeField, DataMember(Name = "_sourceItems", Order = 0)]
	private List<T> _sourceItems = new List<T>();
	public ReadOnlyCollection<T> sourceItems {
		get {
			return _sourceItems.AsReadOnly();
		}
	}
	[SerializeField, DataMember(Name = "_items", Order = 1)]
	private List<T> _items = new List<T>();
	public ReadOnlyCollection<T> items {
		get {
			return _items.AsReadOnly();
		}
	}

	[DataMember(Name = "shuffle", Order = 2)]
	public bool shuffle = true;

	// For deserializers, which need a parameterless constructor when there are several.
	public ShuffleBag () {}

	public ShuffleBag (List<T> sourceItems, bool shuffle = true) {
		Debug.Assert(sourceItems != null && sourceItems.Count > 0);
		this._sourceItems = sourceItems;
		this.shuffle = shuffle;
		RefreshBag(true, shuffle);
	}
	public ShuffleBag (List<T> sourceItems, List<T> items) {
		Debug.Assert(sourceItems != null && sourceItems.Count > 0);
		this._sourceItems = sourceItems;
		this._items = items;
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
		list.Shuffle();
	}

	public static void Shuffle(IList<T> list, int seed) {
		list.Shuffle(seed);
	}
}