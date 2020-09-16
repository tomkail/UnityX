using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using UnityEditorInternal;
using System.Linq;

[CustomEditor(typeof(RuntimeSceneSet))]
[CanEditMultipleObjects]
public class RuntimeSceneSetEditor : BaseEditor<RuntimeSceneSet> {

	private ReorderableList setList;
	private ReorderableList scenesList;

    public override void OnEnable() {
    	base.OnEnable();
		setList = new ReorderableList(serializedObject, serializedObject.FindProperty("sets"), true, true, true, true);
		setList.drawHeaderCallback = (Rect rect) => {  
		    EditorGUI.LabelField(rect, "Sets");
		};
		setList.elementHeightCallback = (int index) => {
			return EditorGUI.GetPropertyHeight(setList.serializedProperty.GetArrayElementAtIndex(index)) + EditorGUIUtility.standardVerticalSpacing;
		};
		setList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			var element = setList.serializedProperty.GetArrayElementAtIndex(index);
		    rect.y += 2;
			rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, element, GUIContent.none);
		};

		scenesList = new ReorderableList(serializedObject, serializedObject.FindProperty("sceneAssets"), true, true, true, true);
		scenesList.drawHeaderCallback = (Rect rect) => {  
		    EditorGUI.LabelField(rect, "Scene Assets");
		};
		scenesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			var element = scenesList.serializedProperty.GetArrayElementAtIndex(index);
		    rect.y += 2;
			rect.height = EditorGUIUtility.singleLineHeight;
			if(index == scenesList.count-1) {
				Color savedColor = GUI.color;
				GUI.color = new Color(1f,0.7f,0.7f,1);
				EditorGUI.PropertyField(rect, element, GUIContent.none);
				GUI.color = savedColor;
			} else {
				EditorGUI.PropertyField(rect, element, GUIContent.none);
			}
		};
		scenesList.onReorderCallback = (ReorderableList list) => {
			data.SetScenePaths();
		};
    }

    public override void OnInspectorGUI() {
		serializedObject.Update();
		setList.DoLayoutList();

		EditorGUI.BeginChangeCheck();
		scenesList.DoLayoutList();
		serializedObject.ApplyModifiedProperties();
		if(EditorGUI.EndChangeCheck()) {
			data.SetScenePaths();
		}

		int numSceneAssets = data.sceneAssets.Where(x => x != null).Count();
		int numPaths = data.scenePaths.Length;
		if(numPaths != numSceneAssets) {
			EditorGUILayout.HelpBox("Scene paths do not match scenes. This means some code is not working!", MessageType.Error);
		}

		if(data.IsIncludedInBuildSettings()) {
			EditorGUILayout.HelpBox("Not all scenes added to build settings. This is critical if this setup is intended outside editor use.", MessageType.Warning);
			if(GUILayout.Button("Add missing scenes")) {
				data.AddMissingToBuildSettings();
			}
		}
        
		if(data.IsCurrentlyUniquelyLoaded()) {
			EditorGUILayout.HelpBox("Currently active", MessageType.Info);
		} else {
			if(data.IsCurrentlyIncluded()) {
				EditorGUILayout.HelpBox("Currently included", MessageType.Info);
			}
			if(GUILayout.Button("Load")) {
                data.SetScenePaths();
				if(Application.isPlaying) {
					RuntimeSceneSetLoader.Instance.LoadSceneSetup(data, LoadTaskMode.LoadSingle);
				} else {
					if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
						data.LoadInEditor();
					}
				}
			}
		}

		serializedObject.ApplyModifiedProperties();
    }
}