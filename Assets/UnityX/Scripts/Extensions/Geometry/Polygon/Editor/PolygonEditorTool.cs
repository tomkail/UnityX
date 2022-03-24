using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using System;
using System.Collections.Generic;
using System.Linq;

class PolygonEditorTool : EditorTool {
    // Serialize this value to set a default value in the Inspector.
    [SerializeField]
    Texture2D m_ToolIcon = null;

    GUIContent m_IconContent;


    public static bool editable = true;
	public static bool editing {
		get {
			return ToolManager.activeToolType == typeof(PolygonEditorTool);
		}
	}

	public static bool showSceneViewTools = false;
    

	bool usingHandOrbitTool {
		get {
			return Tools.viewTool == ViewTool.Orbit && Event.current.alt;
		}
	}
	static bool moveMode {
		get {
			return Event.current.shift;
		}
	}
	static bool deletionMode {
		get {
			#if UNITY_EDITOR_WIN 
			return !isEditingPoint && Event.current.control;
			#else
			return !isEditingPoint && Event.current.control;
			#endif
		}
	}
	
	// When control is down, this snaps to the nearest interval
	public static float snapInterval = 1;
	static bool holdingSnapToPointKey {
		get {
			#if UNITY_EDITOR_WIN 
			return Event.current.control;
			#else
			return Event.current.control;
			#endif
		}
	}
	
	static bool isEditingPoint {
		get {
			return activeInstance != null && editingPointIndex != -1;
		}
	}
	static PolygonEditorInstance targetInstance {
		get {
			return activeInstance ?? hoveredInstance;
		}
	}
	static PolygonEditorInstance activeInstance;
	static PolygonEditorInstance hoveredInstance;
	static int editingPointIndex = -1;

	static bool isMoving;
	static Vector2 moveLastPosition;

	
	const float maxPointSelectionScreenDistance = 10;

	const float handleSize = 0.04f;
	const float pointSnapDistance = 14;
	const float highlightedLineWidth = 3;

	static readonly Color lineColor = new Color(0.8f, 1, 0.8f);
	static readonly Color moveModeLineColor = new Color(0.8f, 0.8f, 1);
	static readonly Color movingLineColor = new Color(0, 0, 1);

	static readonly Color highlightedLineColor = Color.green;
	static readonly Color highlightedDeletionModeLineColor = Color.red;
    
	static Polygon clipboardPolygon;

    void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = m_ToolIcon,
            text = "Polygon Editor Tool",
            tooltip = "Polygon Editor Tool"
        };
    }

    public override GUIContent toolbarIcon
    {
        get { return m_IconContent; }
    }

    [Shortcut("My Project/My Tool", KeyCode.U)]
    private static void Shortcut()
    {
		// var polygonEditorTool = UnityEditor.EditorTools.EditorTool.FindObjectOfType<PolygonEditorTool>();
		// if(polygonEditorTool != null && polygonEditorTool.IsAvailable())
        	ToolManager.SetActiveTool<PolygonEditorTool>();
    }
    
    static Dictionary<object, PolygonEditorInstance> drawActions = new Dictionary<object, PolygonEditorInstance>();
	static Dictionary<PolygonEditorInstance, PolygonEditorInstanceHandler> instanceHandlers = new Dictionary<PolygonEditorInstance, PolygonEditorInstanceHandler>();
	public static void StartDrawing (object key, PolygonEditorInstance handler) {
        drawActions[key] = handler;
	}

	public static void StopDrawing (object key) {
        PolygonEditorInstance editorInstance = null;
		if(!drawActions.TryGetValue(key, out editorInstance)) {
			Debug.LogWarning("Couldn't find draw action with key "+key);
			return;
		}
		if(editorInstance == hoveredInstance) hoveredInstance = null;
		if(editorInstance == activeInstance) activeInstance = null;
		drawActions.Remove(key);
	}

    public override void OnToolGUI(EditorWindow window) {
		if(Event.current.isMouse) SceneView.RepaintAll();

		List<PolygonEditorInstance> instancesEditorsToRemove = new List<PolygonEditorInstance>();
        foreach(var instanceHandler in instanceHandlers) {
			if(!drawActions.Values.Contains(instanceHandler.Key) || !instanceHandler.Key.Valid) {
				instancesEditorsToRemove.Add(instanceHandler.Key);
			}
		}
		foreach(var instanceToRemove in instancesEditorsToRemove) {
			instanceHandlers.Remove(instanceToRemove);
		}

        foreach(var polygonEditorInstance in drawActions.Values) {
			if(!polygonEditorInstance.Valid) continue;
			PolygonEditorInstanceHandler instanceHandler = null;
			if(!instanceHandlers.TryGetValue(polygonEditorInstance, out instanceHandler)) {
				instanceHandler = new PolygonEditorInstanceHandler(polygonEditorInstance);
				instanceHandlers.Add(polygonEditorInstance, instanceHandler);
			}
			instanceHandler.GatherData();
        }

		hoveredInstance = null;
		float bestDistance = Mathf.Infinity;
		if(!moveMode) {
			bestDistance = maxPointSelectionScreenDistance;
		}
		foreach(var instanceHandler in instanceHandlers.Values) {
			if(moveMode) {
				if(instanceHandler.signedScreenDistanceFromEdge < bestDistance) {
					bestDistance = instanceHandler.signedScreenDistanceFromEdge;
					hoveredInstance = instanceHandler.instance;
				}
			} else {
				if(Mathf.Abs(instanceHandler.signedScreenDistanceFromEdge) < bestDistance) {
					bestDistance = Mathf.Abs(instanceHandler.signedScreenDistanceFromEdge);
					hoveredInstance = instanceHandler.instance;
				}
			}
		}

		Color savedHandleColor = Handles.color;

		if(editable && usingHandOrbitTool) 
			StopEditingPoint();

		foreach(var instanceHandler in instanceHandlers.Values) {
			bool changed = false;
			if(instanceHandler.instance.undoTarget != null)
				Undo.RecordObject(instanceHandler.instance.undoTarget, "Edit polygon");
			
			Validate(instanceHandler.instance, ref changed);
			if(instanceHandler.instance.drawPolygon)
				DrawPolygon(instanceHandler.instance);
			if(editable) {
				if(showSceneViewTools)
					DrawSceneViewToolbar(instanceHandler.instance, ref changed);
				if(usingHandOrbitTool) StopEditingPoint();
			}
			if(targetInstance == instanceHandler.instance && editable && !usingHandOrbitTool) {
				instanceHandler.DrawEditor(ref changed);
			}
			if(changed) {
				instanceHandler.instance.OnPolygonChanged(instanceHandler.instance.polygon);
			}
		}

		Handles.color = savedHandleColor;
    }

	void DrawSceneViewToolbar (PolygonEditorInstance handler, ref bool changed) {
		Handles.BeginGUI();
		Vector3 midPointScreenPoint = HandleUtility.WorldToGUIPoint(handler.matrix.MultiplyPoint3x4(handler.polygon.center));
		var halfSize = new Vector2(100, 28)*0.5f;
		GUILayout.BeginArea(Rect.MinMaxRect(midPointScreenPoint.x-halfSize.x, midPointScreenPoint.y-halfSize.y, midPointScreenPoint.x+halfSize.x, midPointScreenPoint.y+halfSize.y));
		GUILayout.BeginHorizontal();

		if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Icon Dropdown")), GUILayout.Width(28), GUILayout.ExpandHeight(true))) {
			bool _changed = changed;
			var contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("Copy"), false, () => {
				var serialized = JsonUtility.ToJson(handler.polygon);
				GUIUtility.systemCopyBuffer = serialized;
			});
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
			if(clipboardPolygon == null) contextMenu.AddDisabledItem(new GUIContent("Paste"));
			else {
				contextMenu.AddItem(new GUIContent("Paste"), false, () => {
					if(clipboardPolygon != null) handler.polygon.CopyFrom((Polygon)clipboardPolygon);
					_changed = true;
				});
			}

			contextMenu.AddSeparator("");

			contextMenu.AddItem(new GUIContent("Flip X"), false, () => {
				for (int i = 0; i < handler.polygon.vertices.Length; i++) {
					handler.polygon.vertices[i] = new Vector2(-handler.polygon.vertices[i].x, handler.polygon.vertices[i].y);
				}
				_changed = true;
			});
			// contextMenu.AddItem(new GUIContent("Simplify"), false, () => {
			// 	List<Vector2> simplified = new List<Vector2>();
			// 	Polygon.SimplifyRamerDouglasPeucker(polygon.vertices.ToList(), 0, simplified);
			// 	polygon.SetVertices(simplified);
			// 	_changed = true;
			// });
			contextMenu.ShowAsContext();
			changed = _changed;
		}

		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		Handles.EndGUI();
	}

	void Validate (PolygonEditorInstance instance, ref bool changed) {
		if(instance.polygon.vertices == null || instance.polygon.vertices.Length < (instance.closed ? 3 : 2)) {
			if( instance.DefaultPolygonFunc != null ) {
				instance.polygon.vertices = instance.DefaultPolygonFunc();
			} else {
				instance.polygon.vertices = new Vector2[] {
					new Vector2(-0.5f, 0.5f),
					new Vector2(0.5f, 0.5f),
					new Vector2(0.5f, -0.5f),
					new Vector2(-0.5f, -0.5f),
				};
			}
            if(isEditingPoint) StopEditingPoint();
			changed = true;
		}
        if(instance.enforcedWindingOrder != 0) {
            if(instance.polygon.GetIsClockwise() != (instance.enforcedWindingOrder > 0)) {
                instance.polygon.FlipWindingOrder();
                if(isEditingPoint) {
                    var pointIndex = (instance.polygon.vertices.Length-1)-editingPointIndex;
                    StopEditingPoint();
                    StartEditingPoint(instance, pointIndex);
                }
                changed = true;
            }
        }
    }

	void DrawPolygon (PolygonEditorInstance instance) {
		if(moveMode) {
			if(isMoving && activeInstance != null && activeInstance == instance) Handles.color = movingLineColor;
			else Handles.color = moveModeLineColor;
		} else Handles.color = lineColor;
		Vector3[] worldPoints = new Vector3[instance.polygon.vertices.Length+(instance.closed ? 1 : 0)];
		for (int i = 0; i < instance.polygon.vertices.Length; i++) {
			worldPoints[i] = instance.PolygonToWorldPoint(instance.polygon.vertices [i]);
			
            if(instance.drawVertNormals) {
				var vertNormal = instance.polygon.GetVertexNormal(i);
				Handles.DrawPolyLine(worldPoints[i], instance.PolygonToWorldPoint(instance.polygon.vertices [i]+vertNormal));
			}
		    
            if(instance.drawEdgeNormals) {
				var isClockwise = instance.polygon.GetIsClockwise();
				var centerPos = Vector2.Lerp(instance.polygon.vertices[i], instance.polygon.vertices[(i+1)%(instance.polygon.vertices.Length-1)], 0.5f);
				var tangent = instance.polygon.GetEdgeTangentAtEdgeIndex(i);
				var edgeNormal = Polygon.GetEdgeNormalAtEdgeIndex(tangent, isClockwise);
				Handles.DrawPolyLine(instance.PolygonToWorldPoint(centerPos), instance.PolygonToWorldPoint(centerPos+edgeNormal * 0.25f));
				// Draw edge tangents
				Handles.ArrowHandleCap(-1, 
					instance.PolygonToWorldPoint(centerPos), 
					Quaternion.LookRotation(instance.PolygonToWorldDirection(tangent), Vector3.Cross(instance.PolygonToWorldDirection(tangent), instance.PolygonToWorldDirection(edgeNormal))), 
					0.5f, 
					EventType.Repaint
				);
			}			
        }
		if(instance.closed) {
			worldPoints[worldPoints.Length-1] = worldPoints[0];
		}
		Handles.DrawPolyLine(worldPoints);
		
		// Draw index
		// Handles.BeginGUI();
		// for (int i = 0; i < polygon.vertices.Length; i++) {
		// 	Vector3 midPointScreenPoint = HandleUtility.WorldToGUIPoint(matrix.MultiplyPoint3x4(polygon.vertices [i]));
		// 	GUILayout.BeginArea(RectX.Create(midPointScreenPoint, new Vector2(24, 24), new Vector2(0.5f, 0.5f)), GUI.skin.box);
		// 	GUILayout.Label(i.ToString(), EditorStyles.centeredGreyMiniLabel);
		// 	GUILayout.EndArea();
		// }
		// Handles.EndGUI();
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

	public class PolygonEditorInstanceHandler {
		public PolygonEditorInstance instance;
		public Polygon screenSpacePolygon;
		public int closestVertexIndex;
		public Vector2 closestScreenVertexPoint;
		public Vector2 bestPointOnScreenPolygon;
		public float screenDistanceFromClosestPoint;
		public float signedScreenDistanceFromEdge;

		public PolygonEditorInstanceHandler (PolygonEditorInstance handler) {
			this.instance = handler;
		}

		public void GatherData () {
			Vector2 mousePosition = Event.current.mousePosition;
			
			Vector2[] screenSpacePolygonVerts = new Vector2[instance.polygon.vertices.Length];
			for (int i = 0; i < screenSpacePolygonVerts.Length; i++) {
				Vector3 worldSpace = instance.PolygonToWorldPoint(instance.polygon.GetVertex(i));
				screenSpacePolygonVerts [i] = HandleUtility.WorldToGUIPoint(worldSpace);
			}
			screenSpacePolygon = new Polygon(screenSpacePolygonVerts);

			closestVertexIndex = screenSpacePolygon.FindClosestVertexIndex(mousePosition);

			closestScreenVertexPoint = screenSpacePolygon.GetVertex(closestVertexIndex);

			bestPointOnScreenPolygon = screenSpacePolygon.FindClosestPointOnPolygon(mousePosition, instance.closed);

			screenDistanceFromClosestPoint = Vector2.Distance(closestScreenVertexPoint, bestPointOnScreenPolygon);

			signedScreenDistanceFromEdge = Vector2.Distance(bestPointOnScreenPolygon, mousePosition);
			signedScreenDistanceFromEdge *= screenSpacePolygon.ContainsPoint(mousePosition) ? -1 : 1;
		}

		public void DrawEditor (ref bool changed) {
			Vector2 mousePosition = Event.current.mousePosition;
			bool mouseDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;
			bool mouseUp = (Event.current.button == 0 && Event.current.type == EventType.MouseUp) || Event.current.type == EventType.MouseLeaveWindow;
			
			var worldMousePointOnPlane = Vector3.zero;
			instance.GetScreenPointIntersectingRegionPlane(mousePosition, ref worldMousePointOnPlane);
			bool vertexEditMode = !moveMode && (screenDistanceFromClosestPoint < pointSnapDistance || deletionMode);
			if(vertexEditMode) {
				int[] indices = new int[3] {
					closestVertexIndex == 0 ? instance.polygon.vertices.Length-1 : closestVertexIndex-1,
					closestVertexIndex, 
					closestVertexIndex == instance.polygon.vertices.Length-1 ? 0 : closestVertexIndex+1
				};

				if(!isEditingPoint) {
					Vector3[] closestVertices = new Vector3[3];
					for(int i = 0; i < closestVertices.Length; i++) closestVertices[i] = instance.PolygonToWorldPoint(instance.polygon.vertices[indices[i]]);

					if(deletionMode) Handles.color = highlightedDeletionModeLineColor;
					else Handles.color = highlightedLineColor;
					
					if(instance.closed || Mathf.Abs(indices[0]-closestVertexIndex) == 1) {
						var worldLeftPoint = instance.PolygonToWorldPoint(instance.polygon.vertices[indices[0]]);
						Handles.DrawAAPolyLine(highlightedLineWidth, closestVertices[1], worldLeftPoint);
					}
					if (instance.closed || Mathf.Abs(indices[2]-closestVertexIndex) == 1) {
						var worldRightPoint = instance.PolygonToWorldPoint(instance.polygon.vertices[indices[2]]);
						Handles.DrawAAPolyLine(highlightedLineWidth, closestVertices[1], worldRightPoint);
					}
					Handles.DotHandleCap(0, closestVertices[1], Quaternion.identity, HandleUtility.GetHandleSize(closestVertices[1]) * handleSize, Event.current.type);
				}
				
				bool canRemove = instance.polygon.vertices.Length > (instance.closed ? 3 : 2);
				if((deletionMode && canRemove) || (!deletionMode))
					HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
	
				if(mouseDown) {
					if(deletionMode) {
						if(canRemove) {		
							var poly = instance.polygon;
							instance.RemovePoint(closestVertexIndex);
							changed = true;
						}
					}
					else {
						HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
						StartEditingPoint(instance, closestVertexIndex);
					}
				}
			} else {
				if(moveMode) {
					if(signedScreenDistanceFromEdge <= 0) {
						HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
						EditorGUIUtility.AddCursorRect(SceneView.currentDrawingSceneView.position, MouseCursor.Pan);

						if(mouseDown) {
							StartMoving(instance, instance.WorldToPolygonPoint(worldMousePointOnPlane));
						}
					}
				} else if(!isEditingPoint) {
					HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
					
					int leftIndex = 0;
					int rightIndex = 0;
					screenSpacePolygon.FindClosestEdgeIndices(bestPointOnScreenPolygon, ref leftIndex, ref rightIndex);

					Handles.color = highlightedLineColor;

					if(instance.closed || Mathf.Abs(leftIndex-closestVertexIndex) == 1) {
						var worldLeftPoint = instance.PolygonToWorldPoint(instance.polygon.vertices[leftIndex]);
						Handles.DrawAAPolyLine(highlightedLineWidth, worldLeftPoint);
					}
					if (instance.closed || Mathf.Abs(rightIndex-closestVertexIndex) == 1) {
						var worldRightPoint = instance.PolygonToWorldPoint(instance.polygon.vertices[rightIndex]);
						Handles.DrawAAPolyLine(highlightedLineWidth, worldRightPoint);
					}

					var closestScreenPoint = screenSpacePolygon.FindClosestPointOnPolygon(bestPointOnScreenPolygon, instance.closed);
					var worldBestPointOnPolygon = Vector3.zero;
					instance.GetScreenPointIntersectingRegionPlane(closestScreenPoint, ref worldBestPointOnPolygon);
					if(instance.forceSnapToPoint ^ holdingSnapToPointKey) worldBestPointOnPolygon = instance.SnapToWorldInterval(worldBestPointOnPolygon, snapInterval);
					Handles.DotHandleCap(0, worldBestPointOnPolygon, Quaternion.identity, HandleUtility.GetHandleSize(worldBestPointOnPolygon) * handleSize, Event.current.type);

					if(mouseDown) {
						instance.InsertPoint(rightIndex, worldMousePointOnPlane);
						
						StartEditingPoint(instance, rightIndex);
						changed = true;
					}
				}
			}

			if(isMoving) {
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
				var moveNewPosition = instance.WorldToPolygonPoint(worldMousePointOnPlane);
				var moveDeltaPosition = moveNewPosition - moveLastPosition;
				// This doesn't work, and isnt the right approach. To do, if i can be arsed
				// Vector2 snapOffset = Vector2.zero;
				// if(snapToPoint) {
				// 	snapOffset = SnapToPolygonInterval(polygon.vertices[0]+moveDeltaPosition)-polygon.vertices[0]+moveDeltaPosition;
				// 	moveDeltaPosition += snapOffset;
				// }
				if(moveDeltaPosition != Vector2.zero) {
					instance.polygon.Move(moveDeltaPosition);
			
					moveLastPosition = moveNewPosition;
					changed = true;
				}
				if(mouseUp || !moveMode) StopMoving();
			}

			if(isEditingPoint) {
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
				Vector2 newPolyPoint = instance.WorldToPolygonPoint(worldMousePointOnPlane);
				if(instance.forceSnapToPoint ^ holdingSnapToPointKey) {
					newPolyPoint = PolygonEditorInstance.SnapToPolygonInterval(newPolyPoint, snapInterval);
					worldMousePointOnPlane = instance.PolygonToWorldPoint(newPolyPoint);
				}
				if(instance.polygon.vertices[editingPointIndex] != newPolyPoint) {
					instance.polygon.vertices[editingPointIndex] = newPolyPoint;
					changed = true;
				}
				Handles.DotHandleCap(0, worldMousePointOnPlane, Quaternion.identity, HandleUtility.GetHandleSize(worldMousePointOnPlane) * handleSize, Event.current.type);
				if(mouseUp) StopEditingPoint();
			}
		}
	}


    static void StartMoving (PolygonEditorInstance polygonEditorHandler, Vector2 currentPosition) {
		activeInstance = polygonEditorHandler;
		isMoving = true;
		moveLastPosition = currentPosition;
    }

	static void StopMoving () {
		activeInstance = null;
		isMoving = false;
    }

	static void StartEditingPoint (PolygonEditorInstance polygonEditorHandler, int pointIndex) {
        if(pointIndex >= 0 && pointIndex < polygonEditorHandler.polygon.vertices.Length) {
            activeInstance = polygonEditorHandler;
            editingPointIndex = pointIndex;
        } else {
            Debug.LogWarning("Cannot edit out of range point "+pointIndex+" on poly with length "+polygonEditorHandler.polygon.vertices.Length);
        }
    }

	static void StopEditingPoint () {
		activeInstance = null;
		editingPointIndex = -1;
    }
}