using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace SplineSystem {
	public class SplineEditor {
		const float cursorPointHandleSize = 0.125f;
		const float bezierPointHandleSize = 0.075f;
		const float controlPointHandleSize = 0.175f;

		public bool editable = true;

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

		public Matrix4x4 localToWorldMatrix;
		Transform transform;
		Matrix4x4 matrix {
			get {
				if(transform == null) {
					return localToWorldMatrix;
				} else {
					var lossyScale = transform.lossyScale;
					if(lossyScale.x == 0) lossyScale.x = 1;//Debug.LogWarning("SplineEditor: Transform's lossyScale.x is 0. This can lead to the editor not functioning correctly.");
					if(lossyScale.y == 0) lossyScale.y = 1;//Debug.LogWarning("SplineEditor: Transform's lossyScale.y is 0. This can lead to the editor not functioning correctly.");
					if(lossyScale.z == 0) lossyScale.z = 1;//Debug.LogWarning("SplineEditor: Transform's lossyScale.z is 0. This can lead to the editor not functioning correctly.");
					return Matrix4x4.TRS(transform.position, transform.rotation, lossyScale);
				}
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

		public delegate void ChangeBezierPointPosition(Spline spline, int bezierPointIndex, ref SplineBezierPoint bezierPoint, Vector3 previousPosition);
		public event ChangeBezierPointPosition OnChangeBezierPointPosition;

		/// <summary>
		/// Allows editing splines in world space via the scene view.
		/// </summary>
		/// <param name="transform">Transform.</param>
		public SplineEditor (Transform transform) {
			this.transform = transform;
		}
		public SplineEditor (Matrix4x4 localToWorldMatrix) {
			this.localToWorldMatrix = localToWorldMatrix;
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
			if(editable) {
				DrawSceneViewToolbar(spline, ref changed);
				if(Event.current.alt) {}
				else if(editing) {                
					// Stops clicking on background objects, but breaks 3D rotation sphere (individual axis work though) 
					HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
					DrawEditor(spline, ref changed);
				}
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
						spline.bezierPoints[i] = bezierPoint;
						changed = true;
					}
					var flattenedRotation = GetFlattened2DRotation(bezierPoint.rotation);
					if(bezierPoint.rotation != flattenedRotation) {
						bezierPoint.rotation = flattenedRotation;
						spline.bezierPoints[i] = bezierPoint;
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
				var worldPoint = matrix.MultiplyPoint3x4(spline.bezierPoints[i].position);
				float handleSize = HandleUtility.GetHandleSize(worldPoint) * bezierPointHandleSize;
				Handles.SphereHandleCap(-1, worldPoint, Quaternion.identity, handleSize, EventType.Repaint);
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
			Vector3 midPointScreenPoint = HandleUtility.WorldToGUIPointWithDepth(matrix.MultiplyPoint3x4(spline.bounds.center));
			if(midPointScreenPoint.z < 0) return;
			Handles.BeginGUI();
			var halfSize = new Vector2(140, 40) * 0.5f;
			GUILayout.BeginArea(Rect.MinMaxRect(midPointScreenPoint.x-halfSize.x, midPointScreenPoint.y-halfSize.y, midPointScreenPoint.x+halfSize.x, midPointScreenPoint.y+halfSize.y));
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
			if(!creationMode && !deletionMode) {
				for(int i = 0; i < spline.bezierPoints.Length; i++) {
					var point = spline.bezierPoints[i];
					DrawBezierPointHandle(spline, i, i != 0, i != spline.bezierPoints.Length-1, ref point, ref changed);
					spline.bezierPoints[i] = point;
				}
			}

			if(GUIUtility.hotControl == 0) {
				DrawEditorPoint(spline, ref changed);
			}
		}

		void DrawBezierPointHandle (Spline spline, int bezierPointIndex, bool drawStartHandle, bool drawEndHandle, ref SplineBezierPoint bezierPoint, ref bool changed) {
			var bezierPointPosition = matrix.MultiplyPoint3x4(bezierPoint.position);
			
			// var bezierPointRotation = matrix.rotation * bezierPoint.rotation;
			// This approach correctly displays rotation when using a matrix with non-uniform scale
			var bezierPointRotation = Quaternion.LookRotation(matrix.MultiplyVector(bezierPoint.rotation * Vector3.forward), matrix.MultiplyVector(bezierPoint.rotation * Vector3.up));
			
			if(Tools.current == Tool.Move || Tools.current == Tool.Transform) {
				var newBezierPointPosition = Handles.PositionHandle(bezierPointPosition, Tools.pivotRotation == PivotRotation.Local ? bezierPointRotation : Quaternion.identity);
				if(bezierPointPosition != newBezierPointPosition) {
					var previousPosition = bezierPoint.position;
					bezierPoint.position = matrix.inverse.MultiplyPoint3x4(newBezierPointPosition);
					if(in2DMode) bezierPoint.position = GetFlattened2DPosition(bezierPoint.position);
					if(OnChangeBezierPointPosition != null) OnChangeBezierPointPosition(spline, bezierPointIndex, ref bezierPoint, previousPosition);
					changed = true;
				}
			}
			if(Tools.current == Tool.Rotate || Tools.current == Tool.Transform) {
				var newBezierPointRotation = Handles.RotationHandle(bezierPointRotation, bezierPointPosition);
				if(bezierPointRotation != newBezierPointRotation) {
					// bezierPoint.rotation = matrix.inverse.rotation * newBezierPointRotation;
					bezierPoint.rotation = Quaternion.LookRotation(matrix.inverse.MultiplyVector(newBezierPointRotation * Vector3.forward), matrix.inverse.MultiplyVector(newBezierPointRotation * Vector3.up));
					if(in2DMode) bezierPoint.rotation = GetFlattened2DRotation(bezierPoint.rotation);
					changed = true;
				}
			}
			if(drawStartHandle) DrawControlPointHandle(ref bezierPoint, ref bezierPoint.inControlPoint, ref changed);
			if(drawEndHandle) DrawControlPointHandle(ref bezierPoint, ref bezierPoint.outControlPoint, ref changed);
		}

		private void DrawControlPointHandle (ref SplineBezierPoint bezierPoint, ref SplineBezierControlPoint controlPoint, ref bool changed) {
			var controlPointPosition = matrix.MultiplyPoint3x4(controlPoint.GetPosition(bezierPoint));
			
			var localControlPointRotation = controlPoint.GetRotation(bezierPoint);
			var controlPointRotation = Quaternion.LookRotation(matrix.MultiplyVector(localControlPointRotation * Vector3.forward), matrix.MultiplyVector(localControlPointRotation * Vector3.up));
			
			var bezierPointPosition = matrix.MultiplyPoint3x4(bezierPoint.position);
			Handles.DrawDottedLine(bezierPointPosition, controlPointPosition, 1);

			var newControlPointPosition = DrawEditableControlPointHandle(controlPointPosition, controlPointRotation);
			if(controlPointPosition != newControlPointPosition) {
				// This doesn't work very well when using a matrix with non-uniform scale. This rotation approach seems to help a little bit it's ultimately a bit broken.
				controlPointRotation = matrix.rotation * controlPoint.GetRotation(bezierPoint);
				
				controlPoint.distance = Vector3.Dot(matrix.inverse.MultiplyPoint3x4(newControlPointPosition) - matrix.inverse.MultiplyPoint3x4(bezierPointPosition), controlPointRotation * Vector3.forward);
				changed = true;
			}
		}

		private Vector3 DrawEditableControlPointHandle (Vector3 controlPointPosition, Quaternion controlPointRotation) {
			float handleSize = HandleUtility.GetHandleSize(controlPointPosition) * controlPointHandleSize;
			return Handles.FreeMoveHandle(controlPointPosition, handleSize, Vector3.zero, ((int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType) => {
				Handles.ConeHandleCap(controlID, position, controlPointRotation, handleSize, eventType);
			}));
		}

		
		void DrawEditorPoint (Spline spline, ref bool changed) {
			var samplePoint = GetSamplePoint(spline, Event.current.mousePosition);
			
			var bestCurve = spline.curves[samplePoint.bestCurveIndex];
			var startPoint = bestCurve._points[samplePoint.bestCurvePointIndex-1];
			var endPoint = bestCurve._points[samplePoint.bestCurvePointIndex];
			var point = Vector3.LerpUnclamped(startPoint, endPoint, samplePoint.bestNormalizedDistanceOnLine);
			var worldPoint = matrix.MultiplyPoint3x4(point);
			float handleSize = HandleUtility.GetHandleSize(worldPoint) * cursorPointHandleSize;
			
			Color cachedHandlesColor = Handles.color;
			if(creationMode) Handles.color = Color.blue;
			else if(deletionMode) Handles.color = Color.red;
			Handles.SphereHandleCap(-1, worldPoint, Quaternion.identity, handleSize, EventType.Repaint);
			Handles.color = cachedHandlesColor;

			if(creationMode && Event.current.button == 0 && Event.current.type == EventType.MouseDown) {
				var arcLength = spline.EstimateArcLengthAlongCurve(point);
				var curve = spline.GetCurveAtArcLength(arcLength);
				var bezierPoints = spline.bezierPoints.ToList();
				var rotation = spline.GetRotationAtArcLength(arcLength);
				
				var newPoint = new SplineBezierPoint(point, rotation, curve.length * 0.15f, curve.length * 0.15f);
				bezierPoints.Insert(samplePoint.bestCurveIndex+1, newPoint);
				spline.bezierPoints = bezierPoints.ToArray();
				changed = true;
			} else if(deletionMode && Event.current.button == 0 && Event.current.type == EventType.MouseDown && spline.bezierPoints.Length > 2) {
				var bezierPoints = spline.bezierPoints.ToList();
				bezierPoints.RemoveAt(samplePoint.bestBezierPointIndex);
				spline.bezierPoints = bezierPoints.ToArray();
				changed = true;
			}
		}


		Vector3 GetFlattened2DPosition (Vector3 position) {
			return localPlaneFor2DMode.ClosestPointOnPlane(position);
		}
		Quaternion GetFlattened2DRotation (Quaternion rotation) {
			return Quaternion.LookRotation(Vector3.ProjectOnPlane(rotation * Vector3.forward, localPlaneFor2DMode.normal), localPlaneFor2DMode.normal);
		}

		struct SamplePoint {
			public int bestCurveIndex;
			public int bestCurvePointIndex;
			public int bestBezierPointIndex;
			public float bestNormalizedDistanceOnLine;
			public SamplePoint (int bestCurveIndex, int bestCurvePointIndex, int bestBezierPointIndex, float bestNormalizedDistanceOnLine) {
				this.bestCurveIndex = bestCurveIndex;
				this.bestCurvePointIndex = bestCurvePointIndex;
				this.bestBezierPointIndex = bestBezierPointIndex;
				this.bestNormalizedDistanceOnLine = bestNormalizedDistanceOnLine;
			}
		}

		SamplePoint GetSamplePoint (Spline spline, Vector2 mousePosition) {
			int bestCurveIndex = -1;
			int bestCurvePointIndex = -1;
			float bestNormalizedDistanceOnLine = -1;
			{
				float bestSqrDistance = Mathf.Infinity;
				for (var i = 0; i < spline.curves.Length; i++) {
					var curve = spline.curves[i];
					var lastPoint = HandleUtility.WorldToGUIPoint(matrix.MultiplyPoint3x4(curve._points[0]));
					for (var j = 1; j < curve._points.Length; j++) {
						var nextPoint = HandleUtility.WorldToGUIPoint(matrix.MultiplyPoint3x4(curve._points[j]));
						var normalizedDistanceOnLine = GetNormalizedDistanceOnLine(lastPoint, nextPoint, mousePosition);
						var pointOnLine = Vector2.LerpUnclamped(lastPoint, nextPoint, normalizedDistanceOnLine);
						var sqrDistanceFromLine = (mousePosition - pointOnLine).sqrMagnitude;
						if(sqrDistanceFromLine < bestSqrDistance) {
							bestSqrDistance = sqrDistanceFromLine;
							bestCurveIndex = i;
							bestCurvePointIndex = j;
							bestNormalizedDistanceOnLine = normalizedDistanceOnLine;
						}
						lastPoint = nextPoint;
					}			
				}
			}

			int bestBezierPointIndex = -1;
			{
				float bestBezierPointSqrDistance = Mathf.Infinity;
				for (var i = 0; i < spline.bezierPoints.Length; i++) {
					var bezierPoint = spline.bezierPoints[i];
					var guiBezierPoint = HandleUtility.WorldToGUIPoint(matrix.MultiplyPoint3x4(bezierPoint.position));
					var sqrDistanceFromBezierPoint = (mousePosition - guiBezierPoint).sqrMagnitude;
					if(sqrDistanceFromBezierPoint < bestBezierPointSqrDistance) {
						bestBezierPointSqrDistance = sqrDistanceFromBezierPoint;
						bestBezierPointIndex = i;
					}
				}
			}
			return new SamplePoint(bestCurveIndex, bestCurvePointIndex, bestBezierPointIndex, bestNormalizedDistanceOnLine);
		}

		static float GetNormalizedDistanceOnLine(Vector2 start, Vector2 end, Vector2 p, bool clamped = true) {
			float sqrLength = (start.x-end.x) * (start.x-end.x) + (start.y-end.y) * (start.y-end.y);
			if (sqrLength == 0f) return 0;
			// Divide by length squared so that we can save on normalising (end-start), since
			// we're effectively dividing by the length an extra time.
			float n = Vector2.Dot(p - start, end - start) / sqrLength;
			if(!clamped) return n;
			return Mathf.Clamp01(n);
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