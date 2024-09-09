using TMPro;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(TMP_Text))]
public abstract class BaseTextMeshProEffect : MonoBehaviour {
    public TMP_Text m_TextComponent;
    protected bool isDirty { get; private set; } 
    
    void Reset() {
        m_TextComponent = GetComponent<TMP_Text>();
    }

    void OnValidate() {
        SetDirty();
    }

    void OnEnable() {
        m_TextComponent = GetComponent<TMP_Text>();
        m_TextComponent.RegisterDirtyVerticesCallback(OnDirtyTMPComponent);
        m_TextComponent.RegisterDirtyLayoutCallback(OnDirtyTMPComponent);
        m_TextComponent.RegisterDirtyMaterialCallback(OnDirtyTMPComponent);
        m_TextComponent.OnPreRenderText += OnPreRenderText;
        Refresh();
    }

    void OnDisable() {
        m_TextComponent.UnregisterDirtyVerticesCallback(OnDirtyTMPComponent);
        m_TextComponent.UnregisterDirtyLayoutCallback(OnDirtyTMPComponent);
        m_TextComponent.UnregisterDirtyMaterialCallback(OnDirtyTMPComponent);
        m_TextComponent.OnPreRenderText -= OnPreRenderText;
        // Also refresh on disable if dirty so we catch anything that would have been updated had this component had a final update
        Clear();
    }
    
    void OnDirtyTMPComponent() {
        SetDirty();
        // Refresh();
    }

    protected virtual void Update() {
        if (isDirty)
            Refresh();
    }
    
    public void SetDirty() {
        isDirty = true;
    }

    public void Clear() {
        if (m_TextComponent == null) return;
        m_TextComponent.ForceMeshUpdate();
    }

    public void Refresh(bool evenIfInactive = false) {
        if (!isActiveAndEnabled && !evenIfInactive) return;
        isDirty = false;
        if (m_TextComponent == null || !m_TextComponent.enabled) return;
        // Force the text object to update right away so we can have geometry to modify right from the start.
        m_TextComponent.textInfo.ClearAllMeshInfo();
        m_TextComponent.ForceMeshUpdate(true, true);
        // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
        m_TextComponent.UpdateVertexData();
    }
    
    // Effects are performed here!
    protected abstract void OnPreRenderText(TMP_TextInfo textInfo);
}