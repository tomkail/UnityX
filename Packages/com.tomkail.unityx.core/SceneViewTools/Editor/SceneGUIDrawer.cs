using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityX.SceneViewTools.Editor {
	public class SceneGUIDrawer {
		static readonly Dictionary<object, System.Action> drawActions = new Dictionary<object, System.Action>();
		static readonly Dictionary<object, System.Action> drawOnceActions = new Dictionary<object, System.Action>();

		public static void DrawOnce (object obj, System.Action drawAction) {
			drawOnceActions[obj] = drawAction;
		}

		public static void StartDrawing (object obj, System.Action drawAction) {
			drawActions[obj] = drawAction;
		}

		public static void StopDrawing (object obj) {
			if(drawActions.ContainsKey(obj)) drawActions.Remove(obj);
		}

		[InitializeOnLoadMethod]
		static void SubscribeEditorEvents () {
			SceneView.duringSceneGui -= OnSceneGUI;
			SceneView.duringSceneGui += OnSceneGUI;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
			AssemblyReloadEvents.beforeAssemblyReload += UnsubscribeEditorEvents;
		}

		// Editor events outlive script assemblies, so unsubscribe before a code reload or the old handler keeps firing alongside the new one.
		static void UnsubscribeEditorEvents () {
			SceneView.duringSceneGui -= OnSceneGUI;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
		}

		// Registered actions usually close over scene objects, which are destroyed when play mode starts or ends.
		static void OnPlayModeStateChanged (PlayModeStateChange state) {
			if(state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode) {
				drawActions.Clear();
				drawOnceActions.Clear();
			}
		}

		static void OnSceneGUI (SceneView sceneView) {
			foreach(var drawAction in drawActions) {
				drawAction.Value();
			}
			foreach(var drawAction in drawOnceActions) {
				drawAction.Value();
			}
			// Keep once-actions through the Layout pass so they reach Repaint, then drop them.
			if(Event.current.type == EventType.Repaint) drawOnceActions.Clear();
	    }
	}
}
