using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditorX.SceneManagement;

[CustomPropertyDrawer(typeof(ScenePathAttribute))]
public class ScenePathDrawer : PropertyDrawer {
	int selectedValue = -1;

    private ScenePathAttribute scenePathAttribute {
        get {
            return (ScenePathAttribute)attribute;
        }
    }

	bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String;
	}
	void DrawNotSupportedGUI (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.HelpBox(position, "Type "+property.propertyType+" is not supported with this property attribute. Use Object type instead.", MessageType.Error);
	}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if(!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		position = EditorGUI.PrefixLabel(position, label);

		string[] scenePaths = GetScenePaths();
		if (scenePaths.Length == 0) {
			EditorGUI.LabelField(position, "No Scenes in build.");
			return;
		}

		if(!scenePathAttribute.useFullPath) {
			for(int i = 0; i < scenePaths.Length; i++) {
				scenePaths[i] = System.IO.Path.GetFileNameWithoutExtension(scenePaths[i]);
			}
		}

		selectedValue = GetSceneIndex(scenePaths, property.stringValue);

		
		Rect propertyRect = new Rect(position.x, position.y, 140, position.height);
		Rect objectRect = new Rect(position.x+150, position.y, position.width-150, position.height);
		selectedValue = EditorGUI.Popup(propertyRect, selectedValue, scenePaths);
		property.stringValue = selectedValue == -1 ? "" : scenePaths[selectedValue];

		SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);
		SceneAsset newSceneAsset = EditorGUI.ObjectField(objectRect, GUIContent.none, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
		if(sceneAsset != newSceneAsset) {
			string assetPath = AssetDatabase.GetAssetPath(newSceneAsset);
			if(!scenePathAttribute.useFullPath) {
				assetPath = System.IO.Path.GetFileNameWithoutExtension(property.stringValue);
			}
			if(scenePaths.Contains(assetPath)) {
				property.stringValue = assetPath;
			} else {
				Debug.Log("Scene '"+assetPath+"' does not match attribute search method "+scenePathAttribute.findMethod.ToString());
			}
		}
    }

    private string[] GetScenePaths() {
		if(scenePathAttribute.findMethod == ScenePathAttribute.SceneFindMethod.AllInProject) {
			return EditorSceneManagerX.scenePaths;
		}
        List<EditorBuildSettingsScene> scenes = null;
		if(scenePathAttribute.findMethod == ScenePathAttribute.SceneFindMethod.AllInBuild) scenes = EditorBuildSettings.scenes.ToList();
		else if(scenePathAttribute.findMethod == ScenePathAttribute.SceneFindMethod.EnabledInBuild) scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToList();

        HashSet<string> sceneNames = new HashSet<string>();
		sceneNames.Add("NONE");
		scenes.ForEach(scene => sceneNames.Add(scene.path.Replace("\\", "/")));
        return sceneNames.ToArray();
    }

	private int[] GetSceneIndexes (string[] sceneNames) {
		int[] sceneIndexes = new int[sceneNames.Length];
		for (int i = 0; i < sceneNames.Length; i++) sceneIndexes[i] = i;
        return sceneIndexes;
    }

    private int GetSceneIndex (string[] sceneNames, string sceneName) {
		sceneName = sceneName.Replace("\\", "/");
		for (int i = 0; i < sceneNames.Length; i++) if (sceneName == sceneNames[i]) return i;
        return -1;
    }
}