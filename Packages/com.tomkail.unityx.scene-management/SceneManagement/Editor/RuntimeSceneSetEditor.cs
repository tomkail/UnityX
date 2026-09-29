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
				// The row for the scene that becomes active when the set loads is tinted.
				if (element.FindPropertyRelative("scenePath").stringValue == data.GetActiveScenePath()) {
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

		// Picks the active scene from every scene in the set's hierarchy (nested sets included).
		// Option 0 is "use the last scene", stored as an empty reference.
		void DrawActiveScenePopup () {
			var paths = data.AllScenePaths();
			if (paths.Count == 0) return;
			var activeScene = serializedObject.FindProperty("activeScene");
			var sceneAsset = activeScene.FindPropertyRelative("sceneAsset");
			var scenePath = activeScene.FindPropertyRelative("scenePath");

			var options = new GUIContent[paths.Count + 1];
			options[0] = new GUIContent("Last scene (" + System.IO.Path.GetFileNameWithoutExtension(paths[paths.Count - 1]) + ")");
			for (int i = 0; i < paths.Count; i++)
				options[i + 1] = new GUIContent(System.IO.Path.GetFileNameWithoutExtension(paths[i]), paths[i]);

			int current = string.IsNullOrEmpty(scenePath.stringValue) ? 0 : paths.IndexOf(scenePath.stringValue) + 1;
			if (current == 0 && !string.IsNullOrEmpty(scenePath.stringValue))
				EditorGUILayout.HelpBox("The chosen active scene (" + scenePath.stringValue + ") isn't in this set any more, so the last scene is used.", MessageType.Warning);

			EditorGUI.showMixedValue = activeScene.hasMultipleDifferentValues;
			EditorGUI.BeginChangeCheck();
			int picked = EditorGUILayout.Popup(new GUIContent("Active Scene", "Scene made active when this set loads (lighting, newly created objects, etc.)."), current, options);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck()) {
				string path = picked == 0 ? string.Empty : paths[picked - 1];
				scenePath.stringValue = path;
				sceneAsset.objectReferenceValue = picked == 0 ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
			}
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			setList.DoLayoutList();
			scenesList.DoLayoutList();
			DrawActiveScenePopup();
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
