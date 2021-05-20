using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class ComponentX {

    

	/// <summary>
	/// Recursively travels to the top-level parent, or the parent at level maxLevels and outputs a string of the hierarchy path of the transform
	/// </summary>
	public static string HierarchyPath (this Component c, bool includeSceneName = true, int maxLevels = 0) {
        return c.transform.HierarchyPath() + "("+c.GetType()+")";
    }


	/// <summary>
	/// Send message doesn't work in edit mode, so we use reflection in that case instead.
	/// </summary>
	/// <param name="message">Message.</param>
	public static void BetterSendMessage (this GameObject go, string message, object obj = null, SendMessageOptions sendMessageOptions = SendMessageOptions.DontRequireReceiver) {
		#if UNITY_EDITOR
		if(!Application.isPlaying) {
			foreach(var component in go.GetComponents<Component>()) {
				try {
					MethodInfo tMethod = component.GetType().GetMethod(message, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if(tMethod != null) tMethod.Invoke(component, obj == null ? null : new object[] {obj, sendMessageOptions});
				} catch {
					if(sendMessageOptions == SendMessageOptions.RequireReceiver) Debug.LogError("No reciever found for message "+message+" for type "+component.GetType()+" on gameobject "+go.transform.HierarchyPath());
				}
			}
			return;
		}
		#endif
		go.SendMessage(message, obj, sendMessageOptions);
	}

	public static void BetterBroadcastMessage (this GameObject go, string message, object obj = null, SendMessageOptions sendMessageOptions = SendMessageOptions.DontRequireReceiver) {
		#if UNITY_EDITOR
		if(!Application.isPlaying) {
			foreach(var transform in go.GetComponentsInChildren<Transform>()) 
				go.BetterSendMessage(message, obj, sendMessageOptions);
			return;
		}
		#endif
		go.BroadcastMessage(message, obj, sendMessageOptions);
	}

	public static void BroadcastMessageGlobal (string methodName) {
		BroadcastMessageGlobal(methodName, SendMessageOptions.DontRequireReceiver);
	}

	public static void BroadcastMessageGlobal (string methodName, object parameter) {
		BroadcastMessageGlobal(methodName, parameter, SendMessageOptions.DontRequireReceiver);
    }

	public static void BroadcastMessageGlobal (string methodName, SendMessageOptions sendMessageOptions) {
		foreach (var scene in SceneManagerX.GetCurrentScenes()) {
			BroadcastMessageScene(scene, methodName, sendMessageOptions);
		}
    }

	public static void BroadcastMessageGlobal (string methodName, object parameter, SendMessageOptions sendMessageOptions) {
		foreach (var scene in SceneManagerX.GetCurrentScenes()) {
			BroadcastMessageScene(scene, methodName, parameter, sendMessageOptions);
		}
	}

	public static void BroadcastMessageScene (UnityEngine.SceneManagement.Scene scene, string methodName) {
		BroadcastMessageScene(scene, methodName, SendMessageOptions.DontRequireReceiver);
    }

	public static void BroadcastMessageScene (UnityEngine.SceneManagement.Scene scene, string methodName, SendMessageOptions sendMessageOptions) {
		if(!scene.isLoaded) {
			Debug.LogWarning("Tried to BroadcastMessage '"+methodName+"' to scene '"+scene.name+"', but scene is not loaded.");
			return;
		}
		foreach (GameObject gameObject in scene.GetRootGameObjects().OrderBy(x => x.transform.GetSiblingIndex())) {
			gameObject.BetterBroadcastMessage(methodName, sendMessageOptions);
		}
    }

	public static void BroadcastMessageScene (UnityEngine.SceneManagement.Scene scene, string methodName, object parameter) {
		BroadcastMessageScene(scene, methodName, parameter, SendMessageOptions.DontRequireReceiver);
    }

	public static void BroadcastMessageScene (UnityEngine.SceneManagement.Scene scene, string methodName, object parameter, SendMessageOptions sendMessageOptions) {
		if(!scene.isLoaded) {
			Debug.LogWarning("Tried to BroadcastMessage '"+methodName+"' to scene '"+scene.name+"', but scene is not loaded.");
			return;
		}
		foreach (GameObject gameObject in scene.GetRootGameObjects().OrderBy(x => x.transform.GetSiblingIndex())) {
			gameObject.BetterBroadcastMessage(methodName, parameter, sendMessageOptions);
		}
    }

	public static T CloneComponent<T>(T original, GameObject destination) where T : Component {
		Type type = original.GetType();
		Component copy = destination.AddComponent(type);
		CloneComponentProperties(original, copy);
		return copy as T;
	}
	
	public static void CloneComponentProperties<T>(T original, T destination) where T : Component {
		Type type = original.GetType();
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo field in fields) {
			field.SetValue(destination, field.GetValue(original));
		}
	}
	
	/// <summary>
	/// Gets the first component of type T found in the siblings of the transform.
	/// </summary>
	/// <returns>The component in siblings.</returns>
	/// <param name="current">Current.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T GetComponentInSiblings<T>(this Component current) where T : Component {
		Transform[] transforms = current.transform.GetSiblings();
		T found;
		for(int i = 0; i < transforms.Length; i++) {
			found = transforms[i].GetComponent<T>();
			if(found != null) {
				return found;
			}
		}
		return null;
	}
	
	/// <summary>
	/// Gets all of the components of type T found in the siblings of the transform.
	/// </summary>
	/// <returns>The components in siblings.</returns>
	/// <param name="current">Current.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T[] GetComponentsInSiblings<T>(this Component current) where T : Component {
		Transform[] transforms = current.transform.GetSiblings();
		List<T> all = new List<T>();
		for(int i = 0; i < transforms.Length; i++) {
			T t = transforms[i].GetComponent<T>();
			if(t != null) {
				all.Add(t);
			}
		}
		return all.ToArray();
	}
	
	public static T GetComponentInChildren<T>(this Component current, bool includeInactive) where T : Component {
		if(includeInactive) {
			return current.GetComponentsInChildren<T>(true).First();
		} else {
			return current.GetComponentInChildren<T>();
		}
	}
	
	public static T GetComponentInChildrenExcludingSelf<T>(this Component current, string name) where T : Component {
		Transform[] transforms = current.transform.FindAllInChildren(name);
		T found;
		for(int i = 0; i < transforms.Length; i++) {
			found = transforms[i].GetComponent<T>();
			if(found != null) {
				return found;
			}
		}
		return null;
	}
	
	
	public static T GetComponentInImmediateChildrenExcludingSelf<T>(this Component current) where T : Component {
		Transform[] transforms = current.transform.GetChildren();
		T found;
		for(int i = 0; i < transforms.Length; i++) {
			found = transforms[i].GetComponent<T>();
			if(found != null) {
				return found;
			}
		}
		return null;
	}

	public static T[] GetComponentsInChildren<T>(this Component current, string name) where T : Component {
		List<Transform> transforms = current.transform.FindAllInChildrenList(name);
		List<T> all = new List<T>();
		for(int i = 0; i < transforms.Count; i++) {
			T t = transforms[i].GetComponent<T>();
			if(t != null) {
				all.Add(t);
			}
		}
		return all.ToArray();
	}
	
	/// <summary>
	/// Traverses upwards until it finds the parent or hits the top of the tree. Searches the initial component.
	/// </summary>
	/// <returns>The component in parents.</returns>
	/// <param name="current">Current.</param>
	/// <param name="name">Name.</param>
	/// <typeparam name="T">The target component type.</typeparam>
	public static T GetComponentInSelfOrAncestors<T>(this Component t, bool includeInactive = false) where T : Component {
		T component = t.GetComponent<T>();
		if(component != null)
			return component;
		if(t.transform.parent != null) {
			return GetComponentInSelfOrAncestors<T>(t.transform.parent);
		} else {
			return null;
		}
	}
	
	/// <summary>
	/// Traverses upwards until it finds the parent or hits the top of the tree. Does not search the initial component.
	/// </summary>
	/// <returns>The component in parents.</returns>
	/// <param name="current">Current.</param>
	/// <param name="name">Name.</param>
	/// <typeparam name="T">The target component type.</typeparam>
	public static T GetComponentInAncestors<T>(this Component t, bool includeInactive = false) where T : Component {
		if(t.transform.parent != null) {
			T component = t.transform.parent.GetComponent<T>();
			if(component != null)
				return component;
			else
				return GetComponentInAncestors<T>(t.transform.parent);
		} else {
			return null;
		}
	}
	
	
	/// <summary>
	/// Gets the interface.
	/// </summary>
	/// <returns>The interface.</returns>
	/// <param name="inObj">In object.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T GetInterface<T>(this Component inObj) where T : class {
		#if !UNITY_WINRT
		if (!typeof(T).IsInterface) {
			Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
			return null;
		}
		#endif
		return inObj.GetComponents<Component>().OfType<T>().FirstOrDefault();
		//return typeof(T).GetTypeInfo().ImplementedInterfaces.FirstOrDefault(p => string.Compare(p.Name, name, ignoreCase? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)==0);
	}
	
	/// <summary>
	/// Gets the interfaces.
	/// </summary>
	/// <returns>The interfaces.</returns>
	/// <param name="inObj">In object.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static IEnumerable<T> GetInterfaces<T>(this Component inObj) where T : class {
		#if !UNITY_WINRT
		if (!typeof(T).IsInterface) {
			Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
			return Enumerable.Empty<T>();
		}
		#endif
		return inObj.GetComponents<Component>().OfType<T>();
	}
	
	/// <summary>
	/// Gets the interface in children.
	/// </summary>
	/// <returns>The interface in children.</returns>
	/// <param name="inObj">In object.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T GetInterfaceInChildren<T>(this Component inObj) where T : class {
		#if !UNITY_WINRT
		if (!typeof(T).IsInterface) {
			Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
			return null;
		}
		#endif
		return inObj.GetComponentsInChildren<Component>().OfType<T>().FirstOrDefault();
	}
	
	/// <summary>
	/// Gets the interfaces in children.
	/// </summary>
	/// <returns>The interfaces in children.</returns>
	/// <param name="inObj">In object.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static IEnumerable<T> GetInterfacesInChildren<T>(this Component inObj) where T : class {
		#if !UNITY_WINRT
		if (!typeof(T).IsInterface) {
			Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
			return null;
		}
		#endif
		return inObj.GetComponentsInChildren<Component>().OfType<T>();
	}
}
