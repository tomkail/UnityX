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
		return "X: " + x + " Y: " + y + " Z: " + z;
	}

	public int area {
		get { return x * y * z; }
	}

	public float magnitude {
		get { return Mathf.Sqrt(sqrMagnitude); }
	}

	public Vector3 normalized {
		get { return ((Vector3)this).normalized; }
	}

	public int sqrMagnitude {
		get { return x * x + y * y + z * z; }
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
		return left.x == right.x && left.y == right.y && left.z == right.z;
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

	// Lossless conversions to/from Unity's Vector3Int (identical int x/y/z layout). These let call
	// sites migrate from Point3 to Vector3Int incrementally — assignments and returns auto-convert.
	public static implicit operator Vector3Int(Point3 src) {
		return new Vector3Int(src.x, src.y, src.z);
	}

	public static implicit operator Point3(Vector3Int src) {
		return new Point3(src.x, src.y, src.z);
	}
}
