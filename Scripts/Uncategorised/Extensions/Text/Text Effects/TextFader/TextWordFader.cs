using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class TextWordFader : BaseTextMeshProEffect {
    [SerializeField, Range(0,1)]
    float _strength = 1;
    public float strength {
        get => _strength;
        set {
            if(_strength == value) return;
            _strength = value;
            SetDirty();
        }
    }
    
    [SerializeField]
    Color32 _highlightedColor = Color.white;
    public Color32 highlightedColor {
        get => _highlightedColor;
        set {
            if(_highlightedColor.Compare(value)) return;
            _highlightedColor = value;
            SetDirty();
        }
    }

    [SerializeField]
    Range _highlightedWordRange;
    public Range highlightedWordRange {
        get => _highlightedWordRange;
        set {
            if(_highlightedWordRange == value) return;
            _highlightedWordRange = value;
            SetDirty();
        }
    }
    
    protected override void OnPreRenderText(TMP_TextInfo textInfo) {
        if (textInfo.characterCount == 0) return;
        if (strength <= 0) return;
        if (highlightedWordRange.length <= 0) return;
        
        Color32[] newVertexColors;
        Color32 c0 = m_TextComponent.color;

        var firstWordStart = Mathf.FloorToInt(highlightedWordRange.min);
        var firstWordInfo = textInfo.wordInfo[Mathf.Clamp(firstWordStart, 0, textInfo.wordCount-1)];
        var firstWordCharacterFadeStart = firstWordInfo.firstCharacterIndex;
        var firstWordCharacterFadeEnd = firstWordInfo.lastCharacterIndex;
        
        var lastWordStart = Mathf.FloorToInt(highlightedWordRange.max);
        var lastWordInfo = textInfo.wordInfo[Mathf.Clamp(lastWordStart, 0, textInfo.wordCount-1)];
        var lastWordCharacterFadeStart = lastWordInfo.firstCharacterIndex;
        
        // Using the next word ensures we capture any punctuation at the end of the previous word (punctuation is excluded from wordInfo)
        var lastWordCharacterFadeEnd = textInfo.characterCount-1;
        if(lastWordStart < textInfo.wordCount-1) {
            var nextWordInfo = textInfo.wordInfo[Mathf.Clamp(lastWordStart+1, 0, textInfo.wordCount-1)];
            lastWordCharacterFadeEnd = nextWordInfo.firstCharacterIndex - 1;
        }
        
        for (int i = firstWordCharacterFadeStart; i <= lastWordCharacterFadeEnd; i++) {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            if (!characterInfo.isVisible) continue;
            
            var characterStrength = 1f;
            if (i >= lastWordCharacterFadeStart) {
                characterStrength = highlightedWordRange.max - lastWordStart;
                if (lastWordStart > textInfo.wordCount - 1) characterStrength = 1;
            } else if (i <= firstWordCharacterFadeEnd) {
                characterStrength = 1 - (highlightedWordRange.min - firstWordStart);
                if (firstWordStart < 0) characterStrength = 1;
            }
       
            // Get the index of the material used by the current character.
            int materialIndex = characterInfo.materialReferenceIndex;

            // Get the vertex colors of the mesh used by this text element (character or sprite).
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            Debug.Assert(newVertexColors.Length > 0);
            // Get the index of the first vertex used by this text element.
            
            int vertexIndex = characterInfo.vertexIndex;
            
            newVertexColors[vertexIndex + 0] = Color32.Lerp(newVertexColors[vertexIndex + 0], highlightedColor, characterStrength * strength);
            newVertexColors[vertexIndex + 1] = Color32.Lerp(newVertexColors[vertexIndex + 1], highlightedColor, characterStrength * strength);
            newVertexColors[vertexIndex + 2] = Color32.Lerp(newVertexColors[vertexIndex + 2], highlightedColor, characterStrength * strength);
            newVertexColors[vertexIndex + 3] = Color32.Lerp(newVertexColors[vertexIndex + 3], highlightedColor, characterStrength * strength);
            
            textInfo.meshInfo[materialIndex].colors32 = newVertexColors;
        }
    }
}