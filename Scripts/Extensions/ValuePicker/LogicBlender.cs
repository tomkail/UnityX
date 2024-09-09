using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Outputs a value from a list of values, using a custom blend function.
// A common use case is controlling if a particular view should be shown, which is determined by various elements in the system.
// In this case, each object with a stake in the decision can declare a boolean value, which is blended with an All or Any function depending on use case.
// Another might be determining the volume of some audio, which might be blended with a Min function.
[Serializable]
public class LogicBlender<T> {
	public T value;
	
	public Func<IEnumerable<T>, T> blendFunc;
	
	public event Action<T> onChange;
	
	public LogicBlender (Func<IEnumerable<T>, T> blendFunc) {
		this.blendFunc = blendFunc;
		Refresh();
	}

	// Sets a source (creating if necessary) and refreshes
	public void Set (object source, T value) {
		if (source == null) return;
		// Debug.Assert(sources.All(x => !x.Equals(null)));
		var existingIndex = sources.FindIndex (e => e.source.Equals(source));
		if (existingIndex != -1) {
			var entry = sources [existingIndex];
			if(entry.value.Equals(value)) return;
			entry.value = value;
			sources [existingIndex] = entry;
		} else {
			sources.Add (new LogicGateSource(source, value));
		}

		Refresh();
	}

	// Removes a source and refreshes
	public void Remove (object source) {
		int numItemsRemoved = 0;
		for (int i = sources.Count - 1; i >= 0; i--) {
			var _source = sources[i];
			if (source == _source.source) {
				sources.RemoveAt(i);
				numItemsRemoved++;
			}
		}
		if(numItemsRemoved > 0) Refresh();
		// This creates garbage.
		// RemoveEntriesWhere (p => p.source.Equals (source));
	}
	
	// Removes all sources and refreshes
	public void Clear () {
		if(sources.Count == 0) return;
		sources.Clear();
		Refresh();
	}

	// Can be handy for forcing the result of an event to fire right after creating the instance.
	public void ForceOnChangeEvent () {
		onChange?.Invoke (value);
	}

	public bool TryGetValueForSource (object source, out T value) {
		value = default;
		foreach(var entry in sources) {
			if(entry.source == source) {
				value = entry.value;
				return true;
			}
		}
		return false;
	}
	
	
	protected virtual void Refresh() {
		var previousValue = value;
		value = GetValue();
		if (!previousValue.Equals(value)) {
			onChange?.Invoke (value);
		}
	}
	
	protected T GetValue () {
		Debug.Assert(blendFunc != null);
		return blendFunc((sources == null || !sources.Any()) ? Enumerable.Empty<T>() : sources.Select(x => x.value));
	}
	
	public override string ToString () {
		return $"[{GetType().Name}] Value={value}, Sources=\n{DebugX.ListAsString(sources)}";
	}
	
	[Serializable]
	protected class LogicGateSource {
		// For the inspector
		#pragma warning disable 0414
		[SerializeField, HideInInspector]
		string name;
		#pragma warning restore 0414
		
		public object source;
		public T value;

		public LogicGateSource (object source, T value) {
			name = source.ToString();
			this.source = source;
			this.value = value;
		}

		public override string ToString () {
			return $"{GetType().Name} source={source}, value={value}";
		}
	}
	[SerializeField]
	protected List<LogicGateSource> sources = new();
}