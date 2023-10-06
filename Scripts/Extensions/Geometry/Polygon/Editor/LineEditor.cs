using UnityEngine;
using UnityEditor;
using System;
using UnityX.Geometry;
/*
public class LineEditor {
	bool _editing;
	public bool editing {
		get {
			return _editing;
		} set {
			_editing = value;
			if(_editing) Tools.hidden = true;
			else Tools.hidden = false;
		}
	}
	public Matrix4x4 offsetMatrix = Matrix4x4.identity;
	// When cmd/alt is down, this snaps to the nearest interval
	public float snapInterval = 1;
	Transform transform;
	Matrix4x4 matrix {
		get {
			return Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale) * offsetMatrix;
		}
	}
	Matrix4x4 directionMatrix {
		get {
			return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one) * offsetMatrix;
		}
	}

	/// <summary>
	/// Allows editing lines in world space via the scene view.
	/// </summary>
	/// <param name="transform">Transform.</param>
	public LineEditor (Transform transform) {
		this.transform = transform;
	}
	public LineEditor (Transform transform, Quaternion offsetRotation) : this(transform, Matrix4x4.TRS(Vector3.zero, offsetRotation, Vector3.one)) {}
	public LineEditor (Transform transform, bool onXZPlane) : this(transform, onXZPlane ? Quaternion.Euler(new Vector3(90,0,0)) : Quaternion.identity) {}

	public Func<Vector2[]> defaultLineFunc;

	/// <summary>
	/// Allows editing lines in world space via the scene view. Offset matrix allows the editing plane to be flipped to the XZ plane.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="offsetMatrix">Offset matrix.</param>
	public LineEditor (Transform transform, Matrix4x4 offsetMatrix) {
		this.transform = transform;
		this.offsetMatrix = offsetMatrix;
	}

	bool moveMode {
		get {
			return editing && Event.current.shift;
		}
	}
	bool deletionMode {
		get {
			#if UNITY_EDITOR_WIN 
			return editing && Event.current.control;
			#else
			return editing && Event.current.control;
			#endif
		}
	}
	bool snapToPoint {
		get {
			#if UNITY_EDITOR_WIN 
			// return editing && Event.current.alt;
			#else
			return editing && (Event.current.command);
			#endif
		}
	}

	bool isEditingPoint {
		get {
			return editingPointIndex != -1;
		}
	}
	int editingPointIndex = -1;

	bool isMoving;
	Vector2 moveLastPosition;

	const float handleSize = 0.04f;
	const float pointSnapDistance = 20;
	const float highlightedLineWidth = 3;

	static readonly Color lineColor = new Color(0.8f, 1, 0.8f);
	static readonly Color moveModeLineColor = new Color(0.8f, 0.8f, 1);
	static readonly Color movingLineColor = new Color(0, 0, 1);

	static readonly Color highlightedLineColor = Color.green;
	static readonly Color highlightedDeletionModeLineColor = Color.red;


	Plane plane {
		get {
			return new Plane(directionMatrix.MultiplyVector(Vector3.forward), matrix.MultiplyPoint3x4(Vector3.zero));
		}
	}

	void StartMoving (Vector2 currentPosition) {
		isMoving = true;
		moveLastPosition = currentPosition;
    }

	void StopMoving () {
		isMoving = false;
    }

	void StartEditingPoint (int pointIndex) {
		editingPointIndex = pointIndex;
    }

	void StopEditingPoint () {
		editingPointIndex = -1;
    }

    public void OnInspectorGUI () {
		editing = GUILayout.Toggle(editing, "Editing");
    }

    public bool OnSceneGUI (Line line) {
		if(Event.current.isMouse) SceneView.RepaintAll();
    	bool changed = false;
		Validate(line, ref changed);
		Color savedHandleColor = Handles.color;
		DrawLine(line);
		DrawSceneViewToolbar(line, ref changed);
		if(Event.current.alt) StopEditingPoint();
		else if(editing) {
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			DrawEditor(line, ref changed);
		}
		Handles.color = savedHandleColor;
		return changed;
    }

	static Line clipboardLine;
	void DrawSceneViewToolbar (Line line, ref bool changed) {
		Handles.BeginGUI();
		Vector3 midPointScreenPoint = SceneView.currentDrawingSceneView.WorldToGUI(matrix.MultiplyPoint3x4(line.center));
		GUILayout.BeginArea(RectX.Create(midPointScreenPoint, new Vector2(140, 40), new Vector2(0.5f, 0.5f)));
		GUILayout.BeginHorizontal();

		if (GUILayout.Button(editing ? "Finish" : "Edit")) {
			editing = !editing;
		}
		
		if(GUILayout.Button("Copy")) {
			var serialized = JsonUtility.ToJson(line);
			GUIUtility.systemCopyBuffer = serialized;
		}

		if(GUIUtility.systemCopyBuffer.Length > 0) {
			try {
				if(clipboardLine == null) clipboardLine = JsonUtility.FromJson<Line>(GUIUtility.systemCopyBuffer);
				else JsonUtility.FromJsonOverwrite(GUIUtility.systemCopyBuffer, clipboardLine);
			} catch {
				clipboardLine = null;	
			}
		} else {
			clipboardLine = null;
		}
		EditorGUI.BeginDisabledGroup(clipboardLine == null);
		if(GUIUtility.systemCopyBuffer.Length > 0 && GUILayout.Button("Paste") ) {
			if(clipboardLine != null) line.CopyFrom((Line)clipboardLine);
			changed = true;
		}
		EditorGUI.EndDisabledGroup();

		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		Handles.EndGUI();
	}

	void Validate (Line line, ref bool changed) {
		if(line.vertices == null || line.vertices.Length < 3) {
			if( defaultLineFunc != null ) {
				line.vertices = defaultLineFunc();
			} else {
				line.vertices = RectX.CreateFromCenter(Vector2.one, Vector2.one).GetVertices();
			}

			changed = true;
		}
    }

	void DrawLine (Line line) {
		if(moveMode) {
			if(isMoving) Handles.color = movingLineColor;
			else Handles.color = moveModeLineColor;
		} else Handles.color = lineColor;
		Vector3[] worldPoints = new Vector3[line.vertices.Length+1];
		for (int i = 0; i < line.vertices.Length; i++) {
			worldPoints[i] = LineToWorldSpace(line.vertices [i]);
		}
		worldPoints[worldPoints.Length-1] = worldPoints[0];
		Handles.DrawPolyLine(worldPoints);
    }

	void DrawEditor (Line line, ref bool changed) {
		bool mouseDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;
		bool mouseUp = Event.current.button == 0 && Event.current.type == EventType.MouseUp;

		Vector2 mousePosition = SceneView.currentDrawingSceneView.GetMousePosition();

		Vector2[] screenSpaceLineVerts = new Vector2[line.vertices.Length];
		for (int i = 0; i < screenSpaceLineVerts.Length; i++) {
			Vector3 worldSpace = LineToWorldSpace(line.vertices[i]);
			screenSpaceLineVerts [i] = WorldToScreenSpace(worldSpace);
		}
		Line screenSpaceLine = new Line(screenSpaceLineVerts);

		var closestVertexIndex = screenSpaceLine.FindClosestVertexIndex(mousePosition);

		var closestScreenVertexPoint = screenSpaceLine.vertices[closestVertexIndex];

		var bestPointOnScreenLine = screenSpaceLine.FindClosestPointOnLine(mousePosition);

		
		var screenDistanceFromClosestPoint = Vector2.Distance(closestScreenVertexPoint, bestPointOnScreenLine);
		var worldMousePointOnPlane = Vector3.zero;
		GetScreenPointIntersectingRegionPlane(mousePosition, ref worldMousePointOnPlane);

		bool vertexEditMode = !moveMode && (screenDistanceFromClosestPoint < pointSnapDistance || deletionMode);
		if(vertexEditMode) {
			int[] indices = new int[3] {
				line.vertices.GetRepeatingIndex(closestVertexIndex-1),
				closestVertexIndex, 
				line.vertices.GetRepeatingIndex(closestVertexIndex+1)
			};

			if(!isEditingPoint) {
				Vector3[] closestVertices = new Vector3[3];
				for(int i = 0; i < closestVertices.Length; i++) closestVertices[i] = LineToWorldSpace(line.vertices[indices[i]]);

				if(deletionMode) Handles.color = highlightedDeletionModeLineColor;
				else Handles.color = highlightedLineColor;

				Handles.DrawAAPolyLine(highlightedLineWidth, closestVertices);
				Handles.DotHandleCap(0, closestVertices[1], Quaternion.identity, HandleUtility.GetHandleSize(closestVertices[1]) * handleSize, Event.current.type);
			}
			
			if(mouseDown) {
				if(deletionMode) {
					bool canRemove = line.vertices.Length > 3;
					if(canRemove) {
						RemovePoint(ref line, closestVertexIndex);
						changed = true;
					}
				}
				else StartEditingPoint(closestVertexIndex);
			}
		} else {
			if(moveMode) {
				if(mouseDown) 
					StartMoving(WorldToLineSpace(worldMousePointOnPlane));
			} else if(!isEditingPoint) {
				int leftIndex = 0;
				int rightIndex = 0;
				screenSpaceLine.FindClosestEdgeIndices(bestPointOnScreenLine, ref leftIndex, ref rightIndex);

				Handles.color = highlightedLineColor;

				var worldLeftPoint = LineToWorldSpace(line.vertices[leftIndex]);
				var worldRightPoint = LineToWorldSpace(line.vertices[rightIndex]);
				Handles.DrawAAPolyLine(highlightedLineWidth, worldLeftPoint, worldRightPoint);

				var closestScreenPoint = screenSpaceLine.FindClosestPointOnLine(bestPointOnScreenLine);
				var worldBestPointOnLine = Vector3.zero;
				GetScreenPointIntersectingRegionPlane(closestScreenPoint, ref worldBestPointOnLine);
				if(snapToPoint) worldBestPointOnLine = SnapToWorldInterval(worldBestPointOnLine);
				Handles.DotHandleCap(0, worldBestPointOnLine, Quaternion.identity, HandleUtility.GetHandleSize(worldBestPointOnLine) * handleSize, Event.current.type);

				if(mouseDown) {
					InsertPoint(ref line, rightIndex, worldMousePointOnPlane);
					StartEditingPoint(rightIndex);
					changed = true;
				}
			}
		}

		if(isMoving) {
			var moveNewPosition = WorldToLineSpace(worldMousePointOnPlane);
			var moveDeltaPosition = moveNewPosition - moveLastPosition;
			// This doesn't work, and isnt the right approach. To do, if i can be arsed
			// Vector2 snapOffset = Vector2.zero;
			// if(snapToPoint) {
			// 	snapOffset = SnapToLineInterval(line.vertices[0]+moveDeltaPosition)-line.vertices[0]+moveDeltaPosition;
			// 	moveDeltaPosition += snapOffset;
			// }
			if(moveDeltaPosition != Vector2.zero) {
				line.Move(moveDeltaPosition);
		
				moveLastPosition = moveNewPosition;
				changed = true;
			}
			if(mouseUp || !moveMode) StopMoving();
		}

		if(isEditingPoint) {
			Vector2 newPolyPoint = WorldToLineSpace(worldMousePointOnPlane);
			if(snapToPoint) {
				newPolyPoint = SnapToLineInterval(newPolyPoint);
				worldMousePointOnPlane = LineToWorldSpace(newPolyPoint);
			}
			if(line.vertices[editingPointIndex] != newPolyPoint) {
				line.vertices[editingPointIndex] = newPolyPoint;
				changed = true;
			}
			Handles.DotHandleCap(0, worldMousePointOnPlane, Quaternion.identity, HandleUtility.GetHandleSize(worldMousePointOnPlane) * handleSize, Event.current.type);
			if(mouseUp) StopEditingPoint();
		}
    }

	Vector2 SnapToLineInterval (Vector2 vector) {
		vector.x = MathX.RoundToNearest(vector.x, snapInterval);
		vector.y = MathX.RoundToNearest(vector.y, snapInterval);
		return vector;
	}
	Vector3 SnapToWorldInterval (Vector3 vector) {
		return LineToWorldSpace(SnapToLineInterval(WorldToLineSpace(vector)));
	}

	void InsertPoint (ref Line line, int index, Vector2 point) {
		var vertList = line.vertices.ToList();
		vertList.Insert(index, point);
		line.vertices = vertList.ToArray();
    }

	void RemovePoint (ref Line line, int index) {
		var vertList = line.vertices.ToList();
		vertList.RemoveAt(index);
		line.vertices = vertList.ToArray();
    }

	Vector2 WorldToLineSpace (Vector3 point) {
		return matrix.inverse.MultiplyPoint3x4(point);
	}

	Vector3 LineToWorldSpace (Vector2 point) {
		return matrix.MultiplyPoint3x4(point);
	}

	Vector2 WorldToScreenSpace (Vector3 point) {
		var camera = SceneView.currentDrawingSceneView.camera;
		return camera.WorldToScreenPoint(point);
	}

	Vector3 ScreenToWorldSpace (Vector2 point) {
		var camera = SceneView.currentDrawingSceneView.camera;
		return camera.ScreenToWorldPoint(point);
	}

	bool GetMousePointIntersectingRegionPlane (ref Vector3 point) {
		Vector2 mousePosition = Event.current.mousePosition * 2;
		var camera = SceneView.currentDrawingSceneView.camera;
		Vector2 screenPoint = new Vector2(mousePosition.x, camera.pixelHeight - mousePosition.y);
		return GetScreenPointIntersectingRegionPlane(screenPoint, ref point);
    }

	bool GetScreenPointIntersectingRegionPlane (Vector2 screenPoint, ref Vector3 point) {
		var camera = SceneView.currentDrawingSceneView.camera;
		Ray ray = camera.ScreenPointToRay (new Vector3(screenPoint.x, screenPoint.y, 0.0f));
        float rayDistance;
		if (plane.Raycast(ray, out rayDistance)) {
			point = ray.GetPoint(rayDistance);
			return true;
		}
		return false;
    }
}
 */