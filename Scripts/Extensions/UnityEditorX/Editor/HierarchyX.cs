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
					// SceneHierarchyWindow.SetExpandedRecursive is an internal Unity API that still takes an
					// int instance id (there is no EntityId overload). Unity gives no lossless EntityId->int
					// conversion, so GetInstanceID() is the correct value to pass here — the obsolete warning
					// is suppressed as the documented stopgap for legacy int-typed APIs we can't change.
#pragma warning disable 618
					setExpandedRecursiveMethod.Invoke(hierarchy, new object[] { root.GetInstanceID(), false });
#pragma warning restore 618
				}
			}
		}
		EditorApplication.RepaintHierarchyWindow();
	}
}
