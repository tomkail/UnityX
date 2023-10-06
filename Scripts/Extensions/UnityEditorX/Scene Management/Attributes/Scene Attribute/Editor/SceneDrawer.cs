using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditorX.SceneManagement;
using System.IO;


[CustomPropertyDrawer(typeof(SceneAttribute))]
public class SceneDrawer : PropertyDrawer {
	
	private SceneAttribute sceneNameAttribute {
        get {
			return (SceneAttribute)attribute;
        }
    }

	bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.ObjectReference;
	}
	void DrawNotSupportedGUI (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.HelpBox(position, "Type "+property.propertyType+" is not supported with this property attribute. Use Object type instead.", MessageType.Error);
	}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if(!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

    	float oldLabelWidth = EditorGUIUtility.labelWidth;

		position = EditorGUI.PrefixLabel(position, label);
		Rect propertyRect = new Rect(position.x, position.y, 140, position.height);
		Rect popupRect = new Rect(position.x+150, position.y, position.width-150, position.height);

		EditorGUI.BeginProperty (position, label, property);

		Object[] scenes = GetScenes();
		string[] sceneNames = GetSceneNames();
		for(int i = 0; i < sceneNames.Length; i++) {
			sceneNames[i] = PathX.GetFullPathWithoutExtension(sceneNames[i]);
			sceneNames[i] = sceneNames[i].Substring(sceneNames[i].IndexOf("Assets/") + 7);
		}

        if (sceneNames.Length == 0) {
            EditorGUI.LabelField(position, ObjectNames.NicifyVariableName(property.name), "No Scenes in build.");
            return;
        }

		if(!(property.objectReferenceValue is SceneAsset)) {
			property.objectReferenceValue = null;
		}
		Object last = property.objectReferenceValue;
		property.objectReferenceValue = EditorGUI.ObjectField(propertyRect, GUIContent.none, property.objectReferenceValue, typeof(SceneAsset), false) as SceneAsset;
		if(property.objectReferenceValue == null) {
			property.objectReferenceValue = last;
		}
		sceneNameAttribute.selectedValue = System.Array.IndexOf(scenes, property.objectReferenceValue);
		if(sceneNameAttribute.selectedValue == -1)
			sceneNameAttribute.selectedValue = 0;

		sceneNameAttribute.selectedValue = EditorGUI.Popup(popupRect, sceneNameAttribute.selectedValue, sceneNames);
        property.objectReferenceValue = scenes[sceneNameAttribute.selectedValue];

		EditorGUI.EndProperty();

		EditorGUIUtility.labelWidth = oldLabelWidth;
    }

	private Object[] GetScenes() {
		string[] paths = EditorSceneManagerX.scenePaths;
		Object[] o = new Object[paths.Length+1];
		o[0] = null;
		for(int i = 0; i < paths.Length; i++) {
			o[i+1] = AssetDatabase.LoadAssetAtPath<Object>(paths[i]);
		}
//		if(sceneNameAttribute.findMethod == SceneNameAttribute.SceneFindMethod.AllInProject) {
//			return ;
//		}
//        List<EditorBuildSettingsScene> scenes = null;
//		if(sceneNameAttribute.findMethod == SceneNameAttribute.SceneFindMethod.AllInBuild) scenes = EditorBuildSettings.scenes.ToList();
//		else if(sceneNameAttribute.findMethod == SceneNameAttribute.SceneFindMethod.EnabledInBuild) scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToList();
//        HashSet<string> sceneNames = new HashSet<string>();
//        scenes.ForEach(scene => {
//			sceneNames.Add(scene.path);
//        });
//        return sceneNames.ToArray();
		return o;
	}

    private string[] GetSceneNames()
    {
		List<string> paths = EditorSceneManagerX.scenePaths.ToList();
//		if(sceneNameAttribute.findMethod == SceneNameAttribute.SceneFindMethod.AllInProject) {
//		}
//        List<EditorBuildSettingsScene> scenes = null;
//		if(sceneNameAttribute.findMethod == SceneNameAttribute.SceneFindMethod.AllInBuild) scenes = EditorBuildSettings.scenes.ToList();
//		else if(sceneNameAttribute.findMethod == SceneNameAttribute.SceneFindMethod.EnabledInBuild) scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToList();
//        HashSet<string> sceneNames = new HashSet<string>();
//        scenes.ForEach(scene => {
//			sceneNames.Add(scene.path);
//        });
//        return sceneNames.ToArray();
		paths.Insert(0, "None");
		return paths.ToArray();
    }

    private void SetSceneNumbers(int[] sceneNumbers, string[] sceneNames) {
        for (int i = 0; i < sceneNames.Length; i++) {
            sceneNumbers[i] = i;
        }
    }
}