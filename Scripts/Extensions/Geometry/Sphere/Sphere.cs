using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityX.Geometry {
	[System.Serializable]
	public struct Sphere {
		public Vector3 center;
		public float radius;
		public float sqrRadius {
			get {
				return radius * radius;
			}
		}

		public float Volume {
			get {
				return (4f/3f)*Mathf.PI*(Mathf.Pow(radius, 3));
			}
		}

		public Sphere (Vector3 center, float radius) {
			this.center = center;
			this.radius = radius;
		}
		
		public Sphere CreateFromBounds(Bounds bounds) {
			return new Sphere(bounds.center, Mathf.Max(bounds.extents.x,bounds.extents.y,bounds.extents.z));
		}

		// If the sphere contains a point
		public bool ContainsPoint (Vector3 point) {
			return SqrDistance(center, point) < sqrRadius;
		}

		// If the sphere entirely contains a sphere
		public bool ContainsSphere (Sphere otherSphere) {
			var sqrRadiusDelta = radius - otherSphere.radius;
			if(sqrRadiusDelta < 0) return false;
			sqrRadiusDelta *= sqrRadiusDelta;
			var sqrCenterDist = SqrDistance(center, otherSphere.center);
			return sqrCenterDist < sqrRadiusDelta;
		}

		// If the sphere touches another sphere
		public bool OverlapsSphere (Sphere otherSphere) {
			var sqrRadiusDelta = radius + otherSphere.radius;
			if(sqrRadiusDelta < 0) return false;
			sqrRadiusDelta *= sqrRadiusDelta;
			var sqrCenterDist = SqrDistance(center, otherSphere.center);
			return sqrCenterDist < sqrRadiusDelta;
		}

		// If inside the sphere, returns unchanged point. If not, returns ClosestPointOnSurface
		public Vector3 ClosestPointInSphere (Vector3 position) {
			if(ContainsPoint(position)) return position;
			else return ClosestPointOnSurface(position);
		}
		
		// Clamps the point to the closest point on the surface of the sphere
		public Vector3 ClosestPointOnSurface (Vector3 position) {
			var dirFromCenter = Vector3.Normalize(position-center);
			return center + dirFromCenter * radius;
		}

		// The distance from the surface of the sphere. Negative if inside the sphere.
		public float SignedDistanceFromPoint (Vector3 position) {
			var pointOnEdge = ClosestPointOnSurface(position);
			var distance = Vector3.Distance(position, pointOnEdge);
			if(ContainsPoint(position)) distance = -distance;
			return distance;
		}

        

		/// <summary>
		/// Computes the bounding sphere from a collection of 3D points.
		/// 
		/// </summary>
		/// <param name="points">Collection of points</param>
		public Sphere CreateFromPoints(Vector3[] points) {
			Vector3[] copy = new Vector3[points.Length];
			Array.Copy(points, copy, points.Length);
			return CalculateWelzl(copy, copy.Length, 0, 0);
		}
		
		
		//For welzl calculations
		private const float RADIUS_EPSILON = 1.00001f;
	
		
		//Welzl minimum bounding sphere algorithm
		Sphere CalculateWelzl(Vector3[] points, int length, int supportCount, int index) {
			Sphere sphere = new Sphere(Vector3.zero, 0);
			switch(supportCount) {
				case 0:
					break;
				case 1:
					sphere = new Sphere(points[index-1], 1.0f - RADIUS_EPSILON);
					break;
				case 2:
					sphere = CalculateWelzl(points[index-1], points[index-2]);
					break;
				case 3:
					sphere = CalculateWelzl(points[index-1], points[index-2], points[index-3]);
					break;
				case 4:
					return CalculateWelzl(points[index-1], points[index-2], points[index-3], points[index-4]);
			}
			
			for(int i = 0; i < length; i++) {
				Vector3 comp = points[i + index];
				float distSqr;
				
				distSqr = (comp-sphere.center).sqrMagnitude;
				
				if(distSqr - (sphere.radius * sphere.radius) > RADIUS_EPSILON - 1.0f) {
					for(int j = i; j > 0; j--) {
						Vector3 a = points[j + index];
						Vector3 b = points[j - 1 + index];
						points[j + index] = b;
						points[j - 1 + index] = a;
					}
					return CalculateWelzl(points, i, supportCount + 1, index + 1);
				}
			}
			Debug.LogError("Should never get here");
			return sphere;
		}
		
		//For Welzl calc - 2 support points
		public Sphere CalculateWelzl(Vector3 O, Vector3 A) {
			radius = (float) System.Math.Sqrt(((A.x - O.x) * (A.x - O.x) + (A.y - O.y)
											* (A.y - O.y) + (A.z - O.z) * (A.z - O.z)) / 4.0f) + RADIUS_EPSILON - 1.0f;
			float x = (1 - .5f) * O.x + .5f * A.x;
			float y = (1 - .5f) * O.y + .5f * A.y;
			float z = (1 - .5f) * O.z + .5f * A.z;
			
			return new Sphere(new Vector3(x,y,z), radius);
		}
		
		//For Welzl calc - 3 support points
		public Sphere CalculateWelzl(Vector3 O, Vector3 A, Vector3 B) {
			Vector3 a = A - O;
			Vector3 b = B - O;
			Vector3 aCrossB = Vector3.Cross(a, b);
			float denom = 2.0f * Vector3.Dot(aCrossB, aCrossB);
			if(denom == 0) {
				return new Sphere(Vector3.zero,0);
			} else {
				Vector3 o = ((Vector3.Cross(aCrossB, a) * b.sqrMagnitude)+ (Vector3.Cross(b, aCrossB) * a.sqrMagnitude)) / denom;
				return new Sphere(O + o, o.magnitude * RADIUS_EPSILON);
			}
		}
		
		//For Welzl calc - 4 support points
		public Sphere CalculateWelzl(Vector3 O, Vector3 A, Vector3 B, Vector3 C) {
			Vector3 a = A - O;
			Vector3 b = B - O;
			Vector3 c = C - O;
			
			float denom = 2.0f * (a.x * (b.y * c.z - c.y * b.z) - b.x
								* (a.y * c.z - c.y * a.z) + c.x * (a.y * b.z - b.y * a.z));
			if(denom == 0) {
				return new Sphere(Vector3.zero,0);
			} else {
				Vector3 o = ((Vector3.Cross(a, b) * c.sqrMagnitude)
							+ (Vector3.Cross(c, a) * b.sqrMagnitude)
							+ (Vector3.Cross(b, c) * a.sqrMagnitude)) / denom;
				return new Sphere(O + o, o.magnitude * RADIUS_EPSILON);
			}
		}
		



		
		/// <summary>
		/// Tests if the bounding box intersects with this bounding sphere.
		/// </summary>
		/// <param name="box">Bounding box to test</param>
		/// <returns>True if they intersect</returns>
		public bool Intersects(Bounds box) {
			if(box == null) {
				return false;
			}
			
			Vector3 bCenter = box.center;
			Vector3 extents = box.extents;
			
			if(Mathf.Abs(center.x - bCenter.x) < radius + extents.x
			&& Mathf.Abs(center.y - bCenter.y) < radius + extents.y
			&& Mathf.Abs(center.z - bCenter.z) < radius + extents.z) {
				return true;
			}
			
			return false;
		}
		
		// /// <summary>
		// /// Tests if the ray intersects with this sphere.
		// /// </summary>
		// /// <param name="ray">Ray to test</param>
		// /// <returns>True if the ray intersects the sphere</returns>
		// public bool Intersects(Ray ray) {
		// 	//Test if the origin is inside the sphere
		// 	Vector3 rOrigin = ray.origin;
		// 	Vector3 diff;
		// 	Vector3.Subtract(ref rOrigin, ref center, out diff);
		// 	float radSquared = radius * radius;
			
		// 	float dot;
		// 	Vector3.Dot(ref diff, ref diff, out dot);
		// 	float a = dot - radSquared;
			
		// 	if(a <= 0.0f) {
		// 		return true;
		// 	}
			
		// 	//Outside sphere
		// 	Vector3 rDir = ray.direction;
		// 	float b;
		// 	Vector3.Dot(ref rDir, ref diff, out b);
		// 	if(b >= 0.0f) {
		// 		return false;
		// 	}
			
		// 	return b * b >= a;
		// }
		
		// /// <summary>
		// /// Tests if the ray intersects with this sphere.
		// /// </summary>
		// /// <param name="ray">Ray to test</param>
		// /// <param name="result">Bool to hold the result, true if they intersect</param>
		// public void Intersects(ref Ray ray, out bool result) {
		// 	//Test if the origin is inside the sphere
		// 	Vector3 rOrigin = ray.origin;
		// 	Vector3 diff;
		// 	Vector3.Subtract(ref rOrigin, ref center, out diff);
		// 	float radSquared = radius * radius;
			
		// 	float dot;
		// 	Vector3.Dot(ref diff, ref diff, out dot);
		// 	float a = dot - radSquared;
			
		// 	if(a <= 0.0f) {
		// 		result = true;
		// 		return;
		// 	}
			
		// 	//Outside sphere
		// 	Vector3 rDir = ray.direction;
		// 	float b;
		// 	Vector3.Dot(ref rDir, ref diff, out b);
		// 	if(b >= 0.0f) {
		// 		result = false;
		// 		return;
		// 	}
			
		// 	result = b * b >= a;
		// }

		static float SqrDistance (Vector3 a, Vector3 b) {
			return (a.x-b.x) * (a.x-b.x) + (a.y-b.y) * (a.y-b.y) + (a.z-b.z) * (a.z-b.z);
		}
	}


}