using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Editor-side component management helper functions
public static class ComponentMenuX {

	#if UNITY_EDITOR
	public static void MoveToTop(Component component) {
		if(component == null) return;
		if(component.gameObject.GetComponent<Component>() == component) return;
		Component[] comps = component.gameObject.GetComponents<Component>();
		if(component == comps[0]) return;
		int componentIndex = Array.IndexOf(comps, component);
		for(int i = 0; i < componentIndex-1; i++)
			ComponentUtility.MoveComponentUp (component);
	}
	
	public static void MoveToBottom(Component component) {
		if(component == null) return;
		Component[] comps = component.gameObject.GetComponents<Component>();
		if(component == comps[comps.Length-1]) return;
		int componentIndex = Array.IndexOf(comps, component);
		for(int i = 0; i < comps.Length-componentIndex-1; i++)
			ComponentUtility.MoveComponentDown (component);
	}
	
	
	[MenuItem("CONTEXT/Component/Move To Top")]
	static void MoveToTopContextMenu(MenuCommand command) {
		Component context = command.context as Component;
		MoveToTop(context);
	}
	
	[MenuItem("CONTEXT/Component/Move To Bottom")]
	static void MoveToBottomContextMenu(MenuCommand command) {
		Component context = command.context as Component;
		MoveToBottom(context);
	}
	
	[MenuItem("CONTEXT/Component/Remove All Other Components")]
	static void RemoveAllOtherComponentsContextMenu(MenuCommand command) {
		Component context = command.context as Component;
		Component[] comps = context.gameObject.GetComponents<Component>();
		if(EditorUtility.DisplayDialog("Remove All Other Components?", "Are you sure you want to remove all " + (comps.Length-2) + " other Components? This action can not be undone.", "Remove", "Cancel")) {
			DestroyAllImmediateExcept(comps, context);
		}
	}
	
	[MenuItem("CONTEXT/Component/Remove All Components")]
	static void RemoveAllComponentsContextMenu(MenuCommand command) {
		Component context = command.context as Component;
		Component[] comps = context.gameObject.GetComponents<Component>();
		if(EditorUtility.DisplayDialog("Remove All Components?", "Are you sure you want to remove all " + (comps.Length-1) + " Components? This action can not be undone.", "Remove", "Cancel")) {
			DestroyAllImmediate(comps);
		}
	}

	/// <summary>
	/// Destroy all objects.
	/// </summary>
	/// <param name="objects">Objects.</param>
	static void DestroyAllImmediateExcept<T>(IList<T> objects, T exception) where T : Object {
		for(int i = 0; i < objects.Count; i++) {
			if(objects[i].GetType() == typeof(Transform)) continue;
			if(objects[i] == exception) continue;
			Object.DestroyImmediate(objects[i]);
		}
	}
	
	/// <summary>
	/// Destroy all objects.
	/// </summary>
	/// <param name="objects">Objects.</param>
	static void DestroyAllImmediate<T>(IList<T> objects) where T : Object {
		for(int i = 0; i < objects.Count; i++) {
			if(objects[i].GetType() == typeof(Transform)) continue;
			Object.DestroyImmediate(objects[i]);
		}
	}

	#endif
}