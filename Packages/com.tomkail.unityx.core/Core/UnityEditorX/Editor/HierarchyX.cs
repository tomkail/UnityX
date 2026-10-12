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
		var svhType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
		var getAllMethod = svhType?.GetMethod("GetAllSceneHierarchyWindows", BindingFlags.Public | BindingFlags.Static);
		var setExpandedRecursiveMethod = svhType?.GetMethod("SetExpandedRecursive", BindingFlags.Public | BindingFlags.Instance);
		if (getAllMethod == null || setExpandedRecursiveMethod == null) {
			Debug.LogWarning("Collapse All: SceneHierarchyWindow internals weren't found in this Unity version.");
			return;
		}
		var hierarchies = (IEnumerable)getAllMethod.Invoke(null, null);

		// SceneHierarchyWindow.SetExpandedRecursive keys the hierarchy tree by the legacy int instance id, and
		// Unity offers no lossless EntityId->int conversion (a hash code isn't unique, so it wouldn't match a
		// tree item). GetInstanceID() is the only correct source of that int, but it's deprecated and slated
		// for removal along with int instance ids — so we fetch it via reflection rather than referencing the
		// obsolete API at compile time. This keeps the tool compiling regardless, and if the accessor is ever
		// removed it simply no-ops instead of breaking the build. (When Unity retires int instance ids
		// entirely, SetExpandedRecursive itself will change and this whole reflection will need revisiting.)
		var getInstanceIDMethod = typeof(UnityEngine.Object).GetMethod("GetInstanceID", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
		if (getInstanceIDMethod == null) return;

		foreach (var hierarchy in hierarchies) {
			for (int i = 0; i < SceneManager.sceneCount; i++) {
				var scene = SceneManager.GetSceneAt(i);
				if (!scene.isLoaded) continue;
				foreach (var root in scene.GetRootGameObjects()) {
					var instanceId = getInstanceIDMethod.Invoke(root, null);
					setExpandedRecursiveMethod.Invoke(hierarchy, new object[] { instanceId, false });
				}
			}
		}
		EditorApplication.RepaintHierarchyWindow();
	}
}
