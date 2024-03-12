using System;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2X {

	public static Vector2 Average(this IEnumerable<Vector2> source) {
		var total = Vector2.zero;
        int num = 0;
        foreach(var vector in source) {
            total += vector;
            num++;
        }
        return total / num;
	}

	public static IEnumerable<float> AsEnumerable(this Vector2 vector) {
		yield return vector.x;
		yield return vector.y;
	}

	#region Static Members
	/// <summary>
	/// Returns the half.
	/// </summary>
	/// <value>The half.</value>
	public static Vector2 half => new(0.5f, 0.5f);

	#endregion


	#region Logging functions
	/// <summary>
	/// Produces a string from a Vector, with components rounded to a specified number of decimal places.
	/// </summary>
	/// <returns>The string.</returns>
	/// <param name="v">The vector.</param>
	/// <param name="numDecimalPlaces">Number decimal places.</param>
	public static string ToString(this Vector2 v, int numDecimalPlaces = 3){
		return ("("+v.x.RoundTo(numDecimalPlaces)+", "+v.y.RoundTo(numDecimalPlaces)+")");
	}
	#endregion

	#region Manipulation functions
	/// <summary>
	/// Component-wise multiplication of two vectors.
	/// </summary>
	public static Vector2 Multiply(Vector2 left, Vector2 right){
		return new Vector2(left.x * right.x, left.y * right.y);
	}

	/// <summary>
	/// Divides the first vector by the second component-wise.
	/// </summary>
	/// <param name="left">Left.</param>
	/// <param name="right">Right.</param>
	public static Vector2 Divide(Vector2 left, Vector2 right){
		return new Vector2(left.x / right.x, left.y / right.y);
	}
	#endregion

	/// <summary>
	/// Returns direction from a to b.
	/// </summary>
	/// <param name="a">The first component.</param>
	/// <param name="b">The second component.</param>
	public static Vector2 FromTo(Vector2 a, Vector2 b){
		return b - a;
	}

	/// <summary>
	/// Changes the direction of the vector without affecting magnitude. Magnitude of direction vector is irrelevant.
	/// </summary>
	/// <returns>The rotated vector.</returns>
	/// <param name="vector">Vector.</param>
	/// <param name="newDirection">New direction.</param>
	public static Vector2 InDirection(Vector2 vector, Vector2 newDirection){
		return newDirection.normalized * vector.magnitude;
	}

	/// <summary>
	/// Returns normalized direction from a to b.
	/// </summary>
	/// <param name="a">The first component.</param>
	/// <param name="b">The second component.</param>
	public static Vector2 NormalizedDirection(Vector2 a, Vector2 b){
		return FromTo(a, b).normalized;
	}

	static string[] headings = { "E", "NE", "N", "NW", "W", "SW", "S", "SE" };
	public static string DirectionName(this Vector2 dir) {
		if(dir == Vector2.zero) return string.Empty;
		float angle = Mathf.Atan2( dir.y, dir.x );
		int octant = Mathf.RoundToInt(8 * angle / (2*Mathf.PI) + 8 ) % 8;

		string dirStr = headings[octant];
		return dirStr;
	}


	/// <summary>
	/// Takes the reciprocal of each component in the vector.
	/// </summary>
	/// <param name="value">The vector to take the reciprocal of.</param>
	/// <returns>A vector that is the reciprocal of the input vector.</returns>
	public static Vector2 Reciprocal(this Vector2 v) {
		return new Vector2(1f / v.x, 1f / v.y);
	}
	
	/// <summary>
	/// Projects a vector onto another vector.
	/// </summary>
	/// <param name="vector">vector.</param>
	/// <param name="direction">direction.</param>
	public static Vector2 Project(Vector2 vector, Vector2 direction) {
		Vector2 directionNormalized = direction.normalized;	
		float projectedDist = Vector2.Dot(vector, directionNormalized);
		return directionNormalized * projectedDist;
	}
	public static Vector2 ProjectOnPlane(Vector2 vector, Vector2 planeNormal) {
		float sqrMag = Vector2.Dot(planeNormal, planeNormal);
		if (sqrMag < Mathf.Epsilon)
			return vector;
		else {
			var dot = Vector2.Dot(vector, planeNormal);
			return new Vector2(vector.x - planeNormal.x * dot / sqrMag, vector.y - planeNormal.y * dot / sqrMag);
		}
	}

	public static float SqrDistance (Vector2 a, Vector2 b) {
		return (a.x-b.x) * (a.x-b.x) + (a.y-b.y) * (a.y-b.y);
	}
	
	/// <summary>
	/// Returns the distance between two vectors in a specific direction using projection. The sign of the direction is ignored.
	/// For example, if the direction is forward and a is (0,0,0) and b is (0,2,1), the function will return 1, as the upwards component of b is ignored by the forward direction.
	/// </summary>
	/// <returns>The in direction.</returns>
	/// <param name="a">The first vector.</param>
	/// <param name="b">The second vector.</param>
	/// <param name="direction">Direction.</param>
	public static float DistanceInDirection (Vector2 fromVector, Vector2 toVector, Vector2 direction) {
		Vector2 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
		Vector2 projectedA = Project(fromVector, normalizedDirection);
		Vector2 projectedB = Project(toVector, normalizedDirection);
		return Vector2.Distance(projectedA, projectedB);
	}

	public static float SignedDistanceInDirection (Vector2 fromVector, Vector2 toVector, Vector2 direction) {
		Vector2 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
		return Vector2.Dot(FromTo(fromVector, toVector), normalizedDirection);
	}

	/// <summary>
	/// Rotate a 2d vector by a number of degrees, where clockwise is positive.
	/// </summary>
	public static Vector2 Rotate (Vector2 v, float degrees) {
		degrees *= Mathf.Deg2Rad;
		float sin = Mathf.Sin( -degrees );
		float cos = Mathf.Cos( -degrees );
		float tx = v.x;
		float ty = v.y;
		v.x = (cos * tx) - (sin * ty);
		v.y = (cos * ty) + (sin * tx);
		return v;
	}

	// Gets angle between a and b where 0 is up and pi/2 is right. Use Mathf.Rad2Deg to get degrees.
	public static float RadiansBetween(Vector2 a, Vector2 b) {
		return Vector2.SignedAngle(b-a, Vector2.up) * Mathf.Deg2Rad;
	}

	const float radiansFor90Degrees = Mathf.Deg2Rad * 90;
	// Gets angle of the direction of a where 0 is up and pi/2 is right. Use Mathf.Rad2Deg to get degrees.
	public static float Radians(Vector2 a) {
		return Vector2.SignedAngle(a, Vector2.up) * Mathf.Deg2Rad;
	}
	
	// Gets angle of the vector from a to b where 0 is up and 90 is right.
	public static float DegreesBetween(Vector2 a, Vector2 b) {
		return Vector2.SignedAngle(b-a, Vector2.up);
	}
	
	// Gets angle of the direction of a as degrees where 0 is up and 90 is right
	public static float Degrees(Vector2 a) {
		return Vector2.SignedAngle(a, Vector2.up);
	}

	public static Vector2 WithDegrees(float degrees) {
		var rad = degrees * Mathf.Deg2Rad;
		return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
	}
	

	public static Vector2 ClampMagnitudeInDirection (Vector2 vector, Vector2 direction, float clampValue, bool outwardsOnly = false) {
		
		direction = direction.normalized;
		float actualComponentInDirection = Vector2.Dot (vector, direction);
		float desiredComponentInDirection = Mathf.Clamp (actualComponentInDirection, -clampValue, clampValue);
		float requiredOffsetInDirection = desiredComponentInDirection - actualComponentInDirection;
		
		return vector + requiredOffsetInDirection * direction;
		
		// Hey Ben! This was what I was using before. Seems to have the same result as above.
		//		Vector2 projectedVector = Vector2X.Project(vector, direction);
		//		Vector2 velocityDiff = projectedVector - vector;
		//		Vector2 clampedProjectedVelocity = Vector2.ClampMagnitude(projectedVector, clampValue);
		//		Vector2 deltaProjectedVector = clampedProjectedVelocity - projectedVector;
		//		vector += deltaProjectedVector;
		//		return vector;
	}

	/// <summary>
	/// Returns one of four cardinal directions from a vector: up (North), right (East), down (South), left (West)
	/// </summary>
	/// <param name="v">Vector to get direction from.</param>
	public static Vector2 CardinalDirection(this Vector2 v){
		return MathX.DegreesToVector2(v.CardinalDirectionDegrees());
	}
	
	/// <summary>
	/// Returns the index of the closest vector to v in the values list
	/// </summary>
	/// <param name="v">Vector to get direction from.</param>
	public static int CardinalDirectionIndex(this Vector2 v){
		return (int)(v.CardinalDirectionDegrees()/90);
	}

	/// <summary>
	/// Returns one of four cardinal directions from a vector as degrees.
	/// </summary>
	/// <param name="v">Vector to get direction from.</param>
	public static float CardinalDirectionDegrees(this Vector2 v){
		float angle = Degrees(v);
		return angle.RoundToNearestInt(90);
	}

	/// <summary>
	/// Returns one of four ordinal directions from a vector: northeast, southeast, southwest or northwest.
	/// </summary>
	/// <param name="v">Vector to get direction from.</param>
	public static Vector2 OrdinalDirection(this Vector2 v){
		return MathX.DegreesToVector2(v.OrdinalDirectionDegrees());
	}

	/// <summary>
	/// Returns one of four ordinal directions from a vector as an index: northeast(0), southeast (1), southwest (2), northwest (3).
	/// </summary>
	/// <param name="v">Vector to get direction from.</param>
	public static int OrdinalDirectionIndex(this Vector2 v){
		return (int)((v.OrdinalDirectionDegrees()-45)/90);
	}
	
	/// <summary>
	/// Returns one of four ordinal directions from a vector as degrees.
	/// Returns the closest vector to v in the values list
	/// </summary>
	/// <param name="v">Vector to get direction from.</param>
	public static float OrdinalDirectionDegrees(this Vector2 v){
		float angle = Degrees(v) - 45;
		return angle.RoundToNearestInt(90) + 45;
	}


    

	public static Vector2 NearestPointOnLine(Vector2 vector, Vector2 lineDirection, Vector2 pointOnLine) {
        lineDirection.Normalize();
        var v = vector - pointOnLine;
        var d = Vector2.Dot(v, lineDirection);
        return pointOnLine + lineDirection * d;
	}


    public static Vector2 Slerp (Vector2 dirA, Vector2 dirB, float l) {
        var angleA = Degrees(dirA);
        var angleB = Degrees(dirB);
        return MathX.DegreesToVector2(Mathf.LerpAngle(angleA, angleB, l));
    }


	/// <returns>The index.</returns>
	/// <param name="v">V.</param>
	/// <param name="values">Values.</param>
	public static int ClosestIndex(Vector2 v, IList<Vector2> values){
		int index = 0;
		float closest = SqrDistance(v, values[index]);
		for(int i = 1; i < values.Count; i++){
			float distance = SqrDistance(v, values[i]);
			if (distance < closest) {
				closest = distance;
				index = i;
			}
		}
		return index;
	}

	/// <returns>The index.</returns>
	/// <param name="v">V.</param>
	/// <param name="values">Values.</param>
	public static float ClosestDistance(Vector2 v, IList<Vector2> values){
		return Vector2.Distance(v, Closest(v, values));
	}
	/// <param name="v">V.</param>
	/// <param name="values">Values.</param>
	public static Vector2 Closest(Vector2 v, IList<Vector2> values){
		return values[ClosestIndex(v, values)];
	}

	
	/// <summary>
	/// Sets the value of the X component.
	/// </summary> 
	public static Vector2 WithX (this Vector2 v, float newX) {
		return new Vector2(newX, v.y);
	}
	
	/// <summary>
	/// Sets the value of the Y component
	/// </summary> 
	public static Vector2 WithY (this Vector2 v, float newY) {
		return new Vector2(v.x, newY);
	}
	
	/// <summary>
	/// Adds a value to the X component
	/// </summary> 
	public static Vector2 AddX (this Vector2 v, float addX) {
		return new Vector2(v.x + addX, v.y);
	}
	
	/// <summary>
	/// Adds a value to the Y component
	/// </summary> 
	public static Vector2 AddY (this Vector2 v, float addY) {
		return new Vector2(v.x, v.y + addY);
	}

	public static Vector3 ToVector3XZY (this Vector2 v, float y = 0) {
		return new Vector3(v.x, y, v.y);
	}

	public static Vector3 ToVector3XYZ (this Vector2 v, float z = 0) {
		return new Vector3(v.x, v.y, z);
	}


	// Fits dimentions with a given aspect ratio in a container of fixed size. 
	// If forceUseContainerWidth/forceUseContainerHeight are true, the output can exceed the size of the container.
	// If we're constrained vertically, forceUseContainerWidth will force the output width to match the container size, and the height will expand to the aspect ratio, exceeding the bounds.
	// forceUseContainerHeight does the same thing on the other axis.
	public static Vector2 Rescale(Vector2 containerSize, float targetAspect, bool forceUseContainerWidth = false, bool forceUseContainerHeight = false) {
        Vector2 destRect = new Vector2();
		if(containerSize.x == Mathf.Infinity) {
            destRect.x = containerSize.y * targetAspect;
			destRect.y = containerSize.y;
		} else if(containerSize.y == Mathf.Infinity) {
            destRect.x = containerSize.x;
            destRect.y = containerSize.x / targetAspect;
		}

        float rectAspect = containerSize.x / containerSize.y;

        if (targetAspect > rectAspect) {
            // wider than high keep the width and scale the height
            destRect.x = containerSize.x;
            destRect.y = containerSize.x / targetAspect;

            if (forceUseContainerHeight)
            {
                float resizePerc = containerSize.y / destRect.y;
                destRect.x = containerSize.x * resizePerc;
                destRect.y = containerSize.y;
            }
        } else {
            // higher than wide – keep the height and scale the width
            destRect.x = containerSize.y * targetAspect;
            destRect.y = containerSize.y;

            if (forceUseContainerWidth) {
                float resizePerc = containerSize.x / destRect.x;
                destRect.x = containerSize.x;
                destRect.y = containerSize.y * resizePerc;
            }
        }

        return destRect;
    }



	#region Interception

	[Serializable]
	public struct InterceptionResult {
		public bool interceptionPossible;
		public Vector2 interceptionPosition;
		public float timeToInterception;
		public Vector2 chaserVelocity;

		public InterceptionResult (bool interceptionPossible, Vector2 interceptionPosition, float timeToInterception, Vector2 chaserVelocity) {
			this.interceptionPossible = interceptionPossible;
			this.interceptionPosition = interceptionPosition;
			this.timeToInterception = timeToInterception;
			this.chaserVelocity = chaserVelocity;
		}
	}

	// Given a moving target a, calculates required velocity for a projectile of speed b to intercept
	public static bool Intercept(Vector2 fromPos, Vector2 targetPosition, Vector2 targetVelocity, float interceptSpeed, out InterceptionResult result) {
		if (targetPosition == fromPos) {	
			result = new InterceptionResult(true, fromPos, 0, Vector2.zero);
	        return true;
	    }
		if (interceptSpeed <= 0) {
			result = new InterceptionResult(false, Vector2.zero, 0, Vector2.zero);
			return false; // No interception
		}

		Vector2 interceptionPosition = Vector2.zero;
		float timeToInterception = 0;

		if (targetVelocity == Vector2.zero) {
			timeToInterception = Vector2.Distance(fromPos, targetPosition) / interceptSpeed;
			interceptionPosition = targetPosition;
		} else {
			Vector2 relativePosition = targetPosition - fromPos;

			// Get quadratic equation components
			float a = targetVelocity.sqrMagnitude - interceptSpeed * interceptSpeed;
			float b = (2f*Vector2.Dot(targetVelocity, relativePosition));
//			2 * (targetVelocity.x * relativePosition.x + targetVelocity.y * relativePosition.y);
			float c = relativePosition.sqrMagnitude;


			//handle similar velocities
			if (Mathf.Abs(a) < 0.0001f) {
				float t = -c/b;
				timeToInterception = Mathf.Max(t, 0f);
			} else {
				// Solve quadratic
				float t1;
				float t2;
				if (!MathX.QuadraticSolver(a, b, c, out t1, out t2)) {
					// No real-valued solution, so no interception possible
					result = new InterceptionResult(false, Vector2.zero, 0, Vector2.zero);
					return false;
				}
				
				if (t1 > 0 && t2 > 0) // Both are positive, take the smaller one
					timeToInterception = Mathf.Min( t1, t2 );
				else // One has to be negative, so take the larger one
					timeToInterception = Mathf.Max( t1, t2 );
			}

			if (timeToInterception <= 0) {
				// Both values for t are negative, so the interception would have to have
				// occured in the past
				result = new InterceptionResult(false, Vector2.zero, timeToInterception, Vector2.zero);
				return false;
			}

			interceptionPosition = targetPosition + targetVelocity * timeToInterception;
	    }
		// Calculate the resulting velocity based on the time and intercept position
	    var chaserVelocity = (interceptionPosition - fromPos) / timeToInterception;
		result = new InterceptionResult(true, interceptionPosition, timeToInterception, chaserVelocity);
		return true;
	}
	
	/// <summary>
	/// Returns true if any of the components of the vector as NaN.
	/// </summary>
	public static bool HasNaN(Vector2 v) {
		return float.IsNaN(v.x) || float.IsNaN(v.y);
	}

	#endregion
}