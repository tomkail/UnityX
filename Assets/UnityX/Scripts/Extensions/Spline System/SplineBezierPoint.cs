using System.Linq;
using System.Collections;
using System.Collections.Generic;
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
		public Vector3 forward {
			get {
				return rotation * Vector3.forward;
			}	
		}
		public Vector3 normal {
			get {
				return rotation * Vector3.up;
			}	
		}
		public Vector3 binormal {
			get {
				return rotation * Vector3.right;
			}	
		}

		public SplineBezierPoint (Vector3 position, Quaternion rotation, float inControlPointDistance, float outControlPointDistance) {
			this.position = position;
			this.rotation = rotation;
			this.inControlPoint = new SplineBezierControlPoint(-1, inControlPointDistance);
			this.outControlPoint = new SplineBezierControlPoint(1, outControlPointDistance);
		}
	}
}