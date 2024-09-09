using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Prioritise list of values, as a way of having multiple sources that want to affect a particular
/// value, and a way to determined what the actual prioritised value is. For example, if you want
/// a particular piece of global UI to be visible or not depending on the state of the game, with
/// multiple layered views deciding whether or not the UI should be visible.
/// 
/// For example, to create the Selector for a bool variable:
/// 
///     public Selector<bool> uiVisible = new Selector<bool>(defaultValue:false, onChange:newValue => {
///         SetUIVisible(newValue);
///     });
/// 
/// The the individual sources can set whether the UI is visible:
/// 
///     // Indicates that 'this' wants the UI to be visible
///     game.uiVisible[this] = true;
/// 
/// By default, the priority will increment in order, so later setters have priority over earlier ones.
/// But you can also set priority by hand. Higher numerical value for priorities take precedence over lower.
/// 
///     // Set the priority first. You only need to set the priority of a particular source once
///     game.uiVisible.AddPriority(this, 100);
/// 
/// Or in a single step:
/// 
///     game.uiVisible.SetWithPriority(this, true, 100);
/// 
/// Remember that the priority of individual sources is stored separately from their desired values themselves, 
/// so different methods may affect one or both:
/// 
///     AddPriority      - priority only
///     SetWithPriority  - priority and value
///     Set, [] operator - by default, value only, except if there's no priority set, one will be implicitly used
///     Remove           - priority and value
///     Unset            - value only
/// 
/// Note that a Selector also has an implicit cast to the value that it stores, so you can do something like:
/// 
///     bool isUIVisible = uiVisibleSelector;
/// 
/// </summary>
public class Selector<T, TPrioritySource>
{
	public event Action<T> onChange;

	public T Value {
		get {
			if (_entries.Count == 0) return _defaultValue;
			return _entries [0].desiredValue;
		}
	}

	public object activeSource {
		get {
			if( _entries.Count == 0 ) return null;
			return _entries[0].source;
		}
	}

	public bool nullRemovesValue {
		get => _nullRemovesValue;
		set {
			if (_nullRemovesValue != value) {
				_nullRemovesValue = value;
				if (_nullRemovesValue)
					RemoveEntriesWhere (p => p.desiredValue == null);
			}
		}
	}
	bool _nullRemovesValue;

	public Selector (T defaultValue = default, Action<T> onChange = null)
	{
		// If the priority source is an enum, automatically 
		// glean the priority values from  it.
		var enumType = typeof(TPrioritySource);
		if (enumType.IsEnum) {
			foreach(object enumVal in Enum.GetValues(enumType))
				_priorities [(TPrioritySource)enumVal] = (int)enumVal;
		}
		
		_defaultValue = defaultValue;
		this.onChange += onChange;
	}

	/// <summary>
	/// Higher numerical value for priorities take precedence over lower.
	/// </summary>
	public void AddPriority (TPrioritySource source, int priority)
	{
		if( typeof(TPrioritySource).IsEnum ) throw new Exception("Shouldn't explicitly add priorities for enum sources.");
		_priorities [source] = priority;
	}

	public void SetWithPriority (TPrioritySource source, T desiredValue, int priority)
	{
		if( typeof(TPrioritySource).IsEnum ) throw new Exception("Shouldn't explicitly add priorities for enum sources.");
		AddPriority (source, priority);
		Set (source, desiredValue);
	}

	public void Remove (TPrioritySource source)
	{
		if( typeof(TPrioritySource).IsEnum ) throw new Exception("Should not remove an enum source. Use Unset instead.");
		_priorities.Remove (source);
		Unset (source);
	}

	public T this [TPrioritySource source] {
		get {
			var entryIndex = _entries.FindIndex (e => e.source.Equals(source));
			if (entryIndex == -1) throw new ArgumentException ("Source doesn't exist in Selector so cannot get value");
			return _entries [entryIndex].desiredValue;
		}
		set => Set (source, value);
	}

	public bool IsSet(TPrioritySource source) {
		return _entries.FindIndex(e => e.source.Equals(source)) != -1;
	}

	public bool TryGetValue(TPrioritySource source, out T value) {
		var entryIndex = _entries.FindIndex(e => e.source.Equals(source));
		if( entryIndex == -1 ) {
			value = default;
			return false;
		}

		var entry = _entries[entryIndex];
		value = entry.desiredValue;
		return true;
	}

	public void Set (TPrioritySource source, T desiredValue)
	{
		if (_nullRemovesValue && desiredValue == null) {
			Unset (source);
			return;
		}

		T previous = Value;

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
			entry.desiredValue = desiredValue;
			_entries [existingIndex] = entry;
		} else {
			_entries.Add (new Entry { source = source, desiredValue = desiredValue });
			_entries.Sort (EntryComparer);
		}

		T current = Value;
		if (!previous.Equals (current) && onChange != null)
			onChange (current);
	}

	public void Unset (TPrioritySource source)
	{
		RemoveEntriesWhere (p => p.source.Equals (source));
	}

	public void Clear () {
		T previous = Value;
		_entries.Clear();
		T current = Value;
		if (!previous.Equals (current) && onChange != null)
			onChange (current);
	}

	void RemoveEntriesWhere(Predicate<Entry> predicate)
	{
		T previous = Value;

		_entries.RemoveAll(predicate);

		T current = Value;
		if (!previous.Equals (current) && onChange != null)
			onChange (current);
	}

	// Implicit conversion from Selector to T
	public static implicit operator T (Selector<T, TPrioritySource> s)
	{
		return s.Value;
	}

	public void Refresh ()
	{
		if (onChange != null) onChange (Value);
	}

	public string traceString {
		get {
			var sb = new StringBuilder();

			sb.Append("CURRENT: ");
			sb.Append(Value);
			sb.Append(". ");

			for(int i=_entries.Count-1; i>=0; i--) {
				var entry = _entries[i];
				sb.Append(entry.source);
				sb.Append(" (");
				sb.Append(_priorities[entry.source]);
				sb.Append("): ");
				sb.Append(entry.desiredValue);
				sb.Append(i > 0 ? ", " : ". ");
			}

			sb.Append("DEFAULT: ");
			sb.Append(_defaultValue);

			return sb.ToString();
		}
	}

	int EntryComparer (Entry e1, Entry e2)
	{
		int priority1 = _priorities [e1.source];
		int priority2 = _priorities [e2.source];
		return priority2.CompareTo (priority1);
	}

	Dictionary<TPrioritySource, int> _priorities = new();

	struct Entry
	{
		public TPrioritySource source;
		public T desiredValue;
	}
	List<Entry> _entries = new();

	T _defaultValue;
}

/// <summary>
/// Selector where it's not necessary to define TSource, which is generally used for an enum.
/// </summary>
public class Selector<T> : Selector<T, object>
{
	public Selector (T defaultValue = default, Action<T> onChange = null) : base(defaultValue, onChange) {}
}
