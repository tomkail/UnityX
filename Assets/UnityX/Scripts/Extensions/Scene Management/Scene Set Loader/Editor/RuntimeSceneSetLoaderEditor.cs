using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;

[CustomEditor(typeof(RuntimeSceneSetLoader))]
public class RuntimeSceneSetLoaderEditor : BaseEditor<RuntimeSceneSetLoader> {

	public override void OnInspectorGUI () {
		base.Repaint();
		base.OnInspectorGUI ();
		RuntimeSceneSetLoader.debugLogging = EditorGUILayout.Toggle(new GUIContent("Enable logging"), RuntimeSceneSetLoader.debugLogging);
		if(data.currentLevelSetLoadTask != null) {
			DrawSceneSetLoadTask(data.currentLevelSetLoadTask, "Loading");
			foreach(var task in data.pendingLevelSetLoadTasks) {
				DrawSceneSetLoadTask(task, "Pending");
			}
		} else {
			EditorGUILayout.HelpBox("No scene sets currently being loaded", MessageType.Info);
		}
	}

	void DrawSceneSetLoadTask (RuntimeSceneSetLoadTask task, string prefix) {
		EditorGUILayout.BeginVertical(GUI.skin.box);
		EditorGUILayout.LabelField(prefix +" "+ task.sceneSet.name +" ("+task.sceneLoadMode+")");
		GUILayout.Label(new GUIContent(task.state.ToString()));
		if(task.cancelled) {
			if (GUILayout.Button("Uncancel")) {
				task.Uncancel();
			}
		} else {
			task.yieldActivation = GUILayout.Toggle(task.yieldActivation, "Yield Activation");
			// if(task.state != RuntimeSceneSetLoadTask.State.Loading) {
			// 	if (GUILayout.Button("Load")) {
			// 		task.AssignTasks();
			// 		task.Load();
			// 	}
			// }
			if (GUILayout.Button("Cancel")) {
				task.Cancel();
			}
		}

		ReorderableList unloadTasksList = CreateUnloadTasksList(task.unloadTasks);
		if(unloadTasksList != null && unloadTasksList.count > 0) {
			unloadTasksList.DoLayoutList();
		}
		ReorderableList loadTasksList = CreateLoadTasksList(task.loadTasks);
		if(loadTasksList != null && loadTasksList.count > 0) {
			loadTasksList.DoLayoutList();
		}
		EditorGUILayout.EndVertical();
	}

	ReorderableList CreateLoadTasksList (List<SceneLoadTask> loadTasks) {
		ReorderableList loadTasksList = new ReorderableList(loadTasks, typeof(SceneLoadTask), false, false, false, false);
//			includesFileList.elementHeight = 16;
		loadTasksList.drawHeaderCallback = (Rect rect) => {  
			EditorGUI.LabelField(rect, "Load Tasks");
		};
		loadTasksList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			SceneLoadTask loadTask = ((List<SceneLoadTask>)loadTasksList.list)[index];
			string state = loadTask.state.ToString();
            List<string> extraStates = new List<string>();
            if(loadTask.state != SceneLoadTask.State.Complete) {
                if(loadTask.cancel) extraStates.Add("Cancel");
                else if(loadTask.allowActivation) extraStates.Add("Activation allowed");
            }
            var label = loadTask.sceneName+" ("+state+")" + (extraStates.IsEmpty() ? string.Empty : " ("+string.Join(", ", extraStates.ToArray())+")");
			EditorGUI.ProgressBar(rect, loadTask.progress, label);
            // EditorGUI.LabelField(rect, );
		};
		return loadTasksList;
	}

	ReorderableList CreateUnloadTasksList (List<SceneUnloadTask> loadTasks) {
		ReorderableList loadTasksList = new ReorderableList(loadTasks, typeof(SceneUnloadTask), false, false, false, false);
//			includesFileList.elementHeight = 16;
		loadTasksList.drawHeaderCallback = (Rect rect) => {  
            EditorGUI.LabelField(rect, "Unload Tasks");
		};
		loadTasksList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			SceneUnloadTask loadTask = ((List<SceneUnloadTask>)loadTasksList.list)[index];
			string state = loadTask.state.ToString();
            var label = loadTask.sceneName+" ("+state+")";
			EditorGUI.ProgressBar(rect, loadTask.progress, label);
		};
		return loadTasksList;
	}
}
