using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(Region)), CanEditMultipleObjects]
public class RegionEditor : Editor {
	Region data;
	PolygonEditorHandles polygonEditor;

	public void OnEnable() {
		if(target == null) {
			data = null;
		} else {
			Debug.Assert(target as Region != null, "Cannot cast "+target + " to "+typeof(Region));
			data = (Region) target;
		}
		
		Undo.undoRedoPerformed += HandleUndoRedoCallback;
		SceneView.duringSceneGui += OnSceneView;
		polygonEditor = new PolygonEditorHandles(data.transform, data.offsetMatrix);
		polygonEditor.defaultPolygonFunc = () => {
			return new Vector2[] {
				new Vector2(-0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, -0.5f),
				new Vector2(-0.5f, -0.5f),
			};
		};
	}		

	void OnDisable() {
		Undo.undoRedoPerformed -= HandleUndoRedoCallback;
		SceneView.duringSceneGui -= OnSceneView;
        // polygonEditor.Destroy();
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		EditorGUI.BeginChangeCheck();
		
		// base.OnInspectorGUI();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("_polygon"));
		var is2D = serializedObject.FindProperty("_height").floatValue <= 0;
		var newIs2D = EditorGUILayout.Toggle(new GUIContent("2D", "If this region has height"), is2D);
		if(is2D != newIs2D) {
			serializedObject.FindProperty("_height").floatValue = newIs2D ? 0 : 1;
		}
		if(!newIs2D) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_height"));
			serializedObject.FindProperty("_height").floatValue = Mathf.Max(serializedObject.FindProperty("_height").floatValue, 0.001f);
		}

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_selectedFillColor"));

		if(EditorGUI.EndChangeCheck()) 
			data.OnPropertiesChanged();
		serializedObject.ApplyModifiedProperties();
	}

	void OnSceneGUI () {
		Undo.RecordObject(data, "Edit polygon");
		polygonEditor.offsetMatrix = data.offsetMatrix;
		polygonEditor.OnSceneGUI(data.polygon);
		
		if(!data.in2DMode && !polygonEditor.editing) {
			var worldPosition = data.transform.position;
			var matrix = data.matrix;
			var handleSize = HandleUtility.GetHandleSize(worldPosition) * 0.2f;
			var rotation = Quaternion.LookRotation(data.worldNormal, matrix.MultiplyVector(Vector3.up));
			var distance = data.height * 0.5f;
			distance = matrix.MultiplyVector(new Vector3(0,0,distance)).magnitude;
			Vector3 newPos = Handles.FreeMoveHandle(worldPosition + data.worldNormal * distance, handleSize, Vector3.zero, ((int controlID, Vector3 handlePosition, Quaternion handleRotation, float size, EventType eventType) => {
				handleRotation = rotation;
				Handles.DrawDottedLine(worldPosition, handlePosition, 5);
				Handles.ConeHandleCap(controlID, handlePosition, handleRotation, handleSize, eventType);
			}));
			// var newDistance = Vector3.Distance();
			data.height = matrix.inverse.MultiplyVector(Vector3.Project(worldPosition - newPos, data.worldNormal)).magnitude * 2;
		}
	}

	void HandleUndoRedoCallback () {
		data.OnPropertiesChanged();
	}

	[DrawGizmo(GizmoType.InSelectionHierarchy)]
	static void DrawGizmoForMyScript(Region region, GizmoType gizmoType) {
		region.OnPropertiesChanged();
		
		if(!region.polygon.IsValid()) return;
		if(region.selectedFillColor.a < 0.01f) return;
		var cachedMatrix = Gizmos.matrix;
		Gizmos.matrix = region.matrix;
		var cachedColor = Gizmos.color;
		Gizmos.color = region.selectedFillColor;
		if(region.in2DMode) {
			var mesh = CreatePolygonMesh(region.polygon.vertices, true);
			Gizmos.DrawMesh(mesh);
			for(int i = 0; i < region.polygon.vertices.Length; i++) Gizmos.DrawLine(region.polygon.vertices[i], region.polygon.vertices[(i+1) % region.polygon.vertices.Length]);
		} else {
			DrawExtrudedPolygon(region.polygon.vertices, region.height);
			DrawExtrudedWirePolygon(region.polygon.vertices, region.height);
		}
		Gizmos.color = cachedColor;
		Gizmos.matrix = cachedMatrix;
	}


	static Mesh CreatePolygonMesh (Vector2[] points, bool doubleSided = false) {
		var mesh = CreateMesh();

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
	}

	static void DrawPlane (Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, bool doubleSided = false) {
		var mesh = CreateMesh();
		
		Vector3[] verts = null;
		int[] tris = null;
		if (doubleSided) {
			verts = new Vector3[12] {
				topLeft,topRight,bottomLeft,
				topRight,bottomRight,bottomLeft,
				bottomLeft,topRight,topLeft,
				bottomLeft,bottomRight,topRight
			};
		} else {
			verts = new Vector3[6] {
				topLeft,topRight,bottomLeft,
				topRight,bottomRight,bottomLeft
			};
		};
		tris = new int[verts.Length];
		for(int t = 0; t < tris.Length; t++) tris[t] = t;

		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.RecalculateNormals();

		Gizmos.DrawMesh(mesh);
	}

	static void DrawExtrudedPolygon (Vector2[] points, float height) {
		if(points == null || points.Length == 0) return;
		Vector3 aLow, aHigh, bLow, bHigh = Vector3.zero;
		int i = 0;
		var heightOffset = Vector3.forward * height * 0.5f;
		var localPolyPos = (Vector3)points[i];
		aLow = -heightOffset + localPolyPos;
		aHigh = heightOffset + localPolyPos;
		for(i = 0; i <= points.Length; i++) {
			localPolyPos = (Vector3)points[i % points.Length];
			bLow = -heightOffset + localPolyPos;
			bHigh = heightOffset + localPolyPos;
			DrawPlane(aLow, bLow, aHigh, bHigh, true);
			aLow = bLow;
			aHigh = bHigh;
		}

		var mesh = CreatePolygonMesh(points, true);
		if(mesh.vertexCount > 0 && mesh.normals.Length > 0) {
			var cachedMatrix = Gizmos.matrix;
			Gizmos.matrix = cachedMatrix * Matrix4x4.TRS(-heightOffset, Quaternion.identity, Vector3.one);
			Gizmos.DrawMesh(mesh);

			Gizmos.matrix = cachedMatrix * Matrix4x4.TRS(heightOffset, Quaternion.identity, Vector3.one);
			Gizmos.DrawMesh(mesh);
			Gizmos.matrix = cachedMatrix;
		}
	}

	static void DrawExtrudedWirePolygon (Vector2[] points, float height) {
		if(points == null || points.Length == 0) return;
		Vector3 aLow, aHigh, bLow, bHigh = Vector3.zero;
		int i = 0;
		var heightOffset = Vector3.forward * height * 0.5f;
		var localPolyPos = (Vector3)points[i];
		aLow = -heightOffset + localPolyPos;
		aHigh = heightOffset + localPolyPos;
		for(i = 0; i <= points.Length; i++) {
			localPolyPos = (Vector3)points[i%points.Length];
			bLow = -heightOffset + localPolyPos;
			bHigh = heightOffset + localPolyPos;
			Gizmos.DrawLine(aLow, aHigh);
			Gizmos.DrawLine(aLow, bLow);
			Gizmos.DrawLine(aHigh, bHigh);
			aLow = bLow;
			aHigh = bHigh;
		}
	}

	static Mesh CreateMesh () {
		// Don't even try making meshes outside the editor, since we won't be able to clear them and we'll leak memory.
		#if UNITY_EDITOR
		var mesh = new Mesh();
		mesh.name = "Region Editor Temp Mesh";
		meshes.Add(mesh);
		return mesh;
		#else
		return null;
		#endif
	}

	static void OnSceneView (SceneView sceneView) {
		foreach(var mesh in meshes) {
			if(Application.isPlaying) UnityEngine.Object.Destroy (mesh);
			else UnityEngine.Object.DestroyImmediate (mesh);
		}
		meshes.Clear();
	}

	static List<Mesh> meshes = new List<Mesh>();
}