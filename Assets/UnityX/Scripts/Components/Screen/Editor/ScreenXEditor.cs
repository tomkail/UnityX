using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ScreenX))]
public class ScreenXEditor : BaseEditor<ScreenX> {
	
	private static bool showScreen;
	private static bool showViewport;
	private static bool showInches;
	private static bool showCentimeters;
	
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
		
		showScreen = EditorGUILayout.Foldout(showScreen, "Screen Properties");
		if(showScreen) EditorGUILayout.HelpBox(ScreenX.screen.ToString(), MessageType.None);
		
		showViewport = EditorGUILayout.Foldout(showViewport, "Viewport Properties");
		if(showViewport) EditorGUILayout.HelpBox(ScreenX.viewport.ToString(), MessageType.None);
		
		showInches = EditorGUILayout.Foldout(showInches, "Inches Properties");
		if(showInches) EditorGUILayout.HelpBox(ScreenX.inches.ToString(), MessageType.None);
		
		showCentimeters = EditorGUILayout.Foldout(showCentimeters, "Centimeters Properties");
		if(showCentimeters) EditorGUILayout.HelpBox(ScreenX.centimeters.ToString(), MessageType.None);
	}
	
	private void RenderScreenProperties (ScreenProperties properties) {
		
	}
}
