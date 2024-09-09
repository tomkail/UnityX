using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class WorldSpaceTextGradient : BaseTextMeshProEffect {
    public GradientArea gradientArea;

    // protected override void Update() {
    //     SetDirty();
    //     base.Update();
    // }

    protected override void OnPreRenderText(TMP_TextInfo textInfo) {
        if (textInfo.characterCount == 0) return;
        Color32[] newVertexColors;
        
        Matrix4x4 matrix = m_TextComponent.transform.localToWorldMatrix;
        
        for (int i = 0; i < textInfo.characterInfo.Length; i++) {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            
            if (!characterInfo.isVisible) continue;
            
            int materialIndex = characterInfo.materialReferenceIndex;
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            Debug.Assert(newVertexColors.Length > 0);
            int vertexIndex = characterInfo.vertexIndex;
            

            var vertices = textInfo.meshInfo[materialIndex].vertices;
            newVertexColors[vertexIndex + 0] = gradientArea.EvaluateAtPosition(matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]));
            newVertexColors[vertexIndex + 1] = gradientArea.EvaluateAtPosition(matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]));
            newVertexColors[vertexIndex + 2] = gradientArea.EvaluateAtPosition(matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]));
            newVertexColors[vertexIndex + 3] = gradientArea.EvaluateAtPosition(matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]));
            
            textInfo.meshInfo[materialIndex].colors32 = newVertexColors;
        }
    }
}