using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Outputs a value from a list of values, using a custom blend function.
// A common use case is controlling if a particular view should be shown, which is determined by various elements in the system.
// In this case, each object with a stake in the decision can declare a boolean value, which is blended with an All or Any function depending on use case.
// Another might be determining the volume of some audio, which might be blended with a Min function.
[System.Serializable]
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
		if(onChange != null)
			onChange (value);
	}

	public bool TryGetValueForSource (object source, out T value) {
		value = default(T);
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
			previousValue = value;
			if(onChange != null)
				onChange (value);
		}
	}
	
	protected T GetValue () {
		Debug.Assert(blendFunc != null);
		return blendFunc((sources == null || !sources.Any()) ? Enumerable.Empty<T>() : sources.Select(x => x.value));
	}
	
	public override string ToString () {
		return string.Format ("[AndBlender] Value={0}, Sources=\n{1}", value, DebugX.ListAsString(sources));
	}
	
	[System.Serializable]
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
			return string.Format ("[LogicGateSource] source={0}, value={1}", source, value);
		}
	}
	[SerializeField]
	protected List<LogicGateSource> sources = new List<LogicGateSource> ();
}
/*
// Blender using "and" logic. All gate sources must be true.
// When no values exist the blender is true, unless a default value is set in the constructor
[System.Serializable]
public class AndBlender : LogicBlender<bool> {

	
	public AndBlender () : this(true) {}
	public AndBlender (bool fallbackValue) {
        this.fallbackValue = fallbackValue;
        Refresh();
    }

	

	// // Sets a source (creating if necessary) and refreshes
	// public void Set (object source, bool value) {
	// 	if (source == null) return;
	// 	// Debug.Assert(sources.All(x => !x.Equals(null)));
	// 	var existingIndex = sources.FindIndex (e => e.source.Equals(source));
	// 	if (existingIndex != -1) {
	// 		var entry = sources [existingIndex];
 //            if(entry.value == value) return;
	// 		entry.value = value;
	// 		sources [existingIndex] = entry;
	// 	} else {
	// 		sources.Add (new LogicGateSource(source, value));
	// 	}
 //
	// 	Refresh();
	// }
 //
	// protected override bool GetValue () {
 //        if(sources.Count == 0) return fallbackValue;
	// 	return sources.Any(x => x.value);
	// }
 //
	// public bool TryGetValueForSource (object source, out bool value) {
	// 	foreach(var entry in sources) {
	// 		if(entry.source == source) {
	// 			value = entry.value;
	// 			return true;
	// 		}
	// 	}
	// 	value = false;
	// 	return false;
	// }
	// void RemoveEntriesWhere(Predicate<LogicGateSource> predicate) {
	// 	int numItemsRemoved = sources.RemoveAll(predicate);
	// 	if(numItemsRemoved > 0) Refresh();
	// }

	// public override string ToString () {
	// 	return string.Format ("[AndBlender] Value={0}, Sources=\n{1}", value, DebugX.ListAsString(sources));
	// }
}
*/