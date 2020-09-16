using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class HierarchyX {
	[MenuItem("Tools/Hierarchy/Collapse All")]
	public static void CollapseHierarchyView() {
       var svhType = Type.GetType("UnityEditor.SceneHierarchyWindow, UnityEditor");
       var hierarchies = (IEnumerable)svhType.GetMethod("GetAllSceneHierarchyWindows", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
       var getExpandedSceneNameMethod = svhType.GetMethod("GetExpandedSceneNames", BindingFlags.NonPublic | BindingFlags.Instance);
       var setScenesExpandedMethod = svhType.GetMethod("SetScenesExpanded", BindingFlags.NonPublic | BindingFlags.Instance);
	//    var setExpandedRecursiveMethod = svhType.GetMethod("SetExpandedRecursive", BindingFlags.Public | BindingFlags.Instance);

       foreach (var hierarchy in hierarchies) {
			var expandedScenes = new List<string>();
			foreach (var scene in (IEnumerable<string>)getExpandedSceneNameMethod.Invoke(hierarchy, null)) {
				if (!scene.StartsWith("sgnode_")) 
					expandedScenes.Add(scene);
			}
			setScenesExpandedMethod.Invoke(hierarchy, new object[] { expandedScenes });
		}
		EditorApplication.RepaintHierarchyWindow();
	}
}
