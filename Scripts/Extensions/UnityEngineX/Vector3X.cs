using System.Collections.Generic;
using UnityEngine;

public static class Vector3X {

	public static Vector3 Average(this IEnumerable<Vector3> source) {
        var total = Vector3.zero;
        int num = 0;
        foreach(var vector in source) {
            total += vector;
            num++;
        }
        return total / num;
	}

	public static IEnumerable<float> AsEnumerable(this Vector3 vector) {
		yield return vector.x;
		yield return vector.y;
		yield return vector.z;
    }

	/// <summary>
	/// Returns a half.
	/// </summary>
	/// <value>The half.</value>
	public static Vector3 half => new(0.5f, 0.5f, 0.5f);

	/// <summary>
	/// Returns direction from a to b.
	/// </summary>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	public static Vector3 FromTo(Vector3 a, Vector3 b){
		return b - a;
	}

	/// <summary>
	/// Changes the direction of the vector without affecting magnitude. Magnitude of direction vector is irrelevant.
	/// </summary>
	/// <returns>The rotated vector.</returns>
	/// <param name="vector">Vector.</param>
	/// <param name="newDirection">New direction.</param>
	public static Vector3 InDirection(Vector3 vector, Vector3 newDirection){
		return newDirection.normalized * vector.magnitude;
	}
	
	/// <summary>
	/// Returns normalized direction from a to b.
	/// </summary>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	public static Vector3 NormalizedDirection(Vector3 a, Vector3 b){
		return FromTo(a, b).normalized;
	}

	/// <summary>
	/// Takes the reciprocal of each component in the vector.
	/// </summary>
	/// <param name="value">The vector to take the reciprocal of.</param>
	/// <returns>A vector that is the reciprocal of the input vector.</returns>
	public static Vector3 Reciprocal(this Vector3 v) {
		return new Vector3(1f / v.x, 1f / v.y, 1f / v.z);
	}

	/// <summary>
	/// Reflects a vector off the plane defined by a normal, also using an optional coefficient of restituion.
	/// </summary>
	/// <param name="inDirection">In direction.</param>
	/// <param name="inNormal">In normal.</param>
	/// <param name="coeficientOfRestitution">Coeficient of restitution.</param>
	public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal, float coefficientOfRestitution = 1.0f) {
		return (2 * Vector3.Project(inDirection, inNormal.normalized) - inDirection) * coefficientOfRestitution;
	}

	public static Vector3 ProjectOnPlane(Vector3 vector, Plane plane) {
		float distanceToIntersection = plane.GetDistanceToPoint(vector);
		return vector - plane.normal * distanceToIntersection;
	}

	public static float SqrDistance (Vector3 a, Vector3 b) {
		return (a.x-b.x) * (a.x-b.x) + (a.y-b.y) * (a.y-b.y) + (a.z-b.z) * (a.z-b.z);
	}
	
	/// <summary>
	/// Returns the distance between two vectors in a specific direction using projection. The sign of the direction is ignored.
	/// For example, if the direction is forward and a is (0,0,0) and b is (0,2,1), the function will return 1, as the upwards component of b is ignored by the forward direction.
	/// </summary>
	/// <returns>The in direction.</returns>
	/// <param name="a">The first vector.</param>
	/// <param name="b">The second vector.</param>
	/// <param name="direction">Direction.</param>
	public static float DistanceInDirection (Vector3 fromVector, Vector3 toVector, Vector3 direction) {
		return SignedDistanceInDirection(fromVector, toVector, direction).Abs();
	}


	/// <summary>
	/// Returns the signed distance between two vectors in a specific direction using projection. The sign of the direction is ignored.
	/// For example, if the direction is forward and a is (0,0,0) and b is (0,2,-1), the function will return -1, as the upwards component of b is ignored by the forward direction.
	/// </summary>
	/// <returns>The in direction.</returns>
	/// <param name="a">The first vector.</param>
	/// <param name="b">The second vector.</param>
	/// <param name="direction">Direction.</param>
	public static float SignedDistanceInDirection (Vector3 fromVector, Vector3 toVector, Vector3 direction) {
		Vector3 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
		return Vector3.Dot(FromTo(fromVector, toVector), normalizedDirection);
	}

	public static float SqrDistanceAgainstDirection (Vector3 fromVector, Vector3 toVector, Vector3 direction) {
        Vector3 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
		var projected = Vector3.ProjectOnPlane(FromTo(fromVector, toVector), normalizedDirection);
		return projected.sqrMagnitude;
    }

	// Find the distance that "fromVector" to "toVector" travels away from a given direction vector.
	public static float DistanceAgainstDirection (Vector3 fromVector, Vector3 toVector, Vector3 direction) {
		Vector3 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
		var projected = Vector3.ProjectOnPlane(FromTo(fromVector, toVector), normalizedDirection);
		return projected.magnitude;
	}

	// Returns the distance between two vectors, projected against the specified direction; 
	// additionally signed by if the direction between the two vectors and the specified direction face the same way
	public static float SignedDistanceAgainstDirection (Vector3 fromVector, Vector3 toVector, Vector3 direction) {
		Vector3 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
		var fromTo = FromTo(fromVector, toVector);
		var projected = Vector3.ProjectOnPlane(fromTo, normalizedDirection);
		return projected.magnitude * Vector3.Dot(fromTo, normalizedDirection).Sign();
	}

	// A signed dot of two directions flattened against a plane. I'm not sure what this is used for any more. Depreciate?
	public static float SignedDirectionAgainstDirection (Vector3 fromVector, Vector3 toVector, Vector3 direction) {
		Vector3 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
		var projectedForward = Vector3.ProjectOnPlane(fromVector, normalizedDirection);
		var projectedTest = Vector3.ProjectOnPlane(toVector, normalizedDirection);
		var output = Vector3.Dot(projectedForward, projectedTest);

		// var projected = Vector3.ProjectOnPlane(Vector3X.FromTo(fromVector, toVector), normalizedDirection);
		// var output2 = projected.magnitude * Vector3.Dot(Vector3X.FromTo(fromVector, toVector), normalizedDirection).Sign();
		// Debug.Assert(MathX.NearlyEqual(output,output2), output.ToString()+" "+output2);
		return output;
	}

	/// <summary>
	/// The angle between two directions projected against a direction
	/// </summary>
	/// <returns>The in direction.</returns>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	/// <param name="direction">Direction.</param>
	public static float DegreesAgainstDirection (Vector3 a, Vector3 b, Vector3 direction) {
		Vector3 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
		Vector3 projectedA = Vector3.ProjectOnPlane(a, normalizedDirection).normalized;
		Vector3 projectedB = Vector3.ProjectOnPlane(b, normalizedDirection).normalized;
		return Vector3.Angle(projectedA, projectedB);
	}
	public static float SignedDegreesAgainstDirection (Vector3 a, Vector3 b, Vector3 direction) {
		Vector3 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
		Vector3 projectedA = Vector3.ProjectOnPlane(a, normalizedDirection).normalized;
		Vector3 projectedB = Vector3.ProjectOnPlane(b, normalizedDirection).normalized;
		return Vector3.SignedAngle(projectedA, projectedB, direction);
	}

	/// <summary>
	/// Find distance between two points on the XZ plane, ignoring the Y component.
	/// </summary>
	public static float DistanceXZ(Vector3 v1, Vector3 v2) {
		return (v2-v1).WithY(0.0f).magnitude;
	}

	/// <summary>
	/// Create a vector on the XZ plane pointing in a particular direction.
	/// </summary>
	public static Vector3 CreateXZWithDegrees(float degrees) {
		float radians = degrees * Mathf.Deg2Rad;
		float sin = Mathf.Sin(radians);
		float cos = Mathf.Cos(radians);
		return new Vector3(sin, 0.0f, cos);
	}
	
	/// <summary>
	/// Returns a Vector3 with all components made absolute.
	/// </summary>
	/// <param name="v">V.</param>
	public static Vector3 Abs( this Vector3 v ){
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	/// <summary>
	/// Rotates the specified Vector3 as if it were a eulerAngle. 
	/// Shortcut to Quaternion.Rotate.
	/// </summary>
	/// <param name="rotation">Rotation.</param>
	/// <param name="eulerAngles">Euler angles.</param>
	/// <param name="space">Space.</param>
	public static Vector3 Rotate (this Vector3 rotation, Vector3 eulerAngles, Space space = Space.Self) {
		return (Quaternion.Euler(rotation).Rotate(eulerAngles, space)).eulerAngles;
	}

	/// <summary>
	/// Rotates the given vector by 90° degrees on the XZ plane (Y axis), very efficiently.
	/// </summary>
	public static Vector3 RotateRight90(this Vector3 vec)
	{
		return new Vector3(vec.z, vec.y, -vec.x);
	}

	/// <summary>
	/// Rotates the given vector by 90° degrees on the XZ plane (Y axis), very efficiently.
	/// </summary>
	public static Vector3 RotateLeft90(this Vector3 vec)
	{
		return new Vector3(-vec.z, vec.y, vec.x);
	}
	
	public static Vector3 RotateX(this Vector3 v,  float degrees) {
		float sin = Mathf.Sin( degrees * Mathf.Deg2Rad );
		float cos = Mathf.Cos( degrees * Mathf.Deg2Rad );
		float ty = v.y;
		float tz = v.z;
		v.y = (cos * ty) - (sin * tz);
		v.z = (cos * tz) + (sin * ty);

		return v;
	}

	public static Vector3 RotateY(this Vector3 v, float degrees) {
		float sin = Mathf.Sin( degrees * Mathf.Deg2Rad );
		float cos = Mathf.Cos( degrees * Mathf.Deg2Rad );
		float tx = v.x;
		float tz = v.z;
		v.x = (cos * tx) + (sin * tz);
		v.z = (cos * tz) - (sin * tx);

		return v;
	}

	public static Vector3 RotateZ (this Vector3 v, float degrees) {
		float sin = Mathf.Sin( degrees * Mathf.Deg2Rad );
		float cos = Mathf.Cos( degrees * Mathf.Deg2Rad );
		float tx = v.x;
		float ty = v.y;
		v.x = (cos * tx) - (sin * ty);
		v.y = (cos * ty) + (sin * tx);

		return v;
	}

	public static float GetPitch (this Vector3 v){
		float len = Mathf.Sqrt((v.x * v.x) + (v.z * v.z));    // Length on xz plane.
		return -Mathf.Atan2(v.y, len) * Mathf.Rad2Deg;
	}

	public static float GetYaw (this Vector3 v)  {
		return( Mathf.Atan2( v.x, v.z ) ) * Mathf.Rad2Deg;
	}

	public static Vector2 GetPitchAndYaw (this Vector3 v)  {
		return new Vector2(GetPitch(v), GetYaw(v));
	}

	public static Vector3 DirectionFromPitchAndYaw (float pitch, float yaw, float roll = 0)  {
		Quaternion rotation = Quaternion.Euler(new Vector3(pitch, yaw, roll));
		return rotation * Vector3.forward;
	}

	/// <summary>
	/// Returns a sign indicating the direction of forward relative to the target direction, rotating around axis up.
	/// </summary>
	/// <returns>The dir.</returns>
	/// <param name="fwd">Fwd.</param>
	/// <param name="targetDir">Target dir.</param>
	/// <param name="up">Up. The axis the rotation check is measured against</param>
	public static float AngleDirection(Vector3 forward, Vector3 targetDir, Vector3 up) {
		Vector3 side = Vector3.Cross(forward, up);
		float dir = Vector3.Dot(targetDir, side);

		if (dir > 0.0f) {
			return 1.0f;
		} else if (dir < 0.0f) {
			return -1.0f;
		} else {
			return 0.0f;
		}
	}


	
	public static Vector3 ClampMagnitudeInDirection (Vector3 vector, Vector3 direction, float clampValue) {
		float magnitudeAlongTangent = Vector3.Dot(vector, direction);
		if(Mathf.Abs(magnitudeAlongTangent) > clampValue) {
			float clampedSpeed = Mathf.Clamp(magnitudeAlongTangent, -clampValue, +clampValue);
			float speedDiff = clampedSpeed - magnitudeAlongTangent;
			vector += speedDiff * direction;
		}
		return vector;
	}

	public static Vector3 AddMagnitude(this Vector3 vec, float magnitudeOffset) {
		var mag = vec.magnitude;
		var vecNorm = vec / mag;
		return (mag+magnitudeOffset) * vecNorm;
	}
	
	// To Vector2
	/// <summary>
	/// Creates a Vector2 from a Vector3, using the X and Y components (in that order).
	/// </summary> 
	public static Vector2 XY (this Vector3 v) {
		return new Vector2(v.x,v.y);
	}

	/// <summary>
	/// Creates a Vector2 from a Vector3, using the X and Z components (in that order).
	/// </summary> 
	public static Vector2 XZ (this Vector3 v) {
		return new Vector2(v.x, v.z);
	}



	/// <summary>
	/// Sets the value of the X component.
	/// </summary> 
	public static Vector3 WithX (this Vector3 v, float newX) {
		return new Vector3(newX, v.y, v.z);
	}
 	
 	/// <summary>
	/// Sets the value of the Y component
	/// </summary> 
	public static Vector3 WithY (this Vector3 v, float newY) {
		return new Vector3(v.x, newY, v.z);
	}
 	
 	/// <summary>
	/// Sets the value of the Z component
	/// </summary> 
	public static Vector3 WithZ (this Vector3 v, float newZ) {
		return new Vector3(v.x, v.y, newZ);
	}	
	
	

	/// <summary>
	/// Adds a value to the X component
	/// </summary> 
	public static Vector3 AddX (this Vector3 v, float addX) {
		return new Vector3(v.x + addX, v.y, v.z);
	}
 	
 	/// <summary>
	/// Adds a value to the Y component
	/// </summary> 
	public static Vector3 AddY (this Vector3 v, float addY) {
		return new Vector3(v.x, v.y + addY, v.z);
	}
	
	/// <summary>
	/// Adds a value to the Z component
	/// </summary> 
	public static Vector3 AddZ (this Vector3 v, float addZ) {
		return new Vector3(v.x, v.y, v.z + addZ);
	}

	/// <summary>
	/// Creates a vector3 from a vector2 where the x and y components are mapped to x and z
	/// </summary>
	/// <param name="v">V.</param>
	public static Vector3 XZ(Vector2 v) {
		return new Vector3(v.x, 0, v.y);
	}

    //Similar to Unity's epsilon comparison, but allows for any precision.
	public static bool NearlyEqual(Vector3 a, Vector3 b, float maxDifference = 0.001f) {
		if (a == b)  { 
			return true;
		} else {
			return 
				MathX.Difference(a.x, b.x) < maxDifference && 
				MathX.Difference(a.y, b.y) < maxDifference && 
				MathX.Difference(a.z, b.z) < maxDifference;
	    }
	}

	/// <summary>
	/// Returns true if any of the components of the vector as NaN.
	/// </summary>
	public static bool HasNaN(Vector3 v) {
		return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
	}

	public static string ToString(this Vector3 v, int significantFigures) {
		return $"({v.x.ToString("F" + significantFigures)}, {v.y.ToString("F" + significantFigures)}, {v.z.ToString("F" + significantFigures)})";
	}
}