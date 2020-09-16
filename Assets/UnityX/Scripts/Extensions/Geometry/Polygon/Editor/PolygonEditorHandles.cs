using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityX.Geometry;

public class PolygonEditorHandles {
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
	// When control is down, this snaps to the nearest interval
	public float snapInterval = 1;
	Transform transform;
	Matrix4x4 matrix {
		get {
			var lossyScale = transform.lossyScale;
			if(lossyScale.x == 0) lossyScale.x = 1;//Debug.LogWarning("PolygonEditor: Transform's lossyScale.x is 0. This can lead to the editor not functioning correctly.");
			if(lossyScale.y == 0) lossyScale.y = 1;//Debug.LogWarning("PolygonEditor: Transform's lossyScale.y is 0. This can lead to the editor not functioning correctly.");
			if(lossyScale.z == 0) lossyScale.z = 1;//Debug.LogWarning("PolygonEditor: Transform's lossyScale.z is 0. This can lead to the editor not functioning correctly.");
			return Matrix4x4.TRS(transform.position, transform.rotation, lossyScale) * offsetMatrix;
		}
	}
	Matrix4x4 directionMatrix {
		get {
			return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one) * offsetMatrix;
		}
	}

	/// <summary>
	/// Allows editing polygons in world space via the scene view.
	/// </summary>
	/// <param name="transform">Transform.</param>
	public PolygonEditorHandles (Transform transform) {
		this.transform = transform;
	}
	public PolygonEditorHandles (Transform transform, Quaternion offsetRotation) : this(transform, Matrix4x4.TRS(Vector3.zero, offsetRotation, Vector3.one)) {}
	public PolygonEditorHandles (Transform transform, bool onXZPlane) : this(transform, onXZPlane ? Quaternion.Euler(new Vector3(90,0,0)) : Quaternion.identity) {}


	public Func<Vector2[]> defaultPolygonFunc;

	/// <summary>
	/// Allows editing polygons in world space via the scene view. Offset matrix allows the editing plane to be flipped to the XZ plane.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="offsetMatrix">Offset matrix.</param>
	public PolygonEditorHandles (Transform transform, Matrix4x4 offsetMatrix) {
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
			return editing && !isEditingPoint && Event.current.control;
			#else
			return editing && !isEditingPoint && Event.current.control;
			#endif
		}
	}
	
	bool snapToPoint {
		get {
			#if UNITY_EDITOR_WIN 
			return editing && Event.current.control;
			#else
			return editing && Event.current.control;
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

    public bool OnSceneGUI (Polygon polygon) {
		if(Event.current.isMouse) SceneView.RepaintAll();
    	bool changed = false;
		Validate(polygon, ref changed);
		Color savedHandleColor = Handles.color;
		DrawPolygon(polygon);
		DrawSceneViewToolbar(polygon, ref changed);
		if(Event.current.alt) StopEditingPoint();
		else if(editing) {
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			DrawEditor(polygon, ref changed);
		}
		Handles.color = savedHandleColor;
		return changed;
    }

    public void Destroy () {
        editing = false;
    }

	static Polygon clipboardPolygon;
	void DrawSceneViewToolbar (Polygon polygon, ref bool changed) {
		Vector3 midPointScreenPoint = HandleUtility.WorldToGUIPointWithDepth(matrix.MultiplyPoint3x4(polygon.center));
		if(midPointScreenPoint.z <= 0) return;

		Handles.BeginGUI();
		GUILayout.BeginArea(RectX.Create(midPointScreenPoint, new Vector2(140, 40), new Vector2(0.5f, 0.5f)));
		GUILayout.BeginHorizontal();

		if (GUILayout.Button(editing ? "Finish" : "Edit")) {
			editing = !editing;
		}
		
		if(GUILayout.Button("Copy")) {
			var serialized = JsonUtility.ToJson(polygon);
			GUIUtility.systemCopyBuffer = serialized;
		}

		if(GUIUtility.systemCopyBuffer.Length > 0) {
			try {
				if(clipboardPolygon == null) clipboardPolygon = JsonUtility.FromJson<Polygon>(GUIUtility.systemCopyBuffer);
				else JsonUtility.FromJsonOverwrite(GUIUtility.systemCopyBuffer, clipboardPolygon);
			} catch {
				clipboardPolygon = null;	
			}
		} else {
			clipboardPolygon = null;
		}
		EditorGUI.BeginDisabledGroup(clipboardPolygon == null);
		if(GUIUtility.systemCopyBuffer.Length > 0 && GUILayout.Button("Paste") ) {
			if(clipboardPolygon != null) polygon.CopyFrom((Polygon)clipboardPolygon);
			changed = true;
		}
		EditorGUI.EndDisabledGroup();

		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		Handles.EndGUI();
	}

	void Validate (Polygon polygon, ref bool changed) {
		if(polygon.vertices == null || polygon.vertices.Length < 3) {
			if( defaultPolygonFunc != null ) {
				polygon.vertices = defaultPolygonFunc();
			} else {
				polygon.vertices = new RegularPolygon(4, 45).ToPolygonVerts();
			}

			changed = true;
		}
    }

	void DrawPolygon (Polygon polygon) {
		if(moveMode) {
			if(isMoving) Handles.color = movingLineColor;
			else Handles.color = moveModeLineColor;
		} else Handles.color = lineColor;
		Vector3[] worldPoints = new Vector3[polygon.vertices.Length+1];
		for (int i = 0; i < polygon.vertices.Length; i++) {
			worldPoints[i] = PolygonToWorldPoint(polygon.vertices [i]);
            
            // Draw vert normals
            // var vertNormal = polygon.GetVertexNormal(i);
		    // Handles.DrawPolyLine(worldPoints[i], PolygonToWorldSpace(polygon.vertices [i]+vertNormal));
		    
            // Draw edge normals
            // var isClockwise = polygon.GetIsClockwise();
            // var centerPos = Vector2.Lerp(polygon.vertices [i], polygon.vertices.GetRepeating(i+1), 0.5f);
            // var tangent = polygon.GetEdgeTangentAtEdgeIndex(i);
			// var edgeNormal = Polygon.GetEdgeNormalAtEdgeIndex(tangent, isClockwise);
		    // Handles.DrawPolyLine(PolygonToWorldPoint(centerPos), PolygonToWorldPoint(centerPos+edgeNormal * 0.25f));
            // // Draw edge tangents
            // Handles.ArrowHandleCap(-1, PolygonToWorldPoint(centerPos), Quaternion.LookRotation(PolygonToWorldDirection(tangent), Vector3.Cross(PolygonToWorldDirection(tangent), PolygonToWorldDirection(edgeNormal))), 0.5f, EventType.Repaint);
        }
		worldPoints[worldPoints.Length-1] = worldPoints[0];
		Handles.DrawPolyLine(worldPoints);
    }

    Vector2 GUIToScreen () {
        var sv = SceneView.currentDrawingSceneView;

        var style = (GUIStyle) "GV Gizmo DropDown";
        Vector2 ribbon = style.CalcSize( sv.titleContent );
        
        Vector2 sv_correctSize = sv.position.size;
        sv_correctSize.y -= ribbon.y; //exclude this nasty ribbon
        
        //flip the position:
        Vector2 mousePosFlipped = Event.current.mousePosition;
        mousePosFlipped.y = sv_correctSize.y - mousePosFlipped.y;
        return mousePosFlipped;
    }

	void DrawEditor (Polygon polygon, ref bool changed) {
		bool mouseDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;
		bool mouseUp = Event.current.button == 0 && Event.current.type == EventType.MouseUp;

		Vector2 mousePosition = Event.current.mousePosition;

		Vector2[] screenSpacePolygonVerts = new Vector2[polygon.vertices.Length];
		for (int i = 0; i < screenSpacePolygonVerts.Length; i++) {
			Vector3 worldSpace = PolygonToWorldPoint(polygon.vertices[i]);
			screenSpacePolygonVerts [i] = HandleUtility.WorldToGUIPoint(worldSpace);
		}
		Polygon screenSpacePolygon = new Polygon(screenSpacePolygonVerts);

		var closestVertexIndex = screenSpacePolygon.FindClosestVertexIndex(mousePosition);

		var closestScreenVertexPoint = screenSpacePolygon.vertices[closestVertexIndex];

		var bestPointOnScreenPolygon = screenSpacePolygon.FindClosestPointOnPolygon(mousePosition);

		
		var screenDistanceFromClosestPoint = Vector2.Distance(closestScreenVertexPoint, bestPointOnScreenPolygon);
		var worldMousePointOnPlane = Vector3.zero;
		GetScreenPointIntersectingRegionPlane(mousePosition, ref worldMousePointOnPlane);
		bool vertexEditMode = !moveMode && (screenDistanceFromClosestPoint < pointSnapDistance || deletionMode);
		if(vertexEditMode) {
			int[] indices = new int[3] {
				polygon.vertices.GetRepeatingIndex(closestVertexIndex-1),
				closestVertexIndex, 
				polygon.vertices.GetRepeatingIndex(closestVertexIndex+1)
			};

			if(!isEditingPoint) {
				Vector3[] closestVertices = new Vector3[3];
				for(int i = 0; i < closestVertices.Length; i++) closestVertices[i] = PolygonToWorldPoint(polygon.vertices[indices[i]]);

				if(deletionMode) Handles.color = highlightedDeletionModeLineColor;
				else Handles.color = highlightedLineColor;

				Handles.DrawAAPolyLine(highlightedLineWidth, closestVertices);
				Handles.DotHandleCap(0, closestVertices[1], Quaternion.identity, HandleUtility.GetHandleSize(closestVertices[1]) * handleSize, Event.current.type);
			}
			
			if(mouseDown) {
				if(deletionMode) {
					bool canRemove = polygon.vertices.Length > 3;
					if(canRemove) {
						RemovePoint(ref polygon, closestVertexIndex);
						changed = true;
					}
				}
				else StartEditingPoint(closestVertexIndex);
			}
		} else {
			if(moveMode) {
				if(mouseDown) 
					StartMoving(WorldToPolygonPoint(worldMousePointOnPlane));
			} else if(!isEditingPoint) {
				int leftIndex = 0;
				int rightIndex = 0;
				screenSpacePolygon.FindClosestEdgeIndices(bestPointOnScreenPolygon, ref leftIndex, ref rightIndex);

				Handles.color = highlightedLineColor;

				var worldLeftPoint = PolygonToWorldPoint(polygon.vertices[leftIndex]);
				var worldRightPoint = PolygonToWorldPoint(polygon.vertices[rightIndex]);
				Handles.DrawAAPolyLine(highlightedLineWidth, worldLeftPoint, worldRightPoint);

				var closestScreenPoint = screenSpacePolygon.FindClosestPointOnPolygon(bestPointOnScreenPolygon);
				var worldBestPointOnPolygon = Vector3.zero;
				GetScreenPointIntersectingRegionPlane(closestScreenPoint, ref worldBestPointOnPolygon);
				if(snapToPoint) worldBestPointOnPolygon = SnapToWorldInterval(worldBestPointOnPolygon);
				Handles.DotHandleCap(0, worldBestPointOnPolygon, Quaternion.identity, HandleUtility.GetHandleSize(worldBestPointOnPolygon) * handleSize, Event.current.type);

				if(mouseDown) {
					InsertPoint(ref polygon, rightIndex, worldMousePointOnPlane);
					StartEditingPoint(rightIndex);
					changed = true;
				}
			}
		}

		if(isMoving) {
			var moveNewPosition = WorldToPolygonPoint(worldMousePointOnPlane);
			var moveDeltaPosition = moveNewPosition - moveLastPosition;
			// This doesn't work, and isnt the right approach. To do, if i can be arsed
			// Vector2 snapOffset = Vector2.zero;
			// if(snapToPoint) {
			// 	snapOffset = SnapToPolygonInterval(polygon.vertices[0]+moveDeltaPosition)-polygon.vertices[0]+moveDeltaPosition;
			// 	moveDeltaPosition += snapOffset;
			// }
			if(moveDeltaPosition != Vector2.zero) {
				polygon.Move(moveDeltaPosition);
		
				moveLastPosition = moveNewPosition;
				changed = true;
			}
			if(mouseUp || !moveMode) StopMoving();
		}

		if(isEditingPoint) {
			Vector2 newPolyPoint = WorldToPolygonPoint(worldMousePointOnPlane);
			if(snapToPoint) {
				newPolyPoint = SnapToPolygonInterval(newPolyPoint);
				worldMousePointOnPlane = PolygonToWorldPoint(newPolyPoint);
			}
			if(polygon.vertices[editingPointIndex] != newPolyPoint) {
				polygon.vertices[editingPointIndex] = newPolyPoint;
				changed = true;
			}
			Handles.DotHandleCap(0, worldMousePointOnPlane, Quaternion.identity, HandleUtility.GetHandleSize(worldMousePointOnPlane) * handleSize, Event.current.type);
			if(mouseUp) StopEditingPoint();
		}
    }

	Vector2 SnapToPolygonInterval (Vector2 vector) {
		vector.x = MathX.RoundToNearest(vector.x, snapInterval);
		vector.y = MathX.RoundToNearest(vector.y, snapInterval);
		return vector;
	}
	Vector3 SnapToWorldInterval (Vector3 vector) {
		return PolygonToWorldPoint(SnapToPolygonInterval(WorldToPolygonPoint(vector)));
	}

	void InsertPoint (ref Polygon polygon, int index, Vector2 point) {
		var vertList = polygon.vertices.ToList();
		vertList.Insert(index, point);
		polygon.vertices = vertList.ToArray();
    }

	void RemovePoint (ref Polygon polygon, int index) {
		var vertList = polygon.vertices.ToList();
		vertList.RemoveAt(index);
		polygon.vertices = vertList.ToArray();
    }

	Vector2 WorldToPolygonPoint (Vector3 point) {
		return matrix.inverse.MultiplyPoint3x4(point);
	}

	Vector3 PolygonToWorldPoint (Vector2 point) {
		return matrix.MultiplyPoint3x4(point);
	}

	Vector3 PolygonToWorldDirection (Vector2 vector) {
		return matrix.MultiplyVector(vector);
	}

	bool GetScreenPointIntersectingRegionPlane (Vector2 screenPoint, ref Vector3 point) {
		Ray ray = HandleUtility.GUIPointToWorldRay (screenPoint);
        float rayDistance;
		if (plane.Raycast(ray, out rayDistance)) {
			point = ray.GetPoint(rayDistance);
			return true;
		}
		return false;
    }
}