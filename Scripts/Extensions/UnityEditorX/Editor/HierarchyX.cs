using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public static class HierarchyX {
	[MenuItem("Tools/Hierarchy/Collapse All")]
	public static void CollapseHierarchyView() {
       var svhType = Type.GetType("UnityEditor.SceneHierarchyWindow, UnityEditor");
       var hierarchies = (IEnumerable)svhType.GetMethod("GetAllSceneHierarchyWindows", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
       var setExpandedRecursiveMethod = svhType.GetMethod("SetExpandedRecursive", BindingFlags.Public | BindingFlags.Instance);

       foreach (var hierarchy in hierarchies) {
			for (int i = 0; i < SceneManager.sceneCount; i++) {
				var scene = SceneManager.GetSceneAt(i);
				if (!scene.isLoaded) continue;
				foreach (var root in scene.GetRootGameObjects()) {
					setExpandedRecursiveMethod.Invoke(hierarchy, new object[] { root.GetInstanceID(), false });
				}
			}
		}
		EditorApplication.RepaintHierarchyWindow();
	}
}
