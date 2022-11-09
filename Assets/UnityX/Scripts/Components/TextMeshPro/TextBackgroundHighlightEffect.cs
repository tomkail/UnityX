using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

[RequireComponent(typeof(CanvasRenderer))]
public class TextBackgroundHighlightEffect : MaskableGraphic {
    [SerializeField]
    private TextMeshProUGUI text;
    public RectOffset padding;
    
    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();
        if(text != null) {
            var m_TextInfo = text.textInfo;
            int lineCount = m_TextInfo.lineCount;

            for (int i = 0; i < lineCount; i++) {
                TMP_LineInfo lineInfo = m_TextInfo.lineInfo[i];
                var min = transform.InverseTransformPoint(text.transform.TransformPoint(lineInfo.lineExtents.min));
                var max = transform.InverseTransformPoint(text.transform.TransformPoint(lineInfo.lineExtents.max));
                var rect = Rect.MinMaxRect(min.x-padding.left, min.y-padding.bottom, max.x+padding.right, max.y+padding.top);
                MakeQuad(vh, rect, color);
            }
        }
    }

    UIVertex[] MakeQuad(VertexHelper vh, Rect rect, Color color)
    {
        UIVertex[] verts = new UIVertex[4];
        verts[0].position = new Vector3(rect.xMin, rect.yMin, 0);
        verts[0].uv0 = new Vector2(0, 0);
        verts[1].position = new Vector3(rect.xMax, rect.yMin, 0);
        verts[1].uv0 = new Vector2(1, 0);
        verts[2].position = new Vector3(rect.xMax, rect.yMax, 0);
        verts[2].uv0 = new Vector2(1, 1);
        verts[3].position = new Vector3(rect.xMin, rect.yMax, 0);
        verts[3].uv0 = new Vector2(0, 1);

        verts[0].color = color;
        verts[1].color = color;
        verts[2].color = color;
        verts[3].color = color;

        vh.AddUIVertexQuad(verts);
        return verts;
    }
    /*
    void OnDrawGizmos()
    {
        var m_Transform = text.transform;

        // Get a reference to the text object's textInfo
        var m_TextInfo = text.textInfo;
        
        var m_HandleSize = HandleUtility.GetHandleSize(m_Transform.position) * 1;

        int lineCount = m_TextInfo.lineCount;

        for (int i = 0; i < lineCount; i++)
        {
            TMP_LineInfo lineInfo = m_TextInfo.lineInfo[i];
            TMP_CharacterInfo firstCharacterInfo = m_TextInfo.characterInfo[lineInfo.firstCharacterIndex];
            TMP_CharacterInfo lastCharacterInfo = m_TextInfo.characterInfo[lineInfo.lastCharacterIndex];

            bool isLineVisible = (lineInfo.characterCount == 1 && (firstCharacterInfo.character == 10 || firstCharacterInfo.character == 11 || firstCharacterInfo.character == 0x2028 || firstCharacterInfo.character == 0x2029)) ||
                                    i > text.maxVisibleLines ||
                                    (text.overflowMode == TextOverflowModes.Page && firstCharacterInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

            if (!isLineVisible) continue;

            float lineBottomLeft = firstCharacterInfo.bottomLeft.x;
            float lineTopRight = lastCharacterInfo.topRight.x;

            float ascentline = lineInfo.ascender;
            float baseline = lineInfo.baseline;
            float descentline = lineInfo.descender;

            float dottedLineSize = 12;

            // Draw line extents
            var min = m_Transform.TransformPoint(lineInfo.lineExtents.min);
            var max = m_Transform.TransformPoint(lineInfo.lineExtents.max);
            DrawDottedRectangle(min, max, Color.green, 4);
            

            // Draw Ascent line
            Vector3 ascentlineStart = m_Transform.TransformPoint(new Vector3(lineBottomLeft, ascentline, 0));
            Vector3 ascentlineEnd = m_Transform.TransformPoint(new Vector3(lineTopRight, ascentline, 0));

            Handles.color = Color.yellow;
            Handles.DrawDottedLine(ascentlineStart, ascentlineEnd, dottedLineSize);

            // Draw Base line
            Vector3 baseLineStart = m_Transform.TransformPoint(new Vector3(lineBottomLeft, baseline, 0));
            Vector3 baseLineEnd = m_Transform.TransformPoint(new Vector3(lineTopRight, baseline, 0));

            Handles.color = Color.yellow;
            Handles.DrawDottedLine(baseLineStart, baseLineEnd, dottedLineSize);

            // Draw Descent line
            Vector3 descentLineStart = m_Transform.TransformPoint(new Vector3(lineBottomLeft, descentline, 0));
            Vector3 descentLineEnd = m_Transform.TransformPoint(new Vector3(lineTopRight, descentline, 0));

            Handles.color = Color.yellow;
            Handles.DrawDottedLine(descentLineStart, descentLineEnd, dottedLineSize);

            // Draw text labels for metrics
            if (m_HandleSize < 1.0f)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
                style.fontSize = 12;
                style.fixedWidth = 200;
                style.fixedHeight = 20;
                Vector3 labelPosition;

                // Ascent Line
                labelPosition = m_Transform.TransformPoint(new Vector3(lineBottomLeft, ascentline, 0));
                style.padding = new RectOffset(0, 10, 0, 5);
                style.alignment = TextAnchor.MiddleRight;
                Handles.Label(labelPosition, "Ascent Line", style);

                // Base Line
                labelPosition = m_Transform.TransformPoint(new Vector3(lineBottomLeft, baseline, 0));
                Handles.Label(labelPosition, "Base Line", style);

                // Descent line
                labelPosition = m_Transform.TransformPoint(new Vector3(lineBottomLeft, descentline, 0));
                Handles.Label(labelPosition, "Descent Line", style);
            }
        }

        void DrawDottedRectangle(Vector3 bottomLeft, Vector3 topRight, Color color, float size = 5.0f)
        {
            Handles.color = color;
            Handles.DrawDottedLine(bottomLeft, new Vector3(bottomLeft.x, topRight.y, bottomLeft.z), size);
            Handles.DrawDottedLine(new Vector3(bottomLeft.x, topRight.y, bottomLeft.z), topRight, size);
            Handles.DrawDottedLine(topRight, new Vector3(topRight.x, bottomLeft.y, bottomLeft.z), size);
            Handles.DrawDottedLine(new Vector3(topRight.x, bottomLeft.y, bottomLeft.z), bottomLeft, size);
        }
    }
    */
}