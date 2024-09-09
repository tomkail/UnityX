using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI {
	[CustomEditor(typeof(SwipeView)), CanEditMultipleObjects]
	public class SwipeViewEditor : Editor {
		static int targetPage;
		
		public override void OnInspectorGUI() {
			serializedObject.Update();
			
			var interactable = serializedObject.FindProperty("interactable");
			EditorGUILayout.PropertyField(interactable);
			
			EditorGUILayout.Space();
			
			var viewport = serializedObject.FindProperty("_viewport");
			EditorGUILayout.PropertyField(viewport, new GUIContent("Viewport", "The rectTransform that will be used to determine the visible area of the content.\nIf not set, the rectTransform attached to the gameobject is used."));

			EditorGUILayout.BeginHorizontal();
			var content = serializedObject.FindProperty("content");
			EditorGUILayout.PropertyField(content, new GUIContent("Content", "The rectTransform that will be moved in order to frame the current page."));
			if(content.objectReferenceValue == null) {
				EditorGUILayout.LabelField(new GUIContent(EditorGUIUtility.IconContent("Warning@2x").image, "This field is required"), GUILayout.Width(20));
			}
			EditorGUILayout.EndHorizontal();
			
			var axis = serializedObject.FindProperty("axis");
			EditorGUILayout.PropertyField(axis);
			
			var pivot = serializedObject.FindProperty("pivot");
			EditorGUILayout.PropertyField(pivot, new GUIContent("Pivot", "Determines where we frame the targets, and where we measure distance on the pages to the framing point from. With a pivot of 0, the left/bottom edge of the current page will snap to the left/bottom edge of the viewport."));
			
			EditorGUILayout.Space();
			
			var swipeMode = serializedObject.FindProperty("swipeMode");
			EditorGUILayout.PropertyField(swipeMode);
			
			if(swipeMode.enumValueIndex == (int)SwipeView.SwipeMode.AdjacentThreshold) {
				EditorGUI.indentLevel++;
				var adjacentThresholdInInches = serializedObject.FindProperty("adjacentThresholdInInches");
				EditorGUILayout.PropertyField(adjacentThresholdInInches, new GUIContent("Adjacent threshold (inches)", "Picks the page adjacent to targetPage in the direction of the swipe only if swipe magnitude exceeds this value."));
				EditorGUI.indentLevel--;
			}
			
			EditorGUILayout.Space();
			
			var clampType = serializedObject.FindProperty("clampType");
			EditorGUILayout.PropertyField(clampType);
			
			if(clampType.enumValueIndex == (int)SwipeView.ClampType.Elastic) {
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				var rubberbanding = serializedObject.FindProperty("rubberbandingCoefficient");
				EditorGUILayout.PropertyField(rubberbanding, new GUIContent("Rubberbanding Coefficient", "Higher values result a looser rubberbanding effect"));
				DrawPresets(rubberbanding, new List<KeyValuePair<GUIContent, float>>() {
					new(new GUIContent("Tight"), 0.125f),
					new(new GUIContent("Default"), 0.3f),
					new(new GUIContent("Loose"), 0.6f),
				});
				rubberbanding.floatValue = Mathf.Max(rubberbanding.floatValue, 0);
				
				EditorGUI.BeginChangeCheck();
				
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
			}
			
			EditorGUILayout.Space();
			
			var pages = serializedObject.FindProperty("pages");
			EditorGUILayout.PropertyField(pages);
			
			var pagePivots = serializedObject.FindProperty("pagePivots");
			EditorGUILayout.PropertyField(pagePivots);

			EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup(true);
			
			
			EditorGUI.EndDisabledGroup();
			
			// if(GUILayout.Button("Go to page")) GoToPageImmediate
			serializedObject.ApplyModifiedProperties();
		}

		public override bool HasPreviewGUI() => true;
		public override void OnPreviewGUI(Rect r, GUIStyle background) {
			base.OnPreviewGUI(r, background);
			var swipeView = target as SwipeView;

			GUI.BeginScrollView(r, new Vector2(0, 0), new Rect(0,0,r.width,r.height));

			var y = 0f;
			EditorGUI.LabelField(new Rect(r.x, y, r.width, EditorGUIUtility.singleLineHeight), $"Target page index: {swipeView.targetPageIndex}");
			y += EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(new Rect(r.x, y, r.width, EditorGUIUtility.singleLineHeight), $"Closest page index: {swipeView.closestPageIndex}");
			y += EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(new Rect(r.x, y, r.width, EditorGUIUtility.singleLineHeight), $"Interpolated current page index: {swipeView.GetInterpolatedCurrentPageIndex()}");
			y += EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(new Rect(r.x, y, r.width, EditorGUIUtility.singleLineHeight), $"Normalized progress: {swipeView.GetNormalizedProgress()}");
			y += EditorGUIUtility.singleLineHeight;
			
			y += EditorGUIUtility.singleLineHeight;

			float buttonWidth = 80;
			foreach(var page in swipeView.pages) {
				var x = r.x;
				var itemRect = new Rect(x, y, r.width, EditorGUIUtility.singleLineHeight);
				EditorGUI.ObjectField(new Rect(x, itemRect.y, 260, itemRect.height), page, typeof(RectTransform), true);
				x += 260;
				if (GUI.Button(new Rect(x, itemRect.y, buttonWidth, itemRect.height), "Immediate")) {
					swipeView.GoToPageImmediate(page);
				}
				x += buttonWidth;
				if (GUI.Button(new Rect(x, itemRect.y, buttonWidth, itemRect.height), "Smooth")) {
					swipeView.GoToPageSmooth(page);
				}
				x += buttonWidth;
				y += EditorGUIUtility.singleLineHeight;
			}
			// var labelSB = new StringBuilder();
			// labelSB.AppendLine($"Target page index: {swipeView.targetPageIndex}");
			// labelSB.AppendLine($"Closest page index: {swipeView.closestPageIndex}");
			// labelSB.AppendLine($"Interpolated current page index: {swipeView.GetInterpolatedCurrentPageIndex()}");
			// labelSB.AppendLine($"Normalized progress: {swipeView.GetNormalizedProgress()}");
			//
			// EditorGUI.LabelField(r, labelSB.ToString());
			// if (GUI.Button(r, labelSB.ToString())) {
			// 	swipeView.GoToPageImmediate(0);
			// }
			
			GUI.EndScrollView();
		}
		
		


		static void DrawPresets(SerializedProperty property, List<KeyValuePair<GUIContent, float>> presets) {
			for (var index = 0; index < presets.Count; index++) {
				var preset = presets[index];
				DrawButtonGroupPreset(index, presets.Count, preset, property);
			}
		}
		
		static void DrawButtonGroupPreset(int index, int numButtons, KeyValuePair<GUIContent, float> preset, SerializedProperty property) {
			EditorGUI.BeginChangeCheck();
			bool pressed = property.floatValue == preset.Value;
			pressed = GUILayout.Toggle(pressed, preset.Key, GetButtonGroupGUIStyle(index, numButtons));
			if (EditorGUI.EndChangeCheck()) {
				if (pressed) property.floatValue = preset.Value;
			}
		}
		
		static GUIStyle GetButtonGroupGUIStyle (int index, int numButtons) {
			var style = EditorStyles.miniButton;
			if(numButtons > 1) {
				if(index == 0) style = EditorStyles.miniButtonLeft;
				else if(index == numButtons-1) style = EditorStyles.miniButtonRight;
				else style = EditorStyles.miniButtonMid;
			}
			return style;
		}
	}
}