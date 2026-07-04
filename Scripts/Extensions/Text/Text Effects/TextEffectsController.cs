using System;
using TMPro;
using UnityEngine;

[Serializable]
public class TextEffectProperties {
    [Space]
    [ColorUsage(true, true)]
    public Color faceColor = Color.white;
    [Range(-1f,1f)]
    public float faceDilate;
    [Range(0f,1f)]
    public float softness;

    [Space]
    public bool outlineEnabled;
    [ColorUsage(true, true)]
    public Color outlineColor;
    [Range(0f,1f)]
    public float outlineWidth;
    // Second outline: only takes effect on shaders that expose _Outline2Color/_Outline2Width
    // (e.g. the SDF-Overlay / two-outline TMP shaders); on other shaders the writes are guarded out below.
    [ColorUsage(true, true)]
    public Color outline2Color;
    [Range(0f,1f)]
    public float outline2Width;

    [Space]
    public bool glowEnabled;
    [Range(0f,1f)]
    public float glowPower;
    [ColorUsage(true, true)]
    public Color glowColor;
    [Range(0f,1f)]
    public float glowOuter;
    [Range(0f,1f)]
    public float glowInner;

    public void ApplyToMaterial(Material fontMaterial) {
        fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, faceColor);
        fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);

        if(outlineEnabled) fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
        else fontMaterial.DisableKeyword(ShaderUtilities.Keyword_Outline);
        fontMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, softness);
        fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
        // Zero the width when disabled so the outline vanishes even on shaders that don't gate it by keyword.
        fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineEnabled ? outlineWidth : 0f);
        if(fontMaterial.HasProperty(ShaderUtilities.ID_Outline2Color)) {
            fontMaterial.SetColor(ShaderUtilities.ID_Outline2Color, outline2Color);
            fontMaterial.SetFloat(ShaderUtilities.ID_Outline2Width, outline2Width);
        }

        if(glowEnabled) fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Glow);
        else fontMaterial.DisableKeyword(ShaderUtilities.Keyword_Glow);
        fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, glowPower);
        fontMaterial.SetFloat(ShaderUtilities.ID_GlowOuter, glowOuter);
        fontMaterial.SetFloat(ShaderUtilities.ID_GlowInner, glowInner);
        fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, glowColor);
    }
}

[ExecuteInEditMode]
[RequireComponent(typeof(TMP_Text))]
public class TextEffectsController : MonoBehaviour
{
    public TMP_Text m_TextComponent;
    [NonSerialized] bool isDirty;
    // The shared material the text used before we swapped in our controlled instance; restored on disable.
    [NonSerialized] Material m_TextBaseMaterial;
    [NonSerialized] Material fontMaterial;

    public TextEffectProperties effects;


    public void SetDirty() {
        isDirty = true;
    }

    void Reset() {
        m_TextComponent = GetComponent<TMP_Text>();
    }

    void OnValidate() {
        SetDirty();
    }

    void OnEnable() {
        m_TextComponent = GetComponent<TMP_Text>();
        m_TextComponent.RegisterDirtyVerticesCallback(OnDirtyVerts);

        // Create a per-object instance of the text's material so our effect tweaks don't mutate the shared asset.
        Init();
        Refresh();
    }

    void Init() {
        // Capture the original shared material once, so OnDisable can put it back.
        if (m_TextBaseMaterial == null && m_TextComponent.fontSharedMaterial != null && m_TextComponent.fontSharedMaterial != fontMaterial)
            m_TextBaseMaterial = m_TextComponent.fontSharedMaterial;

        var sourceMaterial = m_TextBaseMaterial != null ? m_TextBaseMaterial : (m_TextComponent.font != null ? m_TextComponent.font.material : null);
        if (sourceMaterial == null) return;

        // Destroy the previous instance before creating a new one — Init runs repeatedly from Update(),
        // so without this every re-init would leak a Material.
        DestroyControlledMaterial();

        fontMaterial = new Material(sourceMaterial);
        fontMaterial.name = "Controlled Font Material";
        // Need to manually copy the shader keywords
        fontMaterial.shaderKeywords = sourceMaterial.shaderKeywords;
        m_TextComponent.fontMaterial = fontMaterial;
    }

    void OnDisable() {
        m_TextComponent.UnregisterDirtyVerticesCallback(OnDirtyVerts);
        // Restore the original shared material and free our instance.
        if (m_TextBaseMaterial != null) m_TextComponent.fontSharedMaterial = m_TextBaseMaterial;
        DestroyControlledMaterial();
        m_TextBaseMaterial = null;
    }

    void DestroyControlledMaterial() {
        if (fontMaterial == null) return;
        if (Application.isPlaying) Destroy(fontMaterial);
        else DestroyImmediate(fontMaterial);
        fontMaterial = null;
    }

    [NonSerialized] bool internalRefresh;
    void OnDirtyVerts() {
        if(internalRefresh) return;
        Refresh();
    }

    void Update() {
        if(m_TextComponent.materialForRendering.mainTexture != m_TextComponent.font.atlasTexture) {
            Init();
            SetDirty();
        }
        if(m_TextComponent.havePropertiesChanged) {
            SetDirty();
            Init();
        }

        if (m_TextComponent.fontSharedMaterial != fontMaterial) {
            Init();
            SetDirty();
        }

        if (isDirty)
            Refresh();
    }

    void Refresh() {
        if (effects == null || fontMaterial == null) return;
        internalRefresh = true;
        effects.ApplyToMaterial(fontMaterial);
        // Reassigning fontMaterial is expensive (it instantiates a new material), so only do it when it actually differs.
        if (m_TextComponent.fontMaterial != fontMaterial)
            m_TextComponent.fontMaterial = fontMaterial;
        isDirty = false;
        internalRefresh = false;
    }
}
