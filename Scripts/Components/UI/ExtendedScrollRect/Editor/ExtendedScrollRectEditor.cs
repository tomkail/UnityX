using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI {
	[CustomEditor(typeof(ExtendedScrollRect), true)]
	[CanEditMultipleObjects]
	public class ExtendedScrollRectEditor : ScrollRectEditor {
		SerializedProperty routeUnusedAxisDragEventsToParentProp;
		SerializedProperty excessDragRerouteEdgesProp;
		SerializedProperty routeScrollEventsToParentProp;
		
		SerializedProperty onScrollProperty;
		SerializedProperty onBeginDragProperty;
		SerializedProperty onEndDragProperty;
		SerializedProperty onDragProperty;
		
		SerializedProperty horizontalProperty;
		SerializedProperty verticalProperty;
				
		protected override void OnEnable() {
			base.OnEnable();
			routeUnusedAxisDragEventsToParentProp = serializedObject.FindProperty("routeUnusedAxisDragEventsToParent");
			excessDragRerouteEdgesProp = serializedObject.FindProperty("excessDragRerouteEdges");
			
			routeScrollEventsToParentProp = serializedObject.FindProperty("routeScrollEventsToParent");

			onScrollProperty = serializedObject.FindProperty("onScroll");
			onBeginDragProperty = serializedObject.FindProperty("onBeginDrag");
			onEndDragProperty = serializedObject.FindProperty("onEndDrag");
			onDragProperty = serializedObject.FindProperty("onDrag");
			
			horizontalProperty = serializedObject.FindProperty("m_Horizontal");
			verticalProperty = serializedObject.FindProperty("m_Vertical");
		}
		
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.Space();
			
			serializedObject.Update();

       		EditorGUILayout.PropertyField(routeUnusedAxisDragEventsToParentProp, new GUIContent("Reroute Unused Axis Drag Events", "When dragging in an axis that is not enabled, reroute the drag event to the parent if possible. This allows for nesting scroll views to work correctly."));
	        
	        // Excess drag rerouting edges
			var edges = new List<KeyValuePair<GUIContent, RectTransform.Edge>>();
			if (horizontalProperty.boolValue) {
				edges.Add(new(new GUIContent("Left"), RectTransform.Edge.Left));
				edges.Add(new(new GUIContent("Right"), RectTransform.Edge.Right));
			}
			if (verticalProperty.boolValue) {
				edges.Add(new(new GUIContent("Top"), RectTransform.Edge.Top));
				edges.Add(new(new GUIContent("Bottom"), RectTransform.Edge.Bottom));
			}
			if (edges.Count > 0) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(new GUIContent("Excess Drag Reroute Edges", "Selected edges will reroute any drags that exceed the bounds"), EditorStyles.boldLabel);
				for (var index = 0; index < edges.Count; index++) {
					var preset = edges[index];
					DrawButtonGroupPreset(index, edges.Count, preset, excessDragRerouteEdgesProp);
				}
				EditorGUILayout.EndHorizontal();
			}
			
       		EditorGUILayout.PropertyField(routeScrollEventsToParentProp, new GUIContent("Reroute Scroll Events", "Stop this scroll view from handling scroll events and route them to the parent instead."));
			EditorGUILayout.PropertyField(onScrollProperty);
			EditorGUILayout.PropertyField(onBeginDragProperty);
			EditorGUILayout.PropertyField(onEndDragProperty);
			EditorGUILayout.PropertyField(onDragProperty);
			serializedObject.ApplyModifiedProperties();
		}
		
		static void DrawButtonGroupPreset(int index, int numButtons, KeyValuePair<GUIContent, RectTransform.Edge> preset, SerializedProperty property) {
			EditorGUI.BeginChangeCheck();
			var edgeFlagValue = EnumToFlagValue((int)preset.Value+1);
			bool pressed = (property.intValue & edgeFlagValue) != 0;
			pressed = GUILayout.Toggle(pressed, preset.Key, GetButtonGroupGUIStyle(index, numButtons));
			if (EditorGUI.EndChangeCheck()) {
				if(pressed) property.intValue |= edgeFlagValue;
				else property.intValue &= ~edgeFlagValue;
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
		
		static int EnumToFlagValue (int enumValue) {
			return enumValue == 0 ? 0 : 1 << (enumValue-1);
		}
	}
}