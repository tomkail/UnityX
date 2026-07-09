/// <summary>
/// A single contributor to a camera's <see cref="CameraProperties"/>, composed by a
/// <see cref="CameraPropertiesBuilderQueue"/> (and driven each frame by a <see cref="CameraRig"/>).
///
/// Implement it on either:
///   - a plain <c>[System.Serializable]</c> class, to author the modifier inline in a rig's list (Unity stores
///     it via <c>[SerializeReference]</c> — see <see cref="CameraPropertiesBuilderQueue.Entry"/> — so its fields
///     are editable and persistent in the inspector); or
///   - a <see cref="UnityEngine.MonoBehaviour"/> with its own lifecycle (e.g. <c>DirectManipulationCamera</c>),
///     which can't live in a <c>[SerializeReference]</c> list and instead registers itself into a rig at runtime.
///
/// Each frame the queue calls <see cref="UpdateModifier"/> once on every enabled modifier, then <see cref="Modify"/>
/// on each in sort order over the shared properties — a pipeline, so earlier modifiers' output is your input.
/// </summary>
public interface ICameraPropertiesModifier {
	// Per-frame tick, before any Modify calls this frame. Use for time-based state (accumulate dt, decay, …).
	void UpdateModifier (float deltaTime);
	// Mutate the running camera properties in place.
	void Modify (ref CameraProperties properties);
}
