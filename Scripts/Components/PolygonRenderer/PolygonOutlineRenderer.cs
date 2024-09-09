using System.Collections;
using System.Linq;
using UnityEngine;
using UnityX.MeshBuilder;
using UnityX.Geometry;


[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class PolygonOutlineRenderer : BasePolygonRenderer {
    // [Space]
    // public const string colorID = "_Color";
    // [SerializeField]
    // Color _tintColor = Color.white;
    // public Color tintColor {
    //     get {
    //         return _tintColor;
    //     } set {
    //         if(_tintColor == value) return;
    //         _tintColor = value;
    //         RefreshMaterialPropertyBlock();
    //     }
    // }
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

	public override void RebuildMesh () {
		GetMesh();
		mesh.Clear();
        
        var points = polygon.vertices;

		MeshBuilder mb = new MeshBuilder();
        
        var clockwise = polygon.GetIsClockwise();
        Vector2 startCorner;
        Vector2 startCornerInner;
        Vector2 startCornerOuter;
        GetVertPoints(0, clockwise, out startCorner, out startCornerInner, out startCornerOuter);
		for(int i = 0; i < points.Length; i++) {
            Vector2 endCorner;
			Vector2 endCornerInner;
			Vector2 endCornerOuter;
            GetVertPoints(i+1, clockwise, out endCorner, out endCornerInner, out endCornerOuter);


			AddPlaneParams planeInput = new AddPlaneParams();
			planeInput.front = front ^ !clockwise;
			planeInput.back = back ^ !clockwise;

			AddTriangleParams triangleInput = new AddTriangleParams();
			triangleInput.front = front ^ !clockwise;
			triangleInput.back = back ^ !clockwise;
			
			triangleInput.uvTopLeft = new Vector2(0,1);
			triangleInput.uvTopRight = new Vector2(1,1);
			triangleInput.uvBottom = new Vector2(0.5f,0);
            
			// var cornerDistance = Vector2.Distance(startCorner, endCorner);

			// Inner
			// var quadStartCornerInner = startCornerInner+edgeNormal * Vector3X.DistanceInDirection(startCorner, startCornerInner, edgeNormal);
			// var quadEndCornerInner = endCornerInner+edgeNormal * Vector3X.DistanceInDirection(endCorner, endCornerInner, edgeNormal);
			
			// var innerCornerDistance = Vector2.Distance(startCornerInner, endCornerInner);
			// var innerQuadUVOffset = ((cornerDistance - innerCornerDistance) / cornerDistance) * 0.5f;
			// innerQuadUVOffset = 0;

			/*
			planeInput.topLeft = quadStartCornerInner; planeInput.topRight = quadEndCornerInner; planeInput.bottomRight = endCornerInner; planeInput.bottomLeft = startCornerInner;

			
			planeInput.uvBottomLeft = new Vector2(innerQuadUVOffset,0);
			planeInput.uvBottomRight = new Vector2(1-innerQuadUVOffset,0);

			AddPlane(mb, planeInput);

			planeInput.uvBottomLeft = new Vector2(0,0);
			planeInput.uvBottomRight = new Vector2(1,0);
			
			triangleInput.topLeft = startCorner; triangleInput.topRight = quadStartCornerInner; triangleInput.bottom = startCornerInner;
			AddTriangle(mb, triangleInput);
			triangleInput.topLeft = quadEndCornerInner; triangleInput.topRight = endCorner; triangleInput.bottom = endCornerInner;
			AddTriangle(mb, triangleInput);
			 */
			
			// Outer 
			// var quadStartCornerOuter = startCorner+edgeNormal * Vector3X.DistanceInDirection(startCorner, startCornerOuter, edgeNormal);
			// var quadEndCornerOuter = endCorner+edgeNormal * Vector3X.DistanceInDirection(endCorner, endCornerOuter, edgeNormal);
			
			/* 
			var outerCornerDistance = Vector2.Distance(startCornerOuter, endCornerOuter);
			var outerQuadUVOffset = ((cornerDistance - outerCornerDistance) / cornerDistance) * 0.5f;
			outerQuadUVOffset = 0;

			planeInput.topLeft = quadStartCornerOuter; planeInput.topRight = quadEndCornerOuter; planeInput.bottomRight = endCorner; planeInput.bottomLeft = startCorner;
			
			planeInput.uvTopLeft = new Vector2(outerQuadUVOffset,1);
			planeInput.uvTopRight = new Vector2(1-outerQuadUVOffset,1);

			AddPlane(mb, planeInput);

			planeInput.uvTopLeft = new Vector2(0,1);
			planeInput.uvTopRight = new Vector2(1,1);

			triangleInput.topLeft = startCornerOuter; triangleInput.topRight = quadStartCornerOuter; triangleInput.bottom = startCorner;
			AddTriangle(mb, triangleInput);
			triangleInput.topLeft = quadEndCornerOuter; triangleInput.topRight = endCornerOuter; triangleInput.bottom = endCorner;
			AddTriangle(mb, triangleInput);
			*/

            // if(innerDistance > 0) {
            //     planeInput.uvTopLeft = new Vector2(0,1);
            //     planeInput.uvTopRight = new Vector2(1,1);
            //     planeInput.uvBottomRight = new Vector2(1,0);
            //     planeInput.uvBottomLeft = new Vector2(0,0);

            //     planeInput.topLeft = startCorner; planeInput.topRight = endCorner; planeInput.bottomRight = endCornerInner; planeInput.bottomLeft = startCornerInner;
            //     mb.AddPlane(planeInput);
            // }

            // if(outerDistance > 0) {
            //     planeInput.uvBottomLeft = new Vector2(0,1);
            //     planeInput.uvBottomRight = new Vector2(1,1);
            //     planeInput.uvTopRight = new Vector2(1,0);
            //     planeInput.uvTopLeft = new Vector2(0,0);
                
            //     planeInput.topLeft = startCornerOuter; planeInput.topRight = endCornerOuter; planeInput.bottomRight = endCorner; planeInput.bottomLeft = startCorner;
            //     mb.AddPlane(planeInput);
            // }
            
            planeInput.uvBottomLeft = new Vector2(0,1);
            planeInput.uvBottomRight = new Vector2(1,1);
            planeInput.uvTopRight = new Vector2(1,0);
            planeInput.uvTopLeft = new Vector2(0,0);
            
            planeInput.topLeft = startCornerOuter; planeInput.topRight = endCornerOuter; planeInput.bottomRight = endCornerInner; planeInput.bottomLeft = startCornerInner;
            mb.AddPlane(planeInput);


            startCorner = endCorner;
            startCornerInner = endCornerInner;
            startCornerOuter = endCornerOuter;
		}

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
        
        var edgeAOffsetInnerLine = new Line(point + edgeANormal * innerDistance, point + edgeANormal * innerDistance - edgeATangent);
        var edgeBOffsetInnerLine = new Line(point + edgeBNormal * innerDistance, point + edgeBNormal * innerDistance + edgeBTangent);
        if(!Line.LineIntersectionPoint(edgeAOffsetInnerLine, edgeBOffsetInnerLine, out innerPoint, false)) {
            innerPoint = point + edgeANormal * innerDistance;
        }
        
        var edgeAOffsetOuterLine = new Line(point + edgeANormal * outerDistance, point + edgeANormal * outerDistance - edgeATangent);
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