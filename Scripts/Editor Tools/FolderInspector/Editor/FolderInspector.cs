using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class FolderInspector : DefaultAssetInspector {
	string absolutePath {
		get {
			var path = AssetDatabase.GetAssetPath(target);
			return EditorApplicationX.UnityRelativeToAbsolutePath(path);
		}
	}
	Vector2 position;

	public override bool IsValid(string assetPath) {
		return PathX.PathIsDirectory(assetPath);
	}

	public override void OnHeaderGUI () {
		GUILayout.BeginHorizontal();
		GUILayout.Space(38f);
		GUILayout.BeginVertical();
		GUILayout.Space(19f);
		GUILayout.BeginHorizontal();

		GUILayoutUtility.GetRect(10f, 10f, 16f, 16f, EditorStyles.layerMaskField);
		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Open", EditorStyles.miniButton)) {
			EditorUtility.RevealInFinder(absolutePath);
			GUIUtility.ExitGUI();
		}

		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		Rect lastRect = GUILayoutUtility.GetLastRect();
		Rect rect = new Rect(lastRect.x, lastRect.y, lastRect.width, lastRect.height);

		Rect titleRect = new Rect(rect.x + 44f, rect.y + 6f, rect.width - 44f - 38f - 4f, 16f);
		titleRect.yMin -= 2f;
		titleRect.yMax += 2f;
		GUI.Label(titleRect, editor.target.name, EditorStyles.largeLabel);
	}

	public override void OnInspectorGUI () {
		editor.Repaint();
		serializedObject.Update();

		position = EditorGUILayout.BeginScrollView(position);

		var directories = Directory.GetDirectories(absolutePath);
		EditorGUILayout.LabelField(directories.Length+" Folders", EditorStyles.boldLabel);
		foreach(var file in directories)
			EditorGUILayout.LabelField(Path.GetFileName(file));

		EditorGUILayout.Space();

		var files = Directory.GetFiles(absolutePath).Where(x => Path.GetExtension(x) != ".meta");
		EditorGUILayout.LabelField(files.Count()+" Files", EditorStyles.boldLabel);
		foreach(var file in files) {
			if(GUILayout.Button(Path.GetFileName(file))) {
				Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(EditorApplicationX.AbsoluteToUnityRelativePath(file));
			}
		}


		EditorGUILayout.EndScrollView();
		
		serializedObject.ApplyModifiedProperties();
	}
}