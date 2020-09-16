using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Vector4X {
	public static IEnumerable<float> AsEnumerable(this Vector4 vector) {
		yield return vector.x;
		yield return vector.y;
		yield return vector.z;
		yield return vector.w;
    }

	/// <summary>
	/// Sets the value of the W component.
	/// </summary> 
	public static Vector4 SetW (this Vector4 v, float newW) {
		return new Vector4(v.x, v.y, v.z, newW);
	}
	public static Quaternion ToQuaternion(this Vector4 vec) {
		return new Quaternion(vec.x, vec.y, vec.z, vec.w);
	}
}
