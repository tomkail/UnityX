using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

public class HeightMapMeshGenerator : MonoBehaviour {
	[AssetSaver]
	public Mesh mesh;
	
	Vector3[] verts;
	int[] tris;
	Vector2[] uvs;
	Color32[] colors;
	
	public bool drawTop = true;
	public bool drawEdges = true;
	public bool drawFloor = true;
	public bool useTriangles = true;
	public bool externals = true;
	public bool internals = false;
	public bool calculateNormals = true;
	public bool useVertColor = false;
	
	public Vector3 offset = new Vector3(-0.5f, -0.5f, -0.5f);
	public Vector3 scale = new Vector3(1, 1, 1);
	
	//for testing
	public bool alwaysDirty = true;
	// If the array sizes have changed and need to be rebuild
	bool isDirty = true;

	private void Setup () {
		mesh = new Mesh();
		mesh.name = gameObject.name;
		isDirty = true;
	}
	
	private void SetMesh(){
		if(mesh == null)
			Setup();

		if(isDirty){
			mesh.Clear();
		}
		mesh.MarkDynamic();
		mesh.vertices = verts;
		if(isDirty){
			mesh.SetTriangles(tris,0);
			mesh.RecalculateBounds();
			isDirty = false;
		}
		if(!colors.IsNullOrEmpty())
		mesh.colors32 = colors;
		mesh.uv = uvs;
		if(calculateNormals)
			mesh.RecalculateNormals();
	}
	
	// public Mesh CreateMeshFromHeightMapArray(Point size, float[] mapArray, float[] colorArray){
	public Mesh CreateMeshFromHeightMap(HeightMap heightMap, ColorMap colorMap = null) {
		if(alwaysDirty && !isDirty) 
			isDirty = true;

		bool usingColorMap = colorMap != null;
		
		Vector3 topLeftVert = Vector3.zero;
		Vector3 topRightVert = Vector3.zero;
		Vector3 bottomLeftVert = Vector3.zero;
		Vector3 bottomRightVert = Vector3.zero;
		
		Color topLeftColor = Color.white;
		Color topRightColor = Color.white;
		Color bottomLeftColor = Color.white;
		Color bottomRightColor = Color.white;
		
		Vector2 uv0 = Vector2.zero;
		Vector2 uv1 = Vector2.zero;
		Vector2 uv2 = Vector2.zero;
		Vector2 uv3 = Vector2.zero;
		
		Point sizeMinusOne = new Point(heightMap.size.x-1, heightMap.size.y-1);
		Vector2 sizeReciprocal = new Vector2(1f/sizeMinusOne.x, 1f/sizeMinusOne.y);
		
		int numVerts = 0;
		int numTris = 0;
		int numUVs = 0;
		
		Point clampedSizeMinusOne = new Point(MathX.Clamp1Infinity(sizeMinusOne.x), MathX.Clamp1Infinity(sizeMinusOne.y));
		if(drawTop) {
			if(internals) {
				if(useTriangles) {
					numVerts += 6 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
					numTris += 6 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
					numUVs += 6 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
				} else {
					numVerts += 4 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
					numTris += 4 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
					numUVs += 4 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
				}
			}
			if(externals) {
				if(useTriangles) {
					numVerts += 6 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
					numTris += 6 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
					numUVs += 6 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
				} else {
					numVerts += 4 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
					numTris += 4 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
					numUVs += 4 * clampedSizeMinusOne.x * clampedSizeMinusOne.y;
				}
			}
		}
		if(drawFloor) {
			if(internals) {
				numVerts += 4;
				numTris += 6;
				numUVs += 4;
			}
			if(externals) {
				numVerts += 4;
				numTris += 6;
				numUVs += 4;
			}
		}
		if(drawEdges) {
			if(internals) {
				numVerts += 6 * clampedSizeMinusOne.x * 2;
				numVerts += 6 * clampedSizeMinusOne.y * 2;
				numTris += 6 * clampedSizeMinusOne.x * 2;
				numTris += 6 * clampedSizeMinusOne.y * 2;
				numUVs += 6 * clampedSizeMinusOne.x * 2;
				numUVs += 6 * clampedSizeMinusOne.y * 2;
			}
			if(externals) {
				numVerts += 6 * clampedSizeMinusOne.x * 2;
				numVerts += 6 * clampedSizeMinusOne.y * 2;
				numTris += 6 * clampedSizeMinusOne.x * 2;
				numTris += 6 * clampedSizeMinusOne.y * 2;
				numUVs += 6 * clampedSizeMinusOne.x * 2;
				numUVs += 6 * clampedSizeMinusOne.y * 2;
			}
		}
		
		verts = new Vector3[numVerts];
		tris = new int[numTris];
		uvs = new Vector2[numUVs];
		if(usingColorMap) {
			colors = new Color32[numVerts];
		}
		int vertIndex = 0;
		int triIndex = 0;
		int uvIndex = 0;
		int colorIndex = 0;

		int savedVertIndex = 0;
		int x,z;
		
		if(drawTop){
			for(z = 0; z < sizeMinusOne.y; z++){
				for(x = 0; x < sizeMinusOne.x; x++){
					float x1 = x * sizeReciprocal.x;
					float x2 = (x+1) * sizeReciprocal.x;
					float z1 = z * sizeReciprocal.y;
					float z2 = (z+1) * sizeReciprocal.y;
					
					if(!drawFloor && (heightMap.values[z * (sizeMinusOne.x+1) + x] == 0f && heightMap.values[z * (sizeMinusOne.x+1) + x] == 0f && heightMap.values[(z+1) * (sizeMinusOne.x+1) + x] == 0f && heightMap.values[(z+1) * (sizeMinusOne.x+1) + (z+1)] == 0f)){
						continue;
					}
					
					topLeftVert = Vector3.Scale(new Vector3(x1 + offset.x, heightMap.values[z * (sizeMinusOne.x+1) + x] + offset.y, z1 + offset.z), scale);
					topRightVert = Vector3.Scale(new Vector3(x2 + offset.x, heightMap.values[z * (sizeMinusOne.x+1) + (x+1)] + offset.y, z1 + offset.z), scale);
					bottomLeftVert = Vector3.Scale(new Vector3(x1 + offset.x, heightMap.values[(z+1) * (sizeMinusOne.x+1) + x] + offset.y, z2 + offset.z), scale);
					bottomRightVert = Vector3.Scale(new Vector3(x2 + offset.x, heightMap.values[(z+1) * (sizeMinusOne.x+1) + (x+1)] + offset.y, z2 + offset.z), scale);
					
					if(usingColorMap) {
						topLeftColor = colorMap.GetValueAtGridPoint(x, z);
						topRightColor = colorMap.GetValueAtGridPoint(x, z);
						bottomLeftColor = colorMap.GetValueAtGridPoint(x, z);
						bottomRightColor = colorMap.GetValueAtGridPoint(x, z);
					}
					
					if(useTriangles){
						if(externals){
							savedVertIndex = vertIndex;
							
							verts[vertIndex] = topLeftVert;
							verts[vertIndex + 1] = topRightVert;
							verts[vertIndex + 2] = bottomLeftVert;
							verts[vertIndex + 3] = topRightVert;
							verts[vertIndex + 4] = bottomLeftVert;
							verts[vertIndex + 5] = bottomRightVert;
							vertIndex += 6;
							
							tris[triIndex] = savedVertIndex + 0;
							tris[triIndex + 1] = savedVertIndex + 2;
							tris[triIndex + 2] = savedVertIndex + 1;
							tris[triIndex + 3] = savedVertIndex + 4;
							tris[triIndex + 4] = savedVertIndex + 5;
							tris[triIndex + 5] = savedVertIndex + 3;
							triIndex += 6;
							
							uv0 = new Vector2(x1, z1);
							uv1 = new Vector2(x2, z1);
							uv2 = new Vector2(x1, z2);
							uv3 = new Vector2(x2, z2);
							
							uvs[uvIndex] = uv0;
							uvs[uvIndex + 1] = uv1;
							uvs[uvIndex + 2] = uv2;
							uvs[uvIndex + 3] = uv1;
							uvs[uvIndex + 4] = uv2;
							uvs[uvIndex + 5] = uv3;
							uvIndex += 6;
							
							if(usingColorMap) {
								colors[colorIndex] = topLeftColor;
								colors[colorIndex + 1] = topRightColor;
								colors[colorIndex + 2] = bottomLeftColor;
								colors[colorIndex + 3] = topRightColor;
								colors[colorIndex + 4] = bottomLeftColor;
								colors[colorIndex + 5] = bottomRightColor;
								colorIndex += 6;
							}
						}
						if(internals){
							savedVertIndex = vertIndex;
							
							verts[vertIndex] = topLeftVert;
							verts[vertIndex + 1] = topRightVert;
							verts[vertIndex + 2] = bottomLeftVert;
							verts[vertIndex + 3] = topRightVert;
							verts[vertIndex + 4] = bottomLeftVert;
							verts[vertIndex + 5] = bottomRightVert;
							vertIndex += 6;
							
							tris[triIndex] = savedVertIndex + 3;
							tris[triIndex + 1] = savedVertIndex + 5;
							tris[triIndex + 2] = savedVertIndex + 4;
							tris[triIndex + 3] = savedVertIndex + 1;
							tris[triIndex + 4] = savedVertIndex + 2;
							tris[triIndex + 5] = savedVertIndex + 0;
							triIndex += 6;
							
							uv0 = new Vector2(x1, z1);
							uv1 = new Vector2(x2, z1);
							uv2 = new Vector2(x1, z2);
							uv3 = new Vector2(x2, z2);
							
							uvs[uvIndex] = uv2;
							uvs[uvIndex + 1] = uv1;
							uvs[uvIndex + 2] = uv0;
							uvs[uvIndex + 3] = uv3;
							uvs[uvIndex + 4] = uv2;
							uvs[uvIndex + 5] = uv1;
							uvIndex += 6;
							
							if(usingColorMap) {
								colors[colorIndex] = topLeftColor;
								colors[colorIndex + 1] = topRightColor;
								colors[colorIndex + 2] = bottomLeftColor;
								colors[colorIndex + 3] = topRightColor;
								colors[colorIndex + 4] = bottomLeftColor;
								colors[colorIndex + 5] = bottomRightColor;
								colorIndex += 6;
							}
						}
						
						
					} else {
						if(externals){
							savedVertIndex = vertIndex;
							
							verts[vertIndex] = topLeftVert;
							verts[vertIndex + 1] = topRightVert;
							verts[vertIndex + 2] = bottomLeftVert;
							verts[vertIndex + 3] = bottomRightVert;
							vertIndex += 4;
							
							tris[triIndex] = savedVertIndex + 0;
							tris[triIndex + 1] = savedVertIndex + 2;
							tris[triIndex + 2] = savedVertIndex + 1;
							tris[triIndex + 3] = savedVertIndex + 2;
							tris[triIndex + 4] = savedVertIndex + 3;
							tris[triIndex + 5] = savedVertIndex + 1;
							triIndex += 6;
							
							uv0 = new Vector2(x1, z1);
							uv1 = new Vector2(x2, z1);
							uv2 = new Vector2(x1, z2);
							uv3 = new Vector2(x2, z2);
							
							uvs[uvIndex] = uv0;
							uvs[uvIndex + 1] = uv1;
							uvs[uvIndex + 2] = uv2;
							uvs[uvIndex + 3] = uv3;
							uvIndex += 4;
							
							if(usingColorMap) {
								colors[colorIndex] = topLeftColor;
								colors[colorIndex + 1] = topRightColor;
								colors[colorIndex + 2] = bottomLeftColor;
								colors[colorIndex + 3] = bottomRightColor;
								colorIndex += 4;
							}
						}
						if(internals){
							savedVertIndex = vertIndex;
							
							verts[vertIndex] = topLeftVert;
							verts[vertIndex + 1] = topRightVert;
							verts[vertIndex + 2] = bottomLeftVert;
							verts[vertIndex + 3] = bottomRightVert;
							vertIndex += 4;
							
							tris[triIndex] = savedVertIndex + 1;
							tris[triIndex + 1] = savedVertIndex + 3;
							tris[triIndex + 2] = savedVertIndex + 2;
							tris[triIndex + 3] = savedVertIndex + 1;
							tris[triIndex + 4] = savedVertIndex + 2;
							tris[triIndex + 5] = savedVertIndex + 0;
							triIndex += 6;
							
							uv0 = new Vector2(x1, z1);
							uv1 = new Vector2(x2, z1);
							uv2 = new Vector2(x1, z2);
							uv3 = new Vector2(x2, z2);
							
							uvs[uvIndex] = uv3;
							uvs[uvIndex + 1] = uv2;
							uvs[uvIndex + 2] = uv1;
							uvs[uvIndex + 3] = uv0;
							uvIndex += 4;
							
							if(usingColorMap) {
								colors[colorIndex] = topLeftColor;
								colors[colorIndex + 1] = topRightColor;
								colors[colorIndex + 2] = bottomLeftColor;
								colors[colorIndex + 3] = bottomRightColor;
								colorIndex += 4;
							}
						}
					}
				}
			}
		}
		
		//NOTE: UVS ARNT PERFECT - I WAS GETTING -0.1 to 0.9 on some tests- possibly linked to size drawing
		if(drawFloor){
			if(externals){
				savedVertIndex = vertIndex;
				
				topLeftVert = Vector3.Scale((new Vector3(0,0,0) + offset), scale);
				topRightVert = Vector3.Scale((new Vector3(0,0,1) + offset), scale);
				bottomLeftVert = Vector3.Scale((new Vector3(1,0,0) + offset), scale);
				bottomRightVert = Vector3.Scale((new Vector3(1,0,1) + offset), scale);
				
				if(usingColorMap) {
					topLeftColor = colorMap.GetValueAtGridPoint(0,0);
					topRightColor = colorMap.GetValueAtGridPoint(sizeMinusOne.x-1,0);
					bottomLeftColor = colorMap.GetValueAtGridPoint(0,sizeMinusOne.y-1);
					bottomRightColor = colorMap.GetValueAtGridPoint(sizeMinusOne.x-1, sizeMinusOne.y-1);
				}
				
				verts[vertIndex] = topLeftVert;
				verts[vertIndex + 1] = topRightVert;
				verts[vertIndex + 2] = bottomLeftVert;
				verts[vertIndex + 3] = bottomRightVert;
				vertIndex += 4;
				
				tris[triIndex] = savedVertIndex + 0;
				tris[triIndex + 1] = savedVertIndex + 2;
				tris[triIndex + 2] = savedVertIndex + 1;
				tris[triIndex + 3] = savedVertIndex + 2;
				tris[triIndex + 4] = savedVertIndex + 3;
				tris[triIndex + 5] = savedVertIndex + 1;
				triIndex += 6;
				
				uv0 = new Vector2(0,0);
				uv1 = new Vector2(1,0);
				uv2 = new Vector2(0,1);
				uv3 = new Vector2(1,1);
				
				uvs[uvIndex] = uv0;
				uvs[uvIndex + 1] = uv1;
				uvs[uvIndex + 2] = uv2;
				uvs[uvIndex + 3] = uv3;
				uvIndex += 4;
				
				if(usingColorMap) {
					colors[colorIndex] = topLeftColor;
					colors[colorIndex + 1] = topRightColor;
					colors[colorIndex + 2] = bottomLeftColor;
					colors[colorIndex + 3] = bottomRightColor;
					colorIndex += 4;
				}
			}
			
			if(internals){
				savedVertIndex = vertIndex;
				
				topLeftVert = Vector3.Scale((new Vector3(0,0,0) + offset), scale);
				topRightVert = Vector3.Scale((new Vector3(0,0,1) + offset), scale);
				bottomLeftVert = Vector3.Scale((new Vector3(1,0,0) + offset), scale);
				bottomRightVert = Vector3.Scale((new Vector3(1,0,1) + offset), scale);
				
				verts[vertIndex] = topLeftVert;
				verts[vertIndex + 1] = topRightVert;
				verts[vertIndex + 2] = bottomLeftVert;
				verts[vertIndex + 3] = bottomRightVert;
				vertIndex += 4;
				
				tris[triIndex] = savedVertIndex + 1;
				tris[triIndex + 1] = savedVertIndex + 3;
				tris[triIndex + 2] = savedVertIndex + 2;
				tris[triIndex + 3] = savedVertIndex + 1;
				tris[triIndex + 4] = savedVertIndex + 2;
				tris[triIndex + 5] = savedVertIndex + 0;
				triIndex += 6;
				
				uv0 = new Vector2(0,0);
				uv1 = new Vector2(1,0);
				uv2 = new Vector2(0,1);
				uv3 = new Vector2(1,1);
				
				uvs[uvIndex] = uv3;
				uvs[uvIndex + 1] = uv2;
				uvs[uvIndex + 2] = uv1;
				uvs[uvIndex + 3] = uv0;
				uvIndex += 4;
				
				if(usingColorMap) {
					colors[colorIndex] = topLeftColor;
					colors[colorIndex + 1] = topRightColor;
					colors[colorIndex + 2] = bottomLeftColor;
					colors[colorIndex + 3] = bottomRightColor;
					colorIndex += 4;
				}
			}
		}
		
		if(drawEdges){
			
			//left & right
			for(z = 0; z < sizeMinusOne.y; z++){
				float z1 = z * sizeReciprocal.y;
				float z2 = (z+1) * sizeReciprocal.y;
				//Left
				topLeftVert = Vector3.Scale(new Vector3(offset.x, heightMap.values[z * (sizeMinusOne.x+1)] + offset.y, z1 + offset.z), scale);
				topRightVert = Vector3.Scale(new Vector3(offset.x, heightMap.values[(z+1) * (sizeMinusOne.x+1)] + offset.y, z2 + offset.z), scale);
				bottomLeftVert = Vector3.Scale(new Vector3(offset.x, offset.y, z1 + offset.z), scale);
				bottomRightVert = Vector3.Scale(new Vector3(offset.x, offset.y, z2 + offset.z), scale);
				
				if(usingColorMap) {
					topLeftColor = colorMap.GetValueAtGridPoint(0, z);
					topRightColor = colorMap.GetValueAtGridPoint(0, z);
					bottomLeftColor = colorMap.GetValueAtGridPoint(0, z);
					bottomRightColor = colorMap.GetValueAtGridPoint(0, z);
				}
				
				if(externals){
					savedVertIndex = vertIndex;
					
					verts[vertIndex] = topLeftVert;
					verts[vertIndex + 1] = topRightVert;
					verts[vertIndex + 2] = bottomLeftVert;
					verts[vertIndex + 3] = topRightVert;
					verts[vertIndex + 4] = bottomLeftVert;
					verts[vertIndex + 5] = bottomRightVert;
					vertIndex += 6;
					
					tris[triIndex] = savedVertIndex;
					tris[triIndex + 1] = savedVertIndex + 2;
					tris[triIndex + 2] = savedVertIndex + 1;
					tris[triIndex + 3] = savedVertIndex + 4;
					tris[triIndex + 4] = savedVertIndex + 5;
					tris[triIndex + 5] = savedVertIndex + 3;
					triIndex += 6;
					
					uv0 = new Vector2(1, z1);
					uv1 = new Vector2(1, z1);
					uv2 = new Vector2(1, z2);
					uv3 = new Vector2(1, z2);
					
					uvs[uvIndex] = uv0;
					uvs[uvIndex+1] = uv1;
					uvs[uvIndex+2] = uv2;
					uvs[uvIndex+3] = uv1;
					uvs[uvIndex+4] = uv2;
					uvs[uvIndex+5] = uv3;
					uvIndex += 6;
					
					if(usingColorMap) {
						colors[colorIndex] = topLeftColor;
						colors[colorIndex + 1] = topRightColor;
						colors[colorIndex + 2] = bottomLeftColor;
						colors[colorIndex + 3] = topRightColor;
						colors[colorIndex + 4] = bottomLeftColor;
						colors[colorIndex + 5] = bottomRightColor;
						colorIndex += 6;
					}
				}
				if(internals){
					savedVertIndex = vertIndex;
					
					verts[vertIndex] = topLeftVert;
					verts[vertIndex + 1] = topRightVert;
					verts[vertIndex + 2] = bottomLeftVert;
					verts[vertIndex + 3] = topRightVert;
					verts[vertIndex + 4] = bottomLeftVert;
					verts[vertIndex + 5] = bottomRightVert;
					vertIndex += 6;
					
					
					tris[triIndex] = savedVertIndex + 3;
					tris[triIndex + 1] = savedVertIndex + 5;
					tris[triIndex + 2] = savedVertIndex + 4;
					tris[triIndex + 3] = savedVertIndex + 1;
					tris[triIndex + 4] = savedVertIndex + 2;
					tris[triIndex + 5] = savedVertIndex + 0;
					triIndex += 6;
					
					uv0 = new Vector2(0,0);
					uv1 = new Vector2(1,0);
					uv2 = new Vector2(0,1);
					uv3 = new Vector2(1,1);
					
					uvs[uvIndex] = uv2;
					uvs[uvIndex + 1] = uv1;
					uvs[uvIndex + 2] = uv0;
					uvs[uvIndex + 3] = uv3;
					uvs[uvIndex + 4] = uv2;
					uvs[uvIndex + 5] = uv1;
					uvIndex += 6;
					
					if(usingColorMap) {
						colors[colorIndex] = topLeftColor;
						colors[colorIndex + 1] = topRightColor;
						colors[colorIndex + 2] = bottomLeftColor;
						colors[colorIndex + 3] = topRightColor;
						colors[colorIndex + 4] = bottomLeftColor;
						colors[colorIndex + 5] = bottomRightColor;
						colorIndex += 6;
					}
				}
				//Right
				topLeftVert = Vector3.Scale(new Vector3(1 + offset.x, heightMap.values[(z+1) * (sizeMinusOne.x+1) + (sizeMinusOne.y)] + offset.y, z2 + offset.z), scale);
				topRightVert = Vector3.Scale(new Vector3(1 + offset.x, heightMap.values[(z) * (sizeMinusOne.x+1) + (sizeMinusOne.y)] + offset.y, z1 + offset.z), scale);
				bottomLeftVert = Vector3.Scale(new Vector3(1 + offset.x, offset.y, z2 + offset.z), scale);
				bottomRightVert = Vector3.Scale(new Vector3(1 + offset.x, offset.y, z1 + offset.z), scale);
				
				if(usingColorMap) {
					topLeftColor = colorMap.GetValueAtGridPoint(sizeMinusOne.x-1, z);
					topRightColor = colorMap.GetValueAtGridPoint(sizeMinusOne.x-1, z);
					bottomLeftColor = colorMap.GetValueAtGridPoint(sizeMinusOne.x-1, z);
					bottomRightColor = colorMap.GetValueAtGridPoint(sizeMinusOne.x-1, z);
				}
				
				if(externals){
					savedVertIndex = vertIndex;
					
					verts[vertIndex] = topLeftVert;
					verts[vertIndex + 1] = topRightVert;
					verts[vertIndex + 2] = bottomLeftVert;
					verts[vertIndex + 3] = topRightVert;
					verts[vertIndex + 4] = bottomLeftVert;
					verts[vertIndex + 5] = bottomRightVert;
					vertIndex += 6;
					
					tris[triIndex] = savedVertIndex;
					tris[triIndex + 1] = savedVertIndex + 2;
					tris[triIndex + 2] = savedVertIndex + 1;
					tris[triIndex + 3] = savedVertIndex + 4;
					tris[triIndex + 4] = savedVertIndex + 5;
					tris[triIndex + 5] = savedVertIndex + 3;
					triIndex += 6;
					
					uv0 = new Vector2(1, z1);
					uv1 = new Vector2(1, z1);
					uv2 = new Vector2(1, z2);
					uv3 = new Vector2(1, z2);
					
					uvs[uvIndex] = uv0;
					uvs[uvIndex+1] = uv1;
					uvs[uvIndex+2] = uv2;
					uvs[uvIndex+3] = uv1;
					uvs[uvIndex+4] = uv2;
					uvs[uvIndex+5] = uv3;
					uvIndex += 6;
					
					if(usingColorMap) {
						colors[colorIndex] = topLeftColor;
						colors[colorIndex + 1] = topRightColor;
						colors[colorIndex + 2] = bottomLeftColor;
						colors[colorIndex + 3] = topRightColor;
						colors[colorIndex + 4] = bottomLeftColor;
						colors[colorIndex + 5] = bottomRightColor;
						colorIndex += 6;
					}
				}
				if(internals){
					savedVertIndex = vertIndex;
					
					verts[vertIndex] = topLeftVert;
					verts[vertIndex + 1] = topRightVert;
					verts[vertIndex + 2] = bottomLeftVert;
					verts[vertIndex + 3] = topRightVert;
					verts[vertIndex + 4] = bottomLeftVert;
					verts[vertIndex + 5] = bottomRightVert;
					vertIndex += 6;
					
					tris[triIndex] = savedVertIndex + 3;
					tris[triIndex + 1] = savedVertIndex + 5;
					tris[triIndex + 2] = savedVertIndex + 4;
					tris[triIndex + 3] = savedVertIndex + 1;
					tris[triIndex + 4] = savedVertIndex + 2;
					tris[triIndex + 5] = savedVertIndex + 0;
					triIndex += 6;
					
					//This is weird? Look into this later.
					uv0 = new Vector2(0,0);
					uv1 = new Vector2(1,0);
					uv2 = new Vector2(0,1);
					uv3 = new Vector2(1,1);
					
					uvs[uvIndex] = uv2;
					uvs[uvIndex + 1] = uv1;
					uvs[uvIndex + 2] = uv0;
					uvs[uvIndex + 3] = uv3;
					uvs[uvIndex + 4] = uv2;
					uvs[uvIndex + 5] = uv1;
					uvIndex += 6;
					
					if(usingColorMap) {
						colors[colorIndex] = topLeftColor;
						colors[colorIndex + 1] = topRightColor;
						colors[colorIndex + 2] = bottomLeftColor;
						colors[colorIndex + 3] = topRightColor;
						colors[colorIndex + 4] = bottomLeftColor;
						colors[colorIndex + 5] = bottomRightColor;
						colorIndex += 6;
					}
				}
			}
			//front & back
			for(x = 0; x < sizeMinusOne.x; x++){
				float x1 = x * sizeReciprocal.x;
				float x2 = (x+1) * sizeReciprocal.x;
				//Front
				topLeftVert = Vector3.Scale(new Vector3(x1 + offset.x, heightMap.values[sizeMinusOne.x*(sizeMinusOne.x+1)+x] + offset.y, 1 + offset.z), scale);
				topRightVert = Vector3.Scale(new Vector3(x2 + offset.x, heightMap.values[sizeMinusOne.x*(sizeMinusOne.x+1)+x+1] + offset.y, 1 + offset.z), scale);
				bottomLeftVert = Vector3.Scale(new Vector3(x1 + offset.x, offset.y, 1 + offset.z), scale);
				bottomRightVert = Vector3.Scale(new Vector3(x2 + offset.x, offset.y, 1 + offset.z), scale);
				
				if(usingColorMap) {
					topLeftColor = colorMap.GetValueAtGridPoint(x, 0);
					topRightColor = colorMap.GetValueAtGridPoint(x, 0);
					bottomLeftColor = colorMap.GetValueAtGridPoint(x, 0);
					bottomRightColor = colorMap.GetValueAtGridPoint(x, 0);
				}
				
				if(externals){
					savedVertIndex = vertIndex;
					
					verts[vertIndex] = topLeftVert;
					verts[vertIndex + 1] = topRightVert;
					verts[vertIndex + 2] = bottomLeftVert;
					verts[vertIndex + 3] = topRightVert;
					verts[vertIndex + 4] = bottomLeftVert;
					verts[vertIndex + 5] = bottomRightVert;
					vertIndex += 6;
					
					tris[triIndex] = savedVertIndex;
					tris[triIndex + 1] = savedVertIndex + 2;
					tris[triIndex + 2] = savedVertIndex + 1;
					tris[triIndex + 3] = savedVertIndex + 4;
					tris[triIndex + 4] = savedVertIndex + 5;
					tris[triIndex + 5] = savedVertIndex + 3;
					triIndex += 6;
					
					uv0 = new Vector2(x1, 1);
					uv1 = new Vector2(x1, 1);
					uv2 = new Vector2(x2, 1);
					uv3 = new Vector2(x2, 1);
					
					// uvs.AddRange(new Vector2[6]{
					// 	uv0, uv1, uv2, uv1, uv2, uv3
					// });
					uvs[uvIndex] = uv0;
					uvs[uvIndex+1] = uv1;
					uvs[uvIndex+2] = uv2;
					uvs[uvIndex+3] = uv1;
					uvs[uvIndex+4] = uv2;
					uvs[uvIndex+5] = uv3;
					uvIndex += 6;
					
					if(usingColorMap) {
						colors[colorIndex] = topLeftColor;
						colors[colorIndex + 1] = topRightColor;
						colors[colorIndex + 2] = bottomLeftColor;
						colors[colorIndex + 3] = topRightColor;
						colors[colorIndex + 4] = bottomLeftColor;
						colors[colorIndex + 5] = bottomRightColor;
						colorIndex += 6;
					}
				}
				if(internals){
					savedVertIndex = vertIndex;
					
					verts[vertIndex] = topLeftVert;
					verts[vertIndex + 1] = topRightVert;
					verts[vertIndex + 2] = bottomLeftVert;
					verts[vertIndex + 3] = topRightVert;
					verts[vertIndex + 4] = bottomLeftVert;
					verts[vertIndex + 5] = bottomRightVert;
					vertIndex += 6;
					
					tris[triIndex] = savedVertIndex + 3;
					tris[triIndex + 1] = savedVertIndex + 5;
					tris[triIndex + 2] = savedVertIndex + 4;
					tris[triIndex + 3] = savedVertIndex + 1;
					tris[triIndex + 4] = savedVertIndex + 2;
					tris[triIndex + 5] = savedVertIndex + 0;
					triIndex += 6;
					
					uv0 = new Vector2(0,0);
					uv1 = new Vector2(1,0);
					uv2 = new Vector2(0,1);
					uv3 = new Vector2(1,1);
					
					uvs[uvIndex] = uv2;
					uvs[uvIndex + 1] = uv1;
					uvs[uvIndex + 2] = uv0;
					uvs[uvIndex + 3] = uv3;
					uvs[uvIndex + 4] = uv2;
					uvs[uvIndex + 5] = uv1;
					uvIndex += 6;
					
					if(usingColorMap) {
						colors[colorIndex] = topLeftColor;
						colors[colorIndex + 1] = topRightColor;
						colors[colorIndex + 2] = bottomLeftColor;
						colors[colorIndex + 3] = topRightColor;
						colors[colorIndex + 4] = bottomLeftColor;
						colors[colorIndex + 5] = bottomRightColor;
						colorIndex += 6;
					}
				}
				//Back
				topLeftVert = Vector3.Scale(new Vector3(x2 + offset.x, heightMap.values[x+1] + offset.y, offset.z), scale);
				topRightVert = Vector3.Scale(new Vector3(x1 + offset.x, heightMap.values[x] + offset.y, offset.z), scale);
				bottomLeftVert = Vector3.Scale(new Vector3(x2 + offset.x, offset.y, offset.z), scale);
				bottomRightVert = Vector3.Scale(new Vector3(x1 + offset.x, offset.y, offset.z), scale);
				
				if(usingColorMap) {
					topLeftColor = colorMap.GetValueAtGridPoint(x, sizeMinusOne.y-1);
					topRightColor = colorMap.GetValueAtGridPoint(x, sizeMinusOne.y-1);
					bottomLeftColor = colorMap.GetValueAtGridPoint(x, sizeMinusOne.y-1);
					bottomRightColor = colorMap.GetValueAtGridPoint(x, sizeMinusOne.y-1);
				}
				
				if(externals){
					savedVertIndex = vertIndex;
					
					verts[vertIndex] = topLeftVert;
					verts[vertIndex + 1] = topRightVert;
					verts[vertIndex + 2] = bottomLeftVert;
					verts[vertIndex + 3] = topRightVert;
					verts[vertIndex + 4] = bottomLeftVert;
					verts[vertIndex + 5] = bottomRightVert;
					vertIndex += 6;
					
					tris[triIndex] = savedVertIndex;
					tris[triIndex + 1] = savedVertIndex + 2;
					tris[triIndex + 2] = savedVertIndex + 1;
					tris[triIndex + 3] = savedVertIndex + 4;
					tris[triIndex + 4] = savedVertIndex + 5;
					tris[triIndex + 5] = savedVertIndex + 3;
					triIndex += 6;
					
					uv0 = new Vector2(x1, 1);
					uv1 = new Vector2(x1, 1);
					uv2 = new Vector2(x2, 1);
					uv3 = new Vector2(x2, 1);
					
					uvs[uvIndex] = uv0;
					uvs[uvIndex+1] = uv1;
					uvs[uvIndex+2] = uv2;
					uvs[uvIndex+3] = uv1;
					uvs[uvIndex+4] = uv2;
					uvs[uvIndex+5] = uv3;
					uvIndex += 6;
					
					if(usingColorMap) {
						colors[colorIndex] = topLeftColor;
						colors[colorIndex + 1] = topRightColor;
						colors[colorIndex + 2] = bottomLeftColor;
						colors[colorIndex + 3] = topRightColor;
						colors[colorIndex + 4] = bottomLeftColor;
						colors[colorIndex + 5] = bottomRightColor;
						colorIndex += 6;
					}
				}
				if(internals){
					savedVertIndex = vertIndex;
					
					verts[vertIndex] = topLeftVert;
					verts[vertIndex + 1] = topRightVert;
					verts[vertIndex + 2] = bottomLeftVert;
					verts[vertIndex + 3] = topRightVert;
					verts[vertIndex + 4] = bottomLeftVert;
					verts[vertIndex + 5] = bottomRightVert;
					vertIndex += 6;
					
					tris[triIndex] = savedVertIndex + 3;
					tris[triIndex + 1] = savedVertIndex + 5;
					tris[triIndex + 2] = savedVertIndex + 4;
					tris[triIndex + 3] = savedVertIndex + 1;
					tris[triIndex + 4] = savedVertIndex + 2;
					tris[triIndex + 5] = savedVertIndex + 0;
					triIndex += 6;
					
					uv0 = new Vector2(0,0);
					uv1 = new Vector2(1,0);
					uv2 = new Vector2(0,1);
					uv3 = new Vector2(1,1);
					
					uvs[uvIndex] = uv2;
					uvs[uvIndex + 1] = uv1;
					uvs[uvIndex + 2] = uv0;
					uvs[uvIndex + 3] = uv3;
					uvs[uvIndex + 4] = uv2;
					uvs[uvIndex + 5] = uv1;
					uvIndex += 6;
					
					if(usingColorMap) {
						colors[colorIndex] = topLeftColor;
						colors[colorIndex + 1] = topRightColor;
						colors[colorIndex + 2] = bottomLeftColor;
						colors[colorIndex + 3] = topRightColor;
						colors[colorIndex + 4] = bottomLeftColor;
						colors[colorIndex + 5] = bottomRightColor;
						colorIndex += 6;
					}
				}
			}
		}
		
		SetMesh();
		return mesh;
	}
}