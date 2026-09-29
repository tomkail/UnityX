using System.Collections.Generic;
using UnityEngine;

// This class stores all enabled instances in a static list. Works in edit mode and play mode.
// Note that in edit mode this class makes use of FindObjectsOfType which can be expensive.
public abstract class MonoInstancer<T> : MonoBehaviour where T : MonoInstancer<T> {
#if UNITY_EDITOR
    public static void CompileReset() {
        ResetMonoInstancer();
        UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChanged;
        // The edit-mode cache also goes stale when the scene set or hierarchy changes (open a scene,
        // add/remove/enable an instance) — not just on play-mode/compile. Invalidate on those too.
        UnityEditor.EditorApplication.hierarchyChanged -= InvalidateCache;
        UnityEditor.EditorApplication.hierarchyChanged += InvalidateCache;
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= SceneOpened;
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpened;
    }

    static void PlayModeStateChanged (UnityEditor.PlayModeStateChange change) {
        if(change == UnityEditor.PlayModeStateChange.EnteredEditMode) {
            ResetMonoInstancer();
        }
    }

    // Mark dirty only — the expensive FindObjectsByType re-runs lazily on the next `all` access.
    static void InvalidateCache () => _upToDate = false;
    static void SceneOpened (UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode) => _upToDate = false;
#endif
    static List<T> _all = new();
#if UNITY_EDITOR
    static bool _upToDate = false;
#endif
    public static List<T> all {
        get {
#if UNITY_EDITOR
            if(!_upToDate) {
                _all.Clear();
                _all.AddRange(FindObjectsByType<T>(FindObjectsInactive.Exclude));
                _upToDate = true;
            }
#endif
            return _all;
        }
    }

#if UNITY_EDITOR
    public static void ResetMonoInstancer () {
        _all.Clear();
        _upToDate = false;
    }
#endif
    
    protected virtual void OnEnable () {
#if UNITY_EDITOR
        // Don't add OnEnable in editor, because we're using Object.FindObjectsOfType already.
        if(!Application.isPlaying)
            return;
#endif
        _all.Add((T)this);
    }
    protected virtual void OnDisable () {
        _all.Remove((T)this);
    }
}