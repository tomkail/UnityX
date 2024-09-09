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






/*
/// <summary>
/// A sphere that is defined by a center and a radius. This is one of the simpler volumes and collision checks
/// are fast, but may yield more false-positives than using volumes that more tightly enclose the geometry (such as OrientedBoundingBox).
/// 
/// The algorithm for generating the sphere is an implemention of Welzl's minimum-volume sphere algorithm.
/// </summary>
public sealed class BoundingSphere {
	
	/// <summary>
	/// Center of the bounding volume, this is common to all bounding volumes.
	/// </summary>
	protected Vector3 m_center;
	
	/// <summary>
	/// Gets or sets the center of the bounding volume.
	/// </summary>
	public Vector3 Center {
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
	public float Radius {
		get {
			return m_radius;
		}
		set {
			m_radius = value;
		}
	}
	
	/// <summary>
	/// Gets the volume of the sphere.
	/// </summary>
	public float Volume {
		get {
			return MathX.VolumeFromRadius(m_radius);
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
		m_center = source.Center;
		m_radius = source.Radius;
	}
	
	/// <summary>
	/// Sets the bounding sphere to copy from the source volume or to contain it.
	/// </summary>
	/// <param name="volume">Source volume</param>
	public void Set(Bounds bounds) {
		m_center = bounds.center;
		m_radius = bounds.extents.Length();			
	}
	
	/// <summary>
	/// Sets the bounding sphere to copy from the source volume or to contain it.
	/// </summary>
	/// <param name="volume">Source volume</param>
	public void Set(BoundingSphere bounds) {
		m_center = bounds.Center;
		m_radius = bounds.Radius;
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
		m_center.X = x;
		m_center.Y = y;
		m_center.Z = z;
	}
	
	
	
	
	
	/// <summary>
	/// Computes the distance from the center of this volume
	/// to the point.
	/// </summary>
	/// <param name="point">Vector3</param>
	/// <returns>Distance</returns>
	public float DistanceTo(Vector3 point) {
		return Vector3.Distance(Center, point);
	}
	
	
	/// <summary>
	/// Compute this bounding volume from a set of 3D points.
	/// </summary>
	/// <param name="points">DataBuffer of points</param>
	public void ComputeFromPoints(DataBuffer<Vector3> points) {
		ComputeFromPoints(points.Buffer);
	}
	
	
	
	/// <summary>
	/// Compute this bounding volume from a set of primitives.
	/// </summary>
	/// <param name="vertices">Vertex buffer</param>
	/// <param name="indices">Index buffer</param>
	public void ComputeFromPrimitives(DataBuffer<Vector3> vertices, DataBuffer<int> indices) {
		ComputeFromPrimitives(vertices.Buffer, indices.Buffer);
	}
	
	/// <summary>
	/// Compute this bounding volume from a set of primitives.
	/// </summary>
	/// <param name="vertices">Vertex buffer</param>
	/// <param name="indices">Index buffer</param>
	public abstract void ComputeFromPrimitives(Vector3[] vertices, short[] indices);
	
	/// <summary>
	/// Compute this bounding volume from a set of primitives.
	/// </summary>
	/// <param name="vertices">Vertex buffer</param>
	/// <param name="indices">Index buffer</param>
	public void ComputeFromPrimitives(DataBuffer<Vector3> vertices, DataBuffer<short> indices) {
		ComputeFromPrimitives(vertices.Buffer, indices.Buffer);
	}
	
	
	
	
	
	
	
	/// <summary>
	/// Returns a new instance of BoundingSphere with the same center/radius.
	/// </summary>
	/// <returns></returns>
	public BoundingSphere Clone() {
		return new BoundingSphere(this);
	}
	
	/// <summary>
	/// Computes the distance from the point to the nearest edge of the volume.
	/// </summary>
	/// <param name="point">Point to use</param>
	/// <returns>Distance from the point to the volume</returns>
	public float DistanceToEdge(Vector3 point) {
		return Vector3.Distance(m_center, point) - m_radius;
	}
	
	/// <summary>
	/// Computes the bounding sphere from a collection of 3D points.
	/// 
	/// </summary>
	/// <param name="points">Collection of points</param>
	public void ComputeFromPoints(Vector3[] points) {
		Vector3[] copy = new Vector3[points.Length];
		Array.Copy(points, copy, points.Length);
		CalculateWelzl(copy, copy.Length, 0, 0);
	}
	
	/// <summary>
	/// Computes the bounding sphere from a collection of indexed primitives.
	/// </summary>
	/// <param name="vertices">Collection of positions</param>
	/// <param name="indices">Collection of indices</param>
	public override void ComputeFromPrimitives(Vector3[] vertices, int[] indices) {
		Vector3[] copy = new Vector3[indices.Length];
		
		for(int i = 0; i < indices.Length; i++) {
			copy[i] = vertices[indices[i]];
		}
		
		CalculateWelzl(copy, copy.Length, 0, 0);
	}
	
	/// <summary>
	/// Computes the bounding sphere from a collection of indexed primitives.
	/// </summary>
	/// <param name="vertices">Collection of positions</param>
	/// <param name="indices">Collection of indices</param>
	public override void ComputeFromPrimitives(Vector3[] vertices, short[] indices) {
		Vector3[] copy = new Vector3[indices.Length];
		
		for(int i = 0; i < indices.Length; i++) {
			copy[i] = vertices[indices[i]];
		}
		
		CalculateWelzl(copy, copy.Length, 0, 0);
	}
	
	//Welzl minimum bounding sphere algorithm
	private void CalculateWelzl(Vector3[] points, int length, int supportCount, int index) {
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
			Vector3X.SqrDistance(ref comp, ref m_center, out distSqr);
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
	private void SetSphere(Vector3 O, Vector3 A) {
		Radius = (float) System.Math.Sqrt(((A.X - O.X) * (A.X - O.X) + (A.Y - O.Y)
		                                   * (A.Y - O.Y) + (A.Z - O.Z) * (A.Z - O.Z)) / 4.0f) + RADIUS_EPSILON - 1.0f;
		float x = (1 - .5f) * O.X + .5f * A.X;
		float y = (1 - .5f) * O.Y + .5f * A.Y;
		float z = (1 - .5f) * O.Z + .5f * A.Z;
		SetCenter(x, y, z);
	}
	
	//For Welzl calc - 3 support points
	private void SetSphere(Vector3 O, Vector3 A, Vector3 B) {
		Vector3 a = A - O;
		Vector3 b = B - O;
		Vector3 aCrossB = Vector3.Cross(a, b);
		float denom = 2.0f * Vector3.Dot(aCrossB, aCrossB);
		if(denom == 0) {
			m_center = Vector3.zero;
			m_radius = 0;
		} else {
			Vector3 o = ((Vector3.Cross(aCrossB, a) * b.sqrMagnitude) 
			             + (Vector3.Cross(b, aCrossB) * a.sqrMagnitude)) / denom;
			m_radius = o.Length() * RADIUS_EPSILON;
			m_center = O + o;
		}
	}
	
	//For Welzl calc - 4 support points
	private void SetSphere(Vector3 O, Vector3 A, Vector3 B, Vector3 C) {
		Vector3 a = A - O;
		Vector3 b = B - O;
		Vector3 c = C - O;
		
		float denom = 2.0f * (a.X * (b.Y * c.Z - c.Y * b.Z) - b.X 
		                      * (a.Y * c.Z - c.Y * a.Z) + c.X * (a.Y * b.Z - b.Y * a.Z));
		if(denom == 0) {
			m_center = Vector3.zero;
			m_radius = 0;
		} else {
			Vector3 o = ((Vector3.Cross(a, b) * c.sqrMagnitude) 
			             + (Vector3.Cross(c, a) * b.sqrMagnitude)
			             + (Vector3.Cross(b, c) * a.sqrMagnitude)) / denom;
			m_radius = o.Length() * RADIUS_EPSILON;
			m_center = O + o;
		}
	}
	
	/// <summary>
	/// Tests if the bounding box intersects with this bounding sphere.
	/// </summary>
	/// <param name="box">Bounding box to test</param>
	/// <returns>True if they intersect</returns>
	public override bool Intersects(Bounds box) {
		if(box == null) {
			return false;
		}
		
		Vector3 bCenter = box.center;
		Vector3 extents = box.extents;
		
		if(System.Math.Abs(m_center.X - bCenter.X) < m_radius + extents.X
		   && System.Math.Abs(m_center.Y - bCenter.Y) < m_radius + extents.Y
		   && System.Math.Abs(m_center.Z - bCenter.Z) < m_radius + extents.Z) {
			return true;
		}
		
		return false;
	}
	
	/// <summary>
	/// Tests if the bounding sphere intersects with this bounding sphere.
	/// </summary>
	/// <param name="sphere">Bounding sphere to test</param>
	/// <returns>True if they intersect</returns>
	public override bool Intersects(BoundingSphere sphere) {
		if(sphere == null) {
			return false;
		}
		
		Vector3 diff;
		Vector3 sCenter = sphere.Center;
		Vector3.Subtract(ref m_center, ref sCenter, out diff);
		
		float radSum = sphere.Radius + m_radius;
		float dot;
		Vector3.Dot(ref diff, ref diff, out dot);
		
		return dot <= radSum * radSum;
	}
	
	/// <summary>
	/// Tests if the ray intersects with this sphere.
	/// </summary>
	/// <param name="ray">Ray to test</param>
	/// <returns>True if the ray intersects the sphere</returns>
	public override bool Intersects(Ray ray) {
		//Test if the origin is inside the sphere
		Vector3 rOrigin = ray.origin;
		Vector3 diff;
		Vector3.Subtract(ref rOrigin, ref m_center, out diff);
		float radSquared = m_radius * m_radius;
		
		float dot;
		Vector3.Dot(ref diff, ref diff, out dot);
		float a = dot - radSquared;
		
		if(a <= 0.0f) {
			return true;
		}
		
		//Outside sphere
		Vector3 rDir = ray.direction;
		float b;
		Vector3.Dot(ref rDir, ref diff, out b);
		if(b >= 0.0f) {
			return false;
		}
		
		return b * b >= a;
	}
	
	/// <summary>
	/// Tests if the ray intersects with this sphere.
	/// </summary>
	/// <param name="ray">Ray to test</param>
	/// <param name="result">Bool to hold the result, true if they intersect</param>
	public override void Intersects(ref Ray ray, out bool result) {
		//Test if the origin is inside the sphere
		Vector3 rOrigin = ray.origin;
		Vector3 diff;
		Vector3.Subtract(ref rOrigin, ref m_center, out diff);
		float radSquared = m_radius * m_radius;
		
		float dot;
		Vector3.Dot(ref diff, ref diff, out dot);
		float a = dot - radSquared;
		
		if(a <= 0.0f) {
			result = true;
			return;
		}
		
		//Outside sphere
		Vector3 rDir = ray.direction;
		float b;
		Vector3.Dot(ref rDir, ref diff, out b);
		if(b >= 0.0f) {
			result = false;
			return;
		}
		
		result = b * b >= a;
	}
	
	/// <summary>
	/// Tests where on the sphere the ray intersects, if it does.
	/// </summary>
	/// <param name="ray">Ray to test</param>
	/// <returns>Intersection result</returns>
	public BoundingIntersectionRecord IntersectsWhere(Ray ray) {
		Vector3 diff;
		Vector3 rOrigin = ray.Origin;
		Vector3 rDir = ray.Direction;
		Vector3.Subtract(ref rOrigin, ref m_center, out diff);
		float a, a1, discr, sqrt;
		Vector3.Dot(ref diff, ref diff, out a);
		a -= m_radius * m_radius;
		
		//Check if we're inside the sphere, if so then the result must have
		//one exit point
		if(a <= 0.0f) {
			Vector3.Dot(ref rDir, ref diff, out a1);
			discr = (a1 * a1) - a;
			sqrt = MathHelper.Sqrt(discr);
			
			//Calc the distance and exit point
			float dist = sqrt - a1;
			Vector3 p;
			Vector3.Multiply(ref rDir, dist, out p);
			Vector3.Add(ref p, ref rOrigin, out p);
			IntersectionRecord rec = new IntersectionRecord(p, dist);
			
			return new BoundingIntersectionRecord(rec);
		}
		
		Vector3.Dot(ref rDir, ref diff, out a1);
		if(a1 >= 0.0f) {
			//No intersections
			return new BoundingIntersectionRecord();
		}
		
		discr = a1 * a1 - a;
		if(discr < 0.0f) {
			//Two complex-valued roots, No intersections
			return new BoundingIntersectionRecord();
		} else if(discr >= MathHelper.ZeroTolerance) {
			//Two distinct real-valued roots, two intersections
			sqrt = MathHelper.Sqrt(discr);
			
			float dist1 = System.Math.Abs(-a1 - sqrt);
			float dist2 = System.Math.Abs(-a1 + sqrt);
			Vector3 p1;
			Vector3.Multiply(ref rDir, dist1, out p1);
			Vector3.Add(ref p1, ref rOrigin, out p1);
			IntersectionRecord rec1 = new IntersectionRecord(p1, dist1);
			
			Vector3 p2;
			Vector3.Multiply(ref rDir, dist2, out p2);
			Vector3.Add(ref p2, ref rOrigin, out p2);
			IntersectionRecord rec2 = new IntersectionRecord(p2, dist2);
			
			return new BoundingIntersectionRecord(rec1, rec2);
		}
		
		//The ray intersects the sphere at exactly one point.
		float dis = -a1;
		Vector3 point;
		Vector3.Multiply(ref rDir, dis, out point);
		Vector3.Add(ref point, ref rOrigin, out point);
		IntersectionRecord rec0 = new IntersectionRecord(point, dis);
		
		return new BoundingIntersectionRecord(rec0);
	}
	
	/// <summary>
	/// Tests where on the sphere the ray intersects, if it does.
	/// </summary>
	/// <param name="ray">Ray to test</param>
	/// <param name="result">The intersection result</param>
	public void IntersectsWhere(ref Ray ray, out BoundingIntersectionRecord result) {
		Vector3 diff;
		Vector3 rOrigin = ray.Origin;
		Vector3 rDir = ray.Direction;
		Vector3.Subtract(ref rOrigin, ref m_center, out diff);
		float a, a1, discr, sqrt;
		Vector3.Dot(ref diff, ref diff, out a);
		a -= m_radius * m_radius;
		
		//Check if we're inside the sphere, if so then the result must have
		//one exit point
		if(a <= 0.0f) {
			Vector3.Dot(ref rDir, ref diff, out a1);
			discr = (a1 * a1) - a;
			sqrt = MathHelper.Sqrt(discr);
			
			//Calc the distance and exit point
			float dist = sqrt - a1;
			Vector3 p;
			Vector3.Multiply(ref rDir, dist, out p);
			Vector3.Add(ref p, ref rOrigin, out p);
			IntersectionRecord rec = new IntersectionRecord(p, dist);
			
			result = new BoundingIntersectionRecord(rec);
			return;
		}
		
		Vector3.Dot(ref rDir, ref diff, out a1);
		if(a1 >= 0.0f) {
			//No intersections
			result = new BoundingIntersectionRecord();
			return;
		}
		
		discr = a1 * a1 - a;
		if(discr < 0.0f) {
			//No intersections
			result = new BoundingIntersectionRecord();
			return;
		} else if(discr >= MathHelper.ZeroTolerance) {
			//Entry and exit exist
			sqrt = MathHelper.Sqrt(discr);
			
			float dist1 = System.Math.Abs(-a1 - sqrt);
			float dist2 = System.Math.Abs(-a1 + sqrt);
			Vector3 p1;
			Vector3.Multiply(ref rDir, dist1, out p1);
			Vector3.Add(ref p1, ref rOrigin, out p1);
			IntersectionRecord rec1 = new IntersectionRecord(p1, dist1);
			
			Vector3 p2;
			Vector3.Multiply(ref rDir, dist2, out p2);
			Vector3.Add(ref p2, ref rOrigin, out p2);
			IntersectionRecord rec2 = new IntersectionRecord(p2, dist2);
			
			result = new BoundingIntersectionRecord(rec1, rec2);
			return;
		}
		
		//The ray intersects the sphere at exactly one point.
		float dis = -a1;
		Vector3 point;
		Vector3.Multiply(ref rDir, dis, out point);
		Vector3.Add(ref point, ref rOrigin, out point);
		IntersectionRecord rec0 = new IntersectionRecord(point, dis);
		
		result = new BoundingIntersectionRecord(rec0);
		return;
	}
	
	/// <summary>
	/// Tests if the plane intersects with this bounding sphere, and if not
	/// which side the volume is on relative to the plane.
	/// </summary>
	/// <param name="plane">Plane to test</param>
	/// <returns>Plane intersection type</returns>
	public PlaneIntersectionType Intersects(Plane plane) {
		float dotC;
		Plane.DotCoordinate(ref plane, ref m_center, out dotC);
		
		if(dotC > m_radius) {
			return PlaneIntersectionType.Front;
		} else if(dotC < -m_radius) {
			return PlaneIntersectionType.Back;
		}
		
		return PlaneIntersectionType.Intersects;
	}
	
	/// <summary>
	/// Tests if the plane intersects with this bounding sphere, and if not
	/// which side the volume is on relative to the plane.
	/// </summary>
	/// <param name="plane">Plane to test</param>
	/// <param name="result">Result, the plane intersection type</param>
	public void Intersects(ref Plane plane, out PlaneIntersectionType result) {
		float dotC;
		Plane.DotCoordinate(ref plane, ref m_center, out dotC);
		
		if(dotC > m_radius) {
			result = PlaneIntersectionType.Front;
			return;
		} else if(dotC < -m_radius) {
			result = PlaneIntersectionType.Back;
			return;
		}
		
		result = PlaneIntersectionType.Intersects;
	}
	
	/// <summary>
	/// Tests if the specified point is inside this bounding sphere or not.
	/// </summary>
	/// <param name="point">Point to test</param>
	/// <returns>Inside or outside</returns>
	public ContainmentType Contains(Vector3 point) {
		float dist;
		Vector3.DistanceSquared(ref m_center, ref point, out dist);
		if(dist < m_radius * m_radius) {
			return ContainmentType.Inside;
		} else {
			return ContainmentType.Outside;
		}
	}
	
	/// <summary>
	/// Determines if the AABB is contained within this bounding volume.
	/// </summary>
	/// <param name="box">AABB to test</param>
	/// <returns>Containment type</returns>
	public ContainmentType Contains(BoundingBox box) {
		if(box == null) {
			return ContainmentType.Outside;
		}
		
		Vector3 bMin = box.Min;
		Vector3 bMax = box.Max;
		
		Vector3 temp;
		float dist;
		Vector3.Clamp(ref m_center, ref bMin, ref bMax, out temp);
		Vector3.Distance(ref m_center, ref temp, out dist);
		
		//Check if we're outside
		if(dist > (m_radius * m_radius)) {
			return ContainmentType.Outside;
		}
		
		//Or contained
		if(((bMin.X + m_radius) <= m_center.X)
		   && (m_center.X <= (bMax.X - m_radius))
		   && ((bMax.X - bMin.X) > m_radius)
		   && ((bMin.Y + m_radius) <= m_center.Y)
		   && (m_center.Y <= (bMax.Y - m_radius))
		   && ((bMax.Y - bMin.Y) > m_radius)
		   && ((bMin.Z + m_radius) <= m_center.Z)
		   && (m_center.Z <= (bMax.Z - m_radius))
		   && ((bMax.X - bMin.X) > m_radius)) {
			return ContainmentType.Inside;
		}
		
		//Otherwise we overlap
		return ContainmentType.Intersects;
	}
	
	/// <summary>
	/// Determines if the sphere is contained within this bounding volume.
	/// </summary>
	/// <param name="sphere">Sphere to test</param>
	/// <returns>Containment type</returns>
	public ContainmentType Contains(BoundingSphere sphere) {
		if(sphere == null) {
			return ContainmentType.Outside;
		}
		
		float dist;
		Vector3 sCenter = sphere.Center;
		float sRad = sphere.Radius;
		
		Vector3.Distance(ref m_center, ref sCenter, out dist);
		
		if((m_radius + sRad) < dist) {
			return ContainmentType.Outside;
		} else if((m_radius - sRad) < dist) {
			return ContainmentType.Intersects;
		}
		
		return ContainmentType.Inside;
	}
	
	/// <summary>
	/// Merges this bounding volume with a second one. The resulting bounding
	/// volume is stored locally and will contain the original volumes completely.
	/// </summary>
	/// <param name="volume">Volume to merge with</param>
	public void MergeLocal(BoundingVolume volume) {
		if(volume == null) {
			return;
		}
		
		float radius;
		Vector3 center;
		
		switch(volume.BoundingType) {
		case BoundingType.AABB:
			BoundingBox box = volume as BoundingBox;
			radius = box.Extents.Length();
			center = box.Center;
			MergeSphere(ref radius, ref center, this);
			return;
		case BoundingType.Sphere:
			BoundingSphere sphere = volume as BoundingSphere;
			radius  = sphere.Radius;
			center = sphere.Center;
			MergeSphere(ref radius, ref center, this);
			return;
		case Bounding.BoundingType.OBB:
			OrientedBoundingBox obb = volume as OrientedBoundingBox;
			MergeOBB(obb, this);
			return;
		}
	}
	
	/// <summary>
	/// Merges this bounding volume with a second one. The resulting
	/// bounding volume is returned as a new object and will contain the 
	/// original volumes completely. The returned value will be of the
	/// same bounding type as the caller.
	/// </summary>
	/// <param name="volume">Volume to merge with</param>
	/// <returns>New volume that contains the original two</returns>
	public BoundingVolume Merge(BoundingVolume volume) {
		if(volume == null) {
			return this.Clone();
		}
		
		float radius;
		Vector3 center;
		
		BoundingSphere result = new BoundingSphere();
		switch(volume.BoundingType) {
		case BoundingType.AABB:
			BoundingBox box = volume as BoundingBox;
			radius = box.Extents.Length();
			center = box.Center;
			MergeSphere(ref radius, ref center, result);
			break;
		case BoundingType.Sphere:
			BoundingSphere sphere = volume as BoundingSphere;
			radius = sphere.Radius;
			center = sphere.Center;
			MergeSphere(ref radius, ref center, result);
			break;
		case Bounding.BoundingType.OBB:
			OrientedBoundingBox obb = volume as OrientedBoundingBox;
			MergeOBB(obb, result);
			break;
		}
		
		return result;
	}
	
	
	/// <summary>
	/// Handles the merge cases for AABB and other Spheres. Inputs are the
	/// radius and center of the volume to merge with this Sphere, and the
	/// merged volume is stored in the result.
	/// </summary>
	/// <param name="radius">Radius of the first volume</param>
	/// <param name="center">Center of the first volume</param>
	/// <param name="result">Sphere to store the result</param>
	private void MergeSphere(ref float radius, ref Vector3 center, BoundingSphere result) {
		Vector3 diff;
		Vector3.Subtract(ref center, ref m_center, out diff);
		float radDiff = radius - m_radius;
		float radDiffSqr = radDiff * radDiff;
		float lengthSquared = diff.sqrMagnitude;
		
		//Check if one sphere contains the other wholly
		if((radDiffSqr) >= lengthSquared) {
			//This sphere contains the other
			if(radDiff <= 0.0f) {
				result.Set(this);
				return;
			} 
			//Otherwise it contains us
			result.Center = center;
			result.Radius = radius;
			return;
		}
		
		//Find the new center between the two spheres
		Vector3 resCenter = m_center;
		float length = Mathf.Sqrt(lengthSquared);
		
		//If the centers are at least a small amount apart, place the new center
		//between the two weighted by their radii
		if(length > Mathf.Epsilon) {
			float a = (length + radDiff) / (2.0f * length);
			
			Vector3 temp;
			temp = diff * a;
			Vector3.Add(ref resCenter, ref temp, out resCenter);
		}
		
		//Compute the new radius and set
		result.Radius = 0.5f * (length + m_radius + radius);
		result.Center = resCenter;
	}
}
*/