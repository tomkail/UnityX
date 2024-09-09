using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class TextSpriteColorOverride : BaseTextMeshProEffect {
    [SerializeField]
    Color32 _color = Color.white;
    public Color32 color {
        get => _color;
        set {
            if(_color.Compare(value)) return;
            _color = value;
            SetDirty();
        }
    }
    
    protected override void OnPreRenderText(TMP_TextInfo textInfo) {
        if (textInfo.characterCount == 0) return;
        Color32[] newVertexColors;
        
        for (int i = 0; i < textInfo.characterInfo.Length; i++) {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            
            if (!characterInfo.isVisible) continue;
            if (characterInfo.elementType != TMP_TextElementType.Sprite) continue;
            
            int materialIndex = characterInfo.materialReferenceIndex;
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            Debug.Assert(newVertexColors.Length > 0);
            int vertexIndex = characterInfo.vertexIndex;
            
            newVertexColors[vertexIndex + 0] = color;
            newVertexColors[vertexIndex + 1] = color;
            newVertexColors[vertexIndex + 2] = color;
            newVertexColors[vertexIndex + 3] = color;
            
            textInfo.meshInfo[materialIndex].colors32 = newVertexColors;
        }
    }
}