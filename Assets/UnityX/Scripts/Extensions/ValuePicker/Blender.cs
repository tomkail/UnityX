using System;
using System.Collections.Generic;
using System.Linq;

public class Blender<T>
{
	public event Action<T> onChange;

	T previousValue;
	public T Value {
		get {
			T current = _defaultValue;
			foreach(var entry in _entries) current = entry.blendFunc(current);
			return current;
		}
	}

	public Blender ()
	{
		_defaultValue = default (T);
	}

	public Blender (T defaultValue)
	{
		_defaultValue = defaultValue;
	}

	public Blender (T defaultValue, Action<T> onChange)
	{
		_defaultValue = defaultValue;
		this.onChange += onChange;
	}

	/// <summary>
	/// Higher numerical value for priorities take precedence over lower.
	/// </summary>
	public void AddPriority (object source, int priority)
	{
		_priorities [source] = priority;
	}

	public void SetWithPriority (object source, Func<T,T> blendFunc, int priority)
	{
		AddPriority (source, priority);
		Set (source, blendFunc);
	}

	public void Remove (object source)
	{
		_priorities.Remove (source);
		Unset (source);
	}

	public void Set (object source, Func<T,T> blendFunc)
	{
		if (blendFunc == null) {
			Unset (source);
			return;
		}

		// Set priority if it doesn't already have one defined
		if (!_priorities.ContainsKey (source)) {
			int maxPriority = -1;
			if( _priorities.Count > 0 )
				maxPriority = _priorities.Max (objPriority => objPriority.Value);
			var implicitPriority = maxPriority + 1;
			AddPriority (source, implicitPriority);
		}

		var existingIndex = _entries.FindIndex (e => e.source.Equals(source));
		if (existingIndex != -1) {
			var entry = _entries [existingIndex];
			entry.blendFunc = blendFunc;
			_entries [existingIndex] = entry;
		} else {
			_entries.Add (new Entry { source = source, blendFunc = blendFunc });
			_entries.Sort (EntryComparer);
		}

		RefreshValue();
	}

	void RefreshValue () {
		T current = Value;
		if (!previousValue.Equals (current) && onChange != null) {
			onChange (current);
			previousValue = current;
		}
	}

	public void Unset (object source)
	{
		RemoveEntriesWhere (p => p.source.Equals (source));
	}

	public void Clear () {
		_entries.Clear();
		RefreshValue();
	}

	void RemoveEntriesWhere(Predicate<Entry> predicate)
	{
		T previous = Value;

		_entries.RemoveAll(predicate);

		RefreshValue();
	}

	public void Refresh ()
	{
		if (onChange != null) onChange (Value);
	}

	int EntryComparer (Entry e1, Entry e2)
	{
		int priority1 = _priorities [e1.source];
		int priority2 = _priorities [e2.source];
		return priority1.CompareTo (priority2);
	}

	public bool HasEntryWithSource (object obj) {
		return _priorities.ContainsKey(obj);
	}

	Dictionary<object, int> _priorities = new Dictionary<object, int> ();

	struct Entry
	{
		public object source;
		public Func<T, T> blendFunc;
	}
	// Note that higher priority sources are at the end of the list, not the start
	List<Entry> _entries = new List<Entry> ();

	T _defaultValue;
}
