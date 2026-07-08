using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace UnityX.SceneManagement.Editor {

	[CustomEditor(typeof(RuntimeSceneSet))]
	[CanEditMultipleObjects]
	public class RuntimeSceneSetEditor : UnityEditor.Editor {

		RuntimeSceneSet data => (RuntimeSceneSet)target;

		private ReorderableList setList;
		private ReorderableList scenesList;

		void OnEnable() {
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

			scenesList = new ReorderableList(serializedObject, serializedObject.FindProperty("scenes"), true, true, true, true);
			scenesList.drawHeaderCallback = (Rect rect) => {
				EditorGUI.LabelField(rect, "Scenes");
			};
			scenesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
				var element = scenesList.serializedProperty.GetArrayElementAtIndex(index);
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				// The last (newest) row is tinted to hint it is the just-added slot.
				if (index == scenesList.count - 1) {
					Color savedColor = GUI.color;
					GUI.color = new Color(1f, 0.7f, 0.7f, 1);
					EditorGUI.PropertyField(rect, element, GUIContent.none);
					GUI.color = savedColor;
				} else {
					EditorGUI.PropertyField(rect, element, GUIContent.none);
				}
			};
			// No onReorderCallback / SetScenePaths needed any more: each SceneReference caches its own
			// path (via its drawer and RuntimeSceneSet.OnValidate).
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			setList.DoLayoutList();
			scenesList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();

			if (!data.IsIncludedInBuildSettings()) {
				EditorGUILayout.HelpBox("Not all scenes added to build settings. This is critical if this setup is intended outside editor use.", MessageType.Warning);
				if (GUILayout.Button("Add missing scenes")) {
					data.AddMissingToBuildSettings();
				}
			}

			if (data.IsCurrentlyUniquelyLoaded()) {
				EditorGUILayout.HelpBox("Currently active", MessageType.Info);
			} else {
				if (data.IsCurrentlyIncluded()) {
					EditorGUILayout.HelpBox("Currently included", MessageType.Info);
				}
				if (GUILayout.Button("Load")) {
					if (Application.isPlaying) {
						RuntimeSceneSetLoader.Instance.LoadSceneSetup(data, LoadTaskMode.LoadSingle);
					} else {
						if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
							data.LoadInEditor();
						}
					}
				}
			}
		}
	}
}
