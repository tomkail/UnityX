using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class PolygonRenderer : BasePolygonRenderer {
    [Space]
    public const string colorID = "_Color";
    [SerializeField]
    Color _tintColor = Color.white;
    public Color tintColor {
        get {
            return _tintColor;
        } set {
            if(_tintColor == value) return;
            _tintColor = value;
            RefreshMaterialPropertyBlock();
        }
    }
    public const string textureID = "_MainTex";
    [SerializeField]
    Texture2D _texture;
    public Texture2D texture {
        get {
            return _texture;
        } set {
            if(_texture == value) return;
            _texture = value;
            RefreshMaterialPropertyBlock();
        }
    }
    
    [Space]
    // public bool front = true;
	// public bool back;

    [Space]
    public bool flipUVX;
    public bool flipUVY;
    public UVMode uvMode;
    public enum UVMode {
        Rect,
        Shape
    }
    public float uvXAngle = 0;
    public float uvYAngle = 90;


    MaterialPropertyBlock _propBlock = null;
    MaterialPropertyBlock propBlock {
        get {
            if(_propBlock == null) _propBlock = new MaterialPropertyBlock();
            return _propBlock;
        }
    }

	public override void RebuildMesh () {
		GetMesh();
		mesh.Clear();
        
        var polygonRect = polygon.GetRect();
        var points = polygon.vertices;

		// if(doubleSided) {
		// 	var verts = points.Select(v => new Vector3(v.x, v.y, 0)).ToArray();
		// 	var tris = new Triangulator(points).Triangulate();

		// 	var doubleVerts = new Vector3[verts.Length * 2];
		// 	for(int i = 0; i < verts.Length; i++) doubleVerts[i] = doubleVerts[i+verts.Length] = verts[i];

		// 	var doubleTris = new int[tris.Length * 2];
		// 	int triLengthMinusOne = tris.Length-1;
		// 	for(int i = 0; i < tris.Length; i++) {
		// 		doubleTris[i] = tris[i];
		// 		doubleTris[i+tris.Length] = tris[triLengthMinusOne - i] + points.Length;
		// 	}
		// 	mesh.vertices = doubleVerts;
		// 	mesh.triangles = doubleTris;
		// } else {
			
        // }

        // if(back) {
            mesh.vertices = points.Select(v => new Vector3(v.x, v.y, 0)).ToArray();
			
            List<int> triangles = new List<int>();
            Triangulator.GenerateIndices(points, triangles);
            mesh.SetTriangles(triangles, 0);
            
            mesh.uv = RecalculateUVs(polygonRect, points);
            mesh.colors = RecalculateColors(polygonRect, points);
        // }

		mesh.RecalculateNormals();
        
        if(meshFilter != null)
            meshFilter.mesh = mesh;
	}


    Vector2[] RecalculateUVs (Rect polygonRect, Vector2[] points) {
        Vector2[] uvs = new Vector2[points.Length];
            
        if(uvMode == UVMode.Rect) {
            for(int i = 0; i < points.Length; i++) {
                uvs[i] = Rect.PointToNormalized(polygonRect, points[i]);
            }
        } else if(uvMode == UVMode.Shape) {
            Vector2 uvXDirection = MathX.DegreesToVector2(uvXAngle);
            Vector2 uvYDirection = MathX.DegreesToVector2(uvYAngle);
            var distanceXMin = Mathf.Infinity;
            var distanceXMax = Mathf.NegativeInfinity;
            var distanceYMin = Mathf.Infinity;
            var distanceYMax = Mathf.NegativeInfinity;
            foreach(var vert in points) {
                var distanceX = Vector2.Dot(vert, uvXDirection);
                distanceXMin = Mathf.Min(distanceX, distanceXMin);
                distanceXMax = Mathf.Max(distanceX, distanceXMax);
                
                var distanceY = Vector2.Dot(vert, uvYDirection);
                distanceYMin = Mathf.Min(distanceY, distanceYMin);
                distanceYMax = Mathf.Max(distanceY, distanceYMax);
            }
            for(int i = 0; i < points.Length; i++) {
                var distanceY = Vector2.Dot(points[i], uvYDirection);
                var distanceX = Vector2.Dot(points[i], uvXDirection);
                var x = Mathf.InverseLerp(distanceXMin, distanceXMax, distanceX);
                if(flipUVX) x = 1f-x;
                var y = Mathf.InverseLerp(distanceYMin, distanceYMax, distanceY);
                if(flipUVY) y = 1f-y;
                uvs[i] = new Vector2(x,y);
            }
        }
        return uvs;
    }

    Color[] RecalculateColors (Rect polygonRect, Vector2[] points) {
        if(colorMode == ColorMode.Rect || colorMode == ColorMode.Shape) {
            Color[] colors = new Color[points.Length];
            Vector2 colorDirection = MathX.DegreesToVector2(colorAngle+90);
            Vector2 oppositeColorDirection = MathX.DegreesToVector2(colorAngle+90+180);
            if(colorMode == ColorMode.Rect) {
                var startPoint = RectX.SplatVector(polygonRect, colorDirection);
                var endPoint = RectX.SplatVector(polygonRect, oppositeColorDirection);
                for(int i = 0; i < points.Length; i++) {
                    Line3D line = new Line3D(startPoint, endPoint);
                    var n = line.GetNormalizedDistanceOnLine(points[i]);
                    colors[i] = gradient.Evaluate(n);
                }
            } else if(colorMode == ColorMode.Shape) {
                var distanceColorMin = Mathf.Infinity;
                var distanceColorMax = Mathf.NegativeInfinity;
                foreach(var vert in points) {
                    var colorDir = Vector2.Dot(vert, colorDirection);
                    distanceColorMin = Mathf.Min(colorDir, distanceColorMin);
                    distanceColorMax = Mathf.Max(colorDir, distanceColorMax);
                }
                for(int i = 0; i < points.Length; i++) {
                    var colorDir = Vector2.Dot(points[i], colorDirection);
                    if(colorMode == ColorMode.Shape) {
                        var colorN = Mathf.InverseLerp(distanceColorMin, distanceColorMax, colorDir);
                        colors[i] = gradient.Evaluate(colorN);
                    }
                }

                // var distanceColorMin = Mathf.Infinity;
                // var distanceColorMax = Mathf.NegativeInfinity;
                // var pointColorMin = Vector2.zero;
                // var pointColorMax = Vector2.zero;
                // foreach(var vert in points) {
                //     var colorDir = Vector2.Dot(vert, colorDirection);
                //     if(colorDir < distanceColorMin) {
                //         distanceColorMin = colorDir;
                //         pointColorMin = vert;
                //     }
                //     if(colorDir > distanceColorMax) {
                //         distanceColorMax = colorDir;
                //         pointColorMax = vert;
                //     }
                // }
                // for(int i = 0; i < points.Length; i++) {
                //     Line3D line = new Line3D(pointColorMin, pointColorMax);
                //     var n = line.GetNormalizedDistanceOnLine(points[i]);
                //     colors[i] = gradient.Evaluate(n);
                // }
            }
           return colors;
        } else if (colorMode == ColorMode.White) {
            Color[] colors = new Color[points.Length];
            for(int i = 0; i < points.Length; i++) {
                colors[i] = Color.white;
            }
            return colors;
        } else return null;
    }

	public override void RefreshMaterialPropertyBlock () {
        if(meshRenderer != null) {
            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.Clear();
            propBlock.SetColor(colorID, tintColor);
            if(texture != null) propBlock.SetTexture(textureID, texture);
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }

    void OnDrawGizmosSelected () {
        GizmosX.BeginMatrix(transform.localToWorldMatrix);
        var polygonRect = polygon.GetRect();
        GizmosX.DrawWireRect(polygonRect);

        
        Vector2 colorDirection = MathX.DegreesToVector2(colorAngle+90);
        Vector2 oppositeColorDirection = MathX.DegreesToVector2(colorAngle+90+180);
        var startPoint = RectX.SplatVector(polygonRect, colorDirection);
        var endPoint = RectX.SplatVector(polygonRect, oppositeColorDirection);
        Gizmos.DrawSphere(startPoint, Vector2.Distance(startPoint, endPoint) * 0.025f);
        Gizmos.DrawSphere(endPoint, Vector2.Distance(startPoint, endPoint) * 0.025f);
        
        var p = polygon.FindPointInDirection(colorDirection);
        Gizmos.DrawSphere(p, Vector2.Distance(startPoint, endPoint) * 0.05f);
        


        Vector2 uvXDirection = MathX.DegreesToVector2(uvXAngle+90);
        Vector2 uvYDirection = MathX.DegreesToVector2(uvYAngle+90);
        // Vector2[] uvs = new Vector2[polygon.vertices.Length];
        // var distanceXMin = Mathf.Infinity;
        // var distanceXMax = Mathf.NegativeInfinity;
        // var distanceYMin = Mathf.Infinity;
        // var distanceYMax = Mathf.NegativeInfinity;
        // Vector2 distanceXMinPoint = Vector2.zero;
        // Vector2 distanceXMaxPoint = Vector2.zero;
        // Vector2 distanceYMinPoint = Vector2.zero;
        // Vector2 distanceYMaxPoint = Vector2.zero;
        // foreach(var vert in polygon.vertices) {
        //     var distanceY = Vector2.Dot(vert, uvYDirection);
        //     var distanceX = Vector2.Dot(vert, uvXDirection);
        //     if(distanceX < distanceXMin) {
        //         distanceXMin = distanceX;
        //         distanceXMinPoint = vert;
        //     } else if(distanceX > distanceXMax) {
        //         distanceXMax = distanceX;
        //         distanceXMaxPoint = vert;
        //     }
        //     if(distanceY < distanceYMin) {
        //         distanceYMin = distanceY;
        //         distanceYMinPoint = vert;
        //     } else if(distanceY > distanceYMax) {
        //         distanceYMax = distanceY;
        //         distanceYMaxPoint = vert;
        //     }
        // }
        // Gizmos.DrawSphere(distanceXMinPoint, 0.05f);
        // Gizmos.DrawSphere(distanceXMaxPoint, 0.05f);
        // Gizmos.DrawSphere(distanceYMinPoint, 0.05f);
        // Gizmos.DrawSphere(distanceYMaxPoint, 0.05f);
        // var left = new Vector2(distanceXMinPoint.x, Mathf.Lerp(distanceYMinPoint.y, distanceYMaxPoint.y, 0.5f));
        // var bottom = new Vector2(Mathf.Lerp(distanceXMinPoint.x, distanceXMaxPoint.x, 0.5f), distanceYMinPoint.y);
        // var xLength = Vector2.Distance(distanceXMinPoint, distanceXMaxPoint) * 0.1f;
        // var yLength = Vector2.Distance(distanceYMinPoint, distanceYMaxPoint) * 0.1f;
        // GizmosX.DrawArrowLine(left - uvYDirection * xLength * 0.5f, left + uvYDirection * xLength * 0.5f, Vector3.forward);
        // GizmosX.DrawArrowLine(bottom - uvXDirection * yLength * 0.5f, bottom + uvXDirection * yLength * 0.5f, Vector3.forward);
        GizmosX.DrawArrowLine(Vector2.zero, colorDirection * 0.5f, Vector3.forward);
        GizmosX.DrawArrowLine(Vector2.zero, uvYDirection * 0.5f, Vector3.forward);
        GizmosX.DrawArrowLine(Vector2.zero, uvXDirection * 0.5f, Vector3.forward);

        
        GizmosX.EndMatrix();
    }
}