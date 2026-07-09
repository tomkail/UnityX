using UnityEngine;

// Helpers inlined from UnityX's RectTransformX / OnGUIX / MathX so this package has no dependency on those
// Assembly-CSharp extension files. ScreenPointToNormalizedPointInRectangle is used by the core logic;
// ScreenToGUIPoint / DrawCircle / DrawLine are used only by the touch simulator's debug OnGUI. Bodies
// match the originals verbatim.
static class MultitouchDraggableInternal {

    // RectTransformX.ScreenPointToNormalizedPointInRectangle
    public static bool ScreenPointToNormalizedPointInRectangle (RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 normalizedPosition) {
        normalizedPosition = default;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, cam, out var localPosition)) return false;
        var r = rect.rect;
        normalizedPosition = new Vector2((localPosition.x - r.x) / r.width, (localPosition.y - r.y) / r.height);
        normalizedPosition += rect.pivot - (Vector2.one * 0.5f);
        return true;
    }

    // OnGUIX.ScreenToGUIPoint
    public static Vector2 ScreenToGUIPoint (Vector2 point) {
        return new Vector2(point.x, Screen.height - point.y);
    }

    // OnGUIX.DrawCircle
    public static void DrawCircle (Vector2 center, float radius, Color color, float width, int numPoints = 20) {
        if (numPoints < 2) return;
        var step = Mathf.PI * 2f / (numPoints - 1);
        Vector2 lastOffset = RadiansToVector2(0) * radius;
        for (int i = 1; i < numPoints; i++) {
            var offset = RadiansToVector2(i * step) * radius;
            DrawLine(center + lastOffset, center + offset, color, width);
            lastOffset = offset;
        }
    }

    // MathX.RadiansToVector2
    static Vector2 RadiansToVector2 (float radians) {
        return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
    }

    // OnGUIX.DrawLine (IMGUI) — draws a 1x1 white texture stretched/rotated into a line.
    static readonly Vector3 lineOffset = new Vector3(0, -0.5f, 0); // compensate for line width
    static readonly Matrix4x4 guiTransMat = Matrix4x4.TRS(lineOffset, Quaternion.identity, Vector3.one);
    static readonly Matrix4x4 guiTransMatInv = Matrix4x4.TRS(-lineOffset, Quaternion.identity, Vector3.one);
    public static void DrawLine (Vector2 pointA, Vector2 pointB, Color color, float width) {
        if (width <= 0 || pointA == pointB || color.a == 0) return;
        Matrix4x4 matrix = GUI.matrix;
        Color savedColor = GUI.color;
        GUI.color = color;
        var delta = (Vector3)(pointB - pointA);
        Quaternion guiRot = Quaternion.FromToRotation(Vector2.right, delta);
        Matrix4x4 guiRotMat = Matrix4x4.TRS(pointA, guiRot, new Vector3(delta.magnitude, width, 1));
        GUI.matrix = guiTransMatInv * guiRotMat * guiTransMat;
        GUI.DrawTexture(new Rect(0, 0, 1, 1), Texture2D.whiteTexture);
        GUI.matrix = matrix;
        GUI.color = savedColor;
    }
}
