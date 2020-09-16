using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Similar to normal List<T> except that it's a struct, meaning a local instance doesn't allocate any memory 
/// on the heap by default. It has enough capacity for 8 items, beyond which point it'll automatically allocate 
/// a full  list internally. Useful if you want to have a list with a small number of items, and you don't want 
/// any churn in the heap as a result. It'll be a bit slower than a normal list or array though since it needs
/// to do at least one switch statement any time you access it. Although it'll upgrade itself to a full list
/// when it goes over 8 items, you should really try to use a normal List if you think there's a good chance
/// of this happening.
/// </summary>
public struct MiniList<T> : IList<T>
{
	public T this [int index] {

		get {
			if (_fullList != null) return _fullList [index];

			if (index < 0 || index >= _count) throw new System.IndexOutOfRangeException ();

			switch (index) {
			case 0: return _0;
			case 1: return _1;
			case 2: return _2;
			case 3: return _3;
			case 4: return _4;
			case 5: return _5;
			case 6: return _6;
			case 7: return _7;
			}

			throw new System.Exception ();
		}
		set {
			if (_fullList != null) {
				_fullList [index] = value;
				return;
			}

			if (index < 0 || index >= _count) throw new System.IndexOutOfRangeException ();

			switch (index) {
			case 0: _0 = value; break;
			case 1: _1 = value; break;
			case 2: _2 = value; break;
			case 3: _3 = value; break;
			case 4: _4 = value; break;
			case 5: _5 = value; break;
			case 6: _6 = value; break;
			case 7: _7 = value; break;
			}
		}
	}

	IEnumerator<T> GetMiniEnumerator ()
	{
		for (int i = 0; i < _count; i++)
			yield return this [i];
	}

	public IEnumerator<T> GetEnumerator ()
	{
		if (_fullList != null) return _fullList.GetEnumerator ();

		return GetMiniEnumerator ();
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return GetEnumerator ();
	}

	public T [] ToArray ()
	{
		if (_fullList != null) return _fullList.ToArray ();

		var array = new T [Count];
		for (int i = 0; i < Count; i++) array [i] = this [i];
		return array;
	}

	public void Add (T obj)
	{
		if (_count + 1 > kMaxMiniCount) EnsureFullList ();
		if (_fullList != null) {
			_fullList.Add (obj);
			return;
		}

		_count++;
		this [_count - 1] = obj;
	}

	public void Clear ()
	{
		if (_fullList != null) {
			_fullList = null;
		} else {
			for (int i = 0; i < _count; i++) {
				this [i] = default (T);
			}
			_count = 0;
		}
	}

	public bool Contains (T obj)
	{
		if (_fullList != null) return _fullList.Contains (obj);
		for (int i = 0; i < _count; i++) {
			if (this [i].Equals (obj)) return true;
		}
		return false;
	}


	public bool IsReadOnly {
		get {
			return false;
		}
	}

	public int Count {
		get {
			if (_fullList != null) return _fullList.Count;
			return _count;
		}
	}

	public int IndexOf (T item)
	{
		if (_fullList != null) return _fullList.IndexOf (item);

		for (int i = 0; i < _count; i++) {
			if (this [i].Equals (item)) return i;
		}

		return -1;
	}

	public void Insert (int index, T item)
	{
		if (_count + 1 > kMaxMiniCount) EnsureFullList ();

		if (_fullList != null) {
			_fullList.Insert (index, item);
			return;
		}

		if (index < 0 || index > _count) throw new System.IndexOutOfRangeException ();

		if (index == _count) {
			Add (item);
			return;
		}

		// Make space
		Shift (index, 1);

		this [index] = item;
	}

	public bool Remove (T obj)
	{
		if (_fullList != null)
			return _fullList.Remove (obj);

		var index = IndexOf (obj);
		if (index != -1) {
			RemoveAt (index);
			return true;
		}

		return false;
	}

	public void RemoveAt (int index)
	{
		if (_fullList != null) {
			_fullList.RemoveAt (index);
			return;
		}

		if (index < 0 || index >= _count) throw new System.IndexOutOfRangeException ();

		Shift (index, -1);
	}

	public void CopyTo (T [] targetArray, int startIndex)
	{
		if (_fullList != null) {
			_fullList.CopyTo (targetArray, startIndex);
			return;
		}

		for (int i = 0; i < _count; i++)
			targetArray [startIndex + i] = this [i];
	}

	void Shift (int start, int places)
	{

		if (places == 0) return;

		var newCount = _count + places;
		if (newCount > kMaxMiniCount)
			throw new System.Exception ("Shouldn't be able to Shift beyond max size");
		else if (newCount < 0) {
			newCount = 0;
			places = newCount - _count;
		}

		// Adding new
		if (places > 0) {
			_count = newCount;

			for (int i = _count - 1; i > start; i--)
				this [i] = this [i - places];
		}

		// Removing
		else {
			for (int i = start; i < newCount; i++)
				this [i] = this [i - places];

			// Erase any references in later slots
			for (int i = newCount; i < _count; i++)
				this [i] = default (T);

			_count = newCount;
		}
	}

	void EnsureFullList ()
	{
		if (_fullList != null) return;
		var fullList = new List<T> ();
		for (int i = 0; i < _count; i++) {
			fullList.Add (this [i]);
			this [i] = default (T);
		}
		_count = 0;
		_fullList = fullList;
	}

	public MiniList (IEnumerable<T> elements) : this ()
	{
		foreach (var item in elements) {
			Add (item);
		}
	}

	const int kMaxMiniCount = 8;

	T _0;
	T _1;
	T _2;
	T _3;
	T _4;
	T _5;
	T _6;
	T _7;

	int _count;

	List<T> _fullList;
}