using UnityEngine;
using UnityEditor;
using System.Collections;

//namespace UnityX.Editor {
	public static class EditorWindowX {
		public static WindowType[] FindEditorWindows<WindowType>() where WindowType : EditorWindow {
			return Resources.FindObjectsOfTypeAll<WindowType>();
		}

		public static bool EditorWindowInitialized<WindowType>() where WindowType : EditorWindow {
			return !FindEditorWindows<WindowType>().IsNullOrEmpty();
		}

		static readonly System.Type gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
		static readonly System.Reflection.MethodInfo getMainPlayModeView = typeof(EditorWindow).Assembly.GetType("UnityEditor.PlayModeView")?.GetMethod("GetMainPlayModeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

		// Returns null if no Game view is open. The main play mode view may be the Device Simulator rather than a Game view,
		// so fall back to any open Game view.
		public static EditorWindow GetMainGameView() {
			if(gameViewType == null) return null;
			if(getMainPlayModeView?.Invoke(null, null) is EditorWindow main && gameViewType.IsInstanceOfType(main)) return main;
			var gameViews = Resources.FindObjectsOfTypeAll(gameViewType);
			return gameViews.Length > 0 ? (EditorWindow)gameViews[0] : null;
		}

		// This is a massive fudge. It needs System.Windows.Forms, which isn't part of Mono or something
		public static void SetGameViewToFullScreenForMonitor(int monitorIndex) {
			EditorWindow gameView = EditorWindowX.GetMainGameView();
			if(gameView == null) return;
			Rect newPos = new Rect(0, 20, Screen.currentResolution.width, Screen.currentResolution.height);
			if(monitorIndex != 0) {
				newPos.position = newPos.position + new Vector2(Screen.currentResolution.width,0);
			}
			gameView.position = newPos;
			gameView.minSize = gameView.maxSize = newPos.size;
			gameView.position = newPos;
		}
	}
//}