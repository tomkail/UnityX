using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Flags]
public enum MeshDrawFaces {
	Front = 1 << 0,
	Back = 1 << 1,
	Both = Front | Back,
	None = 0
}

// Builds an extruded polygon: top and bottom caps from the triangulated polygon, and a quad per side.
// The vertex orders, the UVs, the ignoredFaces/openSides handling and the bottom caps' UVs (taken from the top
// vertices) are ArcadeTactics'; reverseCapWinding and capUVPlane give Sea-Rising's caps.
public class ExtrudedPolygonMeshGenerator {	
	public static Mesh Create (ExtrudedPolygonMeshParams input) {
		var mesh = new Mesh();
		mesh.name = "Polygon Mesh";
		Create(input, ref mesh);
		return mesh;
	}

	public static void Create (ExtrudedPolygonMeshParams input, ref Mesh mesh) {
		mesh.Clear();

		if(input == null || !input.polygon.IsValid()) return;
		
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		// List<Vector3> normals = new List<Vector3>();
		List<Color> colors = new List<Color>();

		bool flipDirection = !input.polygon.GetIsClockwise() ^ (input.offsetMatrix.determinant < 0);

		
		var faceVerts2D = input.polygon.vertices;
		List<int> faceTrisFacingUp = new List<int>();
		Triangulator.GenerateIndices(faceVerts2D, faceTrisFacingUp);

		bool useTopHeightArray = input.topHeights != null && input.topHeights.Length == faceVerts2D.Length;
		bool useBottomHeightArray = input.bottomHeights != null && input.bottomHeights.Length == faceVerts2D.Length;
		bool useColorArrays = input.topColors != null && input.topColors.Length == faceVerts2D.Length && input.bottomColors != null && input.bottomColors.Length == faceVerts2D.Length;
		var rect = input.polygon.GetRect();
		Vector2 offset = new Vector3(rect.width * (input.pivot.x-0.5f), rect.height * (input.pivot.z-0.5f), 0);
		
		Vector3[] topVerts = new Vector3[faceVerts2D.Length];
		Vector3[] bottomVerts = new Vector3[faceVerts2D.Length];

		bool drawTop = input.topFaces != MeshDrawFaces.None;
		bool drawBottom = input.bottomFaces != MeshDrawFaces.None;
		bool drawSides = input.sideFaces != MeshDrawFaces.None;
        

		for(int i = 0; i < faceVerts2D.Length; i++) {
			float x = faceVerts2D[i].x+offset.x;
			float y = 0;
			float z = faceVerts2D[i].y+offset.y;
			float topY = input.topHeight * 0.5f + y;
			float bottomY = input.bottomHeight * 0.5f + y;
			if(useTopHeightArray) topY = input.topHeights[i];
			if(useBottomHeightArray) bottomY = input.bottomHeights[i];
			// The rotation first, then offsetMatrix (an identity offsetMatrix leaves the values unchanged).
			if(drawTop || drawSides) topVerts[i] = input.offsetMatrix.MultiplyPoint(input.rotation * new Vector3(x, z, topY));
			if(drawBottom || drawSides) bottomVerts[i] = input.offsetMatrix.MultiplyPoint(input.rotation * new Vector3(x, z, bottomY));
		}

		int triOffset = verts.Count;
		
		if(drawTop) {
			bool drawBack = FlagsX.HasFlag((int)input.topFaces, (int)MeshDrawFaces.Back);
			bool drawFront = FlagsX.HasFlag((int)input.topFaces, (int)MeshDrawFaces.Front);
			if(drawFront) {
				triOffset = verts.Count;
				for(int i = 0; i < faceTrisFacingUp.Count; i += 3) {
					AddCapTriangle(input, rect, topVerts, topVerts, input.topColors, input.topColor, useColorArrays, faceTrisFacingUp[i+2], faceTrisFacingUp[i+1], faceTrisFacingUp[i], triOffset + i, verts, tris, uvs, colors);
				}
			}
			
			if(drawBack) {
				triOffset = verts.Count;
				for(int i = 0; i < faceTrisFacingUp.Count; i += 3) {
					AddCapTriangle(input, rect, topVerts, topVerts, input.topColors, input.topColor, useColorArrays, faceTrisFacingUp[i], faceTrisFacingUp[i+1], faceTrisFacingUp[i+2], triOffset + i, verts, tris, uvs, colors);
				}
			}
		}

		if(drawBottom) {
			bool drawBack = FlagsX.HasFlag((int)input.bottomFaces, (int)MeshDrawFaces.Back);
			bool drawFront = FlagsX.HasFlag((int)input.bottomFaces, (int)MeshDrawFaces.Front);
			if(drawFront) {
				triOffset = verts.Count;
				for(int i = 0; i < faceTrisFacingUp.Count; i += 3) {
					AddCapTriangle(input, rect, bottomVerts, topVerts, input.bottomColors, input.bottomColor, useColorArrays, faceTrisFacingUp[i], faceTrisFacingUp[i+1], faceTrisFacingUp[i+2], triOffset + i, verts, tris, uvs, colors);
				}
			}
			
			if(drawBack) {
				triOffset = verts.Count;
				for(int i = 0; i < faceTrisFacingUp.Count; i += 3) {
					AddCapTriangle(input, rect, bottomVerts, topVerts, input.bottomColors, input.bottomColor, useColorArrays, faceTrisFacingUp[i+2], faceTrisFacingUp[i+1], faceTrisFacingUp[i], triOffset + i, verts, tris, uvs, colors);
				}
			}
			// triOffset = verts.Count;
			// int[] bottomTris = new int[faceTrisFacingUp.Count];
			// int triLengthMinusOne = faceTrisFacingUp.Count-1;
			// for(int i = 0; i < bottomTris.Length; i++) bottomTris[i] = triOffset + faceTrisFacingUp[triLengthMinusOne - i];
			// verts.AddRange(bottomVerts);
			// tris.AddRange(bottomTris);
		}
		
		if(drawSides) {
            var numFaces = faceVerts2D.Length + (input.openSides ? -1 : 0);
			
            bool drawBack = FlagsX.HasFlag((int)input.sideFaces, (int)MeshDrawFaces.Back);
			bool drawFront = FlagsX.HasFlag((int)input.sideFaces, (int)MeshDrawFaces.Front);

			Vector3 topLeft;
			Vector3 bottomLeft;
			Vector3 topRight;
			Vector3 bottomRight;

			Color topLeftColor;
			Color bottomLeftColor;
			Color topRightColor;
			Color bottomRightColor;


			if(flipDirection ? drawFront : drawBack) {
				int numVerts = numFaces * 6;
				int numTris = numFaces * 6;
				
				Vector3[] vertArray = new Vector3[numVerts];
				int[] triArray = new int[numTris];
				Vector2[] uvArray = new Vector2[numTris];
				Color[] colorsArray = new Color[numVerts];
				
				int vertIndex = 0;
				triOffset = verts.Count;

				for(int i = 0; i < numFaces; i++) {
                    if(!input.ignoredFaces.IsNullOrEmpty() && input.ignoredFaces[i]) continue;
					topLeft = topVerts[i];
					bottomLeft = bottomVerts[i];
					topRight = topVerts.GetRepeating(i-1);
					bottomRight = bottomVerts.GetRepeating(i-1);

					topLeftColor = useColorArrays ? input.topColors[i] : input.topColor;
					bottomLeftColor = useColorArrays ? input.bottomColors[i] : input.bottomColor;
					topRightColor = useColorArrays ? input.topColors.GetRepeating(i-1) : input.topColor;
					bottomRightColor = useColorArrays ? input.bottomColors.GetRepeating(i-1) : input.bottomColor;
					
					vertArray[vertIndex] = topLeft;
					vertArray[vertIndex + 1] = topRight;
					vertArray[vertIndex + 2] = bottomLeft;
					vertArray[vertIndex + 3] = topRight;
					vertArray[vertIndex + 4] = bottomRight;
					vertArray[vertIndex + 5] = bottomLeft;
					
					triArray[vertIndex] = triOffset + 0;
					triArray[vertIndex + 1] = triOffset + 1;
					triArray[vertIndex + 2] = triOffset + 2;
					triArray[vertIndex + 3] = triOffset + 3;
					triArray[vertIndex + 4] = triOffset + 4;
					triArray[vertIndex + 5] = triOffset + 5;
					
					uvArray[vertIndex] = uvTopLeft;
					uvArray[vertIndex + 1] = uvTopRight;
					uvArray[vertIndex + 2] = uvBottomLeft;
					uvArray[vertIndex + 3] = uvTopRight;
					uvArray[vertIndex + 4] = uvBottomRight;
					uvArray[vertIndex + 5] = uvBottomLeft;

					colorsArray[vertIndex] = topLeftColor;
					colorsArray[vertIndex + 1] = topRightColor;
					colorsArray[vertIndex + 2] = bottomLeftColor;
					colorsArray[vertIndex + 3] = topRightColor;
					colorsArray[vertIndex + 4] = bottomRightColor;
					colorsArray[vertIndex + 5] = bottomLeftColor;
					
					vertIndex += 6;
					triOffset += 6;
				}

				verts.AddRange(vertArray);
				tris.AddRange(triArray);
				uvs.AddRange(uvArray);
				colors.AddRange(colorsArray);
			}

			if(flipDirection ? drawBack : drawFront) {
				int numVerts = numFaces * 6;
				int numTris = numFaces * 6;
				
				Vector3[] vertArray = new Vector3[numVerts];
				int[] triArray = new int[numTris];
				Vector2[] uvArray = new Vector2[numTris];
				Color[] colorsArray = new Color[numVerts];
				
				int vertIndex = 0;
				triOffset = verts.Count;
				
				for(int i = 0; i < numFaces; i++) {
					topLeft = topVerts[i];
					bottomLeft = bottomVerts[i];
					topRight = topVerts.GetRepeating(i-1);
					bottomRight = bottomVerts.GetRepeating(i-1);

					topLeftColor = useColorArrays ? input.topColors[i] : input.topColor;
					bottomLeftColor = useColorArrays ? input.bottomColors[i] : input.bottomColor;
					topRightColor = useColorArrays ? input.topColors.GetRepeating(i-1) : input.topColor;
					bottomRightColor = useColorArrays ? input.bottomColors.GetRepeating(i-1) : input.bottomColor;

					vertArray[vertIndex] = bottomLeft;
					vertArray[vertIndex + 1] = topRight;
					vertArray[vertIndex + 2] = topLeft;
					vertArray[vertIndex + 3] = bottomLeft;
					vertArray[vertIndex + 4] = bottomRight;
					vertArray[vertIndex + 5] = topRight;
					
					triArray[vertIndex] = triOffset + 0;
					triArray[vertIndex + 1] = triOffset + 1;
					triArray[vertIndex + 2] = triOffset + 2;
					triArray[vertIndex + 3] = triOffset + 3;
					triArray[vertIndex + 4] = triOffset + 4;
					triArray[vertIndex + 5] = triOffset + 5;
					
					uvArray[vertIndex] = uvBottomLeft;
					uvArray[vertIndex + 1] = uvTopRight;
					uvArray[vertIndex + 2] = uvTopLeft;
					uvArray[vertIndex + 3] = uvBottomLeft;
					uvArray[vertIndex + 4] = uvBottomRight;
					uvArray[vertIndex + 5] = uvTopRight;
					
					colorsArray[vertIndex] = bottomLeftColor;
					colorsArray[vertIndex + 1] = topRightColor;
					colorsArray[vertIndex + 2] = topLeftColor;
					colorsArray[vertIndex + 3] = bottomLeftColor;
					colorsArray[vertIndex + 4] = bottomRightColor;
					colorsArray[vertIndex + 5] = topRightColor;
					
					vertIndex += 6;
					triOffset += 6;
				}
				
				verts.AddRange(vertArray);
				tris.AddRange(triArray);
				uvs.AddRange(uvArray);
				colors.AddRange(colorsArray);
			}
		}
		
		mesh.SetVertices(verts);
		mesh.SetTriangles(tris, 0);
		mesh.SetUVs(0, uvs);
		mesh.SetColors(colors);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}

	// One cap triangle in the given vertex order (reversed when reverseCapWinding is set). UVs come from uvVerts.
	static void AddCapTriangle (ExtrudedPolygonMeshParams input, Rect rect, Vector3[] capVerts, Vector3[] uvVerts, Color[] capColors, Color capColor, bool useColorArrays, int a, int b, int c, int firstIndex, List<Vector3> verts, List<int> tris, List<Vector2> uvs, List<Color> colors) {
		if(input.reverseCapWinding) {
			var swap = a;
			a = c;
			c = swap;
		}
		verts.Add(capVerts[a]);
		verts.Add(capVerts[b]);
		verts.Add(capVerts[c]);
		tris.Add(firstIndex);
		tris.Add(firstIndex+1);
		tris.Add(firstIndex+2);
		uvs.Add(CapUV(input, rect, uvVerts[a]));
		uvs.Add(CapUV(input, rect, uvVerts[b]));
		uvs.Add(CapUV(input, rect, uvVerts[c]));
		colors.Add(useColorArrays ? capColors[a] : capColor);
		colors.Add(useColorArrays ? capColors[b] : capColor);
		colors.Add(useColorArrays ? capColors[c] : capColor);
	}

	static Vector2 CapUV (ExtrudedPolygonMeshParams input, Rect rect, Vector3 vertex) {
		var point = input.capUVPlane == ExtrudedPolygonMeshParams.CapUVPlane.XZ ? new Vector2(vertex.x, vertex.z) : new Vector2(vertex.x, vertex.y);
		// Unclamped, so points outside the rect map outside 0..1.
		return new Vector2((point.x - rect.x) / rect.width, (point.y - rect.y) / rect.height);
	}

	static Vector2 uvTopLeft = new Vector2(0,1);
	static Vector2 uvTopRight = new Vector2(1,1);
	static Vector2 uvBottomRight = new Vector2(1,0);
	static Vector2 uvBottomLeft = new Vector2(0,0);
}
