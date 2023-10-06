using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Provides callbacks for more specific selection events.
/// </summary>
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public static class SelectionX {
    #if UNITY_EDITOR
	const string editorPrefsPath = "SelectionXLastSelection";

	public static Object[] orderedObjects {
		get {
			return LoadLastSelection().objects;
		}
	}
	public static GameObject[] orderedGameObjects {
		get {
			return LoadLastSelection().gameObjects;
		}
	}

	public delegate void SelectionDelegate ();
	public static SelectionDelegate OnSelectionChanged;

	public delegate void SelectionObjectDelegate (Object obj);
	public static event SelectionObjectDelegate OnSelectObject;
	public static event SelectionObjectDelegate OnDeselectObject;

	public delegate void SelectionGameObjectDelegate (GameObject go);
	public static event SelectionGameObjectDelegate OnSelectGameObject;
	public static event SelectionGameObjectDelegate OnDeselectGameObject;

	public static List<T> GetFiltered<T> () {
		List<T> filtered = new List<T>();
		foreach(var go in orderedGameObjects) {
			var t = go.GetComponents<T>();
			if(t != null) filtered.AddRange(t);
		}
		return filtered;
	}
	public static void Add (params Object[] obj) {
		Selection.objects = Selection.objects.Concat(obj).ToArray();
	}
	public static void Remove (params Object[] obj) {
		Selection.objects = Selection.objects.Except(obj).ToArray();
	}
	static SelectionX () {
		Selection.selectionChanged += SelectionChanged;
	}

	static SerializedSelection CreateSerializedSelection () {
		var selection = new SerializedSelection();
		selection.activeContext = Selection.activeContext;
		selection.activeInstanceID = Selection.activeInstanceID;
		selection.instanceIDs = Selection.instanceIDs;
		return selection;
	}

	static void SelectionChanged () {
		var lastSelection = LoadLastSelection();
		if(lastSelection != null) {
			CompareWithLastSelection(lastSelection);
		}
		if(Selection.instanceIDs.Length == 0)
			lastSelection = CreateSerializedSelection();
		SaveLastSelection(lastSelection);
		if(OnSelectionChanged != null) OnSelectionChanged();
	}

	static SerializedSelection LoadLastSelection () {
		if(EditorPrefs.HasKey(editorPrefsPath)) {
			string jsonLastSelection = EditorPrefs.GetString(editorPrefsPath);
			return JsonUtility.FromJson<SerializedSelection>(jsonLastSelection);
		}
		return null;
	}

	static void SaveLastSelection (SerializedSelection lastSelection) {
		string jsonLastSelection = JsonUtility.ToJson(lastSelection);
		EditorPrefs.SetString(editorPrefsPath, jsonLastSelection);
	}

	static void CompareWithLastSelection (SerializedSelection selection) {
		List<Object> objects = new List<Object>();
		objects.AddRange(selection.objects);
		foreach(var deselected in selection.objects.Except(Selection.gameObjects)) {
			objects.Remove(deselected);
			if(OnDeselectObject != null) OnDeselectObject(deselected);
		}
		foreach(var selected in Selection.objects.Except(selection.gameObjects)) {
			objects.Remove(selected);
			objects.Add(selected);
			if(OnSelectObject != null) OnSelectObject(selected);
		}
		foreach(var deselected in selection.gameObjects.Except(Selection.gameObjects)) {
			if(OnDeselectGameObject != null) OnDeselectGameObject(deselected);
		}
		foreach(var selected in Selection.gameObjects.Except(selection.gameObjects)) {
			if(OnSelectGameObject != null) OnSelectGameObject(selected);
		}
		selection.objects = objects.ToArray();
	}

	class SerializedSelection {
		public int activeContextInstanceID;
		public int activeInstanceID;
		public int[] instanceIDs;

		public Object activeContext {
			get {
				return EditorUtility.InstanceIDToObject(activeContextInstanceID);
			} set {
				if(value == null) activeContextInstanceID = 0;
				else activeContextInstanceID = value.GetInstanceID();
			}
		}

		public Object activeObject {
			get {
				return EditorUtility.InstanceIDToObject(activeInstanceID);
			} set {
				if(value == null) activeInstanceID = 0;
				activeInstanceID = value.GetInstanceID();
			}
		}

		public GameObject activeGameObject {
			get {
				return activeObject == null ? null : activeObject as GameObject;
			}
		}

		public Transform activeTransform {
			get {
				return activeGameObject == null ? null : activeGameObject.transform;
			}
		}

		public Object[] objects {
			get {
				return instanceIDs.Select(instanceID => EditorUtility.InstanceIDToObject(instanceID)).ToArray();
			} set {
				if(value == null) objects = new Object[0];
				else instanceIDs = value.Where(obj => obj != null).Select(obj => obj.GetInstanceID()).ToArray();
			}
		}

		public GameObject[] gameObjects {
			get {
				return objects.Where(obj => obj is GameObject).Select(obj => obj as GameObject).ToArray();
			}
		}
	}
    #endif
}
