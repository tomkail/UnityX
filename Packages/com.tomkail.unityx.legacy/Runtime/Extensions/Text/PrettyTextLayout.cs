using TMPro;
using UnityEngine;

// This component uses margins to produce a more aethetically pleasing layout for text in a RectTransform. 
[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class PrettyTextLayout : MonoBehaviour {
    TextMeshProUGUI textMeshPro => GetComponent<TextMeshProUGUI>();
    void OnEnable() {
        refreshing = false;
        CallbackRefresh();
        textMeshPro.RegisterDirtyLayoutCallback(LayoutDirty);
    }

    void LayoutDirty() {
        CallbackRefresh();
    }

    void CallbackRefresh() {
        if(refreshing) return;
        refreshing = true;
        if(Application.isPlaying) {
            RefreshInternal();
            refreshing = false;
        }
#if UNITY_EDITOR
        else {
            UnityEditor.EditorApplication.update += DelayedCallbackRefresh;
        }
#endif
    }

#if UNITY_EDITOR
    void DelayedCallbackRefresh() {
        UnityEditor.EditorApplication.update -= DelayedCallbackRefresh;
        if (this != null) {
            RefreshInternal();
        }
        refreshing = false;
    }
#endif

    bool refreshing;
    void RefreshInternal() {
        var targetMargin = textMeshPro.GetMarginForPrettyLayout();
        if (textMeshPro.margin != targetMargin) {
            textMeshPro.margin = targetMargin;
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(textMeshPro);
            #endif
        }
    }

    void OnDisable() {
        textMeshPro.UnregisterDirtyLayoutCallback(LayoutDirty);
        if (textMeshPro.margin != Vector4.zero) {
            textMeshPro.margin = Vector4.zero;
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(textMeshPro);
            #endif
        }
    }
}