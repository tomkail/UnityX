using System.Collections.Generic;
using UnityEngine;

namespace UnityX.MeshBuilder
{
    public class MeshBuilder {
        
        public List<Vector3> verts = new List<Vector3>();
		public List<int> tris = new List<int>();
		public List<Vector2> uvs = new List<Vector2>();
		public List<Color> colors = new List<Color>();

		public void Clear () {
			verts.Clear();
			tris.Clear();
			uvs.Clear();
			colors.Clear();
		}
		
        public void ToMesh (Mesh mesh, MeshBakeParams bakeParams) {
			if(mesh.vertexCount != verts.Count) {
				mesh.Clear();
			}
            mesh.SetVertices(verts);
			mesh.SetTriangles(tris,0);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            if(bakeParams.recalculateNormals) mesh.RecalculateNormals();
            if(bakeParams.recalculateBounds) mesh.RecalculateBounds();
        }

        public Mesh ToMesh (MeshBakeParams bakeParams) {
            var mesh = new Mesh();
            mesh.name = "MeshBuilder mesh";
            ToMesh(mesh, bakeParams);
            return mesh;
        }

		
		public void AddPlane (AddPlaneParams input) {
			if(input.front) {
				verts.Add(input.topLeft);verts.Add(input.topRight);verts.Add(input.bottomLeft);
				verts.Add(input.topRight);verts.Add(input.bottomRight);verts.Add(input.bottomLeft);
				
				int t = tris.Count;
				for(int j = 0; j < 6; j++)tris.Add(j+t);
				
				uvs.Add(input.uvTopLeft);uvs.Add(input.uvTopRight);uvs.Add(input.uvBottomLeft);
				uvs.Add(input.uvTopRight);uvs.Add(input.uvBottomRight);uvs.Add(input.uvBottomLeft);
				
				colors.Add(input.colorTopLeft);colors.Add(input.colorTopRight);colors.Add(input.colorBottomLeft);
				colors.Add(input.colorTopRight);colors.Add(input.colorBottomRight);colors.Add(input.colorBottomLeft);
			}
			if(input.back) {
				verts.Add(input.bottomLeft);verts.Add(input.topRight);verts.Add(input.topLeft);
				verts.Add(input.bottomLeft);verts.Add(input.bottomRight);verts.Add(input.topRight);
				
				int t = tris.Count;
				for(int j = 0; j < 6; j++)tris.Add(j+t);

				uvs.Add(input.uvBottomLeft);uvs.Add(input.uvTopRight);uvs.Add(input.uvTopLeft);
				uvs.Add(input.uvBottomLeft);uvs.Add(input.uvBottomRight);uvs.Add(input.uvTopRight);
				
				colors.Add(input.colorBottomLeft);colors.Add(input.colorTopRight);colors.Add(input.colorTopLeft);
				colors.Add(input.colorBottomLeft);colors.Add(input.colorBottomRight);colors.Add(input.colorTopRight);
			}
		}

		public void AddTriangle (AddTriangleParams input) {
			if(input.front) {
				verts.Add(input.topLeft);verts.Add(input.topRight);verts.Add(input.bottom);
				int t = tris.Count;
				for(int j = 0; j < 3; j++)tris.Add(j+t);
				uvs.Add(input.uvTopLeft);uvs.Add(input.uvTopRight);uvs.Add(input.uvBottom);
				colors.Add(input.colorTopLeft);colors.Add(input.colorTopRight);colors.Add(input.colorBottom);
			}
			if(input.back) {
				verts.Add(input.bottom);verts.Add(input.topRight);verts.Add(input.topLeft);
				int t = tris.Count;
				for(int j = 0; j < 3; j++)tris.Add(j+t);
				uvs.Add(input.uvBottom);uvs.Add(input.uvTopRight);uvs.Add(input.uvTopLeft);
				colors.Add(input.colorBottom);colors.Add(input.colorTopRight);colors.Add(input.colorTopLeft);
			}
		}
	}
}