using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

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
					if(tMethod != null) tMethod.Invoke(component, obj == null ? null : new[] {obj, sendMessageOptions});
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

	public static void BroadcastMessageScene (Scene scene, string methodName) {
		BroadcastMessageScene(scene, methodName, SendMessageOptions.DontRequireReceiver);
    }

	public static void BroadcastMessageScene (Scene scene, string methodName, SendMessageOptions sendMessageOptions) {
		if(!scene.isLoaded) {
			Debug.LogWarning("Tried to BroadcastMessage '"+methodName+"' to scene '"+scene.name+"', but scene is not loaded.");
			return;
		}
		foreach (GameObject gameObject in scene.GetRootGameObjects().OrderBy(x => x.transform.GetSiblingIndex())) {
			gameObject.BetterBroadcastMessage(methodName, sendMessageOptions);
		}
    }

	public static void BroadcastMessageScene (Scene scene, string methodName, object parameter) {
		BroadcastMessageScene(scene, methodName, parameter, SendMessageOptions.DontRequireReceiver);
    }

	public static void BroadcastMessageScene (Scene scene, string methodName, object parameter, SendMessageOptions sendMessageOptions) {
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
	

	
	// Search params for GetComponentsX.
	[Serializable]
	public struct ComponentSearchParams<T> {
		// Depths are inclusive. 0 is the queried object. -1 searches parent, 1 searches children.
		public int startDepth {get;}
		public int endDepth {get;}
		public bool includeInactive {get;}
		// This is a reference type - should we make this a class for safety?
		// This can be used to further filter the results of the search; for example to only return objects with a given gameobject name.
		public Predicate<T> predicate {get;}
		public bool sortedTopFirst {get;}

		public ComponentSearchParams (int startDepthInclusive, int endDepthInclusive, bool includeInactive = false, Predicate<T> predicate = null) {
			// We do allow searching from bottom-to-top, but startDepth must be before endDepth for the algorithm to work.
			// To solve this we flip them and set sortedTopFirst to false so to reverse the list at the end of the algorithm.
			if(startDepthInclusive > endDepthInclusive) {
				startDepth = endDepthInclusive;
				endDepth = startDepthInclusive;
				sortedTopFirst = false;
			} else {
				startDepth = startDepthInclusive;
				endDepth = endDepthInclusive;
				sortedTopFirst = true;
			}
			this.includeInactive = includeInactive;
			this.predicate = predicate;
		}

		// Utility functions for common actions.
		public static ComponentSearchParams<T> ImmediateDescendentsExcludingSelf (bool includeInactive = false, Predicate<T> predicate = null) {
			return new ComponentSearchParams<T>(1, 1, includeInactive, predicate);
		}
		public static ComponentSearchParams<T> ImmediateAncestorsExcludingSelf (bool includeInactive = false, Predicate<T> predicate = null) {
			return new ComponentSearchParams<T>(1, 1, includeInactive, predicate);
		}
		public static ComponentSearchParams<T> AllDescendentsExcludingSelf (bool includeInactive = false, Predicate<T> predicate = null) {
			return new ComponentSearchParams<T>(1, int.MaxValue-1, includeInactive, predicate);
		}
		public static ComponentSearchParams<T> AllDescendentsIncludingSelf (bool includeInactive = false, Predicate<T> predicate = null) {
			return new ComponentSearchParams<T>(0, int.MaxValue-1, includeInactive, predicate);
		}
		public static ComponentSearchParams<T> AllAncestorsExcludingSelf (bool includeInactive = false, Predicate<T> predicate = null) {
			return new ComponentSearchParams<T>(-1, int.MinValue+1, includeInactive, predicate);
		}
		public static ComponentSearchParams<T> AllAncestorsIncludingSelf (bool includeInactive = false, Predicate<T> predicate = null) {
			return new ComponentSearchParams<T>(0, int.MinValue+1, includeInactive, predicate);
		}
	}

	/// <summary>
	/// Flexible breadth-first search algorithm to find a relative of a particular type.
	/// Solves some common issues with Unity's system:
	/// - Inability to control the max search depth
	/// - Lack of clarity over when the root object/inactive objects are included
	/// - Lack of GetComponentsInImmediateChildren or GetComponentsInSiblings functions
	/// - Depth first search
	/// - Inability to find parents at the same time as children
	/// - Custom predicate
	/// </summary>
	/// <returns>List of components that fit within the search parameters, sorted according to depth (ordered starting from startDepth to endDepth), then by breadth</returns>
	public static List<T> GetComponentsX<T>(this Component current, ComponentSearchParams<T> searchParams) where T : Component {
		// We could cache these variables to reduce garbage, but not doing so means this is thread-safe.
		List<T> components = new List<T>();
		
		// Try to add components from ancestors
		if(searchParams.startDepth < 0) {
			var transformDepthTuple = new Tuple<Transform, int>(current.transform, 0);
			for(int i = 0; i < -searchParams.startDepth; i++) {
				if(transformDepthTuple.Item1.parent != null) {
					transformDepthTuple = new Tuple<Transform, int>(transformDepthTuple.Item1.parent, transformDepthTuple.Item2-1);
					TryAddComponent(transformDepthTuple, searchParams, components);
				} else {
					break;
				}
			}
			// Reverse the list so that the top-level object is at the start of the list. This ensures the list is entirely sorted with depth top-to-bottom.
			components.Reverse();
		}

		// Try to add ourselves
		{
			var transformDepthTuple = new Tuple<Transform, int>(current.transform, 0);
			TryAddComponent(transformDepthTuple, searchParams, components);
		}

		// Try to add components from children
		if(searchParams.endDepth > 0) {
			var transformDepthTuple = new Tuple<Transform, int>(current.transform, 0);
			Queue<Tuple<Transform, int>> objectDepthQueue = new Queue<Tuple<Transform, int>>();
			TryEnqueueChildren(transformDepthTuple, searchParams, objectDepthQueue);
			GetComponentsXInQueue(searchParams, objectDepthQueue, components);
		}

		if(!searchParams.sortedTopFirst) components.Reverse();
		return components;
		
		static void GetComponentsXInQueue(ComponentSearchParams<T> searchParams, Queue<Tuple<Transform, int>> objectDepthQueue, List<T> components) {
			var queueCount = objectDepthQueue.Count;
			if(queueCount == 0) return;
			
			for(int i = 0; i < queueCount; i++) {
				var transformDepthTuple = objectDepthQueue.Dequeue();
				TryAddComponent(transformDepthTuple, searchParams, components);
				TryEnqueueChildren(transformDepthTuple, searchParams, objectDepthQueue);
			}
			GetComponentsXInQueue(searchParams, objectDepthQueue, components);
		}

		
		static void TryAddComponent (Tuple<Transform, int> objectDepthTuple, ComponentSearchParams<T> searchParams, List<T> components) {
			if(objectDepthTuple.Item2 >= searchParams.startDepth && objectDepthTuple.Item2 <= searchParams.endDepth) {
				var comp = objectDepthTuple.Item1.GetComponent<T>();
				if(comp != null && (searchParams.predicate == null || searchParams.predicate(comp))) {
					components.Add(comp);
				}
			}
		}
	}

	public static T GetComponentX<T>(this Component current, ComponentSearchParams<T> searchParams) where T : Component {
		T component = null;

		// Try to add components from ancestors
		if(searchParams.startDepth < 0) {
			Stack<Tuple<Transform, int>> objectDepthStack = new Stack<Tuple<Transform, int>>();
			
			// When searching parents, start from the top level object. We add objects to a stack until hitting the root/search limit, and then pop them in order.
			var transformDepthTuple = new Tuple<Transform, int>(current.transform, 0);

			for(int i = 0; i < (searchParams.startDepth == int.MinValue ? int.MaxValue : -searchParams.startDepth); i++) {
				if(transformDepthTuple.Item1.parent != null) {
					transformDepthTuple = new Tuple<Transform, int>(transformDepthTuple.Item1.parent, transformDepthTuple.Item2-1);
					objectDepthStack.Push(transformDepthTuple);
				} else {
					break;
				}
			}
			for(int i = 0; i < objectDepthStack.Count; i++) {
				transformDepthTuple = objectDepthStack.Pop();
				if(TryGetComponent(transformDepthTuple, searchParams, ref component)) return component;
				i--;
			}
		}
		// Try to add ourselves
		{
			var transformDepthTuple = new Tuple<Transform, int>(current.transform, 0);
			if(TryGetComponent(transformDepthTuple, searchParams, ref component)) return component;
		}
		// Try to add components from children
		if(searchParams.endDepth > 0) {
			var transformDepthTuple = new Tuple<Transform, int>(current.transform, 0);
			Queue<Tuple<Transform, int>> objectDepthQueue = new Queue<Tuple<Transform, int>>();
			TryEnqueueChildren(transformDepthTuple, searchParams, objectDepthQueue);
			if(GetComponentsXInQueue(searchParams, objectDepthQueue, ref component)) return component;
		}

		return null;
		
		static bool GetComponentsXInQueue(ComponentSearchParams<T> searchParams, Queue<Tuple<Transform, int>> objectDepthQueue, ref T component) {
			var queueCount = objectDepthQueue.Count;
			if(queueCount == 0) return false;
			
			for(int i = 0; i < queueCount; i++) {
				var transformDepthTuple = objectDepthQueue.Dequeue();
				if(TryGetComponent(transformDepthTuple, searchParams, ref component)) return true;
				TryEnqueueChildren(transformDepthTuple, searchParams, objectDepthQueue);
			}
			return GetComponentsXInQueue(searchParams, objectDepthQueue, ref component);
		}

		static bool TryGetComponent (Tuple<Transform, int> objectDepthTuple, ComponentSearchParams<T> searchParams, ref T component) {
			if(objectDepthTuple.Item2 >= searchParams.startDepth && objectDepthTuple.Item2 <= searchParams.endDepth) {
				var comp = objectDepthTuple.Item1.GetComponent<T>();
				if(comp != null && (searchParams.predicate == null || searchParams.predicate(comp))) {
					component = comp;
					return true;
				}
			}
			return false;
		}
	}

	static void TryEnqueueChildren<T> (Tuple<Transform, int> transformDepthTuple, ComponentSearchParams<T> searchParams, Queue<Tuple<Transform, int>> objectDepthQueue) {
		var childDepth = transformDepthTuple.Item2+1;
		if(childDepth > searchParams.endDepth) return;
		for(int i = 0; i < transformDepthTuple.Item1.childCount; i++) {
			// If sortedTopFirst is set we reverse the list before returning. This has the effect of also reversing the order of the breadth-first search, so we enqueue objects in reverse to compensate.
			var child = searchParams.sortedTopFirst ? transformDepthTuple.Item1.GetChild(i) : transformDepthTuple.Item1.GetChild((transformDepthTuple.Item1.childCount-1)-i);
			if(searchParams.includeInactive || child.gameObject.activeInHierarchy)
				objectDepthQueue.Enqueue(new Tuple<Transform, int>(child, childDepth));
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
			Debug.LogError(typeof(T) + ": is not an actual interface!");
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
			Debug.LogError(typeof(T) + ": is not an actual interface!");
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
			Debug.LogError(typeof(T) + ": is not an actual interface!");
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
			Debug.LogError(typeof(T) + ": is not an actual interface!");
			return null;
		}
		#endif
		return inObj.GetComponentsInChildren<Component>().OfType<T>();
	}
}
