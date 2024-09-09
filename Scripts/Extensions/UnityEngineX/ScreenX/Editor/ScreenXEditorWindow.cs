using UnityEngine;
using UnityEditor;

public class ScreenXEditorWindow : EditorWindow {
	private static bool showScreen = true;
	private static bool showViewport = true;
	private static bool showInches = true;
	private static bool showCentimeters = true;
	

	[MenuItem("Window/ScreenX")]
	static void OpenSpriteEditorWindow() {
		GetWindow(typeof(ScreenXEditorWindow), false, "Screen X", true);
	}
	
	void Update () {
		ScreenX.CalculateScreenSizeProperties();
		Repaint();
	}
	
	void OnGUI() {
		ScreenX.usingCustomDPI = EditorGUILayout.Toggle("Use Custom DPI", ScreenX.usingCustomDPI);
		if(ScreenX.usingCustomDPI)
			ScreenX.customDPI = EditorGUILayout.IntField("Custom DPI", ScreenX.customDPI);
		else {
			GUI.enabled = false;
			EditorGUILayout.FloatField("DPI"+(ScreenX.usingDefaultDPI ? " (default)" : ""), ScreenX.dpi);
			GUI.enabled = true;
		}
		
		showScreen = EditorGUILayout.Foldout(showScreen, "Screen Properties", true);
		if(showScreen) RenderScreenProperties(ScreenX.screen);
		
		showViewport = EditorGUILayout.Foldout(showViewport, "Viewport Properties", true);
		if(showViewport) RenderScreenProperties(ScreenX.viewport);
		
		showInches = EditorGUILayout.Foldout(showInches, "Inches Properties", true);
		if(showInches) RenderScreenProperties(ScreenX.inches);
		
		showCentimeters = EditorGUILayout.Foldout(showCentimeters, "Centimeters Properties", true);
		if(showCentimeters) RenderScreenProperties(ScreenX.centimeters);
	}
	
	void RenderScreenProperties (ScreenRectProperties properties) {
		string str = $"Width={properties.width}, Height={properties.height}";
		EditorGUILayout.HelpBox(str, MessageType.None);
	}
}
