using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Line {
	public Vector2 start;
	public Vector2 end;
	
	public Vector2 direction {
		get { return vector.normalized; }
	}

	public Vector2 vector {
		get { return end - start; }
	}

	public float length {
		get { return Vector2.Distance(start, end); }
	}

	public float sqrLength {
		get { return (start.x-end.x) * (start.x-end.x) + (start.y-end.y) * (start.y-end.y); }
	}
	
	public float left {
		get {
			if(start.x < end.x) return start.x;
			else return end.x;
		}
	}
	public float right {
		get {
			if(start.x > end.x) return start.x;
			else return end.x;
		}
	}
	public float top {
		get {
			if(start.y > end.y) return start.y;
			else return end.y;
		}
	}
	public float bottom {
		get {
			if(start.y < end.y) return start.y;
			else return end.y;
		}
	}

	public Line (Vector2 _start, Vector2 _end) {
		start = _start;
		end = _end;
	}

	public Line (Vector2[] _startEnd) {
		start = _startEnd[0];
		end = _startEnd[1];
	}

	public void Set (Vector2 _start, Vector2 _end) {
		start = _start;
		end = _end;
	}

	public Vector2 AtDistance (float distance) {
		return start + direction * distance;
	}

	public override string ToString() {
		return "Start: " + start + " End: " + end;
	}

	public static Line Add(Line left, Line right){
		return new Line(left.start+right.start, left.end+right.end);
	}


	public static Line Subtract(Line left, Line right){
		return new Line(left.start-right.start, left.end-right.end);
	}

	public override bool Equals(System.Object obj) {
		// If parameter is null return false.
		if (obj == null) {
			return false;
		}

		// If parameter cannot be cast to Line return false.
		Line l = (Line)obj;
		if ((System.Object)l == null) {
			return false;
		}

		// Return true if the fields match:
		return Equals(l);
	}

	public bool Equals(Line l) {
		// If parameter is null return false:
		if ((object)l == null) {
			return false;
		}

		// Return true if the fields match:
		return start == l.start && end == l.end;
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * 31 + start.GetHashCode();
			hash = hash * 31 + end.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (Line left, Line right) {
		if (System.Object.ReferenceEquals(left, right))
		{
			return true;
		}

		// If one is null, but not both, return false.
		if (((object)left == null) || ((object)right == null))
		{
			return false;
		}
		
		return left.Equals(right);
	}

	public static bool operator != (Line left, Line right) {
		return !(left == right);
	}

	public static Line operator +(Line left, Line right) {
		return Add(left, right);
	}

	public static Line operator -(Line left, Line right) {
		return Subtract(left, right);
	}

	public static bool IntersectionCheck(Line line1, Line line2) {
		return IntersectionCheck(line1.start, line1.end, line2.start, line2.end);
	}

	// From https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect (Tricks of the Windows Game Programming Gurus)
	public static bool IntersectionCheck(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End) {
		var l1_dx = line1End.x - line1Start.x;     
		var l1_dy = line1End.y - line1Start.y;
		var l2_dx = line2End.x - line2Start.x;
		var l2_dy = line2End.y - line2Start.y;
		var ls_dx = line1Start.x - line2Start.x;
		var ls_dy = line1Start.y - line2Start.y;

		float d = -l2_dx * l1_dy + l1_dx * l2_dy;
		if(d != 0) {
			float s = (-l1_dy * ls_dx + l1_dx * ls_dy) / d;
			if (s >= 0 && s <= 1) {
				float t = (l2_dx * ls_dy - l2_dy * ls_dx) / d;
				if(t >= 0 && t <= 1) {
					return true;
				}
			}
		}
		return false;
	}

	public static bool LineIntersectionPoint(Line line1, Line line2, out Vector2 intersectionPoint, bool clampStart = true, bool clampEnd = true) {
		// if(line1.right < line2.left || line1.left > line2.right || line1.top < line2.bottom || line1.bottom > line2.top) return false;
		var l1_dx = line1.end.x - line1.start.x;     
		var l1_dy = line1.end.y - line1.start.y;
		var l2_dx = line2.end.x - line2.start.x;
		var l2_dy = line2.end.y - line2.start.y;
		var ls_dx = line1.start.x - line2.start.x;
		var ls_dy = line1.start.y - line2.start.y;

		float s = (-l1_dy * ls_dx + l1_dx * ls_dy) / (-l2_dx * l1_dy + l1_dx * l2_dy);
		if ((!clampStart || s >= 0) && (!clampEnd || s <= 1)) {
			float t = (l2_dx * ls_dy - l2_dy * ls_dx) / (-l2_dx * l1_dy + l1_dx * l2_dy);
			if((!clampStart || t >= 0) && (!clampEnd || t <= 1)) {
				// Collision detected
				intersectionPoint.x = line1.start.x + (t * l1_dx);
				intersectionPoint.y = line1.start.y + (t * l1_dy);
				return true;
			}
		}
		intersectionPoint = Vector2.zero;
		return false;
	}
	

	public bool RayLineIntersect(Vector2 rayOrigin, Vector2 rayDirection, out float distance, bool clamped = true) {
		Vector2 seg = vector;
		Vector2 segPerp = new Vector2(seg.y, -seg.x);
		float perpDotd = Vector2.Dot(rayDirection, segPerp);

		// If lines are parallel, return false.
		if (Mathf.Abs(perpDotd) <= float.Epsilon) {
			distance = float.MaxValue;
			return false;
		}
	
		Vector2 d = start - rayOrigin;
	
		distance = Vector2.Dot(segPerp, d) / perpDotd;
		float s = Vector2.Dot(new Vector2(rayDirection.y, -rayDirection.x), d) / perpDotd;
	
		// If intersect is in right direction and in segment bounds, return true.
		return distance >= 0.0f && (!clamped || s >= 0.0f && s <= 1.0f);  
	}


	public float GetClosestDistanceFromLine(Vector2 p) {
//			// Return minimum distance between line segment vw and point p
		return Vector2.Distance(p, GetClosestPointOnLine(p));
	}

	public Vector2 GetClosestPointOnLine(Vector2 p, bool clamped = true) {
		// Consider the line extending the segment, parameterized as v + t (w - v).
		// We find projection of point p onto the line. 
		// It falls where t = [(p-v) . (w-v)] / |w-v|^2
		float t = GetNormalizedDistanceOnLine(p, clamped);
		// Projection falls on the segment
		return Vector2.LerpUnclamped(start, end, t);
	}

	public float GetNormalizedDistanceOnLine(Vector2 p, bool clamped = true) {
		return GetNormalizedDistanceOnLineInternal(start, end, p, sqrLength, clamped);
	}



	public static float GetClosestDistanceFromLine(Vector2 start, Vector2 end, Vector2 p, bool clamped = true) {
		return Vector2.Distance(p, GetClosestPointOnLine(start, end, p, clamped));
	}

	public static Vector2 GetClosestPointOnLine(Vector2 start, Vector2 end, Vector2 p, bool clamped = true) {
		float t = GetNormalizedDistanceOnLine(start, end, p, clamped);
		return Vector2.LerpUnclamped(start, end, t);
	}

	public static float GetNormalizedDistanceOnLine(Vector2 start, Vector2 end, Vector2 p, bool clamped = true) {
		float sqrLength = (start.x-end.x) * (start.x-end.x) + (start.y-end.y) * (start.y-end.y);
		return GetNormalizedDistanceOnLineInternal(start, end, p, sqrLength, clamped);
	}

	// Consider the line extending the segment, parameterized as v + t (w - v).
		// We find projection of point p onto the line. 
		// It falls where t = [(p-v) . (w-v)] / |w-v|^2
//		    float t = (Vector2.Distance(p - start) * Vector2.Distance(end, start)) / length;
	static float GetNormalizedDistanceOnLineInternal(Vector2 start, Vector2 end, Vector2 p, float sqrLength, bool clamped = true) {
		if (sqrLength == 0f) return 0;
		// Divide by length squared so that we can save on normalising (end-start), since
		// we're effectively dividing by the length an extra time.
		float n = Vector2.Dot(p - start, end - start) / sqrLength;
		if(!clamped) return n;
		return Mathf.Clamp01(n);
	}








	

	// public static List<Vector2Int> PointsOnLine(float x0, float y0, float x1, float y1) {
		
	// 	int cx = Mathf.FloorToInt(x0); // Begin/current cell coords
	// 	int cy = Mathf.FloorToInt(y0);
	// 	int ex = Mathf.FloorToInt(x1); // End cell coords
	// 	int ey = Mathf.FloorToInt(y1);

	// 	// Delta or direction
	// 	float dx = x1-x0;
	// 	float dy = y1-y0;

	// 	while (cx < ex && cy < ey)
	// 	{
	// 	// find intersection "time" in x dir
	// 	float t0 = (Mathf.Ceil(x0)-x0)/dx;
	// 	float t1 = (Mathf.Ceil(y0)-y0)/dy;

	// 	visit_cell(cx, cy);

	// 	if (t0 < t1) // cross x boundary first=?
	// 	{
	// 		++cx;
	// 		x0 += t0*dx;
	// 		y0 += t0*dy;
	// 	}
	// 	else
	// 	{
	// 		++cy;
	// 		x0 += t1*dx;
	// 		y0 += t1*dy;
	// 	}
	// 	}
	// }

	//https://stackoverflow.com/questions/27410280/fast-voxel-traversal-algorithm-with-negative-direction
	public static IEnumerable<Vector2Int> GetCrossedCells(Vector2 pPoint1, Vector2 pPoint2) {
		if (pPoint1 != pPoint2) {
			Vector2 V = (pPoint2 - pPoint1) / 1f; // direction & distance vector
			Vector2 U = V.normalized; // direction unit vector
			Vector2Int S = new Vector2Int((int)Mathf.Sign(U.x), (int)Mathf.Sign(U.y)); // sign vector
			Vector2 P = pPoint1 / 1f; // position in grid coord system
			Vector2Int G = new Vector2Int((int) Mathf.Floor(P.x), (int) Mathf.Floor(P.y)); // grid coord
			Vector2 T = new Vector2(Mathf.Abs(1f / U.x), Mathf.Abs(1f / U.y));
			Vector2 D = new Vector2(
				S.x > 0 ? 1 - P.x % 1 : S.x < 0 ? P.x % 1 : 0,
				S.y > 0 ? 1 - P.y % 1 : S.y < 0 ? P.y % 1 : 0);
			Vector2 M = new Vector2(
				Mathf.Infinity == T.x || S.x == 0 ? Mathf.Infinity : T.x * D.x,
				Mathf.Infinity == T.y || S.y == 0 ? Mathf.Infinity : T.y * D.y);

			bool isCanMoveByX = S.x != 0;
			bool isCanMoveByY = S.y != 0;

			while (isCanMoveByX || isCanMoveByY)
			{
				yield return G;

				D = new Vector2(
					S.x > 0 ? (float) Mathf.Floor(P.x) + 1 - P.x :
					S.x < 0 ? (float) Mathf.Ceil(P.x) - 1 - P.x :
					0,
					S.y > 0 ? (float) Mathf.Floor(P.y) + 1 - P.y :
					S.y < 0 ? (float) Mathf.Ceil(P.y) - 1 - P.y :
					0);

				if (Mathf.Abs(V.x) <= Mathf.Abs(D.x))
				{
					D.x = V.x;
					isCanMoveByX = false;
				}

				if (Mathf.Abs(V.y) <= Mathf.Abs(D.y))
				{
					D.y = V.y;
					isCanMoveByY = false;
				}

				if (M.x <= M.y)
				{
					M.x += T.x;
					G.x += S.x;
					if (isCanMoveByY)
					{
						D.y = U.y / U.x * D.x; // U.x / U.y = D.x / D.y => U.x * D.y = U.y * D.x
					}
				}
				else
				{
					M.y += T.y;
					G.y += S.y;
					if (isCanMoveByX)
					{
						D.x = U.x / U.y * D.y;
					}
				}

				V -= D;
				P += D;
			}
		}
	}
	
// 		function getHelpers(cellSize, pos, dir)
// 			local tile = mathf.floor(pos / cellSize) + 1

// 			local dTile, dt
// 			if dir > 0 then
// 				dTile = 1
// 				dt = ((tile+0)*cellSize - pos) / dir
// 			else
// 				dTile = -1
// 				dt = ((tile-1)*cellSize - pos) / dir
// 			end

// 			return tile, dTile, dt, dTile * cellSize / dir
// 		end


// 		public static List<Vector2Int> Traverse(int x, int y, int dx, int dy) {
// 			List<Vector2Int> points = new List<Vector2Int>();
// 		// function castRay_clearer_alldirs_improved_transformed(grid, ray)
// 			// local x, dx, dtX, ddtX = getHelpers(grid.cellSize, ray.startX, ray.dirX)
// 			// local y, dy, dtY, ddtY = getHelpers(grid.cellSize, ray.startY, ray.dirY)
// 			float t = 0;

// 			// if (ray.dirX*ray.dirX + ray.dirY*ray.dirY > 0) {
// 				while (x > 0 && x <= 64 && y > 0 && y <= 64) {
// 					points.Add(new Vector2Int(x, y));
// 					// mark(ray.startX + ray.dirX * t, ray.startY + ray.dirY * t)

// 					if (dtX < dtY) {
// 						x = x + dx
// 						local dt = dtX
// 						t = t + dt
// 						dtX = dtX + ddtX - dt
// 						dtY = dtY - dt
// 					}
// 					else {
// 						y = y + dy
// 						local dt = dtY
// 						t = t + dt
// 						dtX = dtX - dt
// 						dtY = dtY + ddtY - dt
// 					}
// 				}
// 			// } //then -- start and end should not be at the same point
// 			else {
// 				points.Add(new Vector2Int(x, y));
// 			}
// 		}

// 		public static List<Vector2Int> Traverse(int x, int y, int dx, int dy) {
// 			int z;
// 			int dz;

// 			z = 0;

// 			dz = 0;

// 			int n, sx, sy, sz, ax, ay, az, bx, by, bz;
// 			int exy, exz, ezy;

// 			sx = (int)Mathf.Sign(dx);
// 			sy = (int)Mathf.Sign(dy);
// 			sz = (int)Mathf.Sign(dz);

// 			ax = Mathf.Abs(dx);
// 			ay = Mathf.Abs(dy);
// 			az = Mathf.Abs(dz);

// 			bx = 2 * ax;
// 			by = 2 * ay;
// 			bz = 2 * az;

// 			exy = ay - ax;
// 			exz = az - ax;
// 			ezy = ay - az;

// 			List<Vector2Int> points = new List<Vector2Int>();
// 			n = ax + ay + az;
// DebugX.Log(n +" "+x+" "+y+" "+z);
// 			while (
// 				n-- >= 0 &&
// 				0 <= x && x < 63 && 
// 				0 <= y && y < 63 && 
// 				0 <= z && z < 63
// 			)
// 			{
// 				points.Add(new Vector2Int(x,y));
// 				DebugX.LogList(points);
// 				if (exy < 0)
// 				{
// 					if (exz < 0)
// 					{
// 						x += sx;
// 						exy += by; exz += bz;
// 					}
// 					else
// 					{
// 						z += sz;
// 						exz -= bx; ezy += by;
// 					}
// 				}
// 				else
// 				{
// 					if (ezy < 0)
// 					{
// 						z += sz;
// 						exz -= bx; ezy += by;
// 					}
// 					else
// 					{
// 						y += sy;
// 						exy -= bx; ezy -= bz;
// 					}
// 				}
// 			}
// 			return points;
// 		}

	// public static List<Vector2Int> DrawLineNoDiagonalSteps(int x0, int y0, int x1, int y1) {
		// while(true) {
		// 	if(tMaxX < tMaxY) {
		// 		tMaxX= tMaxX + tDeltaX;
		// 		X= X + stepX;
		// 	} else {
		// 		tMaxY= tMaxY + tDeltaY;
		// 		Y= Y + stepY;
		// 	}
		// 	// NextVoxel(X,Y);
		// }
		// List<Vector2Int> points = new List<Vector2Int>();
		// int dx =  Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
		// int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
		// int err = dx + dy, e2;

		// for (;;) {
		// 	points.Add(new Vector2Int(x0, y0));

		// 	if (x0 == x1 && y0 == y1) break;

		// 	e2 = 2 * err;

		// 	// EITHER horizontal OR vertical step (but not both!)
		// 	if (e2 > dy) { 
		// 		err += dy;
		// 		x0 += sx;
		// 	} else if (e2 < dx) { // <--- this "else" makes the difference
		// 		err += dx;
		// 		y0 += sy;
		// 	}
		// }
		// return points;
	// }
	public static List<Vector2Int> PointsOnLine(int x0, int y0, int x1, int y1) {
		List<Vector2Int> points = new List<Vector2Int>();
		int dx = Mathf.Abs(x1 - x0);
		int dy = Mathf.Abs(y1 - y0);
		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;
		int err = dx - dy;

		while (true) {
			points.Add(new Vector2Int(x0, y0));

			if (x0==x1 && y0==y1)
				return points;

			int e2 = err * 2;
			if (e2 > -dx) {
				err -= dy;
				x0 += sx;
			}
			if (e2 < dx){
				err += dx;
				y0 += sy;
			}
		}
	}

	

	//Bresenham's Line Algorithm
	//A way of drawing a line segment onto a square grid.
	//http://www.roguebasin.com/index.php?title=Bresenham%27s_Line_Algorithm

	private static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

	/// <summary>
	/// The plot function delegate
	/// </summary>
	/// <param name="x">The x co-ord being plotted</param>
	/// <param name="y">The y co-ord being plotted</param>
	/// <returns>True to continue, false to stop the algorithm</returns>
	public delegate bool PlotFunction(int x, int y);

	/// <summary>
	/// Plot the line from (x0, y0) to (x1, y10
	/// </summary>
	/// <param name="x0">The start x</param>
	/// <param name="y0">The start y</param>
	/// <param name="x1">The end x</param>
	/// <param name="y1">The end y</param>
	/// <param name="plot">The plotting function (if this returns false, the algorithm stops early)</param>
	public static void Plot(int x0, int y0, int x1, int y1, PlotFunction plot)
	{
		bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
		if (steep) { Swap<int>(ref x0, ref y0); Swap<int>(ref x1, ref y1); }
		if (x0 > x1) { Swap<int>(ref x0, ref x1); Swap<int>(ref y0, ref y1); }
		int dX = (x1 - x0), dY = Mathf.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;

		for (int x = x0; x <= x1; ++x)
		{
			if (!(steep ? plot(y, x) : plot(x, y))) return;
			err = err - dY;
			if (err < 0) { y += ystep;  err += dX; }
		}
	}
}