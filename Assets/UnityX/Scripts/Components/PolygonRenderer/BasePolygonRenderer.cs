using UnityEngine;
using UnityX.Geometry;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public abstract class BasePolygonRenderer : MonoBehaviour {
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
    
    MeshFilter _meshFilter;
    public MeshFilter meshFilter {
        get {
            if(_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
            return _meshFilter;
        }
    }
    [SerializeField]
    MeshRenderer _meshRenderer = null;
    public MeshRenderer meshRenderer {
        get {
            return _meshRenderer;
        }
    }
    [SerializeField]
    MeshCollider _meshCollider = null;
    public MeshCollider meshCollider {
        get {
            return _meshCollider;
        }
    }
    [SerializeField, AssetSaver]
    protected Mesh mesh;

    [SerializeField]
    Polygon _polygon = new Polygon(new RegularPolygon(4, 45).ToPolygonVerts());
    public Polygon polygon {
        get {
            return _polygon;
        } set {
            _polygon = value;
            RebuildMesh();
        }
    }

    public Plane plane;
    public enum Plane {
        XY,
        XZ,
    }

    static Quaternion xzPlaneRotation = Quaternion.Euler(new Vector3(90,0,0));
    public Quaternion offsetRotation {
        get {
            if(plane == Plane.XY) return Quaternion.identity;
            if(plane == Plane.XZ) return xzPlaneRotation;
            return Quaternion.identity;
        }
    }
    
    [Space]
    public ColorMode colorMode;
    public enum ColorMode {
        White,
        Rect,
        Shape,
        Custom
    }

    public Color[] customColors;
    public Gradient gradient;
    public float colorAngle;
    

    protected void Reset () {
        DestroyMesh();
        GetMesh();
        polygon = new Polygon(new RegularPolygon(4, 45).ToPolygonVerts());
    }
    
    protected void OnEnable () {
        if(!Application.isPlaying) {
            GetMesh();
            if(Application.isPlaying) {
                DestroyMesh();
            } else {
                mesh.Clear();
            }
            RebuildMesh();
            RefreshMaterialPropertyBlock();
        }
    }
    
	[ContextMenu("Refresh")]
	public void OnPropertiesChanged () {
		RebuildMesh();
        RefreshMaterialPropertyBlock();
	}

    protected void OnDestroy () {
        DestroyMesh();
    }
    protected void DestroyMesh () {
        if(mesh != null) {
            ObjectX.DestroyAutomatic(mesh);
            mesh = null;
            if(meshFilter != null) meshFilter.mesh = null;
		    if(meshCollider != null) meshCollider.sharedMesh = null;
        }
    }

    protected void GetMesh () {
        if(mesh != null && mesh.name != "Polygon Renderer Mesh "+ GetInstanceID()) {
            mesh = null;
            if(meshFilter != null) meshFilter.mesh = null;
		    if(meshCollider != null) meshCollider.sharedMesh = null;
        }
        if(mesh == null) {
            if(meshFilter.name == "Polygon Renderer Mesh "+ GetInstanceID()) {
                mesh = meshFilter.mesh;
            } else {
                mesh = new Mesh();
                mesh.name = "Polygon Renderer Mesh "+ GetInstanceID();
                if(meshFilter != null) meshFilter.mesh = mesh;
                if(meshCollider != null) meshCollider.sharedMesh = mesh;
            }
        }
    }
    public abstract void RebuildMesh ();
    public abstract void RefreshMaterialPropertyBlock ();
    
    protected Color GetColor (Rect polygonRect, Vector2[] points, Vector2 point) {
        if(colorMode == ColorMode.Rect || colorMode == ColorMode.Shape) {
            Vector2 colorDirection = MathX.DegreesToVector2(colorAngle+90);
            Vector2 oppositeColorDirection = MathX.DegreesToVector2(colorAngle+90+180);
            if(colorMode == ColorMode.Rect) {
                var startPoint = RectX.SplatVector(polygonRect, colorDirection);
                var endPoint = RectX.SplatVector(polygonRect, oppositeColorDirection);
                
                Line3D line = new Line3D(startPoint, endPoint);
                var n = line.GetNormalizedDistanceOnLine(point);
                return gradient.Evaluate(n);
            } else if(colorMode == ColorMode.Shape) {
                var distanceColorMin = Mathf.Infinity;
                var distanceColorMax = Mathf.NegativeInfinity;
                
                var colorDir = Vector2.Dot(point, colorDirection);
                distanceColorMin = Mathf.Min(colorDir, distanceColorMin);
                distanceColorMax = Mathf.Max(colorDir, distanceColorMax);
                
                if(colorMode == ColorMode.Shape) {
                    var colorN = Mathf.InverseLerp(distanceColorMin, distanceColorMax, colorDir);
                    return gradient.Evaluate(colorN);
                }
            }
        } else if (colorMode == ColorMode.White) {
            return Color.white;
        }
        return Color.white;
    }
    protected Color[] RecalculateColors (Rect polygonRect, Vector2[] points) {
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
}