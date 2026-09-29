using System;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;

namespace UnityEngine.UI.Extensions {
    [AddComponentMenu("UI/Extensions/Primitives/UI Polygon")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIPolygon : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter {
        [SerializeField]
        Texture _texture;

        [SerializeField]
		Polygon _polygon = new Polygon(new Vector2(0,0), new Vector2(100,0), new Vector2(100,100), new Vector2(0,100));
		public Polygon polygon {
			get {
				return _polygon;
			}
			set {
				_polygon = value;
				base.SetVerticesDirty();
			}
		}

        public UVMode uvMode;
        public enum UVMode {
            Rect,
            Shape
        }

        public float uvXAngle = 0;
        public float uvYAngle = 90;

        public override Texture mainTexture
        {
            get
            {
                return _texture == null ? s_WhiteTexture : _texture;
            }
        }
        public Texture texture
        {
            get
            {
                return _texture;
            }
            set
            {
                if (_texture == value) return;
                _texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        protected override void Awake() {
            base.Awake();
            useLegacyMeshGeneration = false;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetAllDirty();
        }

        static List<int> triangles = new List<int>();
        protected override void OnPopulateMesh(VertexHelper vh) {
            vh.Clear();
			var points = polygon.vertices;
            var pivotOffset = (Vector3)GetPixelAdjustedRect().position;
			
            triangles.Clear();
            Triangulator.GenerateIndices(points, triangles);
            var rect = polygon.GetRect();
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

            // using (vh) {
                foreach(var point in points) {
                    var pos = new Vector3(point.x, point.y, 0);
                    Vector2 uv = Vector2.zero;
                    
                    if(uvMode == UVMode.Rect) {
                        uv = Rect.PointToNormalized(rect, pos);
                    } else if(uvMode == UVMode.Shape) {
                        
                        var distanceY = Vector2.Dot(pos, uvYDirection);
                        var distanceX = Vector2.Dot(pos, uvXDirection);
                        var x = Mathf.InverseLerp(distanceXMin, distanceXMax, distanceX);
                        var y = Mathf.InverseLerp(distanceYMin, distanceYMax, distanceY);
                        uv = new Vector2(x,y);
                    }

                    vh.AddVert(pos+pivotOffset, color, uv);
                }
                // vh.AddUIVertexQuad()
                for(int i = 0; i < triangles.Count; i+= 3) {
                    vh.AddTriangle(triangles[i], triangles[i+1], triangles[i+2]);
                }
            // }
    
		}

		public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local);
			Rect rect = GetPixelAdjustedRect();
            // Convert to have lower left corner as reference point.
            local.x += rectTransform.pivot.x * rect.width;
            local.y += rectTransform.pivot.y * rect.height;

            return polygon.ContainsPoint(local);
        }

        


        #region ILayoutElement Interface

        public virtual void CalculateLayoutInputHorizontal() { }
        public virtual void CalculateLayoutInputVertical() { }

        public virtual float minWidth { get { return 0; } }

        public virtual float preferredWidth
        {
            get
            {
                return polygon.GetRect().width;
            }
        }

        public virtual float flexibleWidth { get { return -1; } }

        public virtual float minHeight { get { return 0; } }
        
        public virtual float preferredHeight
        {
            get
            {
                return polygon.GetRect().height;
            }
        }

        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return 0; } }

        #endregion
    }
}