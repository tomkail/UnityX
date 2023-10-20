using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class GizmosX {
	// This is for mesh management. 
	// You destroying meshes during any OnDrawGizmos call crashes Unity, 
	// and reassinging the variable prevents the last mesh actually being drawn.
	// Instead we store a list of all the meshes used, and clear it when the scene is drawn.
	// Note that OnSceneGUI seems to run more frequently than meshes are added to the list, but this doesn't seem to matter.
	#if UNITY_EDITOR
	static GizmosX () {
		SceneView.duringSceneGui += OnSceneView;
	}

	static void OnSceneView (SceneView sceneView) {
		foreach(var mesh in meshes) DestroyAutomatic(mesh);
		meshes.Clear();
		
		static void DestroyAutomatic(Object o) {
			#if UNITY_EDITOR
			if(Application.isPlaying)
				UnityEngine.Object.Destroy (o);
			else
			UnityEngine.Object.DestroyImmediate (o);
			#else
			UnityEngine.Object.Destroy (o);
			#endif
		}
	}

	static List<Mesh> meshes = new List<Mesh>();
	#endif

	// static Mesh mesh;
	static Mesh CreateMesh () {
		// Don't even try making meshes outside the editor, since we won't be able to clear them and we'll leak memory.
		#if UNITY_EDITOR
		// if(mesh != null) DestroyAutomatic(mesh);
		var mesh = new Mesh();
		mesh.name = "Gizmos Temp Mesh";
		meshes.Add(mesh);
		return mesh;
		#else
		return null;
		#endif
	}

	static Stack<Color> colors = new Stack<Color>();

	public static void BeginColor (Color color) {
		colors.Push(Gizmos.color);
		Gizmos.color = color;
	}

	public static void EndColor () {
		Gizmos.color = colors.Pop();	
	}

	static Stack<Matrix4x4> matricies = new Stack<Matrix4x4>();
	public static void BeginMatrix (Matrix4x4 matrix) {
		matricies.Push(Gizmos.matrix);
		Gizmos.matrix = matrix;
	}

	public static void EndMatrix () {
		Gizmos.matrix = matricies.Pop();	
	}

	public static void DrawLine (IList<Vector3> positions) {
		for(int i = 0; i < positions.Count-1; i++) Gizmos.DrawLine(positions[i], positions[i+1]);
	}

	public static void DrawWirePolygon (Vector3 position, Quaternion rotation, Vector3 scale, IList<Vector2> points) {
		BeginMatrix(Matrix4x4.TRS(position, rotation, scale));
		DrawWirePolygon(points);
		EndMatrix();
	}

	public static void DrawWirePolygonWithArrows (Vector3 position, Quaternion rotation, Vector3 scale, IList<Vector2> points, Vector3 crossVector) {
		BeginMatrix(Matrix4x4.TRS(position, rotation, scale));
		for(int i = 0; i < points.Count; i++) {
			DrawArrowLine(points.GetRepeating(i), points.GetRepeating(i+1), crossVector);
		}
		EndMatrix();
	}

    

	public static Mesh CreatePolygonMesh (Vector2[] points, bool doubleSided = false) {
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

	public static void DrawPolygon (Vector2[] points, bool doubleSided = false) {
		var mesh = CreatePolygonMesh(points, doubleSided);
		if(mesh.vertexCount > 0 && mesh.normals.Length > 0)
			Gizmos.DrawMesh(mesh);
	}
	public static void DrawPolygon (Vector3 position, Quaternion rotation, Vector3 scale, Vector2[] points, bool doubleSided = false) {
		BeginMatrix(Matrix4x4.TRS(position, rotation, scale));
		DrawPolygon(points, doubleSided);
		EndMatrix();
	}

	public static void DrawWirePolygon (Vector3 position, Quaternion rotation, IList<Vector2> points) {
		for(int i = 0; i < points.Count; i++) {
			Gizmos.DrawLine(position + rotation * points[i], position + rotation * points[(i+1)%points.Count]);
		}
	}

	public static void DrawWirePolygon (IList<Vector2> points) {
		for(int i = 0; i < points.Count; i++) {
			Gizmos.DrawLine(points[i], points[(i+1)%points.Count]);
		}
	}
	
	public static void DrawWirePolygon (IList<Vector3> points) {
		for(int i = 0; i < points.Count; i++) {
			Gizmos.DrawLine(points[i], points[(i+1)%points.Count]);
		}
	}

	public static void DrawExtrudedPolygon (Vector2[] points, float depth) {
		if(points == null || points.Length == 0) return;
		Vector3 aLow, aHigh, bLow, bHigh = Vector3.zero;
		int i = 0;
		var depthOffset = Vector3.forward * depth * 0.5f;
		var localPolyPos = (Vector3)points[i];
		aLow = -depthOffset + localPolyPos;
		aHigh = depthOffset + localPolyPos;
		for(i = 0; i <= points.Length; i++) {
			localPolyPos = points[i % points.Length];
			bLow = -depthOffset + localPolyPos;
			bHigh = depthOffset + localPolyPos;
			DrawPlane(aLow, bLow, aHigh, bHigh, true);
			aLow = bLow;
			aHigh = bHigh;
		}

		var mesh = CreatePolygonMesh(points, true);
		if(mesh.vertexCount > 0 && mesh.normals.Length > 0) {
			var cachedMatrix = Gizmos.matrix;
			Gizmos.matrix = Gizmos.matrix * Matrix4x4.TRS(-depthOffset, Quaternion.identity, Vector3.one);
			Gizmos.DrawMesh(mesh);

			Gizmos.matrix = Gizmos.matrix * Matrix4x4.TRS(depthOffset, Quaternion.identity, Vector3.one);
			Gizmos.DrawMesh(mesh);
			Gizmos.matrix = cachedMatrix;
		}
	}
	
	public static void DrawExtrudedPolygon (Vector3 position, Quaternion rotation, Vector3 scale, Vector2[] points, float depth) {
		if(points == null || points.Length == 0) return;
		BeginMatrix(Matrix4x4.TRS(position, rotation, scale));
		DrawExtrudedPolygon(points, depth);
		EndMatrix();
	}

	static void DrawExtrudedWirePolygon (Vector2[] points, float depth) {
		if(points == null || points.Length == 0) return;
		Vector3 aLow, aHigh, bLow, bHigh = Vector3.zero;
		int i = 0;
		var depthOffset = Vector3.forward * depth * 0.5f;
		var localPolyPos = (Vector3)points[i];
		aLow = -depthOffset + localPolyPos;
		aHigh = depthOffset + localPolyPos;
		for(i = 0; i <= points.Length; i++) {
			localPolyPos = points[i%points.Length];
			bLow = -depthOffset + localPolyPos;
			bHigh = depthOffset + localPolyPos;
			Gizmos.DrawLine(aLow, aHigh);
			Gizmos.DrawLine(aLow, bLow);
			Gizmos.DrawLine(aHigh, bHigh);
			aLow = bLow;
			aHigh = bHigh;
		}
	}
	public static void DrawExtrudedWirePolygon (Vector3 position, Quaternion rotation, Vector3 scale, Vector2[] points, float depth) {
		if(points == null || points.Length == 0) return;
		BeginMatrix(Matrix4x4.TRS(position, rotation, scale));
		DrawExtrudedWirePolygon(points, depth);
		EndMatrix();
	}
	
	public static void DrawWireCircle (Vector3 position, Quaternion rotation, float radius, int numLines = 32) {
		Debug.Assert(numLines > 2);
		Vector3 a, b = Vector3.zero;
		int i = 0;
		var r = (1f/numLines) * 2 * Mathf.PI;
		float radians = i * r;
		var localCirclePos = new Vector3(Mathf.Sin(radians) * radius, Mathf.Cos(radians) * radius);
		a = position + rotation * localCirclePos;
		for(i = 0; i < numLines; i++) {
			radians = (i + 1) * r;
			localCirclePos = new Vector3(Mathf.Sin(radians) * radius, Mathf.Cos(radians) * radius);
			b = position + rotation * localCirclePos;
			Gizmos.DrawLine(a, b);
			a = b;
		}
	}

    public static void DrawWireCylinder (Vector3 position, Quaternion rotation, float radius, float depth, int numColumns = 32) {
		BeginMatrix(Matrix4x4.TRS(position, rotation, Vector3.one));
        Debug.Assert(numColumns > 2);
		Vector3 aLow, aHigh, bLow, bHigh = Vector3.zero;
		int i = 0;
        var r = (1f/numColumns) * 2 * Mathf.PI;
		float radians = i * r;
        var up = new Vector3(0,0, depth);
        var halfUp = new Vector3(0,0, depth*0.5f);
		var localCirclePos = new Vector3(Mathf.Sin(radians) * radius, Mathf.Cos(radians) * radius);
		aLow = localCirclePos - halfUp;
		aHigh = aLow + up;
		for(i = 0; i < numColumns; i++) {
			radians = (i + 1) * r;
			localCirclePos = new Vector3(Mathf.Sin(radians) * radius, Mathf.Cos(radians) * radius);
			bLow = localCirclePos - halfUp;
			bHigh = bLow + up;
			Gizmos.DrawLine(aLow, aHigh);
			Gizmos.DrawLine(aLow, bLow);
			Gizmos.DrawLine(aHigh, bHigh);
			aLow = bLow;
			aHigh = bHigh;
		}
		EndMatrix();
	}

	public static void DrawCylinder (Vector3 position, Quaternion rotation, float radius, float depth, int numColumns = 32) {
		Debug.Assert(numColumns > 2);
		Vector3 aLow, aHigh, bLow, bHigh = Vector3.zero;
		int i = 0;
		var r = (1f/numColumns) * 2 * Mathf.PI;
		float radians = i * r;
		var localCirclePos = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
		aLow = position + rotation * localCirclePos - (rotation * Vector3.up * depth * 0.5f);
		aHigh = aLow + rotation * Vector3.up * depth;
		for(i = 0; i < numColumns; i++) {
			radians = (i + 1) * r;
			localCirclePos = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
			bLow = position + rotation * localCirclePos - (rotation * Vector3.up * depth * 0.5f);
			bHigh = bLow + rotation * Vector3.up * depth;
			DrawPlane(aHigh, bHigh, aLow, bLow);
			aLow = bLow;
			aHigh = bHigh;
		}
	}

	public static void DrawWireArc (Vector3 position, Quaternion rotation, float radius, float startAngle, float endAngle, int numLines = 32) {
		Debug.Assert(numLines > 1);
		if(startAngle == endAngle) return;
		float deltaAngle = Mathf.DeltaAngle(startAngle, endAngle);
		if(deltaAngle == 0) DrawWireCircle(position, rotation, radius, numLines);

		var startRadians = startAngle * Mathf.Deg2Rad;
		var endRadians = endAngle * Mathf.Deg2Rad;
		Vector3 a, b = Vector3.zero;
		float r = 1f/(numLines-1);

		float radians = startRadians;
		var localCirclePos = new Vector3(Mathf.Sin(radians) * radius, Mathf.Cos(radians) * radius);
		a = position + rotation * localCirclePos;
		for(int i = 0; i < numLines; i++) {
			radians = Mathf.Lerp(startRadians, endRadians, i * r);
			localCirclePos = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
			
			b = position + rotation * localCirclePos;
			Gizmos.DrawLine(a, b);
			a = b;
		}
	}
	
	public static void DrawWireArcSegment (Vector3 position, Quaternion rotation, float radius, float startAngle, float endAngle, int numLines = 32) {
		Debug.Assert(numLines > 1);
		if(startAngle == endAngle) return;
		float deltaAngle = Mathf.DeltaAngle(startAngle, endAngle);
		if(deltaAngle == 0) DrawWireCircle(position, rotation, radius, numLines);

		var startRadians = startAngle * Mathf.Deg2Rad;
		var endRadians = endAngle * Mathf.Deg2Rad;
		Vector3 a, b = Vector3.zero;
		float r = 1f/(numLines-1);

		float radians = startRadians;
		var localCirclePos = new Vector3(Mathf.Sin(radians) * radius, Mathf.Cos(radians) * radius);
		a = position + rotation * localCirclePos;
		if(deltaAngle < 180) Gizmos.DrawLine(position, a);
		for(int i = 0; i < numLines; i++) {
			radians = Mathf.Lerp(startRadians, endRadians, i * r);
			localCirclePos = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
			
			b = position + rotation * localCirclePos;
			Gizmos.DrawLine(a, b);
			a = b;
		}
		if(deltaAngle < 180) Gizmos.DrawLine(a, position);
	}
	
	public static void DrawWireCube (Bounds _bounds) {
		Gizmos.DrawWireCube(_bounds.center, _bounds.size);
	}

	public static void DrawWireCube (Bounds _bounds, Quaternion rotation) {
		DrawWireCube(_bounds.center, rotation, _bounds.size);
	}
	
	
	public static void DrawCube (Bounds _bounds) {
		Gizmos.DrawCube(_bounds.center, _bounds.size);
	}

	public static void DrawCube (Bounds _bounds, Quaternion rotation) {
		DrawCube(_bounds.center, rotation, _bounds.size);
	}

	public static void DrawWireCube (Vector3 position, Quaternion rotation, Vector3 scale) {
		BeginMatrix(Matrix4x4.TRS(position, rotation, scale));
		Gizmos.DrawWireCube(new Vector3(0,0,0), Vector3.one);
		EndMatrix();
	}

	public static void DrawCube (Vector3 position, Quaternion rotation, Vector3 scale) {
		BeginMatrix(Matrix4x4.TRS(position, rotation, scale));
		Gizmos.DrawCube(new Vector3(0,0,0), Vector3.one);
		EndMatrix();
	}
	
	public static void DrawWireCube (Rect _rect, float _distance) {
		Gizmos.DrawWireCube(new Vector3(_rect.x + _rect.width * 0.5f, _rect.y + _rect.height * 0.5f, _distance), new Vector3(_rect.width, _rect.height, 0));
	}
	
	public static void DrawCube (Rect _rect, float _distance) {
		Gizmos.DrawCube(new Vector3(_rect.x + _rect.width * 0.5f, _rect.y + _rect.height * 0.5f, _distance), new Vector3(_rect.width, _rect.height, 0));
	}
	
	static Vector3[] rectPoints;
	public static void DrawWireRect (Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) {
		if(rectPoints == null) rectPoints = new Vector3[4];
		rectPoints[0] = topLeft;
		rectPoints[1] = topRight;
		rectPoints[2] = bottomRight;
		rectPoints[3] = bottomLeft;
		DrawWirePolygon(rectPoints);
	}

	public static void DrawWireRect (Rect _rect) {
		DrawWireRect(_rect.TopLeft(), _rect.TopRight(), _rect.BottomRight(), _rect.BottomLeft());
	}

	public static void DrawWireRect (Rect _rect, Quaternion rotation) {
		DrawWireRect(_rect.center, rotation, _rect.size);
	}
	
	public static void DrawWireRect (Vector3 origin, Quaternion rotation, Vector2 scale) {
		scale *= 0.5f;
		Vector3 topLeft = origin + rotation * new Vector3(-scale.x, scale.y, 0);
		Vector3 topRight = origin + rotation * new Vector3(scale.x, scale.y, 0);
		Vector3 bottomRight = origin + rotation * new Vector3(scale.x, -scale.y, 0);
		Vector3 bottomLeft = origin + rotation * new Vector3(-scale.x, -scale.y, 0);
		DrawWireRect(topLeft, topRight, bottomRight, bottomLeft);
	}
	

	public static void DrawPlane (Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, bool doubleSided = false) {
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

	public static void DrawRect (Rect _rect, bool doubleSided = false) {
		DrawPlane(_rect.TopLeft(), _rect.TopRight(), _rect.BottomLeft(), _rect.BottomRight(), doubleSided);
	}

	public static void DrawRect (Vector3 origin, Quaternion rotation, Vector2 scale) {
		scale *= 0.5f;
		Vector3 topLeft = origin + rotation * new Vector3(-scale.x, scale.y, 0);
		Vector3 topRight = origin + rotation * new Vector3(scale.x, scale.y, 0);
		Vector3 bottomLeft = origin + rotation * new Vector3(-scale.x, -scale.y, 0);
		Vector3 bottomRight = origin + rotation * new Vector3(scale.x, -scale.y, 0);
		DrawPlane(topLeft, topRight, bottomLeft, bottomRight);
	}

	public static void DrawArrowLine (IList<Vector3> positions, float normalizedArrowDistance = 0.75f, float arrowSizeMultiplier = 1f) {
		for(int i = 0; i < positions.Count-1; i++) DrawArrowLine(positions[i], positions[i+1], normalizedArrowDistance, arrowSizeMultiplier);
	}

	public static void DrawArrowLine (Vector3 fromPosition, Vector3 toPosition, float normalizedArrowDistance = 0.75f, float arrowSizeMultiplier = 1f) {
		DrawArrowLine(fromPosition, toPosition, Vector3.up, normalizedArrowDistance, arrowSizeMultiplier);
	}
	public static void DrawArrowLine (Vector3 fromPosition, Vector3 toPosition, Vector3 crossVector, float normalizedArrowDistance = 0.75f, float arrowSizeMultiplier = 1f) {
		if(fromPosition == toPosition) return;
		Gizmos.DrawLine(fromPosition, toPosition);
		var fromTo = toPosition-fromPosition;
		DrawArrow(fromPosition + fromTo * normalizedArrowDistance, Quaternion.LookRotation(fromTo, crossVector), fromTo.magnitude * arrowSizeMultiplier * 0.05f);
	}

	public static void DrawArrow (Vector3 position, Quaternion rotation, float arrowSize) {
		Vector3 start = position + (rotation * Vector3.back * arrowSize);
		Vector3 end = position + (rotation * Vector3.forward * arrowSize);
		Gizmos.DrawLine(start+(rotation * Vector3.left * arrowSize), end);
		Gizmos.DrawLine(start+(rotation * Vector3.right * arrowSize), end);
	}


	public static void DrawFrustum (Vector3 position, Quaternion rotation, float fieldOfView, float aspect, float nearClipPlane, float farClipPlane) {
		if(!rotation.IsValid()) return;
		BeginMatrix(Matrix4x4.TRS(position, rotation, Vector3.one));
		Gizmos.DrawFrustum(Vector3.zero, fieldOfView, farClipPlane, nearClipPlane, aspect);
		EndMatrix();
	}
	
	public static void DrawOrthographicFrustum (Vector3 position, Quaternion rotation, float orthographicSize, float aspect, float nearClipPlane, float farClipPlane) {
		BeginMatrix(Matrix4x4.TRS(position, rotation, Vector3.one));
		float spread = farClipPlane - nearClipPlane;
		float center = (farClipPlane + nearClipPlane)*0.5f;
		Gizmos.DrawWireCube(new Vector3(0,0,center), new Vector3(orthographicSize*2*aspect, orthographicSize*2, spread));
		EndMatrix();
	}

	public static void DrawQuaternionAxis (Vector3 position, Quaternion rotation, float size = 1) {
		BeginColor(Color.red);
		DrawArrowLine(position, position + rotation * Vector3.right * size);
		EndColor();
		BeginColor(Color.green);
		DrawArrowLine(position, position + rotation * Vector3.up * size);
		EndColor();
		BeginColor(Color.blue);
		DrawArrowLine(position, position + rotation * Vector3.forward * size);
		EndColor();
	}

	#if UNITY_EDITOR
	static GUIStyle _helpBoxStyle;
	static GUIStyle helpBoxStyle {
		get {
			if(_helpBoxStyle == null) {
				_helpBoxStyle = new GUIStyle(GUI.skin.GetStyle("HelpBox"));
			}
			return _helpBoxStyle;
		}
	}
	public static void DrawMessageBox (Vector3 worldPos, string text, MessageType messageType = MessageType.None) {
		Handles.BeginGUI();
		var guiPos = HandleUtility.WorldToGUIPointWithDepth(worldPos);
		if(guiPos.z > 0) {
			Vector2 size = helpBoxStyle.CalcSize(new GUIContent(text)) * 1.25f;
			// Do it a few times to make it bolder. Yeah, not optimal.
			EditorGUI.HelpBox(CreateFromCenter(guiPos.x, guiPos.y, size.x, size.y), text, messageType);
			EditorGUI.HelpBox(CreateFromCenter(guiPos.x, guiPos.y, size.x, size.y), text, messageType);
			EditorGUI.HelpBox(CreateFromCenter(guiPos.x, guiPos.y, size.x, size.y), text, messageType);
		}
		Handles.EndGUI();
		
		static Rect CreateFromCenter (float centerX, float centerY, float sizeX, float sizeY) {
			return new Rect(centerX - sizeX * 0.5f, centerY - sizeY * 0.5f, sizeX, sizeY);
		}
	}
	#endif
}
