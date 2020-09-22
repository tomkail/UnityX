using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This class stores all enabled instances in a static list. Works in edit mode and play mode.
// Note that in edit mode this class makes use of FindObjectsOfType which can be expensive.
public abstract class MonoInstancer<T> : MonoBehaviour where T : MonoInstancer<T> {
    static List<T> _all = new List<T>();
    static bool _upToDate = false;
    public static List<T> all {
        get {
            #if UNITY_EDITOR
            if(!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !_upToDate) {
                _all = Object.FindObjectsOfType<T>().ToList();
                if(!_all.IsNullOrEmpty()) _upToDate = true;
            }
            #endif
            return _all;
        }
    }

    
    void Reset () {
        _upToDate = false;
    }
    protected virtual void OnEnable () {
        _all.Add((T)this);
    }
    protected virtual void OnDisable () {
        _all.Remove((T)this);
    }
}