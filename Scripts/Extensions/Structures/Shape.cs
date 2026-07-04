using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// A collection of points forming a shape. May or may not be contiguous.
[System.Serializable]
public class Shape {
	public List<Point> points;
	public Vector2 center;
	public Rect bounds;
	public PointRect pointBounds;

	public Shape () {
		points = new List<Point>();
	}

	public Shape (IEnumerable<Point> points) {
		this.points = new List<Point>(points);
		OnChangePoints();
	}

	public IEnumerable<Point> GetTranslatedPoints(Point offset) {
		return points.Select(x => x + offset);
	}

	public void OnChangePoints () {
		Vector2[] pointsAsVectors = new Vector2[points.Count];
		for(int i = 0; i < points.Count; i++)
			pointsAsVectors[i] = (Vector2)points[i];
		bounds = RectX.CreateEncapsulating(pointsAsVectors);
		center = bounds.center;
		// Build the integer bounds by flooring the min corner and ceiling the max corner.
		// Flooring/ceiling (rather than casting, which truncates toward zero) keeps negative
		// origins on the correct side, and clamping each extent to >= 1 guarantees a single
		// point (or a degenerate line) still spans at least one cell instead of collapsing to
		// a zero-size rect.
		int minX = Mathf.FloorToInt(bounds.xMin);
		int minY = Mathf.FloorToInt(bounds.yMin);
		int maxX = Mathf.CeilToInt(bounds.xMax);
		int maxY = Mathf.CeilToInt(bounds.yMax);
		pointBounds = new PointRect(minX, minY, Mathf.Max(maxX - minX, 1), Mathf.Max(maxY - minY, 1));
	}
}

public static class ShapeUtils {
	// create a random joined shape with X points. Think tetromino generator!
	public static Shape CreateContiguous (int numPoints) {
		// Guard degenerate inputs: nothing to build for < 1 point.
		if(numPoints < 1) return new Shape();

		Point[] points = new Point[numPoints];
		TypeMap<bool> shapeMap = new TypeMap<bool>(new Point(numPoints, numPoints));
		// Seed near a corner. For numPoints >= 2 this is (1,1) as before; for numPoints == 1
		// the grid is only 1x1, so clamp the seed to (0,0) to stay on the grid.
		int x = Mathf.Min(1, numPoints - 1);
		int y = Mathf.Min(1, numPoints - 1);
		bool valid = false;
		int rx;
		int ry;

		Point minPoint = new Point(numPoints, numPoints);

		points[0] = new Point(x,y);
		shapeMap.SetValueAtGridPoint(x,y,true);

		// Cap the search for an adjacent free cell so a bad/small numPoints (e.g. a grid too
		// cramped to fit the requested count) can't spin forever. On hitting the cap we bail
		// gracefully and keep the cells placed so far.
		int maxAttempts = Mathf.Max(1000, numPoints * numPoints * 4);
		int placedCount = 1;

		for(var i = 1; i < numPoints; i++) {
			int attempts = 0;
			do {
				rx = Random.Range(0,numPoints);
				ry = Random.Range(0,numPoints);
				valid = false;

				if(shapeMap.GetValueAtGridPoint(new Point(rx,ry)) == false){
					if(shapeMap.IsOnGrid(rx,ry-1) && shapeMap.GetValueAtGridPoint(rx,ry-1)) valid = true;
					if(shapeMap.IsOnGrid(rx,ry+1) && shapeMap.GetValueAtGridPoint(rx,ry+1)) valid = true;
					if(shapeMap.IsOnGrid(rx-1,ry) && shapeMap.GetValueAtGridPoint(rx-1,ry)) valid = true;
					if(shapeMap.IsOnGrid(rx+1,ry) && shapeMap.GetValueAtGridPoint(rx+1,ry)) valid = true;
				}


			} while(!valid && ++attempts < maxAttempts);

			// Couldn't find an adjacent free cell within the cap: stop growing the shape.
			if(!valid) break;

			x = rx;
			y = ry;

			points[i] = new Point(x,y);
			shapeMap.SetValueAtGridPoint(points[i],true);
			placedCount++;



		}

		// If we bailed early, trim off the unused (default) slots so we don't emit stray points.
		if(placedCount < points.Length)
			System.Array.Resize(ref points, placedCount);

		for(int i = 0; i < points.Length; i++) {
			minPoint.x = Mathf.Min(minPoint.x, points[i].x);
			minPoint.y = Mathf.Min(minPoint.y, points[i].y);
		}

		for(int i = 0; i < points.Length; i++) {
			points[i] -= minPoint;
		}
		return new Shape(points);
	}
}
