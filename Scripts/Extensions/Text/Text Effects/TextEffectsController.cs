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
    // public bool outlineEnabled = false;
    [ColorUsage(true, true)]
    public Color outlineColor;
    [Range(0f,1f)]
    public float outlineWidth;
    // public Color outline2Color;
    // [Range(0f,1f)]
    // public float outline2Width;
    
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
        // fontMaterial.set
        fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, faceColor);
        fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);
    
        // if(outlineEnabled) fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
        // else fontMaterial.DisableKeyword(ShaderUtilities.Keyword_Outline);
        fontMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, softness);
        fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
        fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
        // fontMaterial.SetColor(ShaderUtilities.ID_Outline2Color, outline2Color);
        // fontMaterial.SetFloat(ShaderUtilities.ID_Outline2Width, outline2Width);
    
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
    // [System.NonSerialized] Material m_TextBaseMaterial;
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
        
        // Create new instance of the material assigned to the text object
        // Assumes all text objects will use the same highlight
        // m_TextComponent.RegisterDirtyLayoutCallback(OnDirtyVerts);
        // m_TextComponent.RegisterDirtyMaterialCallback(OnDirtyVerts);
        // m_TextComponent.OnPreRenderText += OnPreRenderText;

        Init();
            
        Refresh();
    }

    void Init() {
        // if (m_TextComponent.fontSharedMaterial != null)
        //     m_TextBaseMaterial = new Material(m_TextComponent.font.material);

        if (m_TextComponent.fontSharedMaterial != null) {
            fontMaterial = new Material(m_TextComponent.font.material);
            fontMaterial.name = "Controlled Font Material";

            // Need to manually copy the shader keywords
            fontMaterial.shaderKeywords = m_TextComponent.fontSharedMaterial.shaderKeywords;
            m_TextComponent.fontMaterial = fontMaterial;
        }
    }

    void OnDisable() {
        // m_TextComponent.fontSharedMaterial = m_TextBaseMaterial;
        m_TextComponent.UnregisterDirtyVerticesCallback(OnDirtyVerts);
        // m_TextComponent.UnregisterDirtyLayoutCallback(OnDirtyVerts);
        // m_TextComponent.UnregisterDirtyMaterialCallback(OnDirtyVerts);
        // m_TextComponent.OnPreRenderText -= OnPreRenderText;
    }

    void OnPreRenderText(TMP_TextInfo textInfo) {
        Refresh();
    }

    [NonSerialized] bool internalRefresh;
    void OnDirtyVerts() {
        if(internalRefresh) return;
        Refresh();
    }

    void Update() {
        // Debug.Log(gameObject.name+" "+fontMaterial.mainTexture +" "+ m_TextComponent.font.atlasTexture+" "+m_TextComponent.fontMaterial+" "+m_TextComponent.fontMaterial.mainTexture+" "+m_TextComponent.fontSharedMaterial+" "+m_TextComponent.fontSharedMaterial.mainTexture);
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
        internalRefresh = true;
        if (effects == null) return;
        if (fontMaterial == null) return;
        effects.ApplyToMaterial(fontMaterial);
        // This is expensive and I think always true, because it creates a new material; but for some reason it doesn't work without it?!
        if (m_TextComponent.fontMaterial != fontMaterial)
            m_TextComponent.fontMaterial = fontMaterial;

        // m_TextComponent.SetLayoutDirty();
        // m_TextComponent.UpdateMeshPadding();
        // isDirty = false;
        internalRefresh = false;
    }
}