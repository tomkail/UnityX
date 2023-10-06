using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

[System.Serializable]
public struct Point : IEquatable<Point> {
	public int x, y;

	public static Point zero {
		get {
			return new Point(0,0);
		}
	}
	
	public static Point one {
		get {
			return new Point(1,1);
		}
	}

	public static Point up {
		get {
			return new Point(0,1);
		}
	}

	public static Point down {
		get {
			return new Point(0,-1);
		}
	}

	public static Point left {
		get {
			return new Point(-1,0);
		}
	}

	public static Point right {
		get {
			return new Point(1,0);
		}
	}
	
	public Point(int _x, int _y) {
		x = _x;
		y = _y;
	}

	public Point(float _x, float _y) {
		x = Mathf.RoundToInt(_x);
		y = Mathf.RoundToInt(_y);
	}
	
	public Point(Vector2 v) {
		x = Mathf.RoundToInt(v.x);
		y = Mathf.RoundToInt(v.y);
	}
	
	public Point (int[] xy) {
		x = xy[0];
		y = xy[1];
	}

	public static Point Min (Point p1, Point p2) {
		Vector2 vector;
		vector.x = (p1.x < p2.x) ? p1.x : p2.x;
		vector.y = (p1.y < p2.y) ? p1.y : p2.y;
	    return vector;
	}

	public static Point Max (Point p1, Point p2) {
		Vector2 vector;
		vector.x = (p1.x < p2.x) ? p2.x : p1.x;
		vector.y = (p1.y < p2.y) ? p2.y : p1.y;
	    return vector;
	}
	public static int ManhattanDistance(Point p1, Point p2) {
		return Math.Abs(p1.x-p2.x) + Math.Abs(p1.y-p2.y);
	}

	public static int DiagonalDistance(Point p1, Point p2) {
		return Mathf.Max(Math.Abs(p1.x-p2.x), Math.Abs(p1.y-p2.y));
	}

	public static Point FromVector2(Vector2 vector) {
		return new Point(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
	}

	public static Point FromVector3(Vector3 vector) {
		return new Point(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
	}

	public static Vector2 ToVector2(Point point) {
		return new Vector2(point.x, point.y);
	}

	public static Vector3 ToVector3(Point point) {
		return new Vector3(point.x, point.y, 0);
	}

	public Vector2 ToVector2() {
		return ToVector2(this);
	}

	public Vector3 ToVector3() {
		return ToVector3(this);
	}

	public override string ToString() {
		return string.Format("({0}, {1})",x,y);
	}

	public int area {
		get { return x * y; }
	}

	public float magnitude {
		get { return Mathf.Sqrt(x * x + y * y); }
	}

	public int sqrMagnitude {
		get { return x * x + y * y; }
	}

	public static Point Add(Point left, Point right){
		return new Point(left.x+right.x, left.y+right.y);
	}

	public static Point Add(Point left, float right){
		return new Point(left.x+right, left.y+right);
	}

	public static Point Add(float left, Point right){
		return new Point(left+right.x, left+right.y);
	}


	public static Point Subtract(Point left, Point right){
		return new Point(left.x-right.x, left.y-right.y);
	}

	public static Point Subtract(Point left, float right){
		return new Point(left.x-right, left.y-right);
	}

	public static Point Subtract(float left, Point right){
		return new Point(left-right.x, left-right.y);
	}


	public static Point Multiply(Point left, Point right){
		return new Point(left.x*right.x, left.y*right.y);
	}

	public static Point Multiply(Point left, float right){
		return new Point(left.x*right, left.y*right);
	}

	public static Point Multiply(float left, Point right){
		return new Point(left*right.x, left*right.y);
	}


	public static Point Divide(Point left, Point right){
		return new Point(left.x/right.x, left.y/right.y);
	}

	public static Point Divide(Point left, float right){
		return new Point(left.x/right, left.y/right);
	}

	public static Point Divide(float left, Point right){
		return new Point(left/right.x, left/right.y);
	}

	public override bool Equals(System.Object obj) {
		return obj is Point && this == (Point)obj;
	}

	public bool Equals(Point p) {
		return x == p.x && y == p.y;
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * 31 + x.GetHashCode();
			hash = hash * 31 + y.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (Point left, Point right) {
		return left.Equals(right);
	}

	public static bool operator != (Point left, Point right) {
		return !(left == right);
	}

	public static Point operator +(Point left, Point right) {
		return Add(left, right);
	}

	public static Point operator -(Point left) {
		return new Point(-left.x, -left.y);
	}

	public static Point operator -(Point left, Point right) {
		return Subtract(left, right);
	}

	public static Point operator -(Vector2 left, Point right) {
		return Subtract(left, right);
	}

	public static Point operator -(Point left, Vector2 right) {
		return Subtract(left, right);
	}


	public static Point operator *(Point left, Point right) {
		return Multiply(left, right);
	}

	public static Point operator *(Vector2 left, Point right) {
		return Multiply(left, right);
	}

	public static Point operator *(Point left, Vector2 right) {
		return Multiply(left, right);
	}
	
	public static Point operator *(Point left, float right) {
		return Multiply(left, right);
	}


	public static Point operator /(Point left, Point right) {
		return Divide(left, right);
	}

	public static Point operator /(Vector2 left, Point right) {
		return Divide(left, right);
	}

	public static Point operator /(Point left, Vector2 right) {
		return Divide(left, right);
	}
	
	public static Point operator /(Point left, float right) {
		return Divide(left, right);
	}

	public static implicit operator Point(Vector2 src) {
		return FromVector2(src);
	}

	public static implicit operator Point(Vector3 src) {
		return FromVector3(src);
	}
	
	public static implicit operator Vector2(Point src) {
		return src.ToVector2();
	}

	public static implicit operator Vector3(Point src) {
		return src.ToVector3();
	}

	public static ReadOnlyCollection<Point> cardinalDirections = Array.AsReadOnly(new Point[4] {
		new Point(0, 1),
		new Point(1, 0),
		new Point(0, -1),
		new Point(-1, 0)
	});
	/// <summary>
	/// The four main compass directions N E S W
	/// </summary>
	/// <returns>The directions.</returns>
	public static ReadOnlyCollection<Point> CardinalDirections(){
		return cardinalDirections;
	}

	/// <summary>
	/// The four main compass directions N E S W, relative to a point
	/// </summary>
	/// <returns>The directions.</returns>
	/// <param name="gridPoint">Grid point.</param>
	public static IEnumerable<Point> CardinalDirections(Point gridPoint){
		for(int i = 0; i < 4; i++) {
			yield return cardinalDirections[i] + gridPoint;
		}
	}

	/// <summary>
	/// The four diagonal compass directions NE NW SE SW
	/// </summary>
	/// <returns>The directions.</returns>
	public static Point[] OrdinalDirections(){
		Point[] points = new Point[4];
		points[0] = new Point(1, 1);
		points[1] = new Point(1, -1);
		points[2] = new Point(-1, -1);
		points[3] = new Point(-1, 1);
		return points;
	}

	/// <summary>
	/// The four diagonal compass directions NE NW SE SW, relative to a point
	/// </summary>
	/// <returns>The directions.</returns>
	/// <param name="gridPoint">Grid point.</param>
	public static Point[] OrdinalDirections(Point gridPoint){
		Point[] points = OrdinalDirections();
		for(int i = 0; i < points.Length; i++) points[i] += gridPoint;
		return points;
	}

	/// <summary>
	/// The 8 Cardinal and ordinal directions
	/// </summary>
	/// <returns>The directions.</returns>
	/// <param name="gridPoint">Grid point.</param>
	public static Point[] CompassDirections(Point gridPoint){
		Point[] points = new Point[8];
		points[0] = new Point(0, 1);
		points[1] = new Point(1, 1);
		points[2] = new Point(1, 0);
		points[3] = new Point(1, -1);
		points[4] = new Point(0, -1);
		points[5] = new Point(-1, -1);
		points[6] = new Point(-1, 0);
		points[7] = new Point(-1, 1);
		return points;
	}

	public static Point[] GetPointsInRing(int ringDistance){
		int length = 1;
		for(int distance = 1; distance <= ringDistance; distance++) 
			length += ((distance + 1) * 2) + ((distance - 1) * 2);
		Point[] points = new Point[length];
		int index = 0;
		points[0] = Point.zero;
		index++;
		for(int distance = 1; distance <= ringDistance; distance++) {
			for (int i = 0; i < distance + 1; i++) {
				points[index] = new Point(-distance + i, -i);
				index++;
				points[index] = new Point(distance - i, i);
				index++;
			}
			
			for (int i = 1; i < distance; i++) {
				points[index] = new Point(-i, distance - i);
				index++;
				points[index] = new Point(distance - i, -i);
				index++;
			}
		}
		return points;
	}
	public static Point[] GetPointsInRing(Point gridPoint, int ringDistance){
		Point[] points = GetPointsInRing(ringDistance);
		for(int i = 0; i < points.Length; i++) points[i] += gridPoint;
		return points;
	}

	public static Point[] GetPointsOnRing(int ringDistance) {
		Point[] points = new Point[((ringDistance + 1) * 2) + ((ringDistance - 1) * 2)];
		int index = 0;
		for (int i = 0; i < ringDistance + 1; i++) {
			points[index] = new Point(-ringDistance + i, -i);
			index++;
			points[index] = new Point(ringDistance - i, i);
			index++;
		}
		
		for (int i = 1; i < ringDistance; i++) {
			points[index] = new Point(-i, ringDistance - i);
			index++;
			points[index] = new Point(ringDistance - i, -i);
			index++;
		}
		return points;
	}

	public static Point[] GetPointsOnRing(Point gridPoint, int ringDistance){
		Point[] points = GetPointsOnRing(ringDistance);
		for(int i = 0; i < points.Length; i++) points[i] += gridPoint;
		return points;
	}



	/// <summary>
	/// Get the position of a corner vertex.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper left, others proceed counterclockwise.
	/// </remarks>
	/// <param name="index">Index of the desired corner. Cyclically constrained 0..3.</param>
	public Vector2 Corner(int index) {
		return CornerVector(index) + (Vector2)this;
	}

	public static Vector2 Corner(Point coord, int index) {
		return coord.Corner(index);
	}

	/// <summary>
	/// Enumerate this hex's six corners.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper left, others proceed counterclockwise.
	/// </remarks>
	/// <param name="first">Index of the first corner to enumerate.</param>
	public IEnumerable<Vector2> Corners() {
		Vector2 pos = (Vector2)this;
		foreach (Vector2 v in corners)
			yield return v + pos;
	}

	/// <summary>
	/// Unity position vector from hex center to a corner.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper left, others proceed counterclockwise.
	/// </remarks>
	/// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
	public static Vector2 CornerVector(int index) {
		return corners[NormalizeRotationIndex(index)];
	}

	/// <summary>
	/// Normalize a rotation index within 0 <= index < cycle.
	/// </summary>
	public static int NormalizeRotationIndex(int index, int cycle = numCorners) {
		if (index < 0 ^ cycle < 0)
			return (index % cycle + cycle) % cycle;
		else
			return index % cycle;
	}

	/// <summary>
	/// Determine the equality of two rotation indices for a given cycle.
	/// </summary>
	public static bool IsSameRotationIndex(int a, int b, int cycle = numCorners) {
		return 0 == NormalizeRotationIndex(a - b, cycle);
	}

	// Given a coord and corner index, find the corner index of another coord that shares the same vert.
	public static int GetTouchingCornerPointIndex (Point coord, int coordCornerIndex, Point otherCoord) {
		float SqrDistance (Vector2 a, Vector2 b) {
			return (a.x-b.x) * (a.x-b.x) + (a.y-b.y) * (a.y-b.y);
		}
		if(Math.Abs(coord.x-otherCoord.x) != 1 && Math.Abs(coord.y-otherCoord.y) != 1) return -1;
		var cornerPoint = coord.Corner(coordCornerIndex);
		for(int otherCornerIndex = 0; otherCornerIndex < numCorners; otherCornerIndex++) {
			var otherCornerPoint = otherCoord.Corner(otherCornerIndex);
			if(SqrDistance(cornerPoint, otherCornerPoint) < 0.1f) {
				return otherCornerIndex;
			}
		}
		return -1;
	}


	const int numCorners = 4;
	static Vector2[] corners = {
		// Top left
		new Vector2(-0.5f, 0.5f),
		// Top right
		new Vector2(0.5f, 0.5f),
		// Bottom right
		new Vector2(0.5f, -0.5f),
		// Bottom left
		new Vector2(-0.5f, -0.5f)
	};
}