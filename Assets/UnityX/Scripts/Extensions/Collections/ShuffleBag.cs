using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Generic;

[System.Serializable]
public class ShuffleBag<T> {
	[SerializeField]
	private List<T> _sourceItems = new List<T>();
	public ReadOnlyCollection<T> sourceItems {
		get {
			return _sourceItems.AsReadOnly();
		}
	}
	[SerializeField]
	private List<T> _items = new List<T>();
	public ReadOnlyCollection<T> items {
		get {
			return _items.AsReadOnly();
		}
	}

	public ShuffleBag (List<T> sourceItems) {
		Debug.Assert(!sourceItems.IsNullOrEmpty());
		this._sourceItems = sourceItems;
		RefreshBag();
	}

	public ShuffleBag (ShuffleBag<T> otherBag) {
		Debug.Assert(otherBag != null);
		_sourceItems = new List<T>(otherBag.sourceItems);
		_items = new List<T>(otherBag.items);
	}

	private void RefreshBag () {
		foreach(var item in _sourceItems) {
			_items.Add(item);
		}
		_items.Shuffle();
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
				RefreshBag();
			}
		}
		return success;
	}
}