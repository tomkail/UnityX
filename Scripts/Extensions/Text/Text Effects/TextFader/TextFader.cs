using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class TextFader : BaseTextMeshProEffect {
    public enum TextFaderRenderMode {
        Alpha,
        Color
    }
    [SerializeField]
    TextFaderRenderMode _renderMode = TextFaderRenderMode.Alpha;
    public TextFaderRenderMode renderMode {
        get => _renderMode;
        set {
            if(_renderMode == value) return;
            _renderMode = value;
            SetDirty();
        }
    }

    [SerializeField]
    float _alphaA;
    public float alphaA {
        get => _alphaA;
        set {
            if(_alphaA == value) return;
            _alphaA = value;
            SetDirty();
        }
    }

    [SerializeField]
    float _alphaB = 1;
    public float alphaB {
        get => _alphaB;
        set {
            if(_alphaB == value) return;
            _alphaB = value;
            SetDirty();
        }
    }

    [SerializeField]
    Color32 _colorA = Color.black;
    public Color32 colorA {
        get => _colorA;
        set {
            if(_colorA.Compare(value)) return;
            _colorA = value;
            SetDirty();
        }
    }
    [SerializeField]
    Color32 _colorB = Color.white;
    public Color32 colorB {
        get => _colorB;
        set {
            if(_colorB.Compare(value)) return;
            _colorB = value;
            SetDirty();
        }
    }

    [SerializeField, Range(0,1)]
    float _animationProgress;
    public float animationProgress {
        get => _animationProgress;
        set {
            if(_animationProgress == value) return;
            _animationProgress = value;
            SetDirty();
        }
    }
    
    [SerializeField]
    float _characterFadeWidth = 8;
    public float characterFadeWidth {
        get => _characterFadeWidth;
        set {
            if(_characterFadeWidth == value) return;
            _characterFadeWidth = value;
            SetDirty();
        }
    }
    
    protected override void OnPreRenderText(TMP_TextInfo textInfo) {
        if (textInfo.characterCount == 0) return;
        
        Color32 alphaColorA = new Color(m_TextComponent.color.r, m_TextComponent.color.g, m_TextComponent.color.b, alphaA);
        Color32 alphaColorB = new Color(m_TextComponent.color.r, m_TextComponent.color.g, m_TextComponent.color.b, alphaB);
        
        Color32[] newVertexColors;
        Color32 c0 = m_TextComponent.color;
        Color32 c1 = m_TextComponent.color;

        var revealParams = TextRevealAnimatorCalculatedParams.Calculate(textInfo, animationProgress, characterFadeWidth);
        
        var currentCharacterDistance = 0f;
        for (int i = 0; i < textInfo.characterInfo.Length; i++) {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            
            if (!characterInfo.isVisible) continue;
            
            // Get the index of the material used by the current character.
            int materialIndex = characterInfo.materialReferenceIndex;

            // Get the vertex colors of the mesh used by this text element (character or sprite).
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            Debug.Assert(newVertexColors.Length > 0);
            // Get the index of the first vertex used by this text element.
            int vertexIndex = characterInfo.vertexIndex;
            
            var characterWidth = TMPUtils.GetCharacterWidth(characterInfo);

            
            var l0 = revealParams.GetStrength(currentCharacterDistance); 
            if(renderMode == TextFaderRenderMode.Alpha) c0 = Color32.Lerp(alphaColorA, alphaColorB, l0);
            else if(renderMode == TextFaderRenderMode.Color) c0 = Color32.Lerp(colorA, colorB, l0);

            var l1 = revealParams.GetStrength(currentCharacterDistance+characterWidth);
            if(renderMode == TextFaderRenderMode.Alpha) c1 = Color32.Lerp(alphaColorA, alphaColorB, l1);
            else if(renderMode == TextFaderRenderMode.Color) c1 = Color32.Lerp(colorA, colorB, l1);

            newVertexColors[vertexIndex + 0] = c0;
            newVertexColors[vertexIndex + 1] = c0;
            newVertexColors[vertexIndex + 2] = c1;
            newVertexColors[vertexIndex + 3] = c1;
            
            textInfo.meshInfo[materialIndex].colors32 = newVertexColors;

            currentCharacterDistance += characterWidth;
        }
    }
}