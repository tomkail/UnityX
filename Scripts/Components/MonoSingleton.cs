using UnityEngine;

// We allow this to be found via findobjectoftype only once, when there's a chance the object hasn't had time to be woken up
// When the instance is destroyed we allow findobjectoftype to be used again.
// After that, all instance management is handled via awake/destroy
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> {
    static bool searched;
    static T _Instance;
    public static T Instance {
        get {
#if UNITY_EDITOR
            if(!Application.isPlaying) searched = false;
#endif
            if(!searched && _Instance == null) {
                _Instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                searched = true;
            }
            return _Instance;
        }
    }

    public static bool IsInitialized => _Instance != null;

    protected virtual void Awake () {
        _Instance = (T)this;
    }
    // Nullify the reference
    // to clear up the native Unity representation of the MonoBehaviour
    // so that newly created instances of this class are correctly set to the static _Instance
    protected virtual void OnDestroy () {
        if(_Instance != this) return;
        _Instance = null;
        searched = false;
    }
}