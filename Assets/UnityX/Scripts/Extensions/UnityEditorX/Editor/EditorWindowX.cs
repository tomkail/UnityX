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

		public static EditorWindow GetMainGameView() {
			System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			System.Object Res = GetMainGameView.Invoke(null, null);
			return (EditorWindow)Res;
		}

		// This is a massive fudge. It needs System.Windows.Forms, which isn't part of Mono or something
		public static void SetGameViewToFullScreenForMonitor(int monitorIndex) {
			EditorWindow gameView = EditorWindowX.GetMainGameView();
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