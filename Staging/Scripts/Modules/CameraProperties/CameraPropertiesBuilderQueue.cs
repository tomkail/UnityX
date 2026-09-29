using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Composes an ordered set of <see cref="ICameraPropertiesModifier"/>s onto a single <see cref="CameraProperties"/>.
/// This is the "camera stack": each frame, <see cref="Update"/> ticks every enabled modifier, then
/// <see cref="Generate"/> runs each modifier's Modify in sort order over the running properties. It's a pipeline
/// (earlier output = later input), not a blend — for blending distinct states, do it inside a modifier with
/// CameraProperties.WeightedBlend / Lerp / SmoothDamp.
///
/// Modifiers come from two places, merged and sorted together:
///   - <b>authored</b> — plain [System.Serializable] classes assigned in the inspector (stored via
///     [SerializeReference]); great for tweakable, persistent effects (clamps, framing, offsets).
///   - <b>runtime</b> — registered from code via <see cref="Add(ICameraPropertiesModifier,int,string)"/>; this is
///     how MonoBehaviour modifiers (own lifecycle, e.g. DirectManipulationCamera) and closures participate, since
///     neither can live in a [SerializeReference] list.
/// </summary>
[System.Serializable]
public class CameraPropertiesBuilderQueue {
	public delegate void UpdateCameraPropertiesDelegate (float deltaTime);
	public delegate void ModifyCameraPropertiesDelegate (ref CameraProperties properties);

	/// <summary>One entry in the queue: a modifier plus its composition metadata (name / sort order / on-off).</summary>
	[System.Serializable]
	public class Entry {
		public string name;
		[Tooltip("Uncheck to skip this modifier without removing it — handy for A/B debugging at runtime.")]
		public bool enabled = true;
		[Tooltip("Lower runs first. Put input-driven pan/zoom early; clamps, framing and shake later.")]
		public int sortIndex;
		// [SerializeReference] stores a polymorphic, inline (non-UnityEngine.Object) modifier — i.e. a plain
		// [System.Serializable] class authored in the inspector. MonoBehaviour modifiers can't go here; they
		// register into the runtime list instead.
		[SerializeReference] public ICameraPropertiesModifier modifier;

		public Entry () {}
		public Entry (ICameraPropertiesModifier modifier, int sortIndex, string name) {
			this.modifier = modifier;
			this.sortIndex = sortIndex;
			this.name = name;
		}
	}

	// Authored in the inspector; serialized. Holds inline plain-class modifiers only (see Entry.modifier).
	[SerializeField] List<Entry> authoredModifiers = new List<Entry>();

	// Registered from code at runtime — MonoBehaviours, closures/delegates. Not serialized (rebuilt each session).
	[System.NonSerialized] List<Entry> runtimeModifiers = new List<Entry>();

	// Cached union of both lists, sorted by sortIndex; rebuilt lazily when membership or order changes.
	[System.NonSerialized] List<Entry> active;
	[System.NonSerialized] bool dirty = true;

	/// <summary>Force the sorted active set to rebuild (e.g. after changing an entry's sortIndex at runtime).</summary>
	public void MarkDirty () => dirty = true;

	/// <summary>Register a modifier object (MonoBehaviour, plain class, …). Returns its entry so you can Remove it.</summary>
	public Entry Add (ICameraPropertiesModifier modifier, int sortIndex = 0, string name = null) {
		var entry = new Entry(modifier, sortIndex, name);
		runtimeModifiers.Add(entry);
		dirty = true;
		return entry;
	}

	/// <summary>Sugar: register a pair of (update, modify) delegates — e.g. a closure carrying captured state.</summary>
	public Entry Add (UpdateCameraPropertiesDelegate update, ModifyCameraPropertiesDelegate modify, int sortIndex = 0, string name = null) {
		return Add(new DelegateCameraModifier(update, modify), sortIndex, name);
	}

	/// <summary>Remove a runtime-registered modifier by reference. (Inspector-authored entries aren't removed here.)</summary>
	public bool Remove (ICameraPropertiesModifier modifier) {
		for (int i = runtimeModifiers.Count - 1; i >= 0; i--) {
			if (runtimeModifiers[i].modifier == modifier) {
				runtimeModifiers.RemoveAt(i);
				dirty = true;
				return true;
			}
		}
		return false;
	}

	/// <summary>Remove a runtime entry returned by <see cref="Add(ICameraPropertiesModifier,int,string)"/>.</summary>
	public bool Remove (Entry entry) {
		if (runtimeModifiers.Remove(entry)) {
			dirty = true;
			return true;
		}
		return false;
	}

	public void Update (float deltaTime) {
		if (dirty) RebuildActive();
		for (int i = 0; i < active.Count; i++) {
			var entry = active[i];
			if (entry.enabled && entry.modifier != null) entry.modifier.UpdateModifier(deltaTime);
		}
	}

	public void Generate (ref CameraProperties properties) {
		if (dirty) RebuildActive();
		for (int i = 0; i < active.Count; i++) {
			var entry = active[i];
			if (entry.enabled && entry.modifier != null) entry.modifier.Modify(ref properties);
		}
	}

	void RebuildActive () {
		if (active == null) active = new List<Entry>();
		active.Clear();
		for (int i = 0; i < authoredModifiers.Count; i++)
			if (authoredModifiers[i] != null) active.Add(authoredModifiers[i]);
		for (int i = 0; i < runtimeModifiers.Count; i++)
			active.Add(runtimeModifiers[i]);
		// List.Sort isn't stable, but equal sortIndex ties among camera modifiers are rare and their relative
		// order is unspecified by design — give distinct indices if order among them matters.
		active.Sort((a, b) => a.sortIndex.CompareTo(b.sortIndex));
		dirty = false;
	}

	/// <summary>The inspector-authored entries (read-only view; useful for a debug overlay).</summary>
	public IReadOnlyList<Entry> AuthoredModifiers => authoredModifiers;
	/// <summary>The code-registered entries (read-only view; useful for a debug overlay).</summary>
	public IReadOnlyList<Entry> RuntimeModifiers => runtimeModifiers;
}

/// <summary>
/// Adapts a pair of (update, modify) delegates to <see cref="ICameraPropertiesModifier"/> — the bridge that lets
/// closures and bare methods participate alongside object modifiers. Runtime-only (not serializable).
/// </summary>
public class DelegateCameraModifier : ICameraPropertiesModifier {
	readonly CameraPropertiesBuilderQueue.UpdateCameraPropertiesDelegate update;
	readonly CameraPropertiesBuilderQueue.ModifyCameraPropertiesDelegate modify;

	public DelegateCameraModifier (CameraPropertiesBuilderQueue.UpdateCameraPropertiesDelegate update, CameraPropertiesBuilderQueue.ModifyCameraPropertiesDelegate modify) {
		this.update = update;
		this.modify = modify;
	}

	public void UpdateModifier (float deltaTime) => update?.Invoke(deltaTime);
	public void Modify (ref CameraProperties properties) {
		if (modify != null) modify(ref properties);
	}
}
