using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UnityX.UI.Editor {
	[CustomEditor(typeof(GridLayoutElement)), CanEditMultipleObjects]
	public class GridLayoutEditor : UnityEditor.Editor {
		protected List<GridLayoutElement> datas;
		static Vector3[] rectPoints;

		public virtual void OnEnable() {
			datas = new List<GridLayoutElement>();
			foreach (Object t in targets) {
				if (t == null) continue;
				Debug.Assert(t as GridLayoutElement != null, "Cannot cast " + t + " to " + typeof(GridLayoutElement));
				datas.Add((GridLayoutElement) t);
			}
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			// base.OnInspectorGUI();
			var padding = serializedObject.FindProperty("padding");
			EditorGUILayout.PropertyField(padding);

			var xAxis = serializedObject.FindProperty("_xAxis");
			var yAxis = serializedObject.FindProperty("_yAxis");
			DrawAxis(xAxis);
			DrawAxis(yAxis);
			serializedObject.ApplyModifiedProperties();
		}

		void DrawAxis(SerializedProperty prop) {
			prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, prop.displayName, true);
			if (prop.isExpanded) {
				EditorGUI.indentLevel++;

				var val = (GridLayoutElement.GridLayoutAxisSettings)prop.boxedValue;

				var sizeMode = prop.FindPropertyRelative("_sizeMode");
				var fillMode = prop.FindPropertyRelative("_fillMode");
				var itemSize = prop.FindPropertyRelative("_itemSize");
				var cellCount = prop.FindPropertyRelative("_cellCount");
				var aspectRatio = prop.FindPropertyRelative("_aspectRatio");
				var spacing = prop.FindPropertyRelative("_spacing");
				var offset = prop.FindPropertyRelative("_offset");
				var flip = prop.FindPropertyRelative("_flip");

				EditorGUILayout.PropertyField(sizeMode);
				if (sizeMode.enumValueIndex == (int) GridLayoutElement.CellSizeMode.Defined) {
					EditorGUILayout.PropertyField(itemSize);
				} else if (sizeMode.enumValueIndex == (int) GridLayoutElement.CellSizeMode.FillContainer) {
					// EditorGUILayout.PropertyField(itemSize);
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.FloatField("Cell Size", val.GetItemSize());
					EditorGUI.EndDisabledGroup();
				} else if (sizeMode.enumValueIndex == (int) GridLayoutElement.CellSizeMode.AspectRatio) {
					EditorGUILayout.PropertyField(aspectRatio);
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.FloatField("Cell Size", val.GetItemSize());
					EditorGUI.EndDisabledGroup();
				}

				EditorGUILayout.Space();

				EditorGUILayout.PropertyField(fillMode);
				if (fillMode.enumValueIndex == (int) GridLayoutElement.CellCountMode.Defined) {
					EditorGUILayout.PropertyField(cellCount);
				} else if (fillMode.enumValueIndex == (int) GridLayoutElement.CellCountMode.FitContainer) {
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.FloatField("Cell Count", val.GetCellCount());
					EditorGUI.EndDisabledGroup();
				}

				EditorGUILayout.Space();

				EditorGUILayout.PropertyField(spacing);
				EditorGUILayout.PropertyField(offset);

				EditorGUILayout.Space();

				EditorGUILayout.PropertyField(flip);

				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.FloatField("Total Size", val.GetTotalSize());
				EditorGUI.EndDisabledGroup();
				EditorGUI.BeginDisabledGroup((val.isXAxis ? val.gridLayout.rectTransform.rect.width : val.gridLayout.rectTransform.rect.height) == val.GetTotalSize());
				if (GUILayout.Button("Apply")) {
					foreach (var data in datas) {
						var axis = val.isXAxis ? data.xAxis : data.yAxis;
						Undo.RecordObject(axis.gridLayout.rectTransform, "Apply");
						axis.ApplySizeToRectTransform();
					}
				}

				EditorGUI.EndDisabledGroup();
				GUILayout.EndHorizontal();
				
				EditorGUI.indentLevel--;
			}
		}

		void OnSceneGUI() {
			var col = Handles.color;
			foreach (var data in datas) {
				for (int y = 0; y < Mathf.Min(50, data.yAxis.GetCellCount()); y++) {
					for (int x = 0; x < Mathf.Min(50, data.xAxis.GetCellCount()); x++) {
						var rect = data.GetWorldRectForGridCoord(new Vector2Int(x, y));
						rectPoints ??= new Vector3[4];
						rectPoints[0] = new Vector2(rect.x, rect.y + rect.height);
						rectPoints[1] = rect.max;
						rectPoints[2] = new Vector2(rect.x + rect.width, rect.y);
						rectPoints[3] = rect.min;
						Handles.color = new Color(1, 1, 1, 1f);
						Handles.DrawDottedLine(rectPoints[0], rectPoints[1], 3);
						Handles.DrawDottedLine(rectPoints[2], rectPoints[3], 3);
						Handles.DrawDottedLine(rectPoints[1], rectPoints[2], 3);
						Handles.DrawDottedLine(rectPoints[0], rectPoints[3], 3);
						Handles.Label(rect.center, $"({x},{y})", EditorStyles.miniLabel);
					}
				}
			}

			Handles.color = col;
		}

		public override string GetInfoString() => "Grid Layout Preview";

		public override bool HasPreviewGUI() {
			return true;
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background) {
			if (Event.current.type == EventType.Repaint) {
				// The rect that the grid should fit in. Extra space is left on the left and bottom for the grid axis/labels.
				var containerRect = new Rect(r.x + 30, r.y + 10, r.width - 40, r.height - 40);
				var data = datas[0];
				var gridSize = data.GetTotalSize();
				var scaleFactor = Mathf.Min(containerRect.width / gridSize.x, containerRect.height / gridSize.y);

				// The rect that the grid is actually drawn in. This is a scaled, centered version of the container.
				var gridRect = new Rect(containerRect.position.x, containerRect.position.y, gridSize.x * scaleFactor, gridSize.y * scaleFactor);
				gridRect.x += Mathf.Abs(gridRect.width - containerRect.width) * 0.5f;
				gridRect.y += Mathf.Abs(gridRect.height - containerRect.height) * 0.5f;

				// Draw axis
				DrawLine(new Vector2(gridRect.x, gridRect.y + gridRect.height) + Vector2.up * 10, gridRect.max + Vector2.up * 10, 1, Color.white);
				DrawLine(new Vector2(gridRect.x, gridRect.y + gridRect.height) - Vector2.right * 10, gridRect.min - Vector2.right * 10, 1, Color.white);

				// Draw labels
				DrawText(Vector2.Lerp(new Vector2(gridRect.x, gridRect.y + gridRect.height) + Vector2.up * 16, gridRect.max + Vector2.up * 16, 0.5f), new Vector2(0.5f, 0f), 0, new GUIContent(gridSize.x.ToString()));
				DrawText(Vector2.Lerp(new Vector2(gridRect.x, gridRect.y + gridRect.height) - Vector2.right * 16, gridRect.min - Vector2.right * 16, 0.5f), new Vector2(0.5f, 1f), -90, new GUIContent(gridSize.y.ToString()));

				// Draw grid rect
				EditorGUI.DrawRect(gridRect, new Color(0, 0, 0, 0.2f));
				for (int y = 0; y < Mathf.Min(50, data.yAxis.GetCellCount()); y++) {
					for (int x = 0; x < Mathf.Min(50, data.xAxis.GetCellCount()); x++) {
						var gridCellRect = data.GetRectForGridCoord(new Vector2Int(x, y));
						// Flip it because GUI is drawn upside down compared to world space (y top)
						gridCellRect.center = new Vector2(gridCellRect.center.x, gridSize.y - gridCellRect.center.y);
						gridCellRect = new Rect(gridRect.position + gridCellRect.position * scaleFactor, gridCellRect.size * scaleFactor);
						// gridCellRect.center = new Vector2(gridCellRect.center.x, r.yMax-gridCellRect.center.y);
						EditorGUI.DrawRect(gridCellRect, new Color(0f, 0f, 0f, 0.2f));
						DrawRect(gridCellRect, 1, Color.white);
						GUI.Label(gridCellRect, GridLayoutElement.GridCoordToArrayIndex(new Vector2Int(x, y), data.gridSize.x).ToString(), EditorStyles.centeredGreyMiniLabel);
					}
				}
			}

			void DrawRect(Rect rect, float width, Color color) {
				DrawLine(rect.min, new Vector2(rect.x + rect.width, rect.y), width, color);
				DrawLine(new Vector2(rect.x + rect.width, rect.y), rect.max, width, color);
				DrawLine(rect.max, new Vector2(rect.x, rect.y + rect.height), width, color);
				DrawLine(new Vector2(rect.x, rect.y + rect.height), rect.min, width, color);
			}

			void DrawLine(Vector2 pointA, Vector2 pointB, float width, Color color) {
				Matrix4x4 matrixBackup = GUI.matrix;
				float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * 180f / Mathf.PI;

				GUIUtility.RotateAroundPivot(angle, pointA);
				EditorGUI.DrawRect(new Rect(pointA.x, pointA.y - width * 0.5f, Vector2.Distance(pointA, pointB), width), color);
				GUI.matrix = matrixBackup;
			}

			void DrawText(Vector2 pointA, Vector2 pivot, float angle, GUIContent content) {
				Matrix4x4 matrixBackup = GUI.matrix;
				var size = EditorStyles.centeredGreyMiniLabel.CalcSize(content);
				var rect = new Rect(pointA.x, pointA.y, size.x, size.y);
				rect.position -= size * pivot;
				// EditorGUI.DrawRect(new Rect (pointA.x, pointA.y-width*0.5f, Vector2.Distance(pointA, pointB), width), color);
				GUIUtility.RotateAroundPivot(angle, pointA);
				GUI.Label(rect, content, EditorStyles.centeredGreyMiniLabel);
				GUI.matrix = matrixBackup;
			}
		}

	/// <summary>
	/// Gets the value of a serialized property via SerializedProperty.boxedValue.
	/// </summary>
	/// <returns>The property value, or default(T) on type mismatch.</returns>
	/// <param name="prop">Property.</param>
	/// <typeparam name="T">The value type to read.</typeparam>
	public static T GetBaseProperty<T>(SerializedProperty prop) {
		// SerializedProperty.boxedValue (Unity 2022.1+) is the built-in for this; returns default(T) on type mismatch.
		return prop.boxedValue is T t ? t : default;
	}
	}
}