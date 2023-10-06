using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StrokeGeometryAttributes {
    public float width;        // 1 if not defined
    public float extrusion;        // 0 if not defined
    public Cap cap;          // butt, round, square
    public Join join;          // bevel, round, miter
    public float miterLimit;   // for join miter, the maximum angle value to use the miter
    public bool closed;   // for join miter, the maximum angle value to use the miter

    public StrokeGeometryAttributes (float width, float extrusion, Cap cap, Join join, float miterLimit, bool closed) {
        this.width = width;
        this.extrusion = extrusion;
        this.cap = cap;
        this.join = join;
        this.miterLimit = miterLimit;
        this.closed = closed;
    }
    public StrokeGeometryAttributes @default {
        get {
            return new StrokeGeometryAttributes(1, 0, Cap.Butt, Join.Bevel, 10, true);
        }
    }

}
public enum Cap {
    Butt,
    Round,
    Square
}
public enum Join {
    Bevel,
    Round, 
    Miter
}
public static class LineDraw {
    const float Epsilon = 0.0001f;
    public static List<Vector2> getStrokeGeometry(Vector2[] points, StrokeGeometryAttributes attrs) {
        if (points == null || points.Length < 2 || attrs.width == 0) {
            return null;
        }

        var cap = attrs.cap;
        var join = attrs.join;
        var lineWidth = (attrs.width) / 2f;
        var miterLimit = attrs.miterLimit;
        List<Vector2> vertices = new List<Vector2>();
        var closed = attrs.closed;

        if (points.Length == 2) {
            join = Join.Bevel;
            createTriangles(points[0], Vector2.Lerp(points[0], points[1], 0.5f), points[1], vertices, lineWidth, join, miterLimit, attrs.extrusion);
        } else {

            // if (points[0] == points[points.Length - 1] ||
            //         (points[0].x == points[points.Length - 1].x && points[0].y == points[points.Length - 1].y)) {

            //     var p0 = points.shift();
            //     p0 = Point.Middle(p0, points[0]);
            //     points.unshift(p0);
            //     points.push(p0);
            //     closed= true;
            // }
            

            

            
            if(attrs.closed) {
                for (int i = 0; i < points.Length; i++) {
                    var startMid = Vector2.Lerp(i == 0 ? points[points.Length-1] : points[i-1], points[i], 0.5f);
                    var endMid = Vector2.Lerp(points[i], i == points.Length-1 ? points[0] : points[i+1], 0.5f);
                    createTriangles(startMid, points[i], endMid, vertices, lineWidth, join, miterLimit, attrs.extrusion);
                }
            } else {
                List<Vector2> middlePoints = new List<Vector2>();  // middle points per each line segment.
                for (int i = 0; i < points.Length - 1; i++) {
                    if (i == 0) {
                        middlePoints.Add(points[0]);
                    } else if (i == points.Length - 2) {
                        middlePoints.Add(points[points.Length - 1]);
                    } else {
                        middlePoints.Add(Vector2.Lerp(points[i], points[i + 1], 0.5f));
                    }
                }
                for (int i = 1; i < middlePoints.Count; i++) {
                    createTriangles(middlePoints[i - 1], points[i], middlePoints[i], vertices, lineWidth, join, miterLimit, attrs.extrusion);
                }
            }
        }

        if (!closed) {

            if (cap == Cap.Round) {

                var p00 = vertices[0];
                var p01 = vertices[1];
                var p02 = points[1];
                var p10 = vertices[vertices.Count - 1];
                var p11 = vertices[vertices.Count - 3];
                var p12 = points[points.Length - 2];

                createRoundCap(points[0], p00, p01, p02, vertices);
                createRoundCap(points[points.Length - 1], p10, p11, p12, vertices);

            } else if (cap == Cap.Square) {

                var p00 = vertices[vertices.Count - 1];
                var p01 = vertices[vertices.Count - 3];

                createSquareCap(
                        vertices[0],
                        vertices[1],
                        (points[0]-points[1]).normalized * ((points[0]-vertices[0]).magnitude),
                        vertices);
                createSquareCap(
                        p00,
                        p01,
                        (points[points.Length - 1] - points[points.Length - 2]).normalized * ((p01 - points[points.Length - 1]).magnitude),
                        vertices);
            }
        }

        return vertices;
    }

    static void createSquareCap(Vector2 p0, Vector2 p1, Vector2 dir, List<Vector2> verts) {

        verts.Add(p0);
        verts.Add(p0+dir);
        verts.Add(p1+dir);

        verts.Add(p1);
        verts.Add(p1+dir);
        verts.Add(p0);
    }

    static void createRoundCap(Vector2 center, Vector2 _p0, Vector2 _p1, Vector2 nextPointInLine, List<Vector2> verts) {
        var radius = (center - _p0).magnitude;

        var angle0 = Mathf.Atan2((_p1.y - center.y), (_p1.x - center.x));
        var angle1 = Mathf.Atan2((_p0.y - center.y), (_p0.x - center.x));

        var orgAngle0 = angle0;

        if (angle1 > angle0) {
            if (angle1-angle0>=Mathf.PI-Epsilon) {
                angle1=angle1-2*Mathf.PI;
            }
        }
        else {
            if (angle0-angle1>=Mathf.PI-Epsilon) {
                angle0=angle0-2*Mathf.PI;
            }
        }
        var angleDiff = Mathf.DeltaAngle(angle0, angle1);
        if (Mathf.Abs(angleDiff) >= Mathf.PI - Epsilon && Mathf.Abs(angleDiff) <= Mathf.PI + Epsilon) {
            var r1 = (center - nextPointInLine);
            if (r1.x == 0) {
                if (r1.y > 0) {
                    angleDiff= -angleDiff;
                }
            } else if (r1.x >= -Epsilon) {
                angleDiff= -angleDiff;
            }
        }

        var nsegments = Mathf.Max(Mathf.CeilToInt((Mathf.Abs(angleDiff * radius) / 0.03f)), 1);
        var angleInc = angleDiff / nsegments;

        for (var i = 0; i < nsegments; i++) {
            verts.Add(new Vector2(center.x, center.y));
            verts.Add(new Vector2(
                    center.x + radius * Mathf.Cos(orgAngle0 + angleInc * i),
                    center.y + radius * Mathf.Sin(orgAngle0 + angleInc * i)
           ));
            verts.Add(new Vector2(
                    center.x + radius * Mathf.Cos(orgAngle0 + angleInc * (1 + i)),
                    center.y + radius * Mathf.Sin(orgAngle0 + angleInc * (1 + i))
           ));
        }
        // var roundedcorner = UnityX.Geometry.DrawingUtils.DrawRoundedCorner(center, _p0, _p1, radius, 5);
        // for (var i = 0; i < roundedcorner.Length-1; i++) {
        //     verts.Add(new Vector2(center.x, center.y));
        //     verts.Add(roundedcorner[i]);
        //     verts.Add(roundedcorner[i+1]);
        // }
    }

    static float signedArea(Vector2 p0, Vector2 p1, Vector2 p2) {
        return (p1.x - p0.x) * (p2.y - p0.y) - (p2.x - p0.x) * (p1.y - p0.y);
    }

    static Vector2? lineIntersection(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {

        var a0 = p1.y - p0.y;
        var b0 = p0.x - p1.x;

        var a1 = p3.y - p2.y;
        var b1 = p2.x - p3.x;

        var det = a0 * b1 - a1 * b0;
        if (det > -Epsilon && det < Epsilon) {
            return null;
        } else {
            var c0 = a0 * p0.x + b0 * p0.y;
            var c1 = a1 * p2.x + b1 * p2.y;

            var x = (b1 * c0 - b0 * c1) / det;
            var y = (a0 * c1 - a1 * c0) / det;
            return new Vector2(x, y);
        }
    }

    static void createTriangles(Vector2 p0, Vector2 p1, Vector2 p2, List<Vector2> verts, float width, Join join, float miterLimit, float extrusion) {
        
        Vector2 perpendicular (Vector2 v) {
            var x = v.x;
            v.x = -v.y;
            v.y = x;

            return v;
        }

        var normal0 = p1 - p0;
        var normal2 = p2 - p1;

        normal0 = perpendicular(normal0);
        normal2 = perpendicular(normal2);

        // triangle composed by the 3 points if clockwise or couterclockwise.
        // if counterclockwise, we must invert the line threshold points, otherwise the intersection point
        // could be erroneous and lead to odd results.
        if (signedArea(p0, p1, p2) > 0) {
            normal0 *= -1;
            normal2 *= -1;
        }

        normal0.Normalize();
        normal2.Normalize();
        normal0 *= width;
        normal2 *= width;

        var l1a = normal0+p0 + normal0 * extrusion;
        var l1b = normal0+p1 + normal0 * extrusion;
        var l2a = normal2+p2 + normal2 * extrusion;
        var l2b = normal2+p1 + normal2 * extrusion;

        var pintersect = lineIntersection(l1a, l1b, l2a, l2b);

        Vector2 anchor = Vector2.zero;
        var anchorLength= float.MaxValue;
        if (pintersect != null) {
            anchor= (Vector2)pintersect - p1;
            anchorLength= anchor.magnitude;
        }
        var dd = width != 0 ? (anchorLength / width) : 0;
        var p0p1= p0 -p1;
        var p0p1Length= p0p1.magnitude;
        var p1p2= p1 -p2;
        var p1p2Length= p1p2.magnitude;

        /**
            * the cross point exceeds any of the segments dimension.
            * do not use cross point as reference.
            */
        if (anchorLength>p0p1Length || anchorLength>p1p2Length) {

            verts.Add(p0+normal0);
            verts.Add(p0 - normal0);
            verts.Add(p1+normal0);

            verts.Add(p0 - normal0);
            verts.Add(p1+normal0);
            verts.Add(p1 - normal0);

            if (join == Join.Round) {
                createRoundCap(p1, p1+normal0, p1+normal2, p2, verts);
            } else if (join==Join.Bevel || join==Join.Miter) {
                verts.Add(p1);
                verts.Add(p1+normal0);
                verts.Add(p1+normal2);
            } else if (join == Join.Miter && dd<miterLimit && pintersect != null) {

                verts.Add(p1+normal0);
                verts.Add(p1);
                verts.Add((Vector2)pintersect);

                verts.Add(p1+normal2);
                verts.Add(p1);
                verts.Add((Vector2)pintersect);
            }

            verts.Add(p2 + normal2);
            verts.Add(p1 - normal2);
            verts.Add(p1 + normal2);

            verts.Add(p2 + normal2);
            verts.Add(p1 - normal2);
            verts.Add(p2 - normal2);


        } else {

            verts.Add(p0 + normal0);
            verts.Add(p0 - normal0);
            verts.Add(p1 - anchor);

            verts.Add(p0 + normal0);
            verts.Add(p1 - anchor);
            verts.Add(p1 + normal0);

            if (join == Join.Round) {

                var _p0 = p1 + normal0;
                var _p1 = p1 + normal2;
                var _p2 = p1-anchor;

                var center = p1;

                verts.Add(_p0);
                verts.Add(center);
                verts.Add(_p2);

                createRoundCap(center, _p0, _p1, _p2, verts);

                verts.Add(center);
                verts.Add(_p1);
                verts.Add(_p2);

            } else {

                if (join == Join.Bevel || join == Join.Miter) {
                    verts.Add(p1+normal0);
                    verts.Add(p1+normal2);
                    verts.Add((p1 - anchor));
                }

                if (join == Join.Miter && dd < miterLimit) {
                    verts.Add((Vector2)pintersect);
                    verts.Add(p1+normal0);
                    verts.Add(p1+normal2);
                }
            }

            verts.Add(p2+normal2);
            verts.Add(p1 - anchor);
            verts.Add(p1+normal2);

            verts.Add(p2+normal2);
            verts.Add(p1 - anchor);
            verts.Add(p2 - normal2);
        }
    }
}