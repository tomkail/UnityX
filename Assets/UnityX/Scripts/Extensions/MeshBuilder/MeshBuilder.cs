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
				List<Vector3> frontVerts = new List<Vector3>(){input.topLeft,input.topRight,input.bottom};
				verts.AddRange(frontVerts);
				int t = tris.Count;
				for(int j = 0; j < 3; j++)tris.Add(j+t);
				uvs.AddRange(new List<Vector2>(){input.uvTopLeft,input.uvTopRight,input.uvBottom});
				colors.AddRange(new List<Color>(){input.colorTopLeft,input.colorTopRight,input.colorBottom});
			}
			if(input.back) {
				List<Vector3> backVerts = new List<Vector3>(){input.bottom,input.topRight,input.topLeft};
				verts.AddRange(backVerts);
				int t = tris.Count;
				for(int j = 0; j < 3; j++)tris.Add(j+t);
				uvs.AddRange(new List<Vector2>(){input.uvBottom,input.uvTopRight,input.uvTopLeft});
				colors.AddRange(new List<Color>(){input.colorBottom,input.colorTopRight,input.colorTopLeft});
			}
		}
	}
}