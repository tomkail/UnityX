using UnityEngine;

// Counts play sessions so static state can tell when a new one has started.
// Without domain reload, statics survive from one play session to the next. Non-generic classes can reset themselves
// with [RuntimeInitializeOnLoadMethod], but Unity doesn't invoke that on generic classes, so generic singletons compare
// against this id instead and clear their cached state when it changes.
public static class PlaySession {
	public static int id { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void BeginSession () {
		id++;
	}
}
