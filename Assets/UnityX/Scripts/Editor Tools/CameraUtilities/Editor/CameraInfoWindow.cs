using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CameraInfoWindow : EditorWindow 
{
	[MenuItem("Tools/Camera Utilities/List Cameras", false, 0)]
	static void ShowWindow()
	{
		var win = GetWindow<CameraInfoWindow>();
		var con = new GUIContent(EditorGUIUtility.IconContent("Camera Icon"));
		con.text = "Cameras";
		win.titleContent = con;
		win.Show();
	}

	void OnGUI()
	{
		PrefabViewGUI();		
	}

	void PrefabViewGUI()
	{
		var style = new GUIStyle(EditorStyles.miniButton);
		var labelStyle = new GUIStyle(EditorStyles.boldLabel);
		labelStyle.alignment = TextAnchor.MiddleCenter;
		var colour = GUI.color;

		var enabled = new Color(0.5f, 1.0f, 0.65f, 1);
		var disabled = new Color(1.0f, 1.0f, 0.65f, 1);

		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

		var cameras = new List<Camera>(Resources.FindObjectsOfTypeAll<Camera>());
		cameras.Sort((x, y) => x.depth.CompareTo(y.depth));

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Name", labelStyle);
		GUILayout.Label("Depth", labelStyle, GUILayout.MaxWidth(50));
		GUILayout.Label("HDR", labelStyle, GUILayout.MaxWidth(50));
		GUILayout.Label("MSAA", labelStyle, GUILayout.MaxWidth(50));
		GUILayout.Label("Clear Flags", labelStyle, GUILayout.MaxWidth(100));
		GUILayout.Label("Near", labelStyle, GUILayout.MaxWidth(50));
		GUILayout.Label("Far", labelStyle, GUILayout.MaxWidth(50));
		EditorGUILayout.EndHorizontal();

		for (var i = 0; i < cameras.Count; ++i)
		{
			var cam = cameras[i];

			EditorGUILayout.BeginHorizontal();

			if (cameras[i].gameObject.activeInHierarchy && cameras[i].isActiveAndEnabled)
			{
				GUI.color = enabled;
			}
			else
			{
				GUI.color = disabled;
			}

			if (GUILayout.Button(cam.gameObject.name, style))
			{
				Selection.activeGameObject = cam.gameObject;
			}

			GUILayout.Label(cam.depth.ToString(), style, GUILayout.MaxWidth(50));
			GUILayout.Label(cam.allowHDR.ToString(), style, GUILayout.MaxWidth(50));
			GUILayout.Label(cam.allowMSAA.ToString(), style, GUILayout.MaxWidth(50));
			GUILayout.Label(cam.clearFlags.ToString(), style, GUILayout.MaxWidth(100));
			GUILayout.Label(cam.nearClipPlane.ToString(), style, GUILayout.MaxWidth(50));
			GUILayout.Label(cam.farClipPlane.ToString(), style, GUILayout.MaxWidth(50));
			
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();
	}

	void OnInspectorUpdate()
	{
		Repaint();
	}

	Vector2 _scrollPos;
}
