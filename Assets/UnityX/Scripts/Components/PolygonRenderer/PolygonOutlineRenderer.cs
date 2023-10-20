using System.Collections.Generic;
using UnityEngine;
using UnityX.MeshBuilder;

[RequireComponent(typeof(MeshFilter))]
public class PolygonOutlineRenderer : BasePolygonRenderer {
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
	public bool front = true;
	public bool back;
	public MeshBakeParams bakeParams = new MeshBakeParams(true, true);
    
    [Space]
	public float innerDistance = 0f;
	public float outerDistance = 0.25f;

    
    MaterialPropertyBlock _propBlock = null;
    MaterialPropertyBlock propBlock {
        get {
            if(_propBlock == null) _propBlock = new MaterialPropertyBlock();
            return _propBlock;
        }
    }

    public StrokeGeometryAttributes attributes;
    public float extrusion;
    static MeshBuilder mb = new MeshBuilder();
	public override void RebuildMesh () {
		GetMesh();
		mesh.Clear();
        
        var polygonRect = polygon.GetRect();
        var points = polygon.vertices;
        
        mb.Clear();
        
        var clockwise = polygon.GetIsClockwise();
        Vector2[] extrudedPoints = Polygon.GetExtruded(polygon, extrusion);


        var tris = LineDraw.getStrokeGeometry(extrudedPoints, attributes);
        if(!tris.IsNullOrEmpty()) {
            List<Vector3> verts = new List<Vector3>(tris.Count);
            for (var i = 0; i < tris.Count; i++) {
                verts.Add(offsetRotation * tris[i]);
            }

            for (var i = 0; i < verts.Count; i += 3) {
                var triangle = new AddTriangleParams();
                triangle.front = true;
                triangle.colorTopLeft = triangle.colorTopRight = triangle.colorBottom = tintColor;
                triangle.topLeft = verts[i];
                triangle.topRight = verts[i+1];
                triangle.bottom = verts[i+2];

                if (signedArea(triangle.topLeft, triangle.topRight, triangle.bottom) > 0) {
                    triangle.topLeft = verts[i+2];
                    triangle.topRight = verts[i+1];
                    triangle.bottom = verts[i];
                }
                float signedArea(Vector2 p0, Vector2 p1, Vector2 p2) {
                    return (p1.x - p0.x) * (p2.y - p0.y) - (p2.x - p0.x) * (p1.y - p0.y);
                }

                
                mb.AddTriangle(triangle);
                // Debug.Log("ADD TRI")
            }
        }

        
        // var clockwise = polygon.GetIsClockwise();
        // Vector2 startCorner;
        // Vector2 startCornerInner;
        // Vector2 startCornerOuter;
        // GetVertPoints(0, clockwise, out startCorner, out startCornerInner, out startCornerOuter);
		// for(int i = 0; i < points.Length; i++) {
        //     Vector2 endCorner;
		// 	Vector2 endCornerInner;
		// 	Vector2 endCornerOuter;
        //     GetVertPoints(i+1, clockwise, out endCorner, out endCornerInner, out endCornerOuter);

        //     Color startColor = Color.white;
        //     Color endColor = Color.white;
        //     // GetColors(i+1, clockwise, out endCorner, out endCornerInner, out endCornerOuter);
            

		// 	AddPlaneParams planeInput = new AddPlaneParams();
		// 	planeInput.front = front ^ !clockwise;
		// 	planeInput.back = back ^ !clockwise;

		// 	AddTriangleParams triangleInput = new AddTriangleParams();
		// 	triangleInput.front = front ^ !clockwise;
		// 	triangleInput.back = back ^ !clockwise;
			
		// 	triangleInput.uvTopLeft = new Vector2(0,1);
		// 	triangleInput.uvTopRight = new Vector2(1,1);
		// 	triangleInput.uvBottom = new Vector2(0.5f,0);

        //     planeInput.uvBottomLeft = new Vector2(0,1);
        //     planeInput.uvBottomRight = new Vector2(1,1);
        //     planeInput.uvTopRight = new Vector2(1,0);
        //     planeInput.uvTopLeft = new Vector2(0,0);
            
        //     planeInput.topLeft = startCornerOuter; planeInput.topRight = endCornerOuter; planeInput.bottomRight = endCornerInner; planeInput.bottomLeft = startCornerInner;

        //     planeInput.colorTopLeft = startColor; planeInput.colorTopRight = startColor; planeInput.colorBottomRight = endColor; planeInput.colorBottomLeft = endColor;
        //     mb.AddPlane(planeInput);


        //     startCorner = endCorner;
        //     startCornerInner = endCornerInner;
        //     startCornerOuter = endCornerOuter;
		// }

        mb.ToMesh(mesh, bakeParams);

        if(meshFilter != null)
            meshFilter.mesh = mesh;
	}

    // Gets the inner and outer point for a vert (mitered)
    void GetVertPoints (int i, bool clockwise, out Vector2 point, out Vector2 innerPoint, out Vector2 outerPoint) {
        point = polygon.GetVertex(i);
        
        var edgeATangent = polygon.GetEdgeTangentAtEdgeIndex(i-1);
        var edgeANormal = Polygon.GetEdgeNormalAtEdgeIndex(edgeATangent, clockwise);

        var edgeBTangent = polygon.GetEdgeTangentAtEdgeIndex(i);
        var edgeBNormal = Polygon.GetEdgeNormalAtEdgeIndex(edgeBTangent, clockwise);
        
        // var directionDot = Vector2.Dot(edgeATangent, edgeBTangent);

        var edgeAOffsetInnerLine = new Line(point + edgeANormal * innerDistance, point + edgeANormal * innerDistance + edgeATangent);
        var edgeBOffsetInnerLine = new Line(point + edgeBNormal * innerDistance, point + edgeBNormal * innerDistance + edgeBTangent);
        if(!Line.LineIntersectionPoint(edgeAOffsetInnerLine, edgeBOffsetInnerLine, out innerPoint, false)) {
            innerPoint = point + edgeANormal * innerDistance;
        }
        
        var edgeAOffsetOuterLine = new Line(point + edgeANormal * outerDistance, point + edgeANormal * outerDistance + edgeATangent);
        var edgeBOffsetOuterLine = new Line(point + edgeBNormal * outerDistance, point + edgeBNormal * outerDistance + edgeBTangent);
        if(!Line.LineIntersectionPoint(edgeAOffsetOuterLine, edgeBOffsetOuterLine, out outerPoint, false)) {
            outerPoint = point + edgeANormal * outerDistance;
        }
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

    // void OnDrawGizmos () {
        
        // var points = polygon.vertices;
        // var tris = LineDraw.getStrokeGeometry(points, attributes);
        // if(tris != null) {
        //     GizmosX.DrawWirePolygonWithArrows(transform.position, transform.rotation, transform.localScale, tris, Vector3.forward);
        //     RebuildMesh();
        // }
    // }


    // void OnDrawGizmos () {
    //     var clockwise = polygon.GetIsClockwise();
    //     Vector2 startCorner;
    //     Vector2 startCornerInner;
    //     Vector2 startCornerOuter;
    //     GetVertPoints(0, clockwise, out startCorner, out startCornerInner, out startCornerOuter);
	// 	Gizmos.DrawSphere(transform.TransformPoint(startCorner), 0.25f);
	// 	Gizmos.DrawSphere(transform.TransformPoint(startCornerInner), 0.25f);
	// 	Gizmos.DrawSphere(transform.TransformPoint(startCornerOuter), 0.25f);
    //     GetVertPoints(1, clockwise, out startCorner, out startCornerInner, out startCornerOuter);
	// 	Gizmos.DrawSphere(transform.TransformPoint(startCorner), 0.25f);
	// 	Gizmos.DrawSphere(transform.TransformPoint(startCornerInner), 0.25f);
	// 	Gizmos.DrawSphere(transform.TransformPoint(startCornerOuter), 0.25f);
    //     GetVertPoints(2, clockwise, out startCorner, out startCornerInner, out startCornerOuter);
	// 	Gizmos.DrawSphere(transform.TransformPoint(startCorner), 0.25f);
	// 	Gizmos.DrawSphere(transform.TransformPoint(startCornerInner), 0.25f);
	// 	Gizmos.DrawSphere(transform.TransformPoint(startCornerOuter), 0.25f);
    // }
}