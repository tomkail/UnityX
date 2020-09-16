using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineSystem {
	[System.Serializable]
	public struct SplineBezierControlPoint {
		public int directionSign;
		public float distance;
		public Vector3 GetPosition (SplineBezierPoint bezierPoint) {
			return bezierPoint.position + GetDirection(bezierPoint) * distance;
		}
		public Vector3 GetDirection (SplineBezierPoint bezierPoint) {
			return bezierPoint.forward * directionSign;
		}
		public Quaternion GetRotation (SplineBezierPoint bezierPoint) {
			return Quaternion.LookRotation(GetDirection(bezierPoint), bezierPoint.normal);
		}

		public SplineBezierControlPoint (int directionSign, float distance) {
			this.directionSign = directionSign;
			this.distance = distance;
		}
	}
}