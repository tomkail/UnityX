using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This class stores all enabled instances in a static list. Works in edit mode and play mode.
// Note that in edit mode this class makes use of FindObjectsOfType which can be expensive.
public abstract class MonoInstancer<T> : MonoBehaviour where T : MonoInstancer<T> {
    #if UNITY_EDITOR
    public static void CompileReset() {
        ResetMonoInstancer();
        UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }
    
    static void PlayModeStateChanged (UnityEditor.PlayModeStateChange change) {
        if(change == UnityEditor.PlayModeStateChange.EnteredEditMode) {
            ResetMonoInstancer();
        }
    }
    #endif
    static List<T> _all = new List<T>();
    #if UNITY_EDITOR
    static bool _upToDate = false;
    #endif
    public static List<T> all {
        get {
            #if UNITY_EDITOR
            if(!_upToDate) {
                _all.Clear();
                _all.AddRange(Object.FindObjectsOfType<T>());
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