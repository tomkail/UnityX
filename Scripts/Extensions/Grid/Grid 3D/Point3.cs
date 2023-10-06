using UnityEngine;

[System.Serializable]
public struct Point3 {
	public int x, y, z;

	public static Point3 zero {
		get {
			return new Point3(0,0,0);
		}
	}
	
	public static Point3 one {
		get {
			return new Point3(1,1,1);
		}
	}

	public static Point3 up {
		get {
			return new Point3(0,1,0);
		}
	}

	public static Point3 down {
		get {
			return new Point3(0,-1,0);
		}
	}

	public static Point3 left {
		get {
			return new Point3(-1,0,0);
		}
	}

	public static Point3 right {
		get {
			return new Point3(1,0,0);
		}
	}

	public static Point3 forward {
		get {
			return new Point3(0,0,1);
		}
	}

	public static Point3 back {
		get {
			return new Point3(0,0,-1);
		}
	}

	public Point3(int _x, int _y, int _z) {
		x = _x;
		y = _y;
		z = _z;
	}

	public Point3(float _x, float _y, float _z) {
		x = Mathf.RoundToInt(_x);
		y = Mathf.RoundToInt(_y);
		z = Mathf.RoundToInt(_z);
	}

	public Point3 (int[] xyz) {
		x = xyz[0];
		y = xyz[1];
		z = xyz[2];
	}

	public static Point3 FromVector3(Vector3 vector) {
		return new Point3(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
	}

	public static Vector3 ToVector3(Point3 point3) {
		return new Vector3(point3.x, point3.y, point3.z);
	}

	public Vector3 ToVector3() {
		return ToVector3(this);
	}

	public static Point3 FromPoint(Point point) {
		return new Point3(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), 0);
	}

	public static Point ToPoint(Point3 point3) {
		return new Point(point3.x, point3.y);
	}

	public Point ToPoint() {
		return ToPoint(this);
	}

	public override string ToString() {
		return "X: " + x + " Y: " + y + " Z: " + y;
	}

	public int area {
		get { return x * y * z; }
	}

	public int magnitude {
		get { return x * x + y * y + z * z; }
	}

	public int normalized {
		get { return 1; }
	}

	public int sqrMagnitude {
		get { return 1; }
	}

	public static Point3 Add(Point3 left, Point3 right){
		return new Point3(left.x+right.x, left.y+right.y, left.z+right.z);
	}

	public static Point3 Add(Point3 left, float right){
		return new Point3(left.x+right, left.y+right, left.z+right);
	}

	public static Point3 Add(float left, Point3 right){
		return new Point3(left+right.x, left+right.y, left+right.z);
	}


	public static Point3 Subtract(Point3 left, Point3 right){
		return new Point3(left.x-right.x, left.y-right.y, left.z-right.z);
	}

	public static Point3 Subtract(Point3 left, float right){
		return new Point3(left.x-right, left.y-right, left.z-right);
	}

	public static Point3 Subtract(float left, Point3 right){
		return new Point3(left-right.x, left-right.y, left-right.z);
	}


	public static Point3 Multiply(Point3 left, Point3 right){
		return new Point3(left.x*right.x, left.y*right.y, left.z*right.z);
	}

	public static Point3 Multiply(Point3 left, float right){
		return new Point3(left.x*right, left.y*right, left.z*right);
	}

	public static Point3 Multiply(float left, Point3 right){
		return new Point3(left*right.x, left*right.y, left*right.z);
	}


	public static Point3 Divide(Point3 left, Point3 right){
		return new Point3(left.x/right.x, left.y/right.y, left.z/right.z);
	}

	public static Point3 Divide(Point3 left, float right){
		return new Point3(left.x/right, left.y/right, left.z/right);
	}

	public static Point3 Divide(float left, Point3 right){
		return new Point3(left/right.x, left/right.y, left/right.z);
	}

	public override bool Equals(System.Object obj) {
		return obj is Point3 && this == (Point3)obj;
	}

	public bool Equals(Point3 p) {
		// Return true if the fields match:
		return (x == p.x) && (y == p.y) && (z == p.z);
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * 31 + x.GetHashCode();
			hash = hash * 31 + y.GetHashCode();
			hash = hash * 31 + z.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (Point3 left, Point3 right) {
		if (System.Object.ReferenceEquals(left, right))
		{
			return true;
		}

		// If one is null, but not both, return false.
		if (((object)left == null) || ((object)right == null))
		{
			return false;
		}
		if(left.x == right.x && left.y == right.y && left.z == right.z)return true;
		return false;
	}

	public static bool operator != (Point3 left, Point3 right) {
		return !(left == right);
	}

	public static Point3 operator +(Point3 left, Point3 right) {
		return Add(left, right);
	}

	public static Point3 operator -(Point3 left, Point3 right) {
		return Subtract(left, right);
	}

	public static Point3 operator -(Vector3 left, Point3 right) {
		return Subtract(left, right);
	}

	public static Point3 operator -(Point3 left, Vector3 right) {
		return Subtract(left, right);
	}

	public static implicit operator Point3(Vector3 src) {
		return FromVector3(src);
	}
	
	public static implicit operator Point3(Point src) {
		return FromPoint(src);
	}
	
	public static implicit operator Vector3(Point3 src) {
		return src.ToVector3();
	}

	public static implicit operator Point(Point3 src) {
		return src.ToPoint();
	}
}



/*
FROM PATHFINDING - HAD TO DELETE SOME COMMENTS - THEY MIGHT BE WORTH BRINGING BACK
using Pathfinding;
using UnityEngine;

namespace Pathfinding
{
	public struct Int3 {
		public int x;
		public int y;
		public int z;
		
		
		public const int Precision = 1000;
		
		
		public const float FloatPrecision = 1000F;
		
		
		public const float PrecisionFactor = 0.001F;
		
		
		//public const float CostFactor = 0.01F;
		
		private static Int3 _zero = new Int3(0,0,0);
		public static Int3 zero { get { return _zero; } }
		
		public Int3 (Vector3 position) {
			x = (int)System.Math.Round (position.x*FloatPrecision);
			y = (int)System.Math.Round (position.y*FloatPrecision);
			z = (int)System.Math.Round (position.z*FloatPrecision);
			//x = Mathf.RoundToInt (position.x);
			//y = Mathf.RoundToInt (position.y);
			//z = Mathf.RoundToInt (position.z);
		}
		
		
		public Int3 (int _x, int _y, int _z) {
			x = _x;
			y = _y;
			z = _z;
		}
		
		public static bool operator == (Int3 lhs, Int3 rhs) {
			return 	lhs.x == rhs.x &&
					lhs.y == rhs.y &&
					lhs.z == rhs.z;
		}
		
		public static bool operator != (Int3 lhs, Int3 rhs) {
			return 	lhs.x != rhs.x ||
					lhs.y != rhs.y ||
					lhs.z != rhs.z;
		}
		
		public static explicit operator Int3 (Vector3 ob) {
			return new Int3 (
				(int)System.Math.Round (ob.x*FloatPrecision),
				(int)System.Math.Round (ob.y*FloatPrecision),
				(int)System.Math.Round (ob.z*FloatPrecision)
				);
			//return new Int3 (Mathf.RoundToInt (ob.x*FloatPrecision),Mathf.RoundToInt (ob.y*FloatPrecision),Mathf.RoundToInt (ob.z*FloatPrecision));
		}
		
		public static explicit operator Vector3 (Int3 ob) {
			return new Vector3 (ob.x*PrecisionFactor,ob.y*PrecisionFactor,ob.z*PrecisionFactor);
		}
		
		public static Int3 operator - (Int3 lhs, Int3 rhs) {
			lhs.x -= rhs.x;
			lhs.y -= rhs.y;
			lhs.z -= rhs.z;
			return lhs;
		}
		
		public static Int3 operator + (Int3 lhs, Int3 rhs) {
			lhs.x += rhs.x;
			lhs.y += rhs.y;
			lhs.z += rhs.z;
			return lhs;
		}
		
		public static Int3 operator * (Int3 lhs, int rhs) {
			lhs.x *= rhs;
			lhs.y *= rhs;
			lhs.z *= rhs;
			
			return lhs;
		}
		
		public static Int3 operator * (Int3 lhs, float rhs) {
			lhs.x = (int)System.Math.Round (lhs.x * rhs);
			lhs.y = (int)System.Math.Round (lhs.y * rhs);
			lhs.z = (int)System.Math.Round (lhs.z * rhs);
			
			return lhs;
		}
		
		public static Int3 operator * (Int3 lhs, Vector3 rhs) {
			lhs.x = (int)System.Math.Round (lhs.x * rhs.x);
			lhs.y =	(int)System.Math.Round (lhs.y * rhs.y);
			lhs.z = (int)System.Math.Round (lhs.z * rhs.z);
			
			return lhs;
		}
		
		public static Int3 operator / (Int3 lhs, float rhs) {
			lhs.x = (int)System.Math.Round (lhs.x / rhs);
			lhs.y = (int)System.Math.Round (lhs.y / rhs);
			lhs.z = (int)System.Math.Round (lhs.z / rhs);
			return lhs;
		}
		
		public int this[int i] {
			get {
				return i == 0 ? x : (i == 1 ? y : z);
			}
		}
		
		public static int Dot (Int3 lhs, Int3 rhs) {
			return
					lhs.x * rhs.x +
					lhs.y * rhs.y +
					lhs.z * rhs.z;
		}
		
		public Int3 NormalizeTo (int newMagn) {
			float magn = magnitude;
			
			if (magn == 0) {
				return this;
			}
			
			x *= newMagn;
			y *= newMagn;
			z *= newMagn;
			
			x = (int)System.Math.Round (x/magn);
			y = (int)System.Math.Round (y/magn);
			z = (int)System.Math.Round (z/magn);
			
			return this;
		}
		
		public float magnitude {
			get {
				//It turns out that using doubles is just as fast as using ints with Mathf.Sqrt. And this can also handle larger numbers (possibly with small errors when using huge numbers)!
				
				double _x = x;
				double _y = y;
				double _z = z;
				
				return (float)System.Math.Sqrt (_x*_x+_y*_y+_z*_z);
				
				//return Mathf.Sqrt (x*x+y*y+z*z);
			}
		}
		
		public int costMagnitude {
			get {
				return (int)System.Math.Round (magnitude);
			}
		}
		
		public float worldMagnitude {
			get {
				double _x = x;
				double _y = y;
				double _z = z;
				
				return (float)System.Math.Sqrt (_x*_x+_y*_y+_z*_z)*PrecisionFactor;
				
			}
		}
		
		public float sqrMagnitude {
			get {
				double _x = x;
				double _y = y;
				double _z = z;
				return (float)(_x*_x+_y*_y+_z*_z);
				//return x*x+y*y+z*z;
			}
		}
		
		public int unsafeSqrMagnitude {
			get {
				return x*x+y*y+z*z;
			}
		}
		
		
		[System.Obsolete ("Same implementation as .magnitude")]
		public float safeMagnitude {
			get {
				//Of some reason, it is faster to use doubles (almost 40% faster)
				double _x = x;
				double _y = y;
				double _z = z;
				
				return (float)System.Math.Sqrt (_x*_x+_y*_y+_z*_z);
				
			}
		}
		
		[System.Obsolete (".sqrMagnitude is now per default safe (.unsafeSqrMagnitude can be used for unsafe operations)")]
		public float safeSqrMagnitude {
			get {
				float _x = x*PrecisionFactor;
				float _y = y*PrecisionFactor;
				float _z = z*PrecisionFactor;
				return _x*_x+_y*_y+_z*_z;
			}
		}
		
		public static implicit operator string (Int3 ob) {
			return ob.ToString ();
		}
		
		
		public override string ToString () {
			return "( "+x+", "+y+", "+z+")";
		}
		
		public override bool Equals (System.Object o) {
			
			if (o == null) return false;
			
			Int3 rhs = (Int3)o;
			
			return 	x == rhs.x &&
					y == rhs.y &&
					z == rhs.z;
		}
		
		public override int GetHashCode () {
			return x*9+y*10+z*11;
		}
	}
}
*/
