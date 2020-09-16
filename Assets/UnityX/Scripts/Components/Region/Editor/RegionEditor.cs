using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Region)), CanEditMultipleObjects]
public class RegionEditor : BaseEditor<Region> {
	PolygonEditorHandles polygonEditor;

	public override void OnEnable() {
		base.OnEnable();
		Undo.undoRedoPerformed += HandleUndoRedoCallback;
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
        polygonEditor.Destroy();
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
			Vector3 newPos = Handles.FreeMoveHandle(worldPosition + data.worldNormal * distance, rotation, handleSize, Vector3.zero, ((int controlID, Vector3 handlePosition, Quaternion handleRotation, float size, EventType eventType) => {
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
		
		if(region.selectedFillColor.a < 0.01f) return;
		GizmosX.BeginMatrix(region.matrix);
		GizmosX.BeginColor(region.selectedFillColor);
		if(region.in2DMode) {
			GizmosX.DrawPolygon(region.polygon.vertices, true);
			GizmosX.DrawWirePolygon(region.polygon.vertices);
		} else {
			GizmosX.DrawExtrudedPolygon(region.polygon.vertices, region.height);
			GizmosX.DrawExtrudedWirePolygon(region.polygon.vertices, region.height);
		}
		GizmosX.EndColor();
		GizmosX.EndMatrix();
	}
}