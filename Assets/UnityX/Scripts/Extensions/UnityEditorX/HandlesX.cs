using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;


public static class HandlesX {

	#if UNITY_EDITOR
	static HandlesX () {
		SceneView.duringSceneGui += OnSceneView;
	}

	static void OnSceneView (SceneView sceneView) {
		foreach(var mesh in meshes) ObjectX.DestroyAutomatic(mesh);
		meshes.Clear();
	}

	static List<Mesh> meshes = new List<Mesh>();
	#endif

	// static Mesh mesh;
	static Mesh CreateMesh () {
		// Don't even try making meshes outside the editor, since we won't be able to clear them and we'll leak memory.
		#if UNITY_EDITOR
		// if(mesh != null) ObjectX.DestroyAutomatic(mesh);
		var mesh = new Mesh();
		mesh.name = "Handles Temp Mesh";
		meshes.Add(mesh);
		return mesh;
		#else
		return null;
		#endif
	}

	static Stack<Color> colors = new Stack<Color>();
	public static void BeginColor (Color color) {
		colors.Push(Handles.color);
		Handles.color = color;
	}

	public static void EndColor () {
		Handles.color = colors.Pop();	
	}

	static Stack<Matrix4x4> matricies = new Stack<Matrix4x4>();
	public static void BeginMatrix (Matrix4x4 matrix) {
		matricies.Push(GUI.matrix);
		Handles.matrix = matrix;
	}

	public static void EndMatrix () {
		Handles.matrix = matricies.Pop();	
	}

	public static Vector3 ScaledPostionHandle (Vector3 position, Quaternion rotation, float scale) {
		var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
		HandlesX.BeginMatrix(matrix);
		var newPosition = Handles.PositionHandle(position, rotation);
		HandlesX.EndMatrix();
		return matrix.inverse.MultiplyPoint3x4(newPosition);
	}

	static float angleStart;
	static float lastAngle;

	public static float DrawWheelHandle (Vector3 position, float angle, float handleSize) {
		int id = GUIUtility.GetControlID(FocusType.Passive);
		Vector3 position2 = Handles.matrix.MultiplyPoint(position);
//		Matrix4x4 matrix = Handles.matrix;
		Event current = Event.current;

		Vector2 screenPosition = Camera.current.WorldToScreenPoint(position);

		float newAngle = angle;
		float deltaAngle = 0;
//		Debug.Log(angleStart+" "+angle+" "+lastAngle);

		switch (current.GetTypeForControl(id)) {
			case EventType.Repaint:
			Handles.color = Color.white.WithAlpha(0.5f);

//			Handles.matrix = Matrix4x4.identity;
			Handles.DrawWireDisc(position, Camera.current.transform.forward, handleSize);
//			Handles.matrix = matrix;

			if (GUIUtility.hotControl == id) {
				Handles.color = Color.white.WithAlpha(0.1f);
				Vector3 startDirection = Quaternion.AngleAxis(-angle + angleStart, Camera.current.transform.forward) * Vector3.up;
				Vector3 endDirection = Quaternion.AngleAxis(-angle + angleStart + (lastAngle-angleStart), Camera.current.transform.forward) * Vector3.up;
				float drawAngle = -Mathf.DeltaAngle(lastAngle, angleStart);
//				float drawAngle = lastAngle-angleStart;
//				Debug.Log(drawAngle+" "+lastAngle+" "+angleStart);
				Handles.DrawSolidArc(position, Camera.current.transform.forward, startDirection, drawAngle, handleSize);
				Handles.color = Color.white.WithAlpha(0.5f);
				Handles.DrawLine(position, position + startDirection * handleSize);
				Handles.DrawLine(position, position + endDirection * handleSize);
			}

	        break;

		    case EventType.Layout:
//			Handles.matrix = Matrix4x4.identity;
			HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(position2, handleSize));
//			Handles.matrix = matrix;
	        break;

			case EventType.MouseDown:
			if ((HandleUtility.nearestControl == id && current.button == 0) || (GUIUtility.keyboardControl == id && current.button == 2))
		    {

				GUIUtility.keyboardControl = id;
				GUIUtility.hotControl = id;
				current.Use();
				EditorGUIUtility.SetWantsMouseJumping(1);

				float inputAngle = MathX.WrapDegrees(Vector2X.DegreesBetween(screenPosition, current.mousePosition));
				angleStart = lastAngle = inputAngle;
		    }
		    break;
			    
			case EventType.MouseUp:
			if (GUIUtility.hotControl == id && (current.button == 0 || current.button == 2))
			{
				angleStart = lastAngle = 0;

				GUIUtility.hotControl = 0;
				current.Use();
				EditorGUIUtility.SetWantsMouseJumping(0);
			}
		    break;
			    
			case EventType.MouseDrag:
		    if (GUIUtility.hotControl == id)
		    {
				float inputAngle = MathX.WrapDegrees(Vector2X.DegreesBetween(screenPosition, current.mousePosition));
				deltaAngle = Mathf.DeltaAngle(lastAngle,inputAngle);
				newAngle += deltaAngle;
				lastAngle = inputAngle;

				GUI.changed = true;
				current.Use();
		    }
		    break;
		}
		return newAngle;
	}







	public static Quaternion AxialRotationHandle(Quaternion rotation, Vector3 position) {
		float handleSize = HandleUtility.GetHandleSize(position);
		Color color = Handles.color;
		rotation = Handles.Disc(rotation, position, rotation * Vector3.up, handleSize, true, 15);
		Handles.color = color;
		return rotation;
	}

	public static void DrawWirePolygon (Vector3 position, Quaternion rotation, IList<Vector2> points) {
		for(int i = 0; i < points.Count; i++) {
			Handles.DrawLine(position + rotation * points.GetRepeating(i), position + rotation * points.GetRepeating(i+1));
		}
	}
	
	public static void DrawWirePolygon (IList<Vector2> points) {
		for(int i = 0; i < points.Count; i++) {
			Handles.DrawLine(points.GetRepeating(i), points.GetRepeating(i+1));
		}
	}
	
	public static void DrawWirePolygon (IList<Vector3> points) {
		for(int i = 0; i < points.Count; i++) {
			Handles.DrawLine(points.GetRepeating(i), points.GetRepeating(i+1));
		}
	}

	public static void DrawWirePolygon (Vector3 position, Quaternion rotation, Vector3 scale, IList<Vector2> points) {
		HandlesX.BeginMatrix(Matrix4x4.TRS(position, rotation, scale));
		for(int i = 0; i < points.Count; i++) {
			Handles.DrawLine(points.GetRepeating(i), points.GetRepeating(i+1));
		}
		HandlesX.EndMatrix();
	}

	public static void DrawWireRect (Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight) {
		DrawWirePolygon(new Vector3[]{topLeft, topRight, bottomRight, bottomLeft, topLeft});
	}

	public static void DrawWireRect (Rect _rect) {
		DrawWireRect(_rect.TopLeft(), _rect.TopRight(), _rect.BottomLeft(), _rect.BottomRight());
	}

	public static void DrawWireRect (Rect _rect, Quaternion rotation) {
		DrawWireRect(_rect.center, rotation, _rect.size);
	}
	
	public static void DrawWireRect (Vector3 origin, Quaternion rotation, Vector2 scale) {
		scale *= 0.5f;
		Vector3 topLeft = origin + rotation * new Vector3(-scale.x, scale.y, 0);
		Vector3 topRight = origin + rotation * new Vector3(scale.x, scale.y, 0);
		Vector3 bottomLeft = origin + rotation * new Vector3(-scale.x, -scale.y, 0);
		Vector3 bottomRight = origin + rotation * new Vector3(scale.x, -scale.y, 0);
		DrawWireRect(topLeft, topRight, bottomLeft, bottomRight);
	}



}
#endif