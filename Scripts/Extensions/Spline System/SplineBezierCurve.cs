using UnityEngine;

namespace SplineSystem {
	// Arc length == NormalizedDistance
	//	 The bezier curve groups positions unevenly. This gets the sample position for the bezier algorithm based on a normalized distance

	[System.Serializable]
	/// <summary>
	/// A single curve that forms a river spline. Calculates itself via start and end bezier points.
	/// </summary>
	public class SplineBezierCurve {
		[System.NonSerialized]
		public SplineBezierPoint startPoint;
		[System.NonSerialized]
		public SplineBezierPoint endPoint;

		// This constant affects how precise the GetTAtNormalizedDistance function is. Higher is more precise.
		public int numArcLengthsForArcLengthToTCalculation;
		public float numArcLengthsForArcLengthToTCalculationReciprocal;

		public float[] _arcLengths;
		public Vector3[] _points;
		
		// Fairly accurate tight world space bounds
		public Bounds bounds = new();
		public float length;

		public float startArcLength;
		public float endArcLength;

		[SerializeField]
		Vector3 p0 = Vector3.zero;
		[SerializeField]
		Vector3 p1 = Vector3.zero;
		[SerializeField]
		Vector3 p2 = Vector3.zero;
		[SerializeField]
		Vector3 p3 = Vector3.zero;

		public SplineBezierCurve (SplineBezierPoint startPoint, SplineBezierPoint endPoint) {
			this.startPoint = startPoint;
			this.endPoint = endPoint;
		}

		public void CacheData () {
			p0 = startPoint.position;
			p1 = startPoint.outControlPoint.GetPosition(startPoint);
			p2 = endPoint.inControlPoint.GetPosition(endPoint);
			p3 = endPoint.position;
		}

		public float GetRoughLength () {
			return Bezier.GetRoughLength(p0, p1, p2, p3);
		}

		public void SetLength () {
			var roughLength = Bezier.GetRoughLength(p0, p1, p2, p3);
			float rec = 1f/(numArcLengthsForArcLengthToTCalculation-1);

			Vector3 pA = GetPointAtT(0);
			Vector3 pB;
			length = 0;
			for (int s = 1; s < numArcLengthsForArcLengthToTCalculation; s++) {
				pB = GetPointAtT(rec * s);
				length += (pB - pA).magnitude;
				pA = pB;
			}
		}

		public void SetArcLengths () {
			numArcLengthsForArcLengthToTCalculationReciprocal = 1f/(numArcLengthsForArcLengthToTCalculation-1);
			if(_arcLengths == null || _arcLengths.Length != numArcLengthsForArcLengthToTCalculation) _arcLengths = new float[numArcLengthsForArcLengthToTCalculation];
			if(_points == null || _points.Length != numArcLengthsForArcLengthToTCalculation) _points = new Vector3[numArcLengthsForArcLengthToTCalculation];
			
			Vector3 p0 = GetPointAtT(0);
			Vector3 p1;
			_arcLengths[0] = 0;
			_points[0] = p0;
			float _length = 0;
			for (int s = 1; s < numArcLengthsForArcLengthToTCalculation; s++) {
				p1 = GetPointAtT(numArcLengthsForArcLengthToTCalculationReciprocal * s);
				_length += (p1 - p0).magnitude;
				_arcLengths[s] = _length;
				_points[s] = p1;
				p0 = p1;
			}
		}

		public void SetBounds () {
			int numSamples = 8;
			float numSamplesReciprocal = 1f/(numSamples-1);
			var point = GetPointAtNormalizedLocalArcLength(0);
			bounds = new Bounds(point, Vector3.zero);
			for (int s = 1; s < numSamples; s++) {
				point = GetPointAtNormalizedLocalArcLength(numSamplesReciprocal * s);
				bounds.Encapsulate(point);
			}
		}

		public float GetTAtBezierArcLength (float bezierArcLength) {
			return GetTAtNormalizedBezierArcLength(NormalizeBezierArcLength(bezierArcLength));
		}

		public float GetTAtNormalizedBezierArcLength (float u) {
			if (u <= 0) return 0;
			if (u >= 1) return 1;

//			int index = System.Array.BinarySearch(_arcLengths, u);
//			if (index >= _arcLengths.Length - 1) return 1;
//			float target = u * length;
//			if (_arcLengths[index] > target) index--;
//			if (index < 0) return 0;

			int index = 0;
	        int low = 0;
	        int high = _arcLengths.Length - 1;
	        float target = u * length;
	        float found = -1;

	        while (low < high) {
	//            index = (low + high) / 2;
				index = (int)((uint)(low + high) >> 1);
	            found = _arcLengths[index];
	            if (found < target) {
	                low = index + 1;
	            } else {
	                high = index;
	            }
	        }

	        if (found > target) {
	            index--;
	        }

			if (index < 0) return 0;


	        // Linearly interp between index and index + 1
			float t = index + Mathf.InverseLerp(_arcLengths[index], _arcLengths[index + 1], target);
	        t *= numArcLengthsForArcLengthToTCalculationReciprocal;
	        return t;
		}

		// Warning - Expensive! Estimates the normalized bezier arc length at t. 
		public float EstimateNormalizedBezierArcLengthAtT (float t) {
			return EstimateBezierArcLengthAtT(t) / length;
		}


		// Could be optimised by starting from the total distance and working backwards
		public float EstimateBezierArcLengthAtT (float t) {
			if(t <= 0) return 0;

			float currentT = 0;
			float currentDistance = 0;

			float lastSample = _arcLengths[0];
			float sample = 0;
			for(int i = 1; i < numArcLengthsForArcLengthToTCalculation; i++) {
				currentT += numArcLengthsForArcLengthToTCalculationReciprocal;
				sample = _arcLengths[i];
				float interval = sample - lastSample;
				if(currentT > t) {
					float n = Mathf.InverseLerp(currentT - numArcLengthsForArcLengthToTCalculationReciprocal, currentT, t);
					return currentDistance + interval * n;
				}
				currentDistance += interval;
				lastSample = sample;
			}
			return length;
		}

		public float NormalizeBezierArcLength (float bezierArcLength) {
			return (bezierArcLength - startArcLength) / (endArcLength - startArcLength);
		}

		public Vector3 GetPointAtArcLength (float distance) {
			return GetPointAtNormalizedLocalArcLength(NormalizeBezierArcLength(distance));
		}
		public Vector3 GetPointAtNormalizedLocalArcLength (float normalizedDistance) {
			var t = GetTAtNormalizedBezierArcLength(normalizedDistance);
			return GetPointAtT(t, normalizedDistance);
		}
		public Vector3 GetPointAtT (float t) {
			return Bezier.GetPoint(p0, p1, p2, p3, t);
		}
		public Vector3 GetPointAtT (float t, float normalizedDistance) {
			var point = GetPointAtT(t);
			if(normalizedDistance < 0) point += GetDirectionAtT(t) * normalizedDistance * length;
			else if(normalizedDistance > 1) point += GetDirectionAtT(t) * (normalizedDistance-1) * length;
			return point;
		}

		public Vector3 GetDirectionAtArcLength (float distance) {
			return GetDirectionAtNormalizedLocalArcLength(NormalizeBezierArcLength(distance));
		}
		public Vector3 GetDirectionAtNormalizedLocalArcLength (float normalizedDistance) {
			return GetDirectionAtT(GetTAtNormalizedBezierArcLength(normalizedDistance));
		}
		public Vector3 GetDirectionAtT (float t) {
			if(t <= 0 || p0 == p1) return startPoint.forward;
			if(t >= 1 || p2 == p3) return endPoint.forward;
			return Bezier.GetFirstDerivative(p0, p1, p2, p3, t).normalized;
		}

		public Quaternion GetRotationAtArcLength (float distance) {
			return GetRotationAtNormalizedLocalArcLength(NormalizeBezierArcLength(distance));
		}
		public Quaternion GetRotationAtNormalizedLocalArcLength (float normalizedDistance) {
			var direction = GetDirectionAtNormalizedLocalArcLength(normalizedDistance);
			#if UNITY_EDITOR 
			if(direction == Vector3.zero) Debug.LogWarning("Direction is invalid on curve at normalized distance "+normalizedDistance);
			#endif

			// This isn't an accurate way to calculate normals, but it's enough for LookRotation to go on.
			// We can then get the correct normal out from the rotation.
			var rawNormal = Vector3.Slerp(startPoint.normal, endPoint.normal, normalizedDistance);
			#if UNITY_EDITOR 
			if(rawNormal == Vector3.zero) Debug.LogWarning("Raw normal is invalid on curve at normalized distance "+normalizedDistance);
			#endif
			return Quaternion.LookRotation(direction, rawNormal);
		}
		public Quaternion GetRotationAtNormalizedLocalArcLength (float normalizedDistance, out Vector3 direction) {
			direction = GetDirectionAtNormalizedLocalArcLength(normalizedDistance);
			#if UNITY_EDITOR 
			if(direction == Vector3.zero) Debug.LogWarning("Direction is invalid on curve at normalized distance "+normalizedDistance);
			#endif
			
			// This isn't an accurate way to calculate normals, but it's enough for LookRotation to go on.
			// We can then get the correct normal out from the rotation.
			var rawNormal = Vector3.Slerp(startPoint.normal, endPoint.normal, normalizedDistance);
			#if UNITY_EDITOR 
			if(rawNormal == Vector3.zero) Debug.LogWarning("Raw normal is invalid on curve at normalized distance "+normalizedDistance);
			#endif
			return Quaternion.LookRotation(direction, rawNormal);
		}
	}
}