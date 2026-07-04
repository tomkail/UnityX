using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A sphere that is defined by a center and a radius. This is one of the simpler volumes and collision checks
/// are fast, but may yield more false-positives than using volumes that more tightly enclose the geometry (such as OrientedBoundingBox).
/// 
/// The algorithm for generating the sphere is an implemention of Welzl's minimum-volume sphere algorithm.
/// </summary>
[System.Serializable]
public sealed class BoundingSphere {
	
	/// <summary>
	/// Center of the bounding volume, this is common to all bounding volumes.
	/// </summary>
	private Vector3 m_center;
	
	/// <summary>
	/// Gets or sets the center of the bounding volume.
	/// </summary>
	public Vector3 center {
		get {
			return m_center;
		}
		set {
			m_center = value;
		}
	}
	
	private float m_radius;
	
	//For welzl calculations
	private const float RADIUS_EPSILON = 1.00001f;
	
	/// <summary>
	/// Gets or sets the radius of the sphere.
	/// </summary>
	public float radius {
		get {
			return m_radius;
		}
		set {
			m_radius = value;
		}
	}
	
	/// <summary>
	/// Constructs a new bounding sphere centered at the origin with zero radius.
	/// </summary>
	public BoundingSphere() {
		m_center = Vector3.zero;
		m_radius = 0;
	}
	
	/// <summary>
	/// Constructs a new bounding sphere.
	/// </summary>
	/// <param name="center">Center of the sphere</param>
	/// <param name="radius">Radius of the sphere</param>
	public BoundingSphere(Vector3 center, float radius) {
		m_center = center;
		m_radius = radius;
	}
	
	/// <summary>
	/// Constructs a new bounding sphere cloned from the source sphere.
	/// </summary>
	/// <param name="source">Sphere to clone</param>
	public BoundingSphere(BoundingSphere source) {
		m_center = source.center;
		m_radius = source.radius;
	}
	
	/// <summary>
	/// Sets the bounding sphere to copy from the source volume or to contain it.
	/// </summary>
	/// <param name="volume">Source volume</param>
	public void Set(Bounds bounds) {
		m_center = bounds.center;
		m_radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
	}
	
	/// <summary>
	/// Sets the bounding sphere to copy from the source volume or to contain it.
	/// </summary>
	/// <param name="volume">Source volume</param>
	public void Set(BoundingSphere bounds) {
		m_center = bounds.center;
		m_radius = bounds.radius;
	}
	
	/// <summary>
	/// Sets the bounding sphere to the specified center and radius.
	/// </summary>
	/// <param name="center">New center of the sphere</param>
	/// <param name="radius">New radius of the sphere</param>
	public void Set(Vector3 center, float radius) {
		m_center = center;
		m_radius = radius;
	}
	
	/// <summary>
	/// Sets the center of the volume from the specified coordinates.
	/// </summary>
	/// <param name="x">X coordinate</param>
	/// <param name="y">Y coordinate</param>
	/// <param name="z">Z coordinate</param>
	public void SetCenter(float x, float y, float z) {
		m_center.Set(x,y,z);
	}
	
	
	
	
	
//	public static BoundingSphere CreateFromPoints(IList<Vector3> points) {
//		if (points == null)
//			throw new ArgumentNullException("points");
//		
//		float radius = 0;
//		Vector3 center = new Vector3();
//		// First, we'll find the center of gravity for the point 'cloud'.
//		int num_points = points.Count;
//		
//		foreach (Vector3 v in points) {
//			center += v;    // If we actually knew the number of points, we'd get better accuracy by adding v / num_points.
//		}
//		
//		center /= (float)num_points;
//		
//		// Calculate the radius of the needed sphere (it equals the distance between the center and the point further away).
//		foreach (Vector3 v in points) {
//			float distance = ((Vector3)(v - center)).Length();
//			if (distance > radius)
//				radius = distance;
//		}
//		
//		return new BoundingSphere(center, radius);
//	}
	
	/// <summary>
	/// Computes the bounding sphere from a collection of 3D points.
	/// 
	/// </summary>
	/// <param name="points">Collection of points</param>
	public void CreateFromPoints(IEnumerable<Vector3> points) {
		var copy = points.ToArray();
		CalculateWelzl(copy, copy.Length, 0, 0);
	}
	
	
	//Welzl minimum bounding sphere algorithm
	void CalculateWelzl(Vector3[] points, int length, int supportCount, int index) {
		switch(supportCount) {
		case 0:
			m_radius = 0;
			m_center = Vector3.zero;
			break;
		case 1:
			m_radius = 1.0f - RADIUS_EPSILON;
			m_center = points[index-1];
			break;
		case 2:
			
			SetSphere(points[index-1], points[index-2]);
			
			break;
			
		case 3:
			SetSphere(points[index-1], points[index-2], points[index-3]);
			break;
		case 4:
			SetSphere(points[index-1], points[index-2], points[index-3], points[index-4]);
			return;
		}
		
		for(int i = 0; i < length; i++) {
			Vector3 comp = points[i + index];
			float distSqr;
			
			distSqr = (comp-m_center).sqrMagnitude;
			
			if(distSqr - (m_radius * m_radius) > RADIUS_EPSILON - 1.0f) {
				for(int j = i; j > 0; j--) {
					Vector3 a = points[j + index];
					Vector3 b = points[j - 1 + index];
					points[j + index] = b;
					points[j - 1 + index] = a;
				}
				CalculateWelzl(points, i, supportCount + 1, index + 1);
			}
		}
	}
	
	//For Welzl calc - 2 support points
	void SetSphere(Vector3 O, Vector3 A)
	{
		radius = (float) System.Math.Sqrt(((A.x - O.x) * (A.x - O.x) + (A.y - O.y)
		                                   * (A.y - O.y) + (A.z - O.z) * (A.z - O.z)) / 4.0f) + RADIUS_EPSILON - 1.0f;
		float x = (1 - .5f) * O.x + .5f * A.x;
		float y = (1 - .5f) * O.y + .5f * A.y;
		float z = (1 - .5f) * O.z + .5f * A.z;
		
		// TODO:
		SetCenter(x, y, z);
		
	}
	
	//For Welzl calc - 3 support points
	void SetSphere(Vector3 O, Vector3 A, Vector3 B) {
		Vector3 a = A - O;
		Vector3 b = B - O;
		Vector3 aCrossB = Vector3.Cross(a, b);
		float denom = 2.0f * Vector3.Dot(aCrossB, aCrossB);
		if(denom == 0) {
			m_center = Vector3.zero;
			m_radius = 0;
		} else {
			
			Vector3 o = ((Vector3.Cross(aCrossB, a) * b.sqrMagnitude)+ (Vector3.Cross(b, aCrossB) * a.sqrMagnitude)) / denom;
			m_radius = o.magnitude * RADIUS_EPSILON;
			m_center = O + o;
		}
	}
	
	//For Welzl calc - 4 support points
	void SetSphere(Vector3 O, Vector3 A, Vector3 B, Vector3 C) {
		Vector3 a = A - O;
		Vector3 b = B - O;
		Vector3 c = C - O;
		
		float denom = 2.0f * (a.x * (b.y * c.z - c.y * b.z) - b.x
		                      * (a.y * c.z - c.y * a.z) + c.x * (a.y * b.z - b.y * a.z));
		if(denom == 0) {
			m_center = Vector3.zero;
			m_radius = 0;
		} else {
			Vector3 o = ((Vector3.Cross(a, b) * c.sqrMagnitude)
			             + (Vector3.Cross(c, a) * b.sqrMagnitude)
			             + (Vector3.Cross(b, c) * a.sqrMagnitude)) / denom;
			m_radius = o.magnitude * RADIUS_EPSILON;
			m_center = O + o;
		}
	}
}
