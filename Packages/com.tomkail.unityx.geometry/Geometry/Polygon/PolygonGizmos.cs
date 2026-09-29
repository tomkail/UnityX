using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityX.Geometry {
	// Filled-polygon gizmo helpers. These used to live on GizmosX (Core), but they need Triangulator, and Core
	// must not depend on Geometry — so they live here instead.
	public static class PolygonGizmos {
		// Destroying meshes during OnDrawGizmos crashes Unity, and reusing one mesh stops the last draw showing.
		// Instead every mesh made here is tracked and destroyed on the next scene view draw.
		#if UNITY_EDITOR
		static readonly List<Mesh> meshes = new();

		static PolygonGizmos () {
			SceneView.duringSceneGui += _ => {
				foreach(var mesh in meshes) {
					if(Application.isPlaying) Object.Destroy(mesh);
					else Object.DestroyImmediate(mesh);
				}
				meshes.Clear();
			};
		}
		#endif

		// Builds a flat mesh (z = 0) from a simple polygon. Returns null outside the editor.
		// The mesh is temporary: it's destroyed on the next scene view draw, so don't keep it across frames.
		public static Mesh CreatePolygonMesh (Vector2[] points, bool doubleSided = false) {
			#if UNITY_EDITOR
			var mesh = new Mesh { name = "Polygon Gizmo Temp Mesh" };
			meshes.Add(mesh);

			var tris = new List<int>();
			Triangulator.GenerateIndices(points, tris);
			if(doubleSided) {
				var doubleVerts = new Vector3[points.Length * 2];
				for(int i = 0; i < points.Length; i++) doubleVerts[i] = doubleVerts[i+points.Length] = points[i];

				var doubleTris = new int[tris.Count * 2];
				int triLengthMinusOne = tris.Count-1;
				for(int i = 0; i < tris.Count; i++) {
					doubleTris[i] = tris[i];
					doubleTris[i+tris.Count] = tris[triLengthMinusOne - i] + points.Length;
				}
				mesh.vertices = doubleVerts;
				mesh.triangles = doubleTris;
			} else {
				mesh.vertices = points.Select(v => new Vector3(v.x, v.y, 0)).ToArray();
				mesh.SetTriangles(tris, 0);
			}

			mesh.RecalculateNormals();
			return mesh;
			#else
			return null;
			#endif
		}

		public static void DrawPolygon (Vector2[] points, bool doubleSided = false) {
			var mesh = CreatePolygonMesh(points, doubleSided);
			if(mesh != null && mesh.vertexCount > 0 && mesh.normals.Length > 0)
				Gizmos.DrawMesh(mesh);
		}

		public static void DrawPolygon (Vector3 position, Quaternion rotation, Vector3 scale, Vector2[] points, bool doubleSided = false) {
			var cachedMatrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(position, rotation, scale);
			DrawPolygon(points, doubleSided);
			Gizmos.matrix = cachedMatrix;
		}

		// A solid prism: the polygon extruded along local z, centred on z = 0. Editor only, like CreatePolygonMesh.
		public static void DrawExtrudedPolygon (Vector2[] points, float depth) {
			#if UNITY_EDITOR
			if(points == null || points.Length == 0) return;
			var depthOffset = Vector3.forward * depth * 0.5f;
			var sides = new Mesh { name = "Polygon Gizmo Temp Mesh" };
			meshes.Add(sides);
			var verts = new Vector3[points.Length * 4];
			var tris = new int[points.Length * 12];
			for(int i = 0; i < points.Length; i++) {
				Vector3 a = points[i];
				Vector3 b = points[(i + 1) % points.Length];
				int v = i * 4, t = i * 12;
				verts[v] = a - depthOffset;
				verts[v+1] = b - depthOffset;
				verts[v+2] = a + depthOffset;
				verts[v+3] = b + depthOffset;
				// Both windings, so the sides are visible from inside and out
				tris[t] = v; tris[t+1] = v+1; tris[t+2] = v+2;
				tris[t+3] = v+1; tris[t+4] = v+3; tris[t+5] = v+2;
				tris[t+6] = v+2; tris[t+7] = v+1; tris[t+8] = v;
				tris[t+9] = v+2; tris[t+10] = v+3; tris[t+11] = v+1;
			}
			sides.vertices = verts;
			sides.triangles = tris;
			sides.RecalculateNormals();
			Gizmos.DrawMesh(sides);

			var cap = CreatePolygonMesh(points, true);
			if(cap != null && cap.vertexCount > 0 && cap.normals.Length > 0) {
				Gizmos.DrawMesh(cap, -depthOffset);
				Gizmos.DrawMesh(cap, depthOffset);
			}
			#endif
		}

		public static void DrawExtrudedPolygon (Vector3 position, Quaternion rotation, Vector3 scale, Vector2[] points, float depth) {
			var cachedMatrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(position, rotation, scale);
			DrawExtrudedPolygon(points, depth);
			Gizmos.matrix = cachedMatrix;
		}
	}
}
