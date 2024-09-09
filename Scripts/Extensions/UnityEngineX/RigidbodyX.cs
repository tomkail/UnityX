using UnityEngine;

public static class RigidbodyX {
	public static void ResetForcesImmediate (this Rigidbody rigidbody) {
		rigidbody.isKinematic = true;
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
		rigidbody.isKinematic = false;
	}

	public static void TeleportAndResetForcesImmediate (this Rigidbody rigidbody, Vector3 position, Quaternion rotation) {
		rigidbody.isKinematic = true;
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;

		rigidbody.position = position;
		rigidbody.rotation = rotation;
		rigidbody.isKinematic = false;
	}

	/// <summary>
	/// Manually updates the transform from the rigidbody. Useful in making sure that transform functions such as TransformPoint work correctly in a fixedupdate frame.
	/// </summary>
	/// <param name="rigidbody">Rigidbody.</param>
	public static void UpdateTransform (this Rigidbody rigidbody)
	{
		rigidbody.transform.position = rigidbody.position;
		rigidbody.transform.rotation = rigidbody.rotation;
	}
	
	public static Vector3 GetEulerAngles (this Rigidbody rigidbody)
	{
		return rigidbody.rotation.eulerAngles;
	}
	
	public static void SetEulerAngles (this Rigidbody rigidbody, Vector3 value)
	{
		rigidbody.rotation = Quaternion.Euler (value);
	}
	
	
	public static Vector3 GetUp (this Rigidbody rigidbody)
	{
		return rigidbody.rotation * Vector3.up;
	}
	
	public static void SetUp (this Rigidbody rigidbody, Vector3 value)
	{
		rigidbody.rotation = Quaternion.FromToRotation (Vector3.up, value);
	}
	
	public static Vector3 GetRight (this Rigidbody rigidbody)
	{
		return rigidbody.rotation * Vector3.right;
	}
	
	public static void SetRight (this Rigidbody rigidbody, Vector3 value)
	{
		rigidbody.rotation = Quaternion.FromToRotation (Vector3.right, value);
	}
	
	public static Vector3 GetForward (this Rigidbody rigidbody)
	{
		return rigidbody.rotation * Vector3.forward;
	}
	
	public static void SetForward (this Rigidbody rigidbody, Vector3 value)
	{
		rigidbody.rotation = Quaternion.FromToRotation (Vector3.forward, value);
	}

	public static Vector3 GetBack (this Rigidbody rigidbody)
	{
		return rigidbody.rotation * Vector3.back;
	}
	
	public static void SetBack (this Rigidbody rigidbody, Vector3 value)
	{
		rigidbody.rotation = Quaternion.FromToRotation (Vector3.back, value);
	}
	
	
	public static void Translate (this Rigidbody rigidbody, Vector3 translation, Transform relativeTo)
	{
		if (relativeTo)
		{
			rigidbody.MovePosition(rigidbody.position + relativeTo.TransformDirection (translation));
		}
		else
		{
			rigidbody.MovePosition(rigidbody.position + translation);
		}
	}
	
	public static void Translate (this Rigidbody rigidbody, float x, float y, float z, Transform relativeTo)
	{
		rigidbody.Translate (new Vector3 (x, y, z), relativeTo);
	}
	
	public static void Translate (this Rigidbody rigidbody, float x, float y, float z, Space relativeTo = Space.Self)
	{
		rigidbody.Translate (new Vector3 (x, y, z), relativeTo);
	}
	
	public static void Translate (this Rigidbody rigidbody, Vector3 translation, Space relativeTo = Space.Self)
	{
		if (relativeTo == Space.World)
		{
			rigidbody.MovePosition(rigidbody.position + translation);
		}
		else
		{	
			// We have to manually update the transform before we can use transformdirection, in case the rigidbody has already been changed in this fixedupdate frame.
			rigidbody.UpdateTransform();
			rigidbody.MovePosition(rigidbody.position + rigidbody.transform.TransformDirection (translation));
		}
	}
	
	
	/// <summary>
	/// Rotates the rigidbody about axis passing through point in world coordinates by angle degrees.
	///
	/// This modifies both the position and the rotation of the rigidbody.
	/// </summary>
	/// <param name="point">Point.</param>
	/// <param name="axis">Axis.</param>
	/// <param name="angle">Angle.</param>
	public static void Rotate (this Rigidbody rigidbody, Vector3 eulerAngles, Space relativeTo = Space.Self) {
		rigidbody.MoveRotation(rigidbody.rotation.Rotate(Quaternion.Euler(eulerAngles), relativeTo));
	}
	
	/// <summary>
	/// Rotates the rigidbody about axis passing through point in world coordinates by angle degrees.
	///
	/// This modifies both the position and the rotation of the rigidbody.
	/// </summary>
	/// <param name="point">Point.</param>
	/// <param name="axis">Axis.</param>
	/// <param name="angle">Angle.</param>
	public static void Rotate (this Rigidbody rigidbody, Vector3 axis, float angle, Space relativeTo = Space.Self) {
		rigidbody.MoveRotation(rigidbody.rotation.Rotate(Quaternion.AngleAxis (angle, axis), relativeTo));
	}
	
	/// <summary>
	/// Rotates the rigidbody about axis passing through point in world coordinates by angle degrees.
	///
	/// This modifies both the position and the rotation of the rigidbody.
	/// </summary>
	/// <param name="point">Point.</param>
	/// <param name="axis">Axis.</param>
	/// <param name="angle">Angle.</param>
	public static void RotateAround (this Rigidbody rigidbody, Vector3 point, Vector3 axis, float angle) {
		rigidbody.UpdateTransform();
		Vector3 vector = rigidbody.position;
		Quaternion rotation = Quaternion.AngleAxis (angle, axis);
		Vector3 vector2 = vector - point;
		vector2 = rotation * vector2;
		vector = point + vector2;
		rigidbody.MovePosition(vector);
		rigidbody.Rotate (axis, angle, Space.World);
	}
}