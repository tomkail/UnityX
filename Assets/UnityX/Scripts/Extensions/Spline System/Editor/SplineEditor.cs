using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SplineSystem {
	public class SplineEditor {
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

        Transform transform;
        Matrix4x4 matrix {
            get {
                var lossyScale = transform.lossyScale;
                if(lossyScale.x == 0) lossyScale.x = 1;//Debug.LogWarning("SplineEditor: Transform's lossyScale.x is 0. This can lead to the editor not functioning correctly.");
                if(lossyScale.y == 0) lossyScale.y = 1;//Debug.LogWarning("SplineEditor: Transform's lossyScale.y is 0. This can lead to the editor not functioning correctly.");
                if(lossyScale.z == 0) lossyScale.z = 1;//Debug.LogWarning("SplineEditor: Transform's lossyScale.z is 0. This can lead to the editor not functioning correctly.");
                return Matrix4x4.TRS(transform.position, transform.rotation, lossyScale);
            }
        }

        public float pointsPerMeter = 10;

        public bool in2DMode;
        public Plane localPlaneFor2DMode = new Plane(Vector3.forward, Vector3.zero);

        bool creationMode {
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

        /// <summary>
        /// Allows editing splines in world space via the scene view.
        /// </summary>
        /// <param name="transform">Transform.</param>
        public SplineEditor (Transform transform) {
            this.transform = transform;
        }

        public void OnInspectorGUI () {
            editing = GUILayout.Toggle(editing, "Editing");
        }

		public bool OnSceneGUI (Spline spline) {
		    if(Event.current.isMouse) SceneView.RepaintAll();
    	    bool changed = false;
		    Validate(spline, ref changed);
            Color savedHandleColor = Handles.color;
		    DrawSpline(spline);
		    DrawSceneViewToolbar(spline, ref changed);
            if(Event.current.alt) {}
            else if(editing) {                
                // Stops clicking on background objects, but breaks 3D rotation sphere (individual axis work though) 
			    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                DrawEditor(spline, ref changed);
            }
		    Handles.color = savedHandleColor;
            return changed;
		}

        public void Destroy () {
            editing = false;
        }

        void Validate (Spline spline, ref bool changed) {
            if(in2DMode) {
                for (var i = 0; i < spline.bezierPoints.Length; i++) {
                    var bezierPoint = spline.bezierPoints[i];
                    var flattenedPosition = GetFlattened2DPosition(bezierPoint.position);
                    if(bezierPoint.position != flattenedPosition) {
                        bezierPoint.position = flattenedPosition;
                        changed = true;
                    }
                    var flattenedRotation = GetFlattened2DRotation(bezierPoint.rotation);
                    if(bezierPoint.rotation != flattenedRotation) {
                        bezierPoint.rotation = flattenedRotation;
                        changed = true;
                    }
                }
            }
            if(spline.Validate()) changed = true;
        }

        void DrawSpline (Spline spline) {
			for (var i = 0; i < spline.curves.Length; i++) {
				int numPoints = Mathf.Max(Mathf.CeilToInt(spline.curves[i].length * pointsPerMeter), 2);
				DrawCurveLine(spline, spline.curves[i], matrix, numPoints);
			}
			for (var i = 0; i < spline.bezierPoints.Length; i++) {
                Handles.SphereHandleCap(-1, matrix.MultiplyPoint3x4(spline.bezierPoints[i].position), Quaternion.identity, 0.1f, EventType.Repaint);
			}
        }

		void DrawCurveLine (Spline spline, SplineBezierCurve curve, Matrix4x4 matrix, int numPoints) {
			Vector3 p0;
			Vector3 p1;
			numPoints = Mathf.Min(numPoints, curve.numArcLengthsForArcLengthToTCalculation);
			float r = 1f/(numPoints-1);
			p0 = matrix.MultiplyPoint3x4(spline.GetPointAtArcLength(curve.startArcLength));
			for (int j = 1; j < numPoints; j++) {
				float arcLength = Mathf.Lerp(curve.startArcLength, curve.endArcLength, j * r);
				p1 = matrix.MultiplyPoint3x4(spline.GetPointAtArcLength(arcLength));
				Handles.DrawAAPolyLine(2, p0, p1);
				p0 = p1;
			}
	    }



        static Spline clipboardSpline;
        void DrawSceneViewToolbar (Spline spline, ref bool changed) {
            Handles.BeginGUI();
            Vector3 midPointScreenPoint = HandleUtility.WorldToGUIPoint(matrix.MultiplyPoint3x4(spline.bounds.center));
            GUILayout.BeginArea(RectX.Create(midPointScreenPoint, new Vector2(140, 40), new Vector2(0.5f, 0.5f)));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(editing ? "Finish" : "Edit")) {
                editing = !editing;
            }
            
            if(GUILayout.Button("Copy")) {
                var serialized = JsonUtility.ToJson(spline);
                GUIUtility.systemCopyBuffer = serialized;
            }

            if(GUIUtility.systemCopyBuffer.Length > 0) {
                try {
                    if(clipboardSpline == null) clipboardSpline = JsonUtility.FromJson<Spline>(GUIUtility.systemCopyBuffer);
                    else JsonUtility.FromJsonOverwrite(GUIUtility.systemCopyBuffer, clipboardSpline);
                } catch {
                    clipboardSpline = null;	
                }
            } else {
                clipboardSpline = null;
            }
            EditorGUI.BeginDisabledGroup(clipboardSpline == null);
            if(GUIUtility.systemCopyBuffer.Length > 0 && GUILayout.Button("Paste") ) {
                if(clipboardSpline != null) {
                    spline.bezierPoints = ((Spline)clipboardSpline).bezierPoints;
                    spline.RefreshCurveData();
                }
                changed = true;
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            Handles.EndGUI();
        }

        void DrawEditor (Spline spline, ref bool changed) {
            for(int i = 0; i < spline.bezierPoints.Length; i++) {
				var point = spline.bezierPoints[i];
				DrawBezierPoint(ref point, ref changed);
                spline.bezierPoints[i] = point;
			}

            if(GUIUtility.hotControl == 0) {
                DrawEditorPoint(spline, ref changed);
            }
        }

		void DrawBezierPoint (ref SplineBezierPoint bezierPoint, ref bool changed) {
			var bezierPointPosition = matrix.MultiplyPoint3x4(bezierPoint.position);
			var bezierPointRotation = matrix.rotation.Rotate(bezierPoint.rotation);
			
			if(Tools.current == Tool.Move) {
				var newBezierPointPosition = Handles.PositionHandle(bezierPointPosition, Tools.pivotRotation == PivotRotation.Local ? bezierPoint.rotation : Quaternion.identity);
				if(bezierPointPosition != newBezierPointPosition) {
					bezierPoint.position = matrix.inverse.MultiplyPoint3x4(newBezierPointPosition);
                    if(in2DMode) bezierPoint.position = GetFlattened2DPosition(bezierPoint.position);
                    changed = true;
                }
			}
			if(Tools.current == Tool.Rotate) {
				var newBezierPointRotation = Handles.RotationHandle(bezierPointRotation, bezierPointPosition);
				if(bezierPointRotation != newBezierPointRotation) {
					bezierPoint.rotation = matrix.inverse.rotation.Rotate(newBezierPointRotation);
                    if(in2DMode) bezierPoint.rotation = GetFlattened2DRotation(bezierPoint.rotation);
                    changed = true;
				}
			}
			DrawControlPointHandle(ref bezierPoint, ref bezierPoint.inControlPoint, ref changed);
			DrawControlPointHandle(ref bezierPoint, ref bezierPoint.outControlPoint, ref changed);
		}

		private void DrawControlPointHandle (ref SplineBezierPoint bezierPoint, ref SplineBezierControlPoint controlPoint, ref bool changed) {
			var controlPointPosition = matrix.MultiplyPoint3x4(controlPoint.GetPosition(bezierPoint));
			var controlPointRotation = matrix.rotation.Rotate(controlPoint.GetRotation(bezierPoint));
			
			var bezierPointPosition = matrix.MultiplyPoint3x4(bezierPoint.position);
			Handles.DrawDottedLine(bezierPointPosition, controlPointPosition, 1);

			var newControlPointPosition = DrawEditableControlPointHandle(controlPointPosition, controlPointRotation);
			if(controlPointPosition != newControlPointPosition) {
                controlPoint.distance = Vector3X.DistanceInDirection(bezierPointPosition, newControlPointPosition, controlPointRotation * Vector3.forward);
                changed = true;
            }
		}

		private Vector3 DrawEditableControlPointHandle (Vector3 controlPointPosition, Quaternion controlPointRotation) {
			float handleSize = HandleUtility.GetHandleSize(controlPointPosition) * 0.15f;
			return Handles.FreeMoveHandle(controlPointPosition, controlPointRotation, handleSize, Vector3.zero, ((int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType) => {
				Handles.ConeHandleCap(controlID, position, controlPointRotation, handleSize, eventType);
			}));
	    }

        void DrawEditorPoint (Spline spline, ref bool changed) {
            Vector2 mousePosition = Event.current.mousePosition;

            float bestSqrDistance = Mathf.Infinity;
            int bestCurveIndex = -1;
            int bestPointIndex = -1;
            float bestNormalizedDistanceOnLine = -1;
            for (var i = 0; i < spline.curves.Length; i++) {
                var curve = spline.curves[i];
                var lastPoint = HandleUtility.WorldToGUIPoint(matrix.MultiplyPoint3x4(curve._points[0]));
                for (var j = 1; j < curve._points.Length; j++) {
                    var nextPoint = HandleUtility.WorldToGUIPoint(matrix.MultiplyPoint3x4(curve._points[j]));
                    UnityX.Geometry.Line line = new UnityX.Geometry.Line(lastPoint, nextPoint);
                    var normalizedDistanceOnLine = line.GetNormalizedDistanceOnLine(mousePosition);
                    var pointOnLine = Vector2.LerpUnclamped(line.start, line.end, normalizedDistanceOnLine);
                    var sqrDistanceFromLine = Vector2X.SqrDistance(mousePosition, pointOnLine);
                    if(sqrDistanceFromLine < bestSqrDistance) {
                        bestSqrDistance = sqrDistanceFromLine;
                        bestCurveIndex = i;
                        bestPointIndex = j;
                        bestNormalizedDistanceOnLine = normalizedDistanceOnLine;
                    }
                    lastPoint = nextPoint;
                }			
            }

            if(bestSqrDistance != Mathf.Infinity) {
                var bestCurve = spline.curves[bestCurveIndex];
                var startPoint = bestCurve._points[bestPointIndex-1];
                var endPoint = bestCurve._points[bestPointIndex];
                var point = Vector3.LerpUnclamped(startPoint, endPoint,bestNormalizedDistanceOnLine);
                var worldPoint = matrix.MultiplyPoint3x4(point);
                Handles.SphereHandleCap(-1, worldPoint, Quaternion.identity, 0.1f, EventType.Repaint);

                
                if(creationMode && Event.current.button == 0 && Event.current.type == EventType.MouseDown) {
                    var arcLength = spline.EstimateArcLengthAlongCurve(point);
                    var curve = spline.GetCurveAtArcLength(arcLength);
                    var bezierPoints = spline.bezierPoints.ToList();
                    var rotation = spline.GetRotationAtArcLength(arcLength);
                    // Handles.ConeHandleCap(-1, worldPoint, rotation, 0.15f, EventType.Repaint);
                    
                    var newPoint = new SplineBezierPoint(point, rotation, curve.length * 0.15f, curve.length * 0.15f);
                    bezierPoints.Insert(bestCurveIndex+1, newPoint);
                    spline.bezierPoints = bezierPoints.ToArray();
                }
                if(deletionMode && Event.current.button == 0 && Event.current.type == EventType.MouseDown) {
                    var arcLength = spline.EstimateArcLengthAlongCurve(point);
                    var curve = spline.GetCurveAtArcLength(arcLength);
                    var bezierPoints = spline.bezierPoints.ToList();
                    var rotation = spline.GetRotationAtArcLength(arcLength);
                    
                    var newPoint = new SplineBezierPoint(point, rotation, curve.length * 0.15f, curve.length * 0.15f);
                    bezierPoints.Insert(bestCurveIndex+1, newPoint);
                    spline.bezierPoints = bezierPoints.ToArray();
                }
            }
        }


        Vector3 GetFlattened2DPosition (Vector3 position) {
            return localPlaneFor2DMode.ClosestPointOnPlane(position);
        }
        Quaternion GetFlattened2DRotation (Quaternion rotation) {
            return Quaternion.LookRotation(Vector3.ProjectOnPlane(rotation * Vector3.forward, localPlaneFor2DMode.normal), localPlaneFor2DMode.normal);
        }



        /*
        void Subdivide () {
            Event.current.Use();
            if(Event.current.shift) {
                Undo.RecordObjects(changedRivers, "Split River");
                var newRiverGo = new GameObject();
                newRiverGo.name = NebulaRiverManager.Instance.NewRiverName();
                Undo.MoveGameObjectToScene(newRiverGo, lastSelectedPoint.river.gameObject.scene, "Moved into scene");
                Undo.SetTransformParent(newRiverGo.transform, lastSelectedPoint.river.transform.parent, "Moved point into new river "+newRiverGo.gameObject);
                Undo.RegisterCreatedObjectUndo(newRiverGo, "Create river");
                
                var newFirstPointGo = new GameObject();
                var newFirstPoint = Undo.AddComponent<RiverBezierPoint>(newFirstPointGo);
                newFirstPoint.transform.position = lastSelectedPoint.position;
                newFirstPoint.transform.rotation = lastSelectedPoint.rotation;
                lastSelectedPoint.CopyPropertiesTo(newFirstPoint);
                newFirstPoint.connector.useConnection = true;
                Undo.MoveGameObjectToScene(newFirstPointGo, lastSelectedPoint.river.gameObject.scene, "Moved into scene");
                Undo.SetTransformParent(newFirstPoint.transform, newRiverGo.transform, "Moved point into new river "+newRiverGo.gameObject);
                Undo.RegisterCreatedObjectUndo(newFirstPointGo, "Create point");
                newFirstPointGo = new GameObject();
                newFirstPoint = Undo.AddComponent<RiverBezierPoint>(newFirstPointGo);
                newFirstPoint.transform.position = lastSelectedPoint.position + lastSelectedPoint.rotation * Vector3.forward * 20;
                newFirstPoint.transform.rotation = lastSelectedPoint.rotation;
                lastSelectedPoint.CopyPropertiesTo(newFirstPoint);
                Undo.MoveGameObjectToScene(newFirstPointGo, lastSelectedPoint.river.gameObject.scene, "Moved into scene");
                Undo.SetTransformParent(newFirstPoint.transform, newRiverGo.transform, "Moved point into new river "+newRiverGo.gameObject);
                Undo.RegisterCreatedObjectUndo(newFirstPointGo, "Create point");

                var newRiver = Undo.AddComponent<SpaceRiver>(newRiverGo);
                lastSelectedPoint.river.CopyPropertiesTo(newRiver);
                newRiver.EditorRefresh();
                newRiver.SetDirty(true, true);

                Selection.activeObject = newFirstPoint.gameObject;
            } else {
                RiverBezierPointEditor.Create(lastSelectedPoint);
            }
        }

		public static void SelectAndLookAt (RiverBezierPoint point) {
			Selection.activeObject = point.gameObject;
			RiverBezierPointEditor.LookAtWaypoint(point);
		}

        public static RiverBezierPoint Create (RiverBezierPoint previousPoint, bool lookAt = true) {
			if(previousPoint == null) return null;
			var bezierPoint = new GameObject("New Bezier Point").AddComponent<RiverBezierPoint>();
			if(Application.isPlaying)
				bezierPoint.Init();
			Undo.RegisterCreatedObjectUndo(bezierPoint.gameObject, "Create Bezier Point");
			if(previousPoint.isLast) {
//				bezierPoint.position = previousPoint.position + previousPoint.forward * Vector3.Distance(previousPoint.position, previousPoint.previousPoint.position);
				var distance = SpaceRiverEditorSettings.Instance.newPointDistanceOverSpeed.Evaluate(previousPoint.speed);
				var offset = RandomX.sign * distance * SpaceRiverEditorSettings.Instance.newPointDisplacementOverDistance;
				bezierPoint.position = previousPoint.position + previousPoint.forward * distance + previousPoint.binormal * offset;
				bezierPoint.rotation = previousPoint.rotation;
			} else {
				bezierPoint.position = Vector3.Lerp(previousPoint.position, previousPoint.nextPoint.transform.position, 0.5f);
				bezierPoint.rotation = Quaternion.Slerp(previousPoint.rotation, previousPoint.nextPoint.transform.rotation, 0.5f);
			}
			bezierPoint.transform.SetParent(previousPoint.transform.parent, true);
			bezierPoint.transform.SetSiblingIndex(previousPoint.transform.GetSiblingIndex()+1);

			previousPoint.CopyPropertiesTo(bezierPoint);
			if(lookAt) SelectAndLookAt(bezierPoint);
			bezierPoint.river.SetDirty(true, true);

			return bezierPoint;
	    }

        public static void DestroyPoint (RiverBezierPoint point, bool lookAt = true) {
			var river = point.river;
			RiverBezierPoint nextPointToSelect = null;
			if(point.isFirst) {
				nextPointToSelect = point.nextPoint;
			} else {
				nextPointToSelect = point.previousPoint;
			}
			Undo.DestroyObjectImmediate(point.gameObject);
			river.SetDirty(true, true);
			if(nextPointToSelect != null && lookAt)
				SelectAndLookAt(nextPointToSelect);
		}
        */
	}
}