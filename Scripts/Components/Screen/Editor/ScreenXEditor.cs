using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ScreenX))]
public class ScreenXEditor : BaseEditor<ScreenX> {
	
	private static bool showScreen = true;
	private static bool showViewport = true;
	private static bool showInches = true;
	private static bool showCentimeters = true;
	
	public override void OnEnable () {
		EditorApplication.update += Update;
	}
	
	private void OnDisable () {
		EditorApplication.update -= Update;
	}
	
	private void Update () {
		// Have to call from Update instead of OnInspectorGUI else you get the size of the wrong window.
		ScreenX.CalculateScreenSizeProperties();
	}
	
	public override void OnInspectorGUI() {
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
	
	private void RenderScreenProperties (ScreenProperties properties) {
		string str = string.Format("Width={0}, Height={1}", properties.width, properties.height);
		EditorGUILayout.HelpBox(str, MessageType.None);
	}
}
