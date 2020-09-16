using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ObjectX {
	
	public static bool IsNull (this UnityEngine.Object obj) {
		return obj == null || obj.Equals(null);
	}
	

	/// <summary>
	/// Destroys an object using the correct function for the play mode state.
	/// Uses Destroy in play mode, DestroyImmediate in editor.
	/// </summary>
	/// <param name="o">O.</param>
	public static void DestroyAutomatic(Object o) {
		#if UNITY_EDITOR
		if(Application.isPlaying)
			UnityEngine.Object.Destroy (o);
		else
			UnityEngine.Object.DestroyImmediate (o);
		#else
		UnityEngine.Object.Destroy (o);
		#endif

	}

	/// <summary>
	/// Gets the transform from object.
	/// Necessary because GameObject and Component both inherit Object, but independantly implement Transform.
	///	This means that another class inheriting from Object might not have a Transform.
	/// </summary>
	/// <returns>The transform from object.</returns>
	/// <param name="source">Source.</param>
	private static Transform GetTransformFromObject(Object source) {
		if (source is GameObject) {
			return (source as GameObject).transform;
		} else if (source is Component) {
			return (source as Component).transform;
		} else {
			return null;
		}
	}
}