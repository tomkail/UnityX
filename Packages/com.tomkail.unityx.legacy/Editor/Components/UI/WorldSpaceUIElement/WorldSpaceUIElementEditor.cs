using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(WorldSpaceUIElement)), CanEditMultipleObjects]
public class WorldSpaceUIElementEditor : BaseEditor<WorldSpaceUIElement> {
	public override void OnInspectorGUI () {
		if(!Application.isPlaying) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_updateInEditMode"));
		}

		bool anyUsingWorldSpaceCanvas = false;
		foreach(var data in datas) {
			if(data.rootCanvas != null && data.rootCanvas.renderMode == RenderMode.WorldSpace) {
				anyUsingWorldSpaceCanvas = true;
				break;
			}
		}
		if(anyUsingWorldSpaceCanvas) {
			EditorGUILayout.HelpBox("WorldSpaceUIElement root canvas is in WorldSpace mode, which is not currently supported (what SHOULD this mode do?)", MessageType.Warning);
		}

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_worldCamera"));

		EditorGUILayout.Separator();
		
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_target"));
		if(serializedObject.FindProperty("_target").objectReferenceValue == null) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("worldPosition"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("worldRotation"));
		}

		EditorGUILayout.Separator();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("updatePosition"));
		
		EditorGUILayout.PropertyField(serializedObject.FindProperty("updateRotation"));
		var updateRotation = serializedObject.FindProperty("updateRotation").enumValueIndex;
		if(updateRotation == (int)WorldSpaceUIElement.RotationMode.RotationZ) {
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("worldPointingVectorForZRotation"));
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.PropertyField(serializedObject.FindProperty("updateScale"), new GUIContent("Update Scale", "Scale based on target distance from the camera"));
		if(serializedObject.FindProperty("updateScale").boolValue) {
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("scaleMultiplier"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minScale"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maxScale"));
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.Separator();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("clampToScreen"));
		if(serializedObject.FindProperty("clampToScreen").boolValue) {
			EditorGUI.indentLevel++;
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("onScreen"));
			EditorGUI.EndDisabledGroup();
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.Separator();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("updateOcclusion"));
		if(serializedObject.FindProperty("updateOcclusion").boolValue) {
			EditorGUI.indentLevel++;
			
			EditorGUI.BeginChangeCheck();
			var tempMask = EditorGUILayout.MaskField(serializedObject.FindProperty("occlusionMask").intValue, InternalEditorUtility.layers);
			if(EditorGUI.EndChangeCheck()) {
				serializedObject.FindProperty("occlusionMask").intValue = tempMask;
			}
			
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("occluded"));
			EditorGUI.EndDisabledGroup();
			EditorGUI.indentLevel--;
		}

		if(EditorGUI.EndChangeCheck()) {
			foreach(var data in datas) {
				if(data is WorldSpaceUIElement)
					data.Refresh();
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
}