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

		[InitializeOnLoadMethod]
		static void SubscribeEditorEvents () {
			SceneView.duringSceneGui -= OnSceneGUI;
			SceneView.duringSceneGui += OnSceneGUI;
			AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
			AssemblyReloadEvents.beforeAssemblyReload += UnsubscribeEditorEvents;
		}

		// Editor events outlive script assemblies, so unsubscribe before a code reload or the old handler keeps firing alongside the new one.
		static void UnsubscribeEditorEvents () {
			SceneView.duringSceneGui -= OnSceneGUI;
			AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
			// Nothing will destroy meshes still waiting for a scene view draw once this code is gone.
			OnSceneGUI(null);
		}

		static void OnSceneGUI (SceneView sceneView) {
			foreach(var mesh in meshes) {
				if(Application.isPlaying) Object.Destroy(mesh);
				else Object.DestroyImmediate(mesh);
			}
			meshes.Clear();
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
	}
}
