using UnityEngine;

// Builds its mesh from `input` with ExtrudedPolygonMeshGenerator into the sibling MeshFilter (and MeshCollider, if any).
[RequireComponent(typeof(MeshFilter))]
public class ExtrudedPolygonRenderer : MonoBehaviour {
	// Whether the Scene view polygon editing tool works on this renderer.
	public bool editable = true;
	// Rebuild the mesh whenever the component is enabled, in the editor and in play mode.
	public bool rebuildOnEnable = true;

	MeshFilter _meshFilter;
	public MeshFilter meshFilter {
		get {
			if(_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
			return _meshFilter;
		}
	}
	MeshCollider _meshCollider;
	public MeshCollider meshCollider {
		get {
			if(_meshCollider == null) _meshCollider = GetComponent<MeshCollider>();
			return _meshCollider;
		}
	}

	public ExtrudedPolygonMeshParams input;
	[AssetSaver]
	public Mesh mesh;

	protected void Reset () {
		DestroyMesh();
		GetMesh();
	}

	protected void OnEnable () {
		if(!rebuildOnEnable) return;
		GetMesh();
		if(Application.isPlaying) {
			DestroyMesh();
		} else {
			mesh.Clear();
		}
		RebuildMesh();
	}

	protected void OnDestroy () {
		DestroyMesh();
	}
	protected void DestroyMesh () {
		if(mesh != null) {
			ObjectX.DestroyAutomatic(mesh);
			mesh = null;
			if(meshFilter != null) meshFilter.mesh = null;
		}
	}

	protected void GetMesh () {
		if(mesh != null && mesh.name != "Region Renderer Mesh "+ GetEntityId()) {
			mesh = null;
			if(meshFilter != null) meshFilter.mesh = null;
		}
		if(mesh == null) {
			if(meshFilter != null && meshFilter.name == "Region Renderer Mesh "+ GetEntityId()) {
				mesh = meshFilter.mesh;
			} else {
				mesh = new Mesh();
				mesh.name = "Region Renderer Mesh "+ GetEntityId();
			}
		}
		if(meshFilter != null) meshFilter.mesh = mesh;
		if(meshCollider != null) meshCollider.sharedMesh = mesh;
	}

	public void RebuildMesh () {
		GetMesh();
		ExtrudedPolygonMeshGenerator.Create(input, ref mesh);
	}
}
