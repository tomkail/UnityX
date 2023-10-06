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
		pointBounds = new PointRect((int)bounds.x, (int)bounds.y, (int)bounds.width, (int)bounds.height);
	}
}

public static class ShapeUtils {
	// create a random joined shape with X points. Think tetromino generator!
	public static Shape CreateContiguous (int numPoints) {
		Point[] points = new Point[numPoints];
		TypeMap<bool> shape = new TypeMap<bool>(new Point(numPoints, numPoints));
		int x = 1;
		int y = 1;
		bool valid = false;
		int rx;
		int ry;

		Point minPoint = new Point(numPoints, numPoints);

		points[0] = new Point(x,y);
	    shape.SetValueAtGridPoint(x,y,true);
		

	    for(var i = 1; i < numPoints; i++) {
	        do {
				rx = Random.Range(0,numPoints);
				ry = Random.Range(0,numPoints);
				valid = false;

				if(shape.GetValueAtGridPoint(new Point(rx,ry)) == false){
					if(shape.IsOnGrid(rx,ry-1) && shape.GetValueAtGridPoint(rx,ry-1)) valid = true;
					if(shape.IsOnGrid(rx,ry+1) && shape.GetValueAtGridPoint(rx,ry+1)) valid = true;
					if(shape.IsOnGrid(rx-1,ry) && shape.GetValueAtGridPoint(rx-1,ry)) valid = true;
					if(shape.IsOnGrid(rx+1,ry) && shape.GetValueAtGridPoint(rx+1,ry)) valid = true;
	            }
	            

	        } while(!valid);

			x = rx;
	        y = ry;

			points[i] = new Point(x,y);
			shape.SetValueAtGridPoint(points[i],true);


	        

	    }
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