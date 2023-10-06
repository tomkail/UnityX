using UnityEngine;
using System.Collections.Generic;

namespace SplineSystem {
	public delegate void OnSplineChangeEvent(Spline spline);

	[System.Serializable]
	public class Spline {
		const float defaultQuality = 25;
		public float quality = defaultQuality;
		public Vector3 GetPointAtArcLength (float arcLength) {
			return GetCurveAtArcLength(arcLength).GetPointAtArcLength(arcLength);
		}
		public Vector3 GetPointAtArcLength (float arcLength, Matrix4x4 localToWorldMatrix) {
			return localToWorldMatrix.MultiplyPoint3x4(GetPointAtArcLength(arcLength));
		}
		
		
		public Quaternion GetRotationAtArcLength (float arcLength) {
			return GetCurveAtArcLength(arcLength).GetRotationAtArcLength(arcLength);
		}
		public Quaternion GetRotationAtArcLength (float arcLength, Matrix4x4 localToWorldMatrix) {
			var rawRotation = GetRotationAtArcLength(arcLength);
			return localToWorldMatrix.rotation * rawRotation;
			// return Quaternion.LookRotation(localToWorldMatrix.MultiplyVector(rawRotation * Vector3.forward), localToWorldMatrix.MultiplyVector(rawRotation * Vector3.up));
		}


		public Vector3 GetDirectionAtArcLength (float arcLength) {
			return GetCurveAtArcLength(arcLength).GetDirectionAtArcLength(arcLength);
		}
		public Vector3 GetDirectionAtArcLength (float arcLength, Matrix4x4 localToWorldMatrix) {
			return localToWorldMatrix.MultiplyVector(GetDirectionAtArcLength(arcLength));
		}

		public SplineBezierPoint[] bezierPoints;
		public SplineBezierCurve[] curves;

		public float length {
			get {
				#if UNITY_EDITOR
				if(!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) {
					if(curves == null || curves.Length == 0) {
						Debug.LogError("Spline has no curves!");
						return 0;
					}
				}
				#endif
				return curves[curves.Length-1].endArcLength;
			}
		}
		public Bounds bounds;
		
		public Spline (params SplineBezierPoint[] bezierPoints) {
			this.bezierPoints = bezierPoints;
			RefreshCurveData();
		}

		// Creates a smoothed version of the line defined by the points. The spline doesn't run through the points, but follows its shape.
		// This means a line with a right angle produces a curve that doesn't include a point at the right angle, but curves inside it.
		// normalizedBezierPositionBetweenPoints defines how far from the target points the beziers are created, leaving more space for the curve. 
		// 0.5 means "between the points" and is the max value.
		// normalizedControlPointDistance defines how far the control points are. 
		// This shouldn't be further than normalizedBezierPositionBetweenPoints if you want smooth curves.
		// Half normalizedBezierPositionBetweenPoints is a good default for smooth curves.
		public static Spline CreateFromPoints (IList<Vector3> points, Vector3 upVector, float normalizedBezierPositionBetweenPoints = 0.5f, float normalizedControlPointDistance = 0.25f) {
			if(points.Count <= 1) {
				Debug.LogError("Can't create spline because points array only has "+points.Count+" items");
				return null;
			}
			var createInbetweenPoints = normalizedBezierPositionBetweenPoints < 0.5f;
			var numSplinePoints = createInbetweenPoints ? ((points.Count * 2) - 2) : points.Count+1;
			var bezierPoints = new SplineBezierPoint[numSplinePoints];
			var index = 0;
			if(createInbetweenPoints) {
				for (int i = 0; i < points.Count; i++) {
					if(i > 0) {
						var vectorFromPrevious = points[i] - points[i-1];
						var distanceFromPrevious = vectorFromPrevious.magnitude;
						var rotation = Quaternion.LookRotation(vectorFromPrevious, upVector);
						var point = points[i];
						if(i < points.Count-1) point -= vectorFromPrevious * normalizedBezierPositionBetweenPoints;
						bezierPoints[index] = new SplineBezierPoint(point, rotation, distanceFromPrevious * normalizedControlPointDistance, distanceFromPrevious * normalizedControlPointDistance);
						index++;
					}
					if(i < points.Count-1) {
						var vectorToNext = points[i+1] - points[i];
						var distanceToNext = vectorToNext.magnitude;
						var rotation = Quaternion.LookRotation(vectorToNext, upVector);
						var point = points[i];
						if(i > 0) point += vectorToNext * normalizedBezierPositionBetweenPoints;
						bezierPoints[index] = new SplineBezierPoint(point, rotation, distanceToNext * normalizedControlPointDistance, distanceToNext * normalizedControlPointDistance);
						index++;
					}
				}
			} else {
				for (int i = 0; i < numSplinePoints; i++) {
					var point = points[index];
					var rotation = Quaternion.identity;
					var controlPointDistanceMultiplier = 0f;
					if(i == 0) {
						var vectorToNext = points[i+1] - points[i];
						controlPointDistanceMultiplier = vectorToNext.magnitude;
						rotation = Quaternion.LookRotation(vectorToNext, upVector);
					} else if(i == numSplinePoints-1) {
						point = points[index];
						var vectorFromPrevious = points[index] - points[index-1];
						controlPointDistanceMultiplier = vectorFromPrevious.magnitude;
						rotation = Quaternion.LookRotation(vectorFromPrevious, upVector);
						index++;
					} else {
						point = Vector3.Lerp(points[i-1], points[i], 0.5f);
						var vectorToNext = points[i] - points[i-1];
						controlPointDistanceMultiplier = vectorToNext.magnitude;
						rotation = Quaternion.LookRotation(vectorToNext, upVector);
						index++;
					}
					bezierPoints[i] = new SplineBezierPoint(point, rotation, controlPointDistanceMultiplier * normalizedControlPointDistance, controlPointDistanceMultiplier * normalizedControlPointDistance);
				}
			}
			return new Spline(bezierPoints);
		}

		public static Spline CreateFromPoints (IList<Vector3> points, IList<Quaternion> rotations, float normalizedControlPointDistance = 0.25f) {
			Debug.Assert(points.Count == rotations.Count);
			var numSplinePoints = points.Count;
			var bezierPoints = new SplineBezierPoint[numSplinePoints];
			for (int i = 0; i < numSplinePoints; i++) {
				var point = points[i];
				var rotation = rotations[i];
				var previousControlPointDistanceMultiplier = 0f;
				var nextControlPointDistanceMultiplier = 0f;
				if(i == 0) {
					var vectorToNext = points[i+1] - points[i];
					previousControlPointDistanceMultiplier = nextControlPointDistanceMultiplier = vectorToNext.magnitude;
				} else if(i == numSplinePoints-1) {
					var vectorFromPrevious = points[i] - points[i-1];
					previousControlPointDistanceMultiplier = nextControlPointDistanceMultiplier = vectorFromPrevious.magnitude;
				} else {
					var vectorToNext = points[i+1] - points[i];
					nextControlPointDistanceMultiplier = vectorToNext.magnitude;
					var vectorFromPrevious = points[i] - points[i-1];
					previousControlPointDistanceMultiplier = vectorFromPrevious.magnitude;
				}
				bezierPoints[i] = new SplineBezierPoint(point, rotation, previousControlPointDistanceMultiplier * normalizedControlPointDistance, nextControlPointDistanceMultiplier * normalizedControlPointDistance);
			}
			return new Spline(bezierPoints);
		}

		public void RefreshCurveData () {
			if(bezierPoints == null || bezierPoints.Length <= 1) return;
			if(curves == null || curves.Length != bezierPoints.Length-1) curves = new SplineBezierCurve[bezierPoints.Length-1];
			for (int i = 0; i < bezierPoints.Length-1; i++) curves[i] = new SplineBezierCurve(bezierPoints [i], bezierPoints [i+1]);
			
			float distance = 0;
			// float roughLength = 0;
			// foreach(var curve in curves) {
			// 	curve.CacheData();
			// 	var curveRoughLength = curve.GetRoughLength();
			// 	roughLength += curveRoughLength;
			// }
			foreach(var curve in curves) {
				// var curveRoughLength = curve.GetRoughLength();
				// var cachedPointsPerMeter = (curveRoughLength/roughLength) * quality;
				
				curve.CacheData();
				curve.numArcLengthsForArcLengthToTCalculation = Mathf.Max(2, Mathf.CeilToInt(quality));//Mathf.Max(2, Mathf.CeilToInt(cachedPointsPerMeter));

				curve.SetLength();
				curve.SetArcLengths();
				curve.startArcLength = distance;
				distance += curve.length;
				curve.endArcLength = distance;
			}
			for (int i = 0; i < curves.Length; i++) {
				curves[i].SetBounds();
				if(i == 0) bounds = new Bounds(curves[i].bounds.center, curves[i].bounds.size);
				else bounds.Encapsulate(curves[i].bounds);
			}
		}

		public int GetCurveIndexAtArcLength (float targetDistance) {
//			int index = System.Array.BinarySearch(curves, targetDistance);
			int low = 0;
			int high = curves.Length - 1;
			if (high < 0) Debug.LogError("The array cannot be empty");
			while (low <= high) {
				int mid = (int)((uint)(low + high) >> 1);
				
				float midVal = curves[mid].startArcLength;
				
				if (midVal < targetDistance)
					low = mid + 1;
				else if (midVal > targetDistance)
					high = mid - 1;
				else
					return mid; // key found
			}
			if(high < 0) {
				high = 0;
			}
			return high;
		}

		public SplineBezierCurve GetCurveAtArcLength (float arcLength) {
			return curves[GetCurveIndexAtArcLength(arcLength)];
		}

		public float GetTAtArcLength (float arcLength) {
			int index = GetCurveIndexAtArcLength(arcLength);
			return curves[index].GetTAtBezierArcLength(arcLength);
		}

		// public SplineBezierCurve GetCurveStartingWith (SplineBezierPoint bezierPoint) {
		// 	if(curves.Length == 0 || !curves.ContainsIndex(bezierPoint.indexInCurve)) {
		// 		Debug.LogError("Point not found "+bezierPoint.indexInCurve+" "+curves.Length);
		// 		return null;
		// 	}
		// 	return curves[bezierPoint.indexInCurve];
		// }

		public float ClampArcLength (float targetArcLength) {
			return Mathf.Clamp(targetArcLength, 0, length);
		}

		static List<float> sqrDistances = new List<float>();
		static void RemoveCurves (Vector3 position, ref List<SplineBezierCurve> curvesToTry, int numSamples, ref float smallestSqrDistance) {
			if(curvesToTry.Count <= 1) return;

			sqrDistances.Clear();
			for (int i = 0; i < curvesToTry.Count; i++) {
				var curve = curvesToTry [i];
				sqrDistances.Add(SqrDistance(position, curve.bounds.ClosestPoint(position)));
				smallestSqrDistance = Mathf.Min (smallestSqrDistance, sqrDistances [i]);
			}

			float acceptanceFactor = (1 + ((1f / numSamples) * 2));
			// This is a bit of a hack. The bounds of the splines don't encapsulate their widths, and players can easily bounce a few meters higher than that anyway.
			// Since the bounds are boxes, which isn't precise, in rare cases you can be inside one box on a spline you're not on, which sets the smallestSqrDistance to 0
			// We give a leeway of 20 meters for the height above the spline + bounce distance that should cover this, without a real performance cost.
			float acceptableSqrDistances = Mathf.Max(400, smallestSqrDistance * acceptanceFactor);

			for (int i = curvesToTry.Count - 1; i >= 0; i--) {
				if(sqrDistances[i] > acceptableSqrDistances) curvesToTry.RemoveAt(i);
			}
		}
		/*
		// Finds the best point on the spline by evaluating all the cached points using a scoring function
		public SplineSampleData RoughEstimateBestCurveT (System.Func<Vector3, float> scoringFunction) {
			float bestScore = Mathf.NegativeInfinity;
			SplineBezierCurve bestCurve = null;
			float bestT = 0;
			foreach(var curve in curves) {
				for (int i = 0; i < curve._points.Length; i++) {
					var curvePoint = curve._points[i];
					float score = scoringFunction(curvePoint);
					if(score > bestScore) {
						bestScore = score;
						bestCurve = curve;
						bestT = i * curve.numArcLengthsForArcLengthToTCalculationReciprocal;
					}
				}
			}
			return SplineSampleData.CreateFromCurveAndT(bestCurve, bestT);
		}
 		*/

		// Gets the best T using the cached points only. This could be optimized by using a binary sort.
		public bool RoughEstimateBestCurveT (System.Func<Vector3, float> scoringFunction, ref SplineBezierCurve bestCurve, ref float bestT, ref float bestScore) {
			bool changed = false;
			foreach(var curve in curves) {
				for (int i = 0; i < curve._points.Length; i++) {
					var curvePoint = curve._points[i];
					float score = scoringFunction(curvePoint);
					if(score > bestScore) {
						bestScore = score;
						bestCurve = curve;
						bestT = i * curve.numArcLengthsForArcLengthToTCalculationReciprocal;
						changed = true;
					}
				}
			}
			return changed;
		}

		void GetBestCurve (Vector3 position, SplineBezierCurve curve, ref SplineBezierCurve bestCurve, ref float bestDistance, ref Vector3 bestPoint, ref float bestT) {
			for (int i = 0; i < curve.numArcLengthsForArcLengthToTCalculation; i++) {
				var curvePoint = curve._points[i];
				float distance = SqrDistance(position, curvePoint);
				if(distance == bestDistance) {
					var t = i * curve.numArcLengthsForArcLengthToTCalculationReciprocal;
					Vector3 direction = curve.GetDirectionAtT(t);
					if(Vector3.Dot(position - curvePoint, direction) > 0) {
						bestCurve = curve;
						bestDistance = distance;
						bestPoint = curvePoint;
						bestT = t;
					}
				} else if(distance < bestDistance) {
					bestCurve = curve;
					bestDistance = distance;
					bestPoint = curvePoint;
					bestT = i * curve.numArcLengthsForArcLengthToTCalculationReciprocal;
				}
			}
		}
		// This isn't *perfectly* accurate, although it's pretty damned close. 
		// Works by finding the best curve, subdividing that curve, then finally lerping between the resultant line
		// Accuracy will depend on numArcLengthsForArcLengthToTCalculation and maxSqrDistanceToSpline and maxRange variables in this function.
		static List<SplineBezierCurve> curvesToTry = new List<SplineBezierCurve>();
		public float EstimateArcLengthAlongCurve (Vector3 position, bool clampAtStart = false, bool clampAtEnd = false) {
			if(curves == null || curves.Length == 0) {
				Debug.LogError("No curves in spline");
				return 0;
			}
			// Step 1: Throw away any curves that are clearly further away
			curvesToTry.Clear();
			curvesToTry.AddRange(curves);
			float smallestSqrDistance = Mathf.Infinity;
			RemoveCurves(position, ref curvesToTry, 3, ref smallestSqrDistance);

			// Step 2: Find the best curve and T at that point
			SplineBezierCurve bestCurve = null;
			float bestDistance = Mathf.Infinity;
			Vector3 bestPoint = Vector3.zero;
			float bestT = 0;
			foreach(var curve in curvesToTry)
				GetBestCurve(position, curve, ref bestCurve, ref bestDistance, ref bestPoint, ref bestT);
			return EstimateArcLengthAlongCurve(position, bestCurve, bestT, clampAtStart, clampAtEnd);
		}

		public float EstimateArcLengthAlongCurve (Vector3 position, SplineBezierCurve curve, bool clampAtStart = false, bool clampAtEnd = false) {
			// Step 2: Find the best curve and T at that point
			SplineBezierCurve bestCurve = curve;
			float bestDistance = Mathf.Infinity;
			Vector3 bestPoint = Vector3.zero;
			float bestT = 0;
			GetBestCurve(position, curve, ref bestCurve, ref bestDistance, ref bestPoint, ref bestT);
			return EstimateArcLengthAlongCurve(position, bestCurve, bestT, clampAtStart, clampAtEnd);
		}
		
		float EstimateArcLengthAlongCurve (Vector3 position, SplineBezierCurve bestCurve, float bestT, bool clampAtStart = false, bool clampAtEnd = false) {
			// Home in on the best T by recursively subdividing
			var tRange = bestCurve.numArcLengthsForArcLengthToTCalculationReciprocal;

			float minT = Mathf.Clamp01(bestT - tRange / 2.0f);
			float maxT = Mathf.Clamp01(bestT + tRange / 2.0f);

			float minTDist = SqrDistanceAtTValue(bestCurve, minT, position);
			float maxTDist = SqrDistanceAtTValue(bestCurve, maxT, position);

			int sanityCheck = 0;
			bool foundIt = false;

			// Search parameters. Exit if either:
			// We're this close to the spline
			const float maxSqrDistanceToSpline = 0.01f;
			// The distance between the two points we're comparing is smaller than this
			const float maxRange = 0.05f;
			

			while( !foundIt && sanityCheck < 20) {
				if (minTDist < maxTDist) {
					// we're closer to minT
					maxT = (minT + maxT) / 2.0f;
//					bestT = minT;
					maxTDist = SqrDistanceAtTValue(bestCurve, maxT, position);
				} else {
					// we're closer to maxT
					minT = (minT + maxT) / 2.0f;
//					bestT = maxT;
					minTDist = SqrDistanceAtTValue(bestCurve, minT, position);
				}
				sanityCheck++;
				foundIt = (maxT - minT) * bestCurve.length < maxRange || minTDist < maxSqrDistanceToSpline;
				
			}
			tRange = maxT - minT;

			// Debug.Log("Ran search " + sanityCheck.ToString() + " times to reach tRange of " + tRange.ToString() + ". minTDist "+minTDist+". Range "+((maxT - minT) * bestCurve.length));
			// return bestCurve.startArcLength + bestCurve.EstimateBezierArcLengthAtT(minT);
			// Now we've got a nice small range to work with, we can work find out where our point is on that range as a line.
			bestT = Mathf.Lerp(minT, maxT, 0.5f);
			
			float closestDistanceAlongLeftLine = 0;
			float closestDistanceAlongRightLine = 0;

			var leftT = bestT == maxT ? minT : bestT - tRange * 0.5f;
			var rightT = bestT == minT ? maxT : bestT + tRange * 0.5f;
			float arcLengthOffset;
			var centerPoint = bestCurve.GetPointAtT(bestT);
			var leftPoint = bestCurve.GetPointAtT(leftT);
			var rightPoint = bestCurve.GetPointAtT(rightT);

			closestDistanceAlongLeftLine = GetNormalizedDistanceOnLine(leftPoint, centerPoint, position, clampAtStart);
			closestDistanceAlongRightLine = GetNormalizedDistanceOnLine(centerPoint, rightPoint, position, clampAtEnd);
			float closestDistanceOnLineA = SqrDistance(position, Vector3.LerpUnclamped(leftPoint, centerPoint, closestDistanceAlongLeftLine));
			float closestDistanceOnLineB = SqrDistance(position, Vector3.LerpUnclamped(centerPoint, rightPoint, closestDistanceAlongRightLine));
			var arcLengthOffsetMultiplier = 0f;
			if(closestDistanceOnLineA < closestDistanceOnLineB) {
				arcLengthOffset = Vector3.Distance(leftPoint, centerPoint);
				arcLengthOffsetMultiplier = (closestDistanceAlongLeftLine - 1);
			} else {
				arcLengthOffset = Vector3.Distance(centerPoint, rightPoint);
				arcLengthOffsetMultiplier = closestDistanceAlongRightLine;
			}
			arcLengthOffset *= arcLengthOffsetMultiplier;

			// Debug.Log("best "+ bestT+" min "+minT+" max "+maxT+" left "+closestDistanceOnLineA +" right "+closestDistanceOnLineB+" dOnLeft "+closestDistanceAlongLeftLine+" dOnRight "+closestDistanceAlongRightLine);
			

			var offset = bestCurve.EstimateBezierArcLengthAtT(bestT) + arcLengthOffset;
			if(clampAtStart) offset = Mathf.Max(offset, 0);
			if(clampAtEnd) offset = Mathf.Min(offset, bestCurve.length);
			return bestCurve.startArcLength + offset;
		}


		float SqrDistanceAtTValue(SplineBezierCurve curve, float t, Vector3 samplePosition) {
			var curvePoint = curve.GetPointAtT(t);
			return SqrDistance(samplePosition, curvePoint);
		}

		// This could be more efficient by using a binary search
		void SubdivideInCurve (SplineBezierCurve curve, Vector3 samplePosition, ref float bestT, ref float tRange, int numSamples) {
			var leftT = bestT - tRange * 0.5f;
			var r = 1f/(numSamples-1);
			r *= tRange;
			tRange = r;
			var bestDist = Mathf.Infinity;
			for(int i = 0; i < numSamples; i++) {
				var t = leftT + (i * r);
				var curvePoint = curve.GetPointAtT(t);
				float distance = SqrDistance(samplePosition, curvePoint);
				if(distance < bestDist) {
					bestDist = distance;
					bestT = t;
				}
			}
		}

		public IEnumerable<Vector3> GetVerts (int numPoints) {
			var r = length/(numPoints-1);
			for (var i = 0; i < numPoints; i++)
				yield return GetPointAtArcLength(r * i);
		}
		public IEnumerable<Vector3> GetVerts (int numPoints, Matrix4x4 localToWorldMatrix) {
			var r = length/(numPoints-1);
			for (var i = 0; i < numPoints; i++)
				yield return GetPointAtArcLength(r * i, localToWorldMatrix);
		}

		public IEnumerable<Vector3> GetVerts (int numPoints, float startArcLength, float endArcLength) {
			var length = endArcLength-startArcLength;
			var r = length/(numPoints-1);
			for (var i = 0; i < numPoints; i++)
				yield return GetPointAtArcLength(startArcLength + r * i);
		}
		public IEnumerable<Vector3> GetVerts (int numPoints, float startArcLength, float endArcLength, Matrix4x4 localToWorldMatrix) {
			var length = endArcLength-startArcLength;
			var r = length/(numPoints-1);
			for (var i = 0; i < numPoints; i++)
				yield return GetPointAtArcLength(startArcLength + r * i, localToWorldMatrix);
		}

		public IEnumerable<Vector3> GetVertsWithPointsPerMeter (float pointsPerMeter) {
			int numPoints = Mathf.Max(Mathf.CeilToInt(length * pointsPerMeter), 2);
			var r = length/(numPoints-1);
			return GetVerts(numPoints);
		}
		public IEnumerable<Vector3> GetVertsWithPointsPerMeter (float pointsPerMeter, Matrix4x4 localToWorldMatrix) {
			int numPoints = Mathf.Max(Mathf.CeilToInt(length * pointsPerMeter), 2);
			var r = length/(numPoints-1);
			return GetVerts(numPoints, localToWorldMatrix);
		}

		public IEnumerable<Vector3> GetVertsWithPointsPerMeter (float pointsPerMeter, float startArcLength, float endArcLength) {
			var length = endArcLength-startArcLength;
			int numPoints = Mathf.Max(Mathf.CeilToInt(length * pointsPerMeter), 2);
			return GetVerts(numPoints, startArcLength, endArcLength);
		}
		public IEnumerable<Vector3> GetVertsWithPointsPerMeter (float pointsPerMeter, float startArcLength, float endArcLength, Matrix4x4 localToWorldMatrix) {
			var length = endArcLength-startArcLength;
            int numPoints = Mathf.Max(Mathf.CeilToInt(length * pointsPerMeter), 2);
			return GetVerts(numPoints, startArcLength, endArcLength, localToWorldMatrix);
		}

		public static IEnumerable<Vector3> GetCurveVerts (Spline spline, SplineBezierCurve curve, int numPoints) {
			numPoints = Mathf.Min(numPoints, curve.numArcLengthsForArcLengthToTCalculation);
			float r = 1f/(numPoints-1);
			for (int j = 0; j < numPoints; j++) {
				float arcLength = Mathf.Lerp(curve.startArcLength, curve.endArcLength, j * r);
				yield return spline.GetPointAtArcLength(arcLength);
			}
	    }

		// Transforms a spline into another space. A typical use for this is creating world space splines from local ones, or the reverse.
		// This approach handles matricies with non-uniform scale
		public static Spline Transform (Spline spline, Matrix4x4 matrix) {
			SplineBezierPoint[] newBezierPoints = new SplineBezierPoint[spline.bezierPoints.Length];
			for(int i = 0; i < spline.bezierPoints.Length; i++) {
				var bezierPoint = spline.bezierPoints[i];

				var worldPosition = matrix.MultiplyPoint3x4(bezierPoint.position);
				var worldRotation = Quaternion.LookRotation(matrix.MultiplyVector(bezierPoint.rotation * Vector3.forward), matrix.MultiplyVector(bezierPoint.rotation * Vector3.up));
				
				var inWorldPos = matrix.MultiplyPoint3x4(bezierPoint.inControlPoint.GetPosition(bezierPoint));
				var inWorldDir = matrix.MultiplyVector(bezierPoint.inControlPoint.GetDirection(bezierPoint)).normalized;
				var inDist = Vector3.Dot(inWorldPos - worldPosition, inWorldDir);

				var outWorldPos = matrix.MultiplyPoint3x4(bezierPoint.outControlPoint.GetPosition(bezierPoint));
				var outWorldDir = matrix.MultiplyVector(bezierPoint.outControlPoint.GetDirection(bezierPoint)).normalized;
				var outDist = Vector3.Dot(outWorldPos - worldPosition, outWorldDir);
				
				newBezierPoints[i] = new SplineBezierPoint(
					worldPosition,
					worldRotation,
					inDist,
					outDist
				);
			}
			return new Spline(newBezierPoints);
		}

        public bool Validate () {
			bool didChange = false;
            if(quality <= 0) {
				quality = defaultQuality;
				didChange = true;
			}
			if(bezierPoints == null || bezierPoints.Length < 2) {
                bezierPoints = new SplineBezierPoint[] {
                    new SplineBezierPoint(new Vector3(-1,1,0), Quaternion.LookRotation(Vector3.right, Vector3.forward), 1f, 1f),
                    new SplineBezierPoint(new Vector3(1,-1,0), Quaternion.LookRotation(Vector3.right, Vector3.forward), 1f, 1f)
                };
                didChange = true;
            } else {
                for (var i = 0; i < bezierPoints.Length; i++) {
                    if(bezierPoints[i].rotation.x == 0 && bezierPoints[i].rotation.y == 0 && bezierPoints[i].rotation.z == 0 && bezierPoints[i].rotation.w == 0) {
                        bezierPoints[i].rotation = Quaternion.identity;
                        didChange = true;
                    }
                    
                    if(bezierPoints[i].inControlPoint.directionSign != -1) {
                        bezierPoints[i].inControlPoint.directionSign = -1;
                        didChange = true;
                    }
                    if(bezierPoints[i].inControlPoint.distance < 0) {
                        bezierPoints[i].inControlPoint.distance = 0;
                        didChange = true;
                    }

                    if(bezierPoints[i].outControlPoint.directionSign != 1) {
                        bezierPoints[i].outControlPoint.directionSign = 1;
                        didChange = true;
                    }
                    if(bezierPoints[i].outControlPoint.distance < 0) {
                        bezierPoints[i].outControlPoint.distance = 0;
                        didChange = true;
                    }
                }
            }
            if(didChange) 
    			RefreshCurveData();

            return didChange;
        }

		public static void DrawSplineGizmos (Spline spline, int numPoints) {
			for (var i = 0; i < spline.curves.Length; i++) {
				DrawCurveLineGizmos(spline, spline.curves[i], Matrix4x4.identity, numPoints);
			}
		}
		public static void DrawSplineGizmos (Spline spline, Matrix4x4 localToWorldMatrix, int numPoints) {
			for (var i = 0; i < spline.curves.Length; i++) {
				DrawCurveLineGizmos(spline, spline.curves[i], localToWorldMatrix, numPoints);
			}
		}
		
		public static void DrawSplineGizmosWithPointsPerMeter (Spline spline, Matrix4x4 localToWorldMatrix, int pointsPerMeter) {
			for (var i = 0; i < spline.curves.Length; i++) {
				int numPoints = Mathf.Max(Mathf.CeilToInt(spline.curves[i].length * pointsPerMeter), 2);
				DrawCurveLineGizmos(spline, spline.curves[i], localToWorldMatrix, numPoints);
			}
		}

		public static void DrawCurveLineGizmos (Spline spline, SplineBezierCurve curve, Matrix4x4 localToWorldMatrix, int numPoints) {
			Vector3 p0;
			Vector3 p1;
			numPoints = Mathf.Min(numPoints, curve.numArcLengthsForArcLengthToTCalculation);
			float r = 1f/(numPoints-1);
			p0 = localToWorldMatrix.MultiplyPoint3x4(spline.GetPointAtArcLength(curve.startArcLength));
			for (int j = 1; j < numPoints; j++) {
				float arcLength = Mathf.Lerp(curve.startArcLength, curve.endArcLength, j * r);
				p1 = localToWorldMatrix.MultiplyPoint3x4(spline.GetPointAtArcLength(arcLength));
				Gizmos.DrawLine(p0, p1);
				p0 = p1;
			}
	    }

		public static void DrawSplineGizmos (Spline spline, Matrix4x4 localToWorldMatrix, float startArcLength, float endArcLength, int numPoints) {
            var verts = spline.GetVertsWithPointsPerMeter(numPoints, startArcLength, endArcLength, localToWorldMatrix);
			Vector3 lastVert = Vector3.zero;
            bool isFirst = true;
            foreach(var vert in verts) {
                if(isFirst) {
                    isFirst = false;
                } else {
				    Gizmos.DrawLine(lastVert, vert);
                }
                lastVert = vert;
            }
	    }


		static float SqrDistance (Vector3 a, Vector3 b) {
			return (a.x-b.x) * (a.x-b.x) + (a.y-b.y) * (a.y-b.y) + (a.z-b.z) * (a.z-b.z);
		}

		static float GetNormalizedDistanceOnLine(Vector3 start, Vector3 end, Vector3 p, bool clamped = true) {
			float sqrLength = SqrDistance(start, end);
			return GetNormalizedDistanceOnLineInternal(start, end, p, sqrLength, clamped);
		}

		static float GetNormalizedDistanceOnLineInternal(Vector3 start, Vector3 end, Vector3 p, float sqrLength, bool clamped = true) {
			if (sqrLength == 0f) return 0;
			// Divide by length squared so that we can save on normalising (end-start), since
			// we're effectively dividing by the length an extra time.
			float n = Vector3.Dot(p - start, end - start) / sqrLength;
			if(!clamped) return n;
			return Mathf.Clamp01(n);
		}
	}
}