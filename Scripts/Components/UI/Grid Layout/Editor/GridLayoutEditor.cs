using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GridLayout)), CanEditMultipleObjects]
public class GridLayoutEditor : Editor {
	protected List<GridLayout> datas;
	static Vector3[] rectPoints;
	
	public virtual void OnEnable() {
		datas = new List<GridLayout>();
		foreach(Object t in targets) {
			if( t == null ) continue;
			Debug.Assert(t as GridLayout != null, "Cannot cast "+t + " to "+typeof(GridLayout));
			datas.Add((GridLayout) t); 
		}
	}
	
	public override void OnInspectorGUI () {
		serializedObject.Update();
		// base.OnInspectorGUI();
		var xAxis = serializedObject.FindProperty("_xAxis");
		var yAxis = serializedObject.FindProperty("_yAxis");
		DrawAxis(xAxis);
		DrawAxis(yAxis);
		serializedObject.ApplyModifiedProperties();
	}

	void DrawAxis (SerializedProperty prop) {
		prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, prop.displayName, true);
		if(prop.isExpanded) {
			EditorGUI.indentLevel++;
			
			var val = prop.GetBaseProperty<GridLayout.GridLayoutAxisSettings>();
			
			var sizeMode = prop.FindPropertyRelative("_sizeMode");
			var fillMode = prop.FindPropertyRelative("_fillMode");
			var itemSize = prop.FindPropertyRelative("_itemSize");
			var cellCount = prop.FindPropertyRelative("_cellCount");
			var aspectRatio = prop.FindPropertyRelative("_aspectRatio");
			var spacing = prop.FindPropertyRelative("_spacing");
			var offset = prop.FindPropertyRelative("_offset");
			var flip = prop.FindPropertyRelative("_flip");
			
			EditorGUILayout.PropertyField(sizeMode);
			if(sizeMode.enumValueIndex == (int)GridLayout.CellSizeMode.Defined) {
				EditorGUILayout.PropertyField(itemSize);
			} else if(sizeMode.enumValueIndex == (int)GridLayout.CellSizeMode.FillContainer) {
				// EditorGUILayout.PropertyField(itemSize);
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.FloatField("Cell Size", val.GetItemSize());
				EditorGUI.EndDisabledGroup();
			} else if(sizeMode.enumValueIndex == (int)GridLayout.CellSizeMode.AspectRatio) {
				EditorGUILayout.PropertyField(aspectRatio);
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.FloatField("Cell Size", val.GetItemSize());
				EditorGUI.EndDisabledGroup();
			}
			
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(fillMode);
			if(fillMode.enumValueIndex == (int)GridLayout.CellCountMode.Defined) {
				EditorGUILayout.PropertyField(cellCount);
			} else if(fillMode.enumValueIndex == (int)GridLayout.CellCountMode.FitContainer) {
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
			if(GUILayout.Button("Apply")) {
				foreach(var data in datas) {
					var axis = val.isXAxis ? data.xAxis : data.yAxis;
					Undo.RecordObject(axis.gridLayout.rectTransform, "Apply");
					axis.ApplySizeToRectTransform();
				}
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
			
// ReflectionX.Get
			// CellCountMode _fillMode = CellCountMode.Defined;
			// Calculation _axisMode = Calculation.CellCount;
			// [SerializeField]
			// float _itemSize = 100;
			// [SerializeField]
			// int _cellCount = 3;
			// [SerializeField]
			// float _aspectRatio = 1;
			// [SerializeField]
			// float _spacing;
			EditorGUI.indentLevel--;
		}
	}

	void OnSceneGUI() {
		var col = Handles.color;
		foreach(var data in datas) {
			for(int y = 0; y < Mathf.Min(50, data.yAxis.GetCellCount()); y++) {
				for(int x = 0; x < Mathf.Min(50, data.xAxis.GetCellCount()); x++) {
					var rect = data.GetWorldRectForGridCoord(new Vector2Int(x, y));
					if(rectPoints == null) rectPoints = new Vector3[4];
					rectPoints[0] = rect.TopLeft();
					rectPoints[1] = rect.TopRight();
					rectPoints[2] = rect.BottomRight();
					rectPoints[3] = rect.BottomLeft();
					Handles.color = new Color(1,1,1,0.8f);
					Handles.DrawDottedLine(rectPoints[0],rectPoints[1], 3);
					Handles.DrawDottedLine(rectPoints[2],rectPoints[3], 3);
					Handles.DrawDottedLine(rectPoints[1],rectPoints[2], 3);
					Handles.DrawDottedLine(rectPoints[0],rectPoints[3], 3);
					Handles.color = new Color(0,0,0,0.8f);
					Handles.DrawDottedLine(rectPoints[0],rectPoints[1], 3);
					Handles.DrawDottedLine(rectPoints[2],rectPoints[3], 3);
					Handles.DrawDottedLine(rectPoints[1],rectPoints[2], 3);
					Handles.DrawDottedLine(rectPoints[0],rectPoints[3], 3);
				}
			}
		}
		Handles.color = col;
	}
}