using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GameObjectX {

	/// <summary>
	/// Sets the name if different to current.
	/// Editor selection issues can arise if you constantly set the name of a gameobject, which this avoids doing.
	/// </summary>
	/// <param name="gameObject">Game object.</param>
	/// <param name="newName">New name.</param>
	public static void SetNameIfChanged (this GameObject gameObject, string newName) {
		if(gameObject.name != newName) gameObject.name = newName;
	}

	/// <summary>
	/// Sets the layer on the gameobject and all children recursively.
	/// </summary>
	/// <param name="gameObject">Game object.</param>
	/// <param name="layer">Layer.</param>
	public static void SetLayerRecursively(this GameObject gameObject, int layer) {
		if(gameObject.layer != layer) gameObject.layer = layer;
		foreach(Transform t in gameObject.transform)
			t.gameObject.SetLayerRecursively(layer);
	}
	
	/// <summary>
	/// Gets or add a component. Usage example:
	/// BoxCollider boxCollider = gameObject.GetOrAddComponent<BoxCollider>();
	/// </summary>
	static public T GetOrAddComponent<T> (this GameObject child) where T : Component {
		T result = child.GetComponent<T>();
		if (result == null) {
			result = child.AddComponent<T>();
		}
		return result;
	}
	
	public static T[] GetComponentsInChildrenWithTag<T>(this GameObject gameObject, string tag) where T: Component {
		List<T> results = new List<T>();
		
		if(gameObject.CompareTag(tag))
			results.Add(gameObject.GetComponent<T>());
		
		foreach(Transform t in gameObject.transform)
			results.AddRange(t.gameObject.GetComponentsInChildrenWithTag<T>(tag));
		
		return results.ToArray();
	}
	
	public static int GetCollisionMask(this GameObject gameObject, int layer = -1) {
		if(layer == -1)
			layer = gameObject.layer;
		
		int mask = 0;
		for(int i = 0; i < 32; i++)
			mask |= (Physics.GetIgnoreLayerCollision(layer, i) ? 0 : 1) << i;
		
		return mask;
	}
}