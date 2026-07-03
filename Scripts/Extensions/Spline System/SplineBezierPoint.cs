using UnityEngine;

namespace SplineSystem {
	[System.Serializable]
	public struct SplineBezierPoint {

		[SerializeField]
		public SplineBezierControlPoint inControlPoint;
		[SerializeField]
		public SplineBezierControlPoint outControlPoint;
        
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 forward => rotation * Vector3.forward;

		public Vector3 normal => rotation * Vector3.up;

		public Vector3 binormal => rotation * Vector3.right;

		public SplineBezierPoint (Vector3 position, Quaternion rotation, float inControlPointDistance, float outControlPointDistance) {
			this.position = position;
			this.rotation = rotation;
			inControlPoint = SplineBezierControlPoint.In(inControlPointDistance);
			outControlPoint = SplineBezierControlPoint.Out(outControlPointDistance);
		}
		
		public void SetAuto(Vector3? previousBezierPointPosition, Vector3? nextBezierPointPosition, Vector3 upVector, float normalizedControlPointDistance = 0.25f) {
			if (previousBezierPointPosition == null && nextBezierPointPosition == null) {
				return;
			} else if(previousBezierPointPosition == null) {
				var vectorToNext = (Vector3)nextBezierPointPosition - position;
				rotation = Quaternion.LookRotation(vectorToNext, upVector);
			} else if(nextBezierPointPosition == null) {
				var vectorFromPrevious = position - (Vector3)previousBezierPointPosition;
				rotation = Quaternion.LookRotation(vectorFromPrevious, upVector);
			} else {
				var vectorToNext = (Vector3)nextBezierPointPosition - position;
				var vectorFromPrevious = position - (Vector3)previousBezierPointPosition;
				// This slerp could be considered wrong - should it be biased towards the shorter of the two vectors?
				rotation = Quaternion.LookRotation(Vector3.Slerp(vectorFromPrevious, vectorToNext, 0.5f), upVector);
			}
			// At the first/last point one neighbour is null; fall back to the other so we don't unbox a null.
			var inNeighbour = previousBezierPointPosition ?? (Vector3)nextBezierPointPosition;
			var outNeighbour = nextBezierPointPosition ?? (Vector3)previousBezierPointPosition;
			inControlPoint = SplineBezierControlPoint.InAuto(position, inNeighbour, normalizedControlPointDistance);
			outControlPoint = SplineBezierControlPoint.OutAuto(position, outNeighbour, normalizedControlPointDistance);
		}
		
		public void SetAutoDistance(Vector3? previousBezierPointPosition, Vector3? nextBezierPointPosition, Vector3 upVector, float normalizedControlPointDistance = 0.25f) {
			if (previousBezierPointPosition == null && nextBezierPointPosition == null) {
				return;
			}
			// At the first/last point one neighbour is null; fall back to the other so we don't unbox a null.
			var inNeighbour = previousBezierPointPosition ?? (Vector3)nextBezierPointPosition;
			var outNeighbour = nextBezierPointPosition ?? (Vector3)previousBezierPointPosition;
			inControlPoint = SplineBezierControlPoint.InAuto(position, inNeighbour, normalizedControlPointDistance);
			outControlPoint = SplineBezierControlPoint.OutAuto(position, outNeighbour, normalizedControlPointDistance);
		}
		
		public static SplineBezierPoint CreateAuto(Vector3 position, Vector3? previousBezierPointPosition, Vector3? nextBezierPointPosition, Vector3 upVector, float normalizedControlPointDistance = 0.25f) {
			Quaternion rotation;
			if (previousBezierPointPosition == null && nextBezierPointPosition == null) {
				Debug.LogWarning("Cannot create auto control points for a single point");
				return default;
			} else if(previousBezierPointPosition == null) {
				var vectorToNext = (Vector3)nextBezierPointPosition - position;
				rotation = Quaternion.LookRotation(vectorToNext, upVector);
			} else if(nextBezierPointPosition == null) {
				var vectorFromPrevious = position - (Vector3)previousBezierPointPosition;
				rotation = Quaternion.LookRotation(vectorFromPrevious, upVector);
			} else {
				var vectorToNext = (Vector3)nextBezierPointPosition - position;
				var vectorFromPrevious = position - (Vector3)previousBezierPointPosition;
				// This slerp could be considered wrong - should it be biased towards the shorter of the two vectors?
				rotation = Quaternion.LookRotation(Vector3.Slerp(vectorFromPrevious, vectorToNext, 0.5f), upVector);
			}
			
			// At the first/last point one neighbour is null; fall back to the other so we don't unbox a null.
			var inNeighbour = previousBezierPointPosition ?? (Vector3)nextBezierPointPosition;
			var outNeighbour = nextBezierPointPosition ?? (Vector3)previousBezierPointPosition;
			return new SplineBezierPoint(
				position,
				rotation,
				SplineBezierControlPoint.GetAutoDistanceIn(position, inNeighbour, normalizedControlPointDistance),
				SplineBezierControlPoint.GetAutoDistanceOut(position, outNeighbour, normalizedControlPointDistance)
			);
		}
	}
}