using TMPro;
using UnityEngine;

public class CurvedWorldTextEffect : BaseTextMeshProEffect {
    protected override void OnPreRenderText(TMP_TextInfo textInfo) {
        if (textInfo.characterCount == 0) return;

        foreach (var characterInfo in textInfo.characterInfo) {
            if (!characterInfo.isVisible) continue;
            
            // Get the index of the material used by the current character.
            int materialIndex = characterInfo.materialReferenceIndex;
        
            // Get the index of the first vertex used by this text element.
            int vertexIndex = characterInfo.vertexIndex;
        
            // var characterWidth = TMPUtils.GetCharacterWidth(characterInfo);
        
            // var strength = revealParams.GetStrengthMinusOne(currentCharacterDistance);
            // var heightOffset = Mathf.Sin(strength * Mathf.PI);
            var vertices = textInfo.meshInfo[materialIndex].vertices;
            vertices[vertexIndex + 0] = PerformVertexEffect(vertices[vertexIndex + 0], out float alpha0);
            vertices[vertexIndex + 1] = PerformVertexEffect(vertices[vertexIndex + 1], out float alpha1);
            vertices[vertexIndex + 2] = PerformVertexEffect(vertices[vertexIndex + 2], out float alpha2);
            vertices[vertexIndex + 3] = PerformVertexEffect(vertices[vertexIndex + 3], out float alpha3);
            //
            // currentCharacterDistance += characterWidth;
            
            
            
            var newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            
            newVertexColors[vertexIndex + 0] = MultiplyAlpha(newVertexColors[vertexIndex + 0], alpha0);
            newVertexColors[vertexIndex + 1] = MultiplyAlpha(newVertexColors[vertexIndex + 1], alpha1);
            newVertexColors[vertexIndex + 2] = MultiplyAlpha(newVertexColors[vertexIndex + 2], alpha2);
            newVertexColors[vertexIndex + 3] = MultiplyAlpha(newVertexColors[vertexIndex + 3], alpha3);
            
            textInfo.meshInfo[materialIndex].colors32 = newVertexColors;
        }
    }

    static Color32 MultiplyAlpha(Color32 color, double alpha) {
        var a = color.a * alpha;
        return new Color32(color.r, color.g, color.b, (byte)a);
    }
    
    [Range(0,1)]
    public float pivot = 0.5f;

    Vector3 PerformVertexEffect(Vector3 vertex, out float alpha) {
        var m = Matrix4x4.TRS(new Vector3(0, Mathf.Lerp(m_TextComponent.rectTransform.rect.yMin, m_TextComponent.rectTransform.rect.yMax,pivot), 0), Quaternion.identity, new Vector3(1,-m_TextComponent.rectTransform.rect.size.y*0.5f,1));
        var normalized = m.inverse.MultiplyPoint3x4(vertex);
        
        alpha = (normalized.y > -1f && normalized.y < 1 ? 1 : 0);
        normalized.y = (Mathf.Sin(normalized.y));
        alpha *= 1-Mathf.Abs(normalized.y);
        return m.MultiplyPoint3x4(normalized);
    }

    // void OnDrawGizmos() {
    //     for (int y = 0; y < 50; y++) {
    //         // Debug.Log(Mathf.Sin((y / 50f) * Mathf.PI * 0.5f) * 50);
    //         for (int x = 0; x < 50; x++) {
    //             var offset = new Vector3(x,Mathf.Sin((y / 50f) * Mathf.PI * 0.5f)*50, (Mathf.Cos(y / 50f * Mathf.PI * 0.5f) - 1)*-50);
    //             var pos = offset;
    //             Gizmos.DrawSphere(pos,0.5f);
    //         }
    //         
    //     }
    // }
}