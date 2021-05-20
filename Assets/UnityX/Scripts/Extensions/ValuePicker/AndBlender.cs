using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class LogicBlender {
	public bool defaultValue = true;
	public bool value;

	protected abstract bool GetValue ();
	protected abstract void Refresh ();
}

// Blender using "and" logic. All gate sources must be true.
// When no values exist the blender is true, unless a default value is set in the constructor
[System.Serializable]
public class AndBlender : LogicBlender {

	public event Action<bool> onChange;

	public AndBlender () : this(true) {}
	public AndBlender (bool defaultValue) {
        this.defaultValue = defaultValue;
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

	// Sets a source (creating if necessary) and refreshes
	public void Set (object source, bool value) {
		if (source == null) return;
		Debug.Assert(sources.All(x => !x.Equals(null)));
		var existingIndex = sources.FindIndex (e => e.source.Equals(source));
		if (existingIndex != -1) {
			var entry = sources [existingIndex];
            if(entry.value == value) return;
			entry.value = value;
			sources [existingIndex] = entry;
		} else {
			sources.Add (new LogicGateSource(source, value));
		}

		Refresh();
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

	protected override bool GetValue () {
        if(sources.Count == 0) return defaultValue;
		bool current = true;
		foreach(var entry in sources) current = current && entry.value;
		return current;
	}

	public bool TryGetValueForSource (object source, out bool value) {
		foreach(var entry in sources) {
			if(entry.source == source) {
				value = entry.value;
				return true;
			}
		}
		value = false;
		return false;
	}
	void RemoveEntriesWhere(Predicate<LogicGateSource> predicate) {
		int numItemsRemoved = sources.RemoveAll(predicate);
		if(numItemsRemoved > 0) Refresh();
	}

	protected override void Refresh () {
		bool previousValue = value;
		value = GetValue();
		if (previousValue != value) {
			previousValue = value;
			if(onChange != null)
				onChange (value);
		}
	}

	[System.Serializable]
	class LogicGateSource {
		#if UNITY_EDITOR
		#pragma warning disable 0414
		[SerializeField, HideInInspector]
		string name;
		#pragma warning restore 0414
		#endif
		public object source;
		public bool value;

		public LogicGateSource (object source, bool value) {
			#if UNITY_EDITOR
			name = source.ToString();
			#endif
			this.source = source;
			this.value = value;
		}

		public override string ToString () {
			return string.Format ("[LogicGateSource] source={0}, value={1}", source, value);
		}
	}
	[SerializeField]
	List<LogicGateSource> sources = new List<LogicGateSource> ();

	public override string ToString () {
		return string.Format ("[AndBlender] Value={0}, Sources=\n{1}", value, DebugX.ListAsString(sources));
	}
}
