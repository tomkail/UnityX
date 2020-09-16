using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Collections.Generic;
using System;

namespace UnityEngine.UI.Extensions
{
    [System.Serializable]

	public class AdvancedUILineRenderer : UIPrimitiveBase {
        public float blendWeight;
        public ColorX.BlendMode blendMode;
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

        [ContextMenu("Set all dirty")]
		public override void SetAllDirty () {
			base.SetAllDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (m_points == null)
                return;
            
            vh.Clear();

			// Generate the quads that make up the wide line
            var segments = new List<UIVertex[]>(pointsToDraw.Length);
            float currentUVY = uvY;
            // (joinAsLoop ? 0 : 1)
			for (var i = 0; i < pointsToDraw.Length; i++) {
                var startIndex = i;
                var endIndex = i + 1;
                if(endIndex >= pointsToDraw.Length) endIndex = 0;
				segments.Add(CreateLineSegment(pointsToDraw[startIndex], pointsToDraw[endIndex], ref currentUVY));
            }





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

                        // Positive sign means the line is turning in a 'clockwise' direction
                        var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                        // Calculate the miter point
                        var miterDistance = Vector2.Distance(segments[startIndex][2].position, segments[startIndex][3].position) / (2 * Mathf.Tan(angle / 2));
                        var miterPointA = segments[startIndex][2].position - vec1.normalized * miterDistance * sign;
                        var miterPointB = segments[startIndex][3].position + vec1.normalized * miterDistance * sign;
                        
                        var joinType = LineJoins;
                        if (joinType == JoinType.Miter)
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
                                joinType = JoinType.Bevel;
                            }
                        }

                        if (joinType == JoinType.Bevel)
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

                            var join = new UIVertex[] { segments[startIndex][2], segments[startIndex][3], segments[endIndex][0], segments[endIndex][1] };
                            vh.AddUIVertexQuad(join);
                        }
                    }
                }

                for (var i = 0; i < segments.Count; i++) {
                    if (joinAsLoop || i < segments.Count - 1) {
                        vh.AddUIVertexQuad(segments[i]);
                    }
                }
            // }
		}
           
		private UIVertex[] CreateLineSegment(AdvancedUILineRendererPoint start, AdvancedUILineRendererPoint end, ref float uvY) {
			// var sizeX = rectTransform.rect.width;
            // var sizeY = rectTransform.rect.height;
            // var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
            // var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

			// sizeX = 1;
			// sizeY = 1;


			var startPoint = new Vector2(start.point.x, start.point.y);
			var endPoint = new Vector2(end.point.x, end.point.y);

			Vector2 binormal = new Vector2(startPoint.y - endPoint.y, endPoint.x - startPoint.x).normalized;
            
			Vector2 startOffset = binormal * start.width;
			Vector2 endOffset = binormal * end.width;
			var v1 = startPoint - lineAnchor * startOffset;
			var v2 = startPoint + (1-lineAnchor) * startOffset;
			var v3 = endPoint + (1-lineAnchor) * endOffset;
			var v4 = endPoint - lineAnchor * endOffset;

			var uv1 = new Vector2(0, uvY);
			var uv2 = new Vector2(1, uvY);

			uvY += 1f/(end.width/Vector2.Distance(startPoint, endPoint));

			var uv3 = new Vector2(0, uvY);
			var uv4 = new Vector2(1, uvY);

			var points = new Vector2[] { v1, v2, v3, v4 };
			var uvs = new Vector2[] {uv2, uv1, uv3, uv4 };
			var colors = new Color[] {ColorX.Blend(start.color, color, blendWeight, blendMode).WithAlpha(innerAlpha), ColorX.Blend(start.color, color, blendWeight, blendMode).WithAlpha(outerAlpha), ColorX.Blend(end.color, color, blendWeight, blendMode).WithAlpha(outerAlpha), ColorX.Blend(end.color, color, blendWeight, blendMode).WithAlpha(innerAlpha)};
            return SetVbo(points, uvs, colors);
        }

		protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs, Color[] colors) {
			UIVertex[] vbo = new UIVertex[4];
            for (int i = 0; i < vertices.Length; i++) {
                var vert = UIVertex.simpleVert;
                vert.color = colors[i];
                vert.position = vertices[i];
                vert.uv0 = uvs[i];
                vbo[i] = vert;
			}
			return vbo;
		}
	}
}