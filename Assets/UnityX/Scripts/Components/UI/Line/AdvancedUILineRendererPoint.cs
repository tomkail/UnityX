using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct AdvancedUILineRendererPoint {
    public Vector2 point;
    public float width;
    public Color color;
    public AdvancedUILineRendererPoint (Vector2 point, float width, Color color) {
        this.point = point;
        this.width = width;
        this.color = color;
    }

    public static AdvancedUILineRendererPoint[] GetLineRendererPoints (IEnumerable<Vector2> points, float width, Color color) {
        var linePoints = new AdvancedUILineRendererPoint[points.Count()];
        int i = 0;
        foreach(var point in points) {
            linePoints[i] = new AdvancedUILineRendererPoint(point, width, color);
            i++;
        }
        return linePoints;
    }
}
