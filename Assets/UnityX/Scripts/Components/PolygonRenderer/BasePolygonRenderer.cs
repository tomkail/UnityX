using UnityEngine;
using UnityX.Geometry;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public abstract class BasePolygonRenderer : MonoBehaviour {
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

    

    protected void Reset () {
        GetMesh();
        mesh.Clear();
        polygon = new Polygon(new RegularPolygon(4, 45).ToPolygonVerts());
    }
    
    protected void OnEnable () {
        GetMesh();
        mesh.Clear();
        RebuildMesh();
        RefreshMaterialPropertyBlock();
    }
    
	[ContextMenu("Refresh")]
	public void OnPropertiesChanged () {
		RebuildMesh();
        RefreshMaterialPropertyBlock();
	}

    protected void OnDestroy () {
        if(mesh != null) {
            ObjectX.DestroyAutomatic(mesh);
            meshFilter.mesh = mesh = null;
        }
    }

    protected void GetMesh () {
        if(mesh == null) {
            if(meshFilter.name == "Polygon Renderer Mesh") {
                mesh = meshFilter.mesh;
            } else {
                mesh = new Mesh();
                mesh.name = "Polygon Renderer Mesh";
            }
        }
    }
    public abstract void RebuildMesh ();
    public abstract void RefreshMaterialPropertyBlock ();
}
