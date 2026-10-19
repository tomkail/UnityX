using UnityEngine;

// We allow this to be found via findobjectoftype only once, when there's a chance the object hasn't had time to be woken up
// When the instance is destroyed we allow findobjectoftype to be used again.
// After that, all instance management is handled via awake/destroy
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> {
    static bool searched;
    static T _Instance;
#if UNITY_EDITOR
    static int searchedInPlay;
#endif
    public static T Instance {
        get {
#if UNITY_EDITOR
            // When the editor doesn't reload scripts on Play, a search from the last play doesn't count.
            if(!Application.isPlaying || searchedInPlay != MonoSingletonPlays.count) searched = false;
            searchedInPlay = MonoSingletonPlays.count;
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
        if(_Instance != null && _Instance != this) {
            Debug.LogWarning($"Duplicate {typeof(T).Name} singleton on '{name}'; destroying it.", this);
            Destroy(this);
            return;
        }
        _Instance = (T)this;
        // The instance is known now, so no scene search is needed.
        searched = true;
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

#if UNITY_EDITOR
// Counts plays, so MonoSingleton can tell one play from the next.
static class MonoSingletonPlays {
    public static int count;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void OnPlay () => count++;
}
#endif
