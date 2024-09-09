using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class TransformX {
	///
	///Transform properties
	///
	
	/// <summary>
	/// Determines if the transform has default local values. If true, calling ResetTransform will change nothing.
	/// </summary>
	/// <returns><c>true</c> if is default the specified trans; otherwise, <c>false</c>.</returns>
	/// <param name="trans">Trans.</param>
	public static bool IsDefault(this Transform trans) {
		return trans.localPosition == Vector3.zero && trans.localRotation == Quaternion.identity && trans.localScale == Vector3.one;
	}

	/// <summary>
	/// Resets the transform locally.
	/// </summary>
	/// <param name="trans">Trans.</param>
	public static void ResetTransform(this Transform trans) {
		trans.localPosition = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = Vector3.one;
	}

	public static void SetPositionSelf (this Transform t, Vector3 newPosition) {
		if(newPosition == t.position) return;
		t.TranslateSelf(newPosition - t.position);
	}
	public static void TranslateSelf (this Transform t, Vector3 translation, Space relativeTo = Space.World) {
		if(translation == Vector3.zero) return;
		t.Translate(translation, relativeTo);
		foreach(Transform child in t) {
			if(relativeTo == Space.World) {
				child.Translate(-translation, Space.World);
			} else {
				child.Translate(-translation, t);
			}
		}
	}

	public static void SetPositionX(this Transform t, float newX) {
		t.position = new Vector3(newX, t.position.y, t.position.z);
	}
 
	public static void SetPositionY(this Transform t, float newY) {
		t.position = new Vector3(t.position.x, newY, t.position.z);
	}
 
	public static void SetPositionZ(this Transform t, float newZ) {
		t.position = new Vector3(t.position.x, t.position.y, newZ);
	}
	
	public static void SetLocalPositionX(this Transform t, float newX) {
		t.localPosition = new Vector3(newX, t.localPosition.y, t.localPosition.z);
	}
	
	public static void SetLocalPositionY(this Transform t, float newY) {
		t.localPosition = new Vector3(t.localPosition.x, newY, t.localPosition.z);
	}
	
	public static void SetLocalPositionZ(this Transform t, float newZ) {
		t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, newZ);
	}


	public static void SetEulerAnglesX(this Transform t, float newX) {
		t.eulerAngles = new Vector3(newX, t.eulerAngles.y, t.eulerAngles.z);
	}
	
	public static void SetEulerAnglesY(this Transform t, float newY) {
		t.eulerAngles = new Vector3(t.eulerAngles.x, newY, t.eulerAngles.z);
	}
	
	public static void SetEulerAnglesZ(this Transform t, float newZ) {
		t.eulerAngles = new Vector3(t.eulerAngles.x, t.eulerAngles.y, newZ);
	}

	public static void SetLocalEulerAnglesX(this Transform t, float newX) {
		t.localEulerAngles = new Vector3(newX, t.localEulerAngles.y, t.localEulerAngles.z);
	}
	
	public static void SetLocalEulerAnglesY(this Transform t, float newY) {
		t.localEulerAngles = new Vector3(t.localEulerAngles.x, newY, t.localEulerAngles.z);
	}
	
	public static void SetLocalEulerAnglesZ(this Transform t, float newZ) {
		t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, newZ);
	}
	
	
	public static void SetLocalScaleX(this Transform t, float newX) {
		t.localScale = new Vector3(newX, t.localScale.y, t.localScale.z);
	}
	
	public static void SetLocalScaleY(this Transform t, float newY) {
		t.localScale = new Vector3(t.localScale.x, newY, t.localScale.z);
	}
	
	public static void SetLocalScaleZ(this Transform t, float newZ) {
		t.localScale = new Vector3(t.localScale.x, t.localScale.y, newZ);
	}
	


	/// <summary>
	/// Transforms a rotation in local space into world space
	/// </summary>
	/// <returns>The <see cref="UnityEngine.Quaternion"/>.</returns>
	/// <param name="transform">Transform.</param>
	/// <param name="localRotation">Local Rotation.</param>
	public static Quaternion TransformRotation(this Transform transform, Quaternion localRotation) {
		return transform.rotation * localRotation;
	}

	/// <summary>
	/// Transforms a rotation in world space into local space
	/// </summary>
	/// <returns>The <see cref="UnityEngine.Quaternion"/>.</returns>
	/// <param name="transform">Transform.</param>
	/// <param name="worldRotation">World Rotation.</param>
	public static Quaternion InverseTransformRotation(this Transform transform, Quaternion worldRotation) {
		return Quaternion.Inverse(transform.rotation) * worldRotation;
	}

	/// <summary>
	/// Transforms a point in local space relative to fromTransform into local space relative to toTransform.
	/// </summary>
	/// <returns>The <see cref="UnityEngine.Vector3"/>.</returns>
	/// <param name="fromTransform">From transform.</param>
	/// <param name="toTransform">To transform.</param>
	/// <param name="localPoint">Local point.</param>
	public static Vector3 TransformPointTo(this Transform fromTransform, Vector3 localPoint, Transform toTransform) {
		return toTransform.InverseTransformPoint(fromTransform.TransformPoint(localPoint));
	}

	/// <summary>
	/// Similar to lossyScale, except it doesn't take rotation into account at all, it simply concatenates all the scales
	/// up the hiearchy.
	/// </summary>
	/// <returns>The scale.</returns>
	/// <param name="transform">Transform.</param>
	public static Vector3 OverallScale(this Transform transform)
	{
		var current = transform;
		var scale = transform.localScale;
		while(current.parent) {
			current = current.parent;
			scale = Vector3.Scale(scale, current.localScale);
		}
		return scale;
	}

	///
	///Paths
	///

	/// <summary>
	/// Recursively travels to the top-level parent, or the parent at level maxLevels and outputs a string of the hierarchy path of the transform
	/// </summary>
	public static string HierarchyPath (this Transform t, bool includeSceneName = true, int maxLevels = 0) {
		if(t == null) return "Transform was null";
		_hierarchyPathSB.Length = 0;
		if(includeSceneName) {
			_hierarchyPathSB.Append(t.gameObject.scene.name);
			_hierarchyPathSB.Append("/");
		}
		_hierarchyPathList.Clear();
		t.GetParents(_hierarchyPathList, maxLevels);
		for(int i = _hierarchyPathList.Count-1; i >= 0; i--) {
			_hierarchyPathSB.Append(_hierarchyPathList[i].name);
			_hierarchyPathSB.Append("/");
		}
		_hierarchyPathSB.Append(t.name);
		var str = _hierarchyPathSB.ToString();
		_hierarchyPathSB.Length = 0;
		_hierarchyPathList.Clear();
		return str;
	}
	static StringBuilder _hierarchyPathSB = new(); 		// reuse to avoid excessive allocations
	static List<Transform> _hierarchyPathList = new();	// reuse to avoid excessive allocations

	/// <summary>
	/// Destroys all children.
	/// </summary>
	/// <param name="transform">The parent transform.</param>
	public static void DestroyAllChildren (this Transform transform, bool includeInactive = true) {
		for (int i = transform.childCount-1; i >= 0; i--) {
			if(includeInactive || transform.GetChild(i).gameObject.activeInHierarchy)
				Object.Destroy(transform.GetChild(i).gameObject);
		}
	}
	
	public static void DestroyAllChildrenImmediate (this Transform transform) {
		for (int i = transform.childCount-1; i >= 0; i--) {
			Object.DestroyImmediate(transform.GetChild(i).gameObject);
		}
	}

	public static void DestroyAllChildrenAutomatic (this Transform transform) {
		#if UNITY_EDITOR
		if(Application.isPlaying) transform.DestroyAllChildren();
		else transform.DestroyAllChildrenImmediate();
		#else
		transform.DestroyAllChildren();
		#endif
	}
	
	public static List<Transform> GetAllAncestors (this Transform _transform) {
		List<Transform> ancestors = new List<Transform>();
		while (_transform.parent != null) {
			_transform = _transform.parent;
			ancestors.Add(_transform);
	    }
	    return ancestors;
	}

	public static List<Transform> GetAllDescendents(this Transform current) {
		List<Transform> transforms = new List<Transform>();
		GetAllDescendents(current, transforms);
		return transforms;
	}

	static List<Transform> GetAllDescendents(Transform current, List<Transform> transforms = null) {
		if(transforms == null) transforms = new List<Transform>();
		transforms.Add(current);
		for (int i = 0; i < current.childCount; ++i) {
			GetAllDescendents(current.GetChild(i), transforms);
		}
		return transforms;
	}

	/// <summary>
	/// Gets the children of the transform. 
	/// Remember that you can also loop through children using Transform's enumerator: foreach (Transform child in transform)
	/// </summary>
	/// <returns>The children.</returns>
	/// <param name="current">Current.</param>
	public static Transform[] GetChildren(this Transform current) {
		Transform[] children = new Transform[current.childCount];
		for (int i = 0; i < children.Length; i++) {
			children[i] = current.GetChild(i);
		}
		return children;
	}
	
	/// <summary>
	/// Gets the siblings of a transform.
	/// </summary>
	/// <returns>The siblings.</returns>
	/// <param name="current">Current.</param>
	public static Transform[] GetSiblings(this Transform current) {
		Transform[] siblingsIncludingSelf = current.parent.GetChildren();
		Transform[] siblings = new Transform[siblingsIncludingSelf.Length - 1];
		int siblingsIndex = 0;
		for (int i = 0; i < siblingsIncludingSelf.Length; i++) {
			if(siblingsIncludingSelf[i] != current) {
				siblings[siblingsIndex] = siblingsIncludingSelf[i];
				siblingsIndex++;
			}
		}
		return siblings;
	}
	
	
	///
	///Finding
	///

	/// <summary>
	/// Recursively travels to the top-level parent, or the parent at level maxLevels
	/// </summary>
	public static Transform GetParent (this Transform t, int maxLevels = 0) {
		maxLevels--;
		if(maxLevels == 0) {
			return t.parent;
		} else if(t.parent != null) {
			return GetParent(t.parent, maxLevels);
		} else {
			return t;
		}
	}

	/// <summary>
	/// Recursively travels to the top-level parent, or the parent at level maxLevels
	/// (No allocs, uses passed in list)
	/// </summary>
	public static void GetParents(this Transform t, IList<Transform> parents, int maxLevels = 0)
	{
		bool hasMaxLevels = maxLevels > 0;
		var p = t.parent;
		while(p != null && (maxLevels > 0 || !hasMaxLevels)) {
			parents.Add(p);
			p = p.parent;
			maxLevels--;
		}
	}

	/// <summary>
	/// Recursively travels to the top-level parent, or the parent at level maxLevels
	/// </summary>
	public static int GetNumParents (this Transform t) {
		int count = 0;
		var p = t.parent;
		while(p != null) {
			p = p.parent;
			count++;
		}
		return count;
	}

	/// <summary>
	/// Finds all transforms in the descendants of the transform with a specified name.
	/// </summary>
	/// <returns>The all in children.</returns>
	/// <param name="current">Current.</param>
	/// <param name="name">Name.</param>
	public static Transform[] FindAllInChildren(this Transform current, string name) {
		return current.FindAllInChildrenList(name).ToArray();
	}

	public static List<Transform> FindAllInChildrenList(this Transform current, string name, List<Transform> transforms = null) {
		if(transforms == null) transforms = new List<Transform>();
		//current.name.Contains(name)
		if (current.name == name)
			transforms.Add(current);
		for (int i = 0; i < current.childCount; ++i) {
			current.GetChild(i).FindAllInChildrenList(name, transforms);
		}
		return transforms;
	}

	public static bool IsDescendentOf(this Transform current, Transform ancestor) {
		if(current != null) {
			if(current == ancestor) return true;
			return IsDescendentOf(current.parent, ancestor);
		} else {
			return false;
		}
	}

	// Gets the index of the object in the global heirarchy, traversing depth if a given transform has children.
	public static int GetHeirarchyIndex(this Transform current) {
		var index = 0;
		foreach(var ancestor in current.GetAllAncestors()) index += ancestor.transform.GetSiblingIndex()+1;
		index += current.GetSiblingIndex();
		return index;
	}

	/// <summary>
	/// Find the nearest component in a list to this Transform in 3d space, returning the distance found.
	/// </summary>
	public static T Nearest<T>(this Transform current, IEnumerable<T> objectList, out float distance) where T : Component
	{
		var currPos = current.position;

		float nearestDist = float.MaxValue;
		T nearestObj = null;
		foreach(var obj in objectList) {
			float dist = Vector3.Distance(currPos, obj.transform.position);
			if( nearestObj == null || dist < nearestDist) {
				nearestObj = obj;
				nearestDist = dist;
			}
		}

		distance = nearestDist;

		return nearestObj;
	}

	/// <summary>
	/// Find the nearest component in a list to this Transform in 3d space.
	/// </summary>
	public static T Nearest<T>(this Transform current, IEnumerable<T> objectList) where T : Component
	{
		float distance;
		return Nearest(current, objectList, out distance);
	}

	/// <summary>
	/// Creates vertices of edges from the transform data.
	/// More accurate than Renderer.bounds, as Bounds remove rotation data.
	/// </summary>
	/// <returns>The bounds.</returns>
	public static Vector3[] GetVertices (this Transform transform) {
		Vector3 halfLocalScale = 	transform.localScale * 0.5f;
		Vector3 leftTopFront = 		transform.position + transform.rotation * new Vector3(-halfLocalScale.x, -halfLocalScale.y, halfLocalScale.z);
		Vector3 rightTopFront = 	transform.position + transform.rotation * new Vector3(halfLocalScale.x, -halfLocalScale.y, halfLocalScale.z);
		Vector3 leftTopBack = 		transform.position + transform.rotation * new Vector3(-halfLocalScale.x, -halfLocalScale.y, -halfLocalScale.z);
		Vector3 rightTopBack = 		transform.position + transform.rotation * new Vector3(halfLocalScale.x, -halfLocalScale.y, -halfLocalScale.z);
		Vector3 leftBottomFront = 	transform.position + transform.rotation * new Vector3(-halfLocalScale.x, halfLocalScale.y, halfLocalScale.z);
		Vector3 rightBottomFront = 	transform.position + transform.rotation * new Vector3(halfLocalScale.x, halfLocalScale.y, halfLocalScale.z);
		Vector3 leftBottomBack = 	transform.position + transform.rotation * new Vector3(-halfLocalScale.x, halfLocalScale.y, -halfLocalScale.z);
		Vector3 rightBottomBack = 	transform.position + transform.rotation * new Vector3(halfLocalScale.x, halfLocalScale.y, -halfLocalScale.z);
		return new Vector3[8]{leftTopFront, rightTopFront, leftTopBack, rightTopBack, leftBottomFront, rightBottomFront, leftBottomBack, rightBottomBack};
	}

	// Gets world space axis-aligned bounds
	public static Bounds GetBounds (this Transform transform) {
		Vector3 halfLocalScale = transform.localScale * 0.5f;
		return BoundsX.CreateEncapsulating(transform.position + transform.rotation * new Vector3(-halfLocalScale.x, -halfLocalScale.y, -halfLocalScale.z), transform.position + transform.rotation * new Vector3(halfLocalScale.x, halfLocalScale.y, halfLocalScale.z));
	}
}