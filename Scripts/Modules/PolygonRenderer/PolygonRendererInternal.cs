using UnityEngine;

// Small helpers inlined from UnityX's MathX / RectX / ObjectX / GizmosX so the PolygonRenderer package
// has no dependency on those Assembly-CSharp extension files. Behaviour matches the originals verbatim.
static class PolygonRendererInternal {

    // MathX.DegreesToVector2
    public static Vector2 DegreesToVector2 (float degrees) {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
    }

    // RectX.SplatVector — projects a direction from the rect centre out to the rect's edge.
    public static Vector2 SplatVector (Rect rect, Vector2 vector) {
        if (vector == Vector2.zero) return rect.center;
        float vecAspect = Mathf.Abs(vector.x / vector.y);
        float rectAspect = rect.size.x / rect.size.y;
        float scale = vecAspect > rectAspect
            ? Mathf.Abs((0.5f * rect.size.x) / vector.x)
            : Mathf.Abs((0.5f * rect.size.y) / vector.y);
        return (scale * vector) + rect.center;
    }

    // ObjectX.DestroyAutomatic — Destroy at runtime, DestroyImmediate in the editor.
    public static void DestroyAutomatic (Object o) {
        #if UNITY_EDITOR
        if (Application.isPlaying) Object.Destroy(o);
        else Object.DestroyImmediate(o);
        #else
        Object.Destroy(o);
        #endif
    }

    // GizmosX.DrawWireRect (XY-plane Rect) — draws the four edges with the current Gizmos.matrix/color.
    public static void DrawWireRect (Rect rect) {
        Vector3 bl = new Vector3(rect.xMin, rect.yMin);
        Vector3 br = new Vector3(rect.xMax, rect.yMin);
        Vector3 tr = new Vector3(rect.xMax, rect.yMax);
        Vector3 tl = new Vector3(rect.xMin, rect.yMax);
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }

    // GizmosX.DrawArrowLine — a line with an arrowhead at 75% of its length.
    public static void DrawArrowLine (Vector3 fromPosition, Vector3 toPosition, Vector3 crossVector) {
        if (fromPosition == toPosition) return;
        Gizmos.DrawLine(fromPosition, toPosition);
        Vector3 fromTo = toPosition - fromPosition;
        Vector3 position = fromPosition + fromTo * 0.75f;
        Quaternion rotation = Quaternion.LookRotation(fromTo, crossVector);
        float arrowSize = fromTo.magnitude * 0.05f;
        Vector3 start = position + (rotation * Vector3.back * arrowSize);
        Vector3 end = position + (rotation * Vector3.forward * arrowSize);
        Gizmos.DrawLine(start + (rotation * Vector3.left * arrowSize), end);
        Gizmos.DrawLine(start + (rotation * Vector3.right * arrowSize), end);
    }
}
