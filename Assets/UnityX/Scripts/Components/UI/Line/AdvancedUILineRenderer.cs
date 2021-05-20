using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Collections.Generic;
using System;

namespace UnityEngine.UI.Extensions
{
    [System.Serializable]
    [RequireComponent(typeof(CanvasRenderer))]

	public class AdvancedUILineRenderer : UIPrimitiveBase {
        public float colorBlendWeight;
        public ColorX.BlendMode colorBlendMode;
        public float _uvY = 0;
        public float uvY {
            get {
                return _uvY;
            } set {
                if(_uvY == value) return;
                _uvY = value;
                SetAllDirty();
            }
        }

        public bool joinAsLoop;
        public enum JoinType {
            Bevel,
            Miter
        }
		public JoinType LineJoins = JoinType.Bevel;
		private const float MIN_MITER_JOIN = 15 * Mathf.Deg2Rad;

        // A bevel 'nice' join displaces the vertices of the line segment instead of simply rendering a
        // quad to connect the endpoints. This improves the look of textured and transparent lines, since
        // there is no overlapping.
        private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;
        
        [Range(0,1)]
        public float lineAnchor = 0.5f;

        public float innerAlpha = 1;
        public float outerAlpha = 1;

		[SerializeField]
		private AdvancedUILineRendererPoint[] m_points;
        /// <summary>
        /// Points to be drawn in the line.
        /// </summary>
		public AdvancedUILineRendererPoint[] pointsToDraw {
			get {return m_points;}
            set {
				if (m_points == value) return;
				m_points = value;
				SetAllDirty();
            }
        }

        // Cache
        List<UIVertex[]> segments;

        protected override void Awake() {
            base.Awake();
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (m_points == null)
                return;
            
            vh.Clear();

            
            var pivotOffset = (Vector3)GetPixelAdjustedRect().position;

			if(segments == null) segments = new List<UIVertex[]>(pointsToDraw.Length);
            else if(segments.Count > pointsToDraw.Length) segments.RemoveRange(pointsToDraw.Length, segments.Count - pointsToDraw.Length);
            while(segments.Count < pointsToDraw.Length) segments.Add(new UIVertex[4]);
            float currentUVY = uvY;
            
            var color32 = (Color32)color;
			for (var i = 0; i < pointsToDraw.Length; i++) {
                var startIndex = i;
                var endIndex = i + 1;
                if(endIndex >= pointsToDraw.Length) endIndex = 0;
                CreateLineSegment(segments[i], pointsToDraw[startIndex], pointsToDraw[endIndex], pivotOffset, color32, ref currentUVY);
            }
            Apply(vh, segments, joinAsLoop, LineJoins);
		}
		private void CreateLineSegment(UIVertex[] segment, AdvancedUILineRendererPoint start, AdvancedUILineRendererPoint end, Vector2 pivotOffset, Color32 color, ref float uvY) {
			LineSegmentProperties lineSegmentProperties = new LineSegmentProperties();
            lineSegmentProperties.start = start;
            lineSegmentProperties.end = end;
            lineSegmentProperties.pivotOffset = pivotOffset;
            lineSegmentProperties.lineAnchor = lineAnchor;
            lineSegmentProperties.color = color;
            lineSegmentProperties.innerAlpha = innerAlpha;
            lineSegmentProperties.outerAlpha = outerAlpha;
            lineSegmentProperties.colorBlendMode = colorBlendMode;
            lineSegmentProperties.colorBlendWeight = colorBlendWeight;
            CreateLineSegment(segment, lineSegmentProperties, ref uvY);
        }

        public static void Apply (VertexHelper vh, List<UIVertex[]> segments, bool joinAsLoop, JoinType joinType) {

            // if(LineJoins != JoinType.None) {
                // Add the line segments to the vertex helper, creating any joins as needed
                for (var i = 0; i < segments.Count; i++) {
                    if (joinAsLoop || i < segments.Count - 1) {
                        // var prevIndex = i-1;
                        // if(prevIndex < 0) prevIndex = segments.Count-1;
                        var startIndex = i;
                        var endIndex = i + 1;
                        if(endIndex >= segments.Count) 
                            endIndex = 0;

                        var vec1 = segments[startIndex][1].position - segments[startIndex][2].position;
                        var vec2 = segments[endIndex][2].position - segments[endIndex][1].position;
                        var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                        var vec1Normalized = vec1.normalized;
                        // Positive sign means the line is turning in a 'clockwise' direction
                        var sign = Mathf.Sign(Vector3.Cross(vec1Normalized, vec2.normalized).z);

                        // Calculate the miter point
                        var miterDistance = Vector2.Distance(segments[startIndex][2].position, segments[startIndex][3].position) / (2 * Mathf.Tan(angle / 2));
                        var miterPointA = segments[startIndex][2].position - vec1Normalized * miterDistance * sign;
                        var miterPointB = segments[startIndex][3].position + vec1Normalized * miterDistance * sign;
                        
                        var _joinType = joinType;
                        if (_joinType == JoinType.Miter)
                        {
                            // Make sure we can make a miter join without too many artifacts.
                            if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_MITER_JOIN)
                            {
                                segments[startIndex][2].position = miterPointA;
                                segments[startIndex][3].position = miterPointB;
                                segments[endIndex][0].position = miterPointB;
                                segments[endIndex][1].position = miterPointA;
                            }
                            else
                            {
                                _joinType = JoinType.Bevel;
                            }
                        }

                        if (_joinType == JoinType.Bevel)
                        {
                            if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_BEVEL_NICE_JOIN)
                            {
                                if (sign < 0)
                                {
                                    segments[startIndex][2].position = miterPointA;
                                    segments[endIndex][1].position = miterPointA;
                                }
                                else
                                {
                                    segments[startIndex][3].position = miterPointB;
                                    segments[endIndex][0].position = miterPointB;
                                }
                            }
                            AddQuad(vh, segments[startIndex][3], segments[startIndex][2], segments[endIndex][1], segments[endIndex][0]);
                        }
                    }
                }

                for (var i = 0; i < segments.Count; i++) {
                    if (joinAsLoop || i < segments.Count - 1) {
                        AddQuad(vh, segments[i]);
                    }
                }
            // }
        }

        private static List<UIVertex> vertexBuffer = new List<UIVertex>(4);
        private static List<int> indexBuffer = new List<int>(6);
        static void AddQuad(VertexHelper vh, UIVertex vertex1, UIVertex vertex2, UIVertex vertex3, UIVertex vertex4) {
            var i = vh.currentVertCount;
            // vertexBuffer.Clear();
            // vertexBuffer.Add(vertex1);
            // vertexBuffer.Add(vertex2);
            // vertexBuffer.Add(vertex3);
            // vertexBuffer.Add(vertex4);
            // indexBuffer.Clear();
            // indexBuffer.Add(i+0);
            // indexBuffer.Add(i+2);
            // indexBuffer.Add(i+1);
            // indexBuffer.Add(i+3);
            // indexBuffer.Add(i+2);
            // indexBuffer.Add(i+0);
            // vh.AddUIVertexStream(vertexBuffer, indexBuffer);
            vh.AddVert(vertex1);
            vh.AddVert(vertex2);
            vh.AddVert(vertex3);
            vh.AddVert(vertex4);
            vh.AddTriangle(i+1,i+2,i+3);
            vh.AddTriangle(i+3,i+0,i+1);
        }
        static void AddQuad(VertexHelper vh, UIVertex[] verts) {
            AddQuad(vh, verts[0], verts[1], verts[2], verts[3]);
        }
        
        [System.Serializable]
        public struct LineSegmentProperties {
            public AdvancedUILineRendererPoint start;
            public AdvancedUILineRendererPoint end;
            public Vector2 pivotOffset;
            public float lineAnchor;

            public Color32 color;
            public float innerAlpha;
            public float outerAlpha;
            public float colorBlendWeight;
            public ColorX.BlendMode colorBlendMode;
        }

        
        static Vector3[] points = new Vector3[4];
        static Vector4[] uvs = new Vector4[4];
        static Color32[] colors = new Color32[4];
		public static void CreateLineSegment(UIVertex[] segment, LineSegmentProperties properties, ref float uvY) {
			var startPoint = properties.start.point + properties.pivotOffset;
			var endPoint = properties.end.point + properties.pivotOffset;

			Vector2 binormal = new Vector2(startPoint.y - endPoint.y, endPoint.x - startPoint.x).normalized;
			Vector2 startOffset = binormal * properties.start.width;
			Vector2 endOffset = binormal * properties.end.width;
			points[0] = startPoint - properties.lineAnchor * startOffset;
			points[1] = startPoint + (1-properties.lineAnchor) * startOffset;
			points[2] = endPoint + (1-properties.lineAnchor) * endOffset;
			points[3] = endPoint - properties.lineAnchor * endOffset;
			
            uvs[0] = new Vector4(0, uvY, 0, 0);
			uvs[1] = new Vector4(1, uvY, 0, 0);
			uvY += 1f/(properties.end.width/Vector2.Distance(startPoint, endPoint));
			uvs[2] = new Vector4(1, uvY, 0, 0);
			uvs[3] = new Vector4(0, uvY, 0, 0);
			
            var color1 = properties.start.color;
            if(properties.colorBlendWeight == 1) color1 = properties.color;
            else if(properties.colorBlendWeight > 0) color1 = (Color32)ColorX.Blend(properties.start.color, properties.color, properties.colorBlendWeight, properties.colorBlendMode);
            var color2 = color1;

            var color3 = properties.end.color;
            if(properties.colorBlendWeight == 1) color3 = properties.color;
            else if(properties.colorBlendWeight > 0) color3 = ColorX.Blend(properties.end.color, properties.color, properties.colorBlendWeight, properties.colorBlendMode);
            var color4 = color3;
            
            if(properties.innerAlpha == 0) {
                color1.a = 0;
                color4.a = 0;
            } else if(properties.innerAlpha < 1) {
                color1.a = (byte)(color1.a*properties.innerAlpha*255f);
                color4.a = (byte)(color4.a*properties.innerAlpha*255f);
            }
            if(properties.outerAlpha == 0) {
                color2.a = 0;
                color3.a = 0;
            } else if(properties.outerAlpha < 1) {
                color1.a = (byte)(color2.a*properties.outerAlpha*255f);
                color4.a = (byte)(color3.a*properties.outerAlpha*255f);
            }
            colors[0] = color1;
            colors[1] = color2;
            colors[2] = color3;
            colors[3] = color4;

            for (int i = 0; i < points.Length; i++) {
                var vert = new UIVertex();
                vert.color = colors[i];
                vert.position = points[i];
                vert.uv0 = uvs[i];
                segment[i] = vert;
            }
            // SetVbo(segment, points, uvs, colors);
            // void SetVbo(UIVertex[] segment, Vector3[] vertices, Vector4[] uvs, Color32[] colors) {
            // }
        }
	}
}