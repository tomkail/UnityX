using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// Grid class.
/// Normalized space is in the range (0, 1) on the X, Y and Z axis.
/// Grid space is in the range (0, size), on the X and Y axis.
/// </summary>
[System.Serializable]
public class Grid {
	
	/// <summary>
	/// The size of the grid.
	/// </summary>
	public Point size;
	
	/// <summary>
	/// Gets the number of cells in the grid.
	/// </summary>
	/// <value>The length.</value>
	public int cellCount {
		get {
			return size.area;
		}
	}
	
	/// <summary>
	/// The size of the grid, minus one.
	/// Many functions calculate starting from zero rather than one, where this value is used instead of size.
	/// </summary>
	public Point sizeMinusOne {
		get {
			return new Point(size.x-1, size.y-1);
		}
	}
	
	/// <summary>
	/// The reciprocal of the size of the grid. 
	/// An optimization, used as a multiplier instead of division by the grid size by various functions.
	/// </summary>
	public Vector2 sizeReciprocal {
		get {
			return new Vector2(1f/size.x, 1f/size.y);
		}
	}
	
	/// <summary>
	/// The reciprocal of the size of the grid minus one.
	/// An optimization, used as a multiplier instead of division by the grid size minus one by various functions.
	/// </summary>
	public Vector2 sizeMinusOneReciprocal {
		get {
			return new Vector2(1f/sizeMinusOne.x, 1f/sizeMinusOne.y);
		}
	}


    public int longSide {
        get {
            if(size.x > size.y) return size.x;
            else return size.y;
        }
    }

    public int shortSide {
        get {
            if(size.x < size.y) return size.x;
            else return size.y;
        }
    }

	public delegate void OnResizeEvent(Point lastSize, Point newSize);
	public event OnResizeEvent OnResize;

	
	public Grid (Point _size) {
		SetSize(_size);
	}
	
	public virtual void SetSize(Point _size) {
		size = _size;
	}
	
	public int GridPointToArrayIndex (int x, int y){
		return Grid.GridPointToArrayIndex(x, y, size.x);
	}
	
	public int GridPointToArrayIndex (Point gridPoint){
		return GridPointToArrayIndex(gridPoint.x, gridPoint.y);
	}
	
	public int NormalizedPositionToArrayIndex (Vector2 position){
		return GridPointToArrayIndex(NormalizedToGridPoint(position));
	}
	
	public Point ArrayIndexToGridPoint (int arrayIndex){
		return new Point(arrayIndex%size.x, Mathf.FloorToInt((float)arrayIndex * sizeReciprocal.x));
	}

	public Vector2 ArrayIndexToNormalizedPosition (int arrayIndex){
		Point gridPoint = ArrayIndexToGridPoint(arrayIndex);
		return GridToNormalizedPosition(gridPoint);
	}
	
	public bool IsOnGrid(Point gridPoint){
		return IsOnGrid(gridPoint.x, gridPoint.y);
	}
	
	public bool IsOnGrid(int x, int y){
		return (x >= 0 && x < size.x && y >= 0 && y < size.y);
	}
    
	public bool IndexIsOnGrid(int index){
		return (index >= 0 && index < cellCount);
	}
	
	public static bool IsOnGrid(Point gridPoint, Point gridSize){
		return IsOnGrid(gridPoint.x, gridPoint.y, gridSize.x, gridSize.y);
	}

    public static bool IsOnGrid(int x, int y, int width, int height){
		return (x >= 0 && x < width && y >= 0 && y < height);
	}
    public static bool IndexIsOnGrid(int index, Point gridSize){
		return IndexIsOnGrid(index, gridSize.x, gridSize.y);
	}
    public static bool IndexIsOnGrid(int index, int width, int height){
		return IndexIsOnGrid(index, width * height);
	}
    public static bool IndexIsOnGrid(int index, int length){
		return (index >= 0 && index < length);
	}
	
	
	// RANDOM LOCATION
	
	public Point RandomGridPoint () {
		return Grid.RandomGridPoint(size);
	}
	
	public Vector2 RandomGridPosition () {
		return Grid.RandomGridPosition(size);
	}
	
	public Point GetRandomEdgeGridPoint () {
		Point gridPoint = new Point(0,0);
		
		int r = UnityEngine.Random.Range(0,4);
		
		if(r == 0) {
			gridPoint.x = UnityEngine.Random.Range(0, size.x);
		} else if(r == 1) {
			gridPoint.x = UnityEngine.Random.Range(0, size.x);
			gridPoint.y = size.y - 1;
		} else if(r == 2) {
			gridPoint.y = UnityEngine.Random.Range(0, size.y);
		} else if(r == 3) {
			gridPoint.y = UnityEngine.Random.Range(0, size.y);
			gridPoint.x = size.x - 1;
		}
		
		return gridPoint;
	}	
	
	//Conversion Functions
	
	public Vector2 GridToNormalizedPosition (Vector2 gridPosition){
		return Grid.GridToNormalizedPosition(gridPosition, size);
	}
	
	public Vector2 NormalizedToGridPosition (Vector2 normalizedPosition){
		return Grid.NormalizedToGridPosition(normalizedPosition, size);
	}
	
	public Point NormalizedToGridPoint(Vector2 normalizedPosition) {
		return Grid.NormalizedToGridPoint(normalizedPosition, size);
	}
	
	
	//Clamping Functions
	public Vector2 ClampGridPosition(Vector2 gridPosition){
		return ClampGridPosition(gridPosition, 0, sizeMinusOne.x, 0, sizeMinusOne.y);
	}
	
	public Vector2 ClampGridPosition(Vector2 gridPosition, int minX, int maxX, int minY, int maxY){
		float x = Mathf.Clamp(gridPosition.x, minX, maxX);
		float y = Mathf.Clamp(gridPosition.y, minY, maxY);
		if(gridPosition.x == x && gridPosition.y == y) return gridPosition;
		return new Vector2(x, y);
	}
	
	public void ClampGridPoint(ref int x, ref int y){
		ClampGridPoint(ref x, ref y, 0, sizeMinusOne.x, 0, sizeMinusOne.y);
	}
	
	public void ClampGridPoint(ref int x, ref int y, int minX, int maxX, int minY, int maxY){
		x = Mathf.Clamp(x, minX, maxX);
		y = Mathf.Clamp(y, minY, maxY);
	}
	
	public Point ClampGridPoint(Point gridPoint){
		return ClampGridPoint(gridPoint, 0, sizeMinusOne.x, 0, sizeMinusOne.y);
	}
	
	public Point ClampGridPoint(Point gridPoint, int minX, int maxX, int minY, int maxY){
		int x = Mathf.Clamp(gridPoint.x, minX, maxX);
		int y = Mathf.Clamp(gridPoint.y, minY, maxY);
		if(gridPoint.x == x && gridPoint.y == y) return gridPoint;
		return new Point(x, y);
	}
	
	
	public Vector2 RepeatNormalizedPosition(Vector2 normalizedPosition){
		return RepeatNormalizedPosition(normalizedPosition, 0, 1, 0, 1);
	}
	
	public Vector2 RepeatNormalizedPosition(Vector2 normalizedPosition, float minX, float maxX, float minY, float maxY){
		float x = MathX.RepeatInclusive(normalizedPosition.x, minX, maxX);
		float y = MathX.RepeatInclusive(normalizedPosition.y, minY, maxY);
		if(normalizedPosition.x == x && normalizedPosition.y == y) return normalizedPosition;
		return new Vector2(x, y);
	}
	
	public Vector2 RepeatGridPosition(Vector2 gridPosition){
		return RepeatGridPosition(gridPosition, 0, sizeMinusOne.x, 0, sizeMinusOne.y);
	}
	
	public Vector2 RepeatGridPosition(Vector2 gridPosition, float minX, float maxX, float minY, float maxY){
		float x = MathX.RepeatInclusive(gridPosition.x, minX, maxX);
		float y = MathX.RepeatInclusive(gridPosition.y, minY, maxY);
		if(gridPosition.x == x && gridPosition.y == y) return gridPosition;
		return new Vector2(x, y);
	}
	
	public Point RepeatGridPoint(Point gridPoint){
		return RepeatGridPoint(gridPoint, 0, sizeMinusOne.x, 0, sizeMinusOne.y);
	}
	
	public Point RepeatGridPoint(Point gridPoint, int minX, int maxX, int minY, int maxY){
		int x = MathX.RepeatInclusive(gridPoint.x, minX, maxX);
		int y = MathX.RepeatInclusive(gridPoint.y, minY, maxY);
		if(gridPoint.x == x && gridPoint.y == y) return gridPoint;
		return new Point(x, y);
	}
	



	public Point[] ValidCardinalDirections(Point gridPoint){
		return Grid.Filter(Grid.Filter(Point.CardinalDirections(gridPoint).ToList(), IsOnGrid)).ToArray();
	}

	public Point[] ValidOrdinalDirections(Point gridPoint){
		return Grid.Filter(Grid.Filter(Point.OrdinalDirections(gridPoint).ToList(), IsOnGrid)).ToArray();
	}

	public Point[] ValidCompassDirections(Point gridPoint){
		return Grid.Filter(Grid.Filter(Point.CompassDirections(gridPoint).ToList(), IsOnGrid)).ToArray();
	}
	
	
	
	
	
	// MAP FUNCTIONS
	public Point[] GetAllGridPoints () {
		Point[] gridPoints = new Point[size.area];
		for(int y = 0; y < size.y; y++)
			for(int x = 0; x < size.x; x++)
				gridPoints[GridPointToArrayIndex(x,y)] = new Point(x,y);
		return gridPoints;
	}

	/// <summary>
	/// Determines whether the point is on the edge of the grid;
	/// </summary>
	/// <returns><c>true</c> if this point is on the edge of the grid; otherwise, <c>false</c>.</returns>
	/// <param name="_point">_point.</param>
	public bool IsEdge (Point _point) {
		if(_point.x == 0 || _point.x == size.x-1 || _point.y == 0 || _point.y == size.y-1) return true;
		return false;
	}
	
	/// <summary>
	/// Determines whether the point is on a corner of the grid.
	/// </summary>
	/// <returns><c>true</c> if this point is on a corner of the grid; otherwise, <c>false</c>.</returns>
	/// <param name="_point">_point.</param>
	public bool IsCorner (Point _point) {
		if(_point.x == 0 && _point.y == 0) return true;
		if(_point.x == size.x-1 && _point.y == 0) return true;
		if(_point.x == 0 && _point.y == size.y-1) return true;
		if(_point.x == size.x-1 && _point.y == size.y-1) return true;
		return false;
	}
	
	/// <summary>
	/// Converts a grid point to an index of the array.
	/// </summary>
	/// <returns>The point to array index.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="width">Width.</param>
	public static int GridPointToArrayIndex (int x, int y, int width){
		return y * width + x;
	}
	
	/// <summary>
	/// Converts a grid point to an index of the array.
	/// </summary>
	/// <returns>The point to array index.</returns>
	/// <param name="gridPoint">Grid point.</param>
	/// <param name="width">Width.</param>
	public static int GridPointToArrayIndex (Point gridPoint, int width){
		return GridPointToArrayIndex(gridPoint.x, gridPoint.y, width);
	}
	
	public static Point ArrayIndexToGridPoint (int arrayIndex, int width){
		return new Point(arrayIndex%width, Mathf.FloorToInt((float)arrayIndex/width));
	}

	public static Vector2 GridToNormalizedPosition (Vector2 gridPosition, Point gridSize){
		return new Vector2(gridPosition.x / (gridSize.x - 1), gridPosition.y / (gridSize.y - 1));
	}
	
	public static Vector2 NormalizedToGridPosition (Vector2 normalizedPosition, Point gridSize){
		return new Vector2(normalizedPosition.x * (gridSize.x-1), normalizedPosition.y * (gridSize.y-1));
	}
	
	public static Point NormalizedToGridPoint (Vector2 normalizedPosition, Point gridSize){
		return new Point(Mathf.RoundToInt(normalizedPosition.x * (gridSize.x - 1)) , Mathf.RoundToInt(normalizedPosition.y * (gridSize.y - 1)) );
	}
	
	public static Point RandomGridPoint (Point gridSize) {
		return new Point (UnityEngine.Random.Range(0, gridSize.x), UnityEngine.Random.Range(0, gridSize.y));
	}
	
	public static Vector2 RandomGridPosition (Point gridSize) {
		return new Vector2 (UnityEngine.Random.Range(0f, gridSize.x), UnityEngine.Random.Range(0f, gridSize.y));
	}
	
	public static Vector2 RandomNormalizedPosition () {
		return new Vector2 (UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1));
	}


	/// <summary>
	/// Removes the invalid points in the list as defined by function parameters.
	/// Example usage: List<Point> validAdjacent = Grid.Filter(GetAdjacentPoints(new Point(0,3), IsOnGrid);
	/// </summary>
	/// <returns>The invalid.</returns>
	/// <param name="allPoints">All points.</param>
	public static List<Point> Filter(IList<Point> allPoints, params Func<Point, bool>[] filters){
		List<Point> validPoints = new List<Point>();
		foreach(Point gridPoint in allPoints) {
			bool valid = true;
			foreach(Func<Point, bool> filterFunction in filters) {
				if(!filterFunction(gridPoint)) {
					valid = false;
					break;
				}
			}
			if(valid) validPoints.Add(gridPoint);
		}
		return validPoints;
	}


	public virtual void Resize (Point size) {
		Point lastSize = this.size;
		this.size = size;
		RaiseResizeEvent(lastSize, size);
	}

	protected virtual void RaiseResizeEvent (Point lastSize, Point size) {
		if(OnResize != null)
			OnResize(lastSize, size);
	}


	/// <summary>
	/// Gets the enumerator.
	/// </summary>
	/// <returns>The enumerator.</returns>
	public IEnumerable<Point> Points() {
		for (int y = 0; y < size.y; y++) {
			for (int x = 0; x < size.x; x++) {
				yield return new Point(x,y);
		    }
		}
    }





	public struct GridIntersection {
		public int x;
		public int y;
		public Rect normalizedCellRect;
		public Rect normalizedIntersectingRect;
		public GridIntersection (int x, int y, Rect normalizedCellRect, Rect normalizedIntersectingRect) {
			this.x = x;
			this.y = y;
			this.normalizedCellRect = normalizedCellRect;
			this.normalizedIntersectingRect = normalizedIntersectingRect;
		}
	}



	public virtual IEnumerable<GridIntersection> GetRectGridIntersections (Rect normalizedRect) {
		Rect intersectingRect = Rect.zero;
		var cellViewportSize = new Vector2(1f/size.x, 1f/size.y);

		var pointRect = GetPointRectFromNormalizedRect(normalizedRect);
		int pointRectXMin = pointRect.xMin;
		int pointRectXMax = pointRect.xMax;
		int pointRectYMin = pointRect.yMin;
		int pointRectYMax = pointRect.yMax;
		
		float normalizedRectXMin = normalizedRect.xMin;
		float normalizedRectXMax = normalizedRect.xMax;
		float normalizedRectYMin = normalizedRect.yMin;
		float normalizedRectYMax = normalizedRect.yMax;
		
		float gridCellRectXMin;
		float gridCellRectXMax;
		float gridCellRectYMin;
		float gridCellRectYMax;
		
		gridCellRectYMin = pointRectYMin*cellViewportSize.y;
		gridCellRectYMax = gridCellRectYMin + cellViewportSize.y;
		for(int y = pointRectYMin; y < pointRectYMax; y++) {
			gridCellRectXMin = pointRectXMin*cellViewportSize.x;
			gridCellRectXMax = gridCellRectXMin + cellViewportSize.x;
			for(int x = pointRectXMin; x < pointRectXMax; x++) {
				if(IsOnGrid(x,y)) {
					if(RectX.Intersect(
						normalizedRectXMin, normalizedRectXMax, normalizedRectYMin, normalizedRectYMax, 
						gridCellRectXMin, gridCellRectXMax, gridCellRectYMin, gridCellRectYMax, 
						ref intersectingRect)
					) {
                        var normalizedCellRect = Rect.MinMaxRect(gridCellRectXMin, gridCellRectYMin, gridCellRectXMax, gridCellRectYMax);
						yield return new GridIntersection(x,y, normalizedCellRect, intersectingRect);
					}
				}
				gridCellRectXMin += cellViewportSize.x;
				gridCellRectXMax += cellViewportSize.x;
			}
			gridCellRectYMin += cellViewportSize.y;
			gridCellRectYMax += cellViewportSize.y;
		}
	}


	public PointRect GetPointRectFromNormalizedRect (Rect prospectiveRect) {
		return PointRect.MinMaxRect(
			new Point(Mathf.FloorToInt(prospectiveRect.xMin * size.x), Mathf.FloorToInt(prospectiveRect.yMin * size.y)),
			new Point(Mathf.CeilToInt(prospectiveRect.xMax * size.x), Mathf.CeilToInt(prospectiveRect.yMax * size.y))
		);
	}
	public Rect GetNormalizedRectFromPointRect (PointRect pointRect) {
		return RectX.MinMaxRect(
			new Vector2(pointRect.xMin * sizeReciprocal.x, pointRect.yMin * sizeReciprocal.y),
			new Vector2(pointRect.xMax * sizeReciprocal.x, pointRect.yMax * sizeReciprocal.y)
		);
	}
}