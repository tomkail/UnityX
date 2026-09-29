using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// SquareGrid class.
/// Normalized space is in the range (0, 1) on the X and Y axes.
/// SquareGrid space is in the range (0, size), on the X and Y axis.
/// </summary>
[System.Serializable]
public class SquareGrid {
	
	/// <summary>
	/// The size of the grid.
	/// </summary>
	public Vector2Int size;
	
	/// <summary>
	/// Gets the number of cells in the grid.
	/// </summary>
	/// <value>The length.</value>
	public int cellCount {
		get {
			return (size.x * size.y);
		}
	}
	
	/// <summary>
	/// The size of the grid, minus one.
	/// Many functions calculate starting from zero rather than one, where this value is used instead of size.
	/// </summary>
	public Vector2Int sizeMinusOne {
		get {
			return new Vector2Int(size.x-1, size.y-1);
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

	public delegate void OnResizeEvent(Vector2Int lastSize, Vector2Int newSize);
	public event OnResizeEvent OnResize;

	
	public SquareGrid (Vector2Int _size) {
		SetSize(_size);
	}
	
	public virtual void SetSize(Vector2Int _size) {
		size = _size;
	}
	
	public int GridPointToArrayIndex (int x, int y){
		return GridPointToArrayIndex(x, y, size.x);
	}
	
	public int GridPointToArrayIndex (Vector2Int gridPoint){
		return GridPointToArrayIndex(gridPoint.x, gridPoint.y);
	}
	
	public int NormalizedPositionToArrayIndex (Vector2 position){
		return GridPointToArrayIndex(NormalizedToGridPoint(position));
	}
	
	public Vector2Int ArrayIndexToGridPoint (int arrayIndex){
		return new Vector2Int(arrayIndex%size.x, arrayIndex/size.x);
	}

	public Vector2 ArrayIndexToNormalizedPosition (int arrayIndex){
		Vector2Int gridPoint = ArrayIndexToGridPoint(arrayIndex);
		return GridToNormalizedPosition(gridPoint);
	}
	
	public bool IsOnGrid(Vector2Int gridPoint){
		return IsOnGrid(gridPoint.x, gridPoint.y);
	}
	
	public bool IsOnGrid(int x, int y){
		return (x >= 0 && x < size.x && y >= 0 && y < size.y);
	}
    
	public bool IndexIsOnGrid(int index){
		return (index >= 0 && index < cellCount);
	}
	
	public static bool IsOnGrid(Vector2Int gridPoint, Vector2Int gridSize){
		return IsOnGrid(gridPoint.x, gridPoint.y, gridSize.x, gridSize.y);
	}

    public static bool IsOnGrid(int x, int y, int width, int height){
		return (x >= 0 && x < width && y >= 0 && y < height);
	}
    public static bool IndexIsOnGrid(int index, Vector2Int gridSize){
		return IndexIsOnGrid(index, gridSize.x, gridSize.y);
	}
    public static bool IndexIsOnGrid(int index, int width, int height){
		return IndexIsOnGrid(index, width * height);
	}
    public static bool IndexIsOnGrid(int index, int length){
		return (index >= 0 && index < length);
	}
	
	
	// RANDOM LOCATION
	
	public Vector2Int RandomGridPoint () {
		return RandomGridPoint(size);
	}
	
	public Vector2 RandomGridPosition () {
		return RandomGridPosition(size);
	}
	
	public Vector2Int GetRandomEdgeGridPoint () {
		Vector2Int gridPoint = new Vector2Int(0,0);
		
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
		return GridToNormalizedPosition(gridPosition, size);
	}
	
	public Vector2 NormalizedToGridPosition (Vector2 normalizedPosition){
		return NormalizedToGridPosition(normalizedPosition, size);
	}
	
	public Vector2Int NormalizedToGridPoint(Vector2 normalizedPosition) {
		return NormalizedToGridPoint(normalizedPosition, size);
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
	
	public Vector2Int ClampGridPoint(Vector2Int gridPoint){
		return ClampGridPoint(gridPoint, 0, sizeMinusOne.x, 0, sizeMinusOne.y);
	}
	
	public Vector2Int ClampGridPoint(Vector2Int gridPoint, int minX, int maxX, int minY, int maxY){
		int x = Mathf.Clamp(gridPoint.x, minX, maxX);
		int y = Mathf.Clamp(gridPoint.y, minY, maxY);
		if(gridPoint.x == x && gridPoint.y == y) return gridPoint;
		return new Vector2Int(x, y);
	}
	
	
	public Vector2 RepeatNormalizedPosition(Vector2 normalizedPosition){
		return RepeatNormalizedPosition(normalizedPosition, 0, 1, 0, 1);
	}
	
	public Vector2 RepeatNormalizedPosition(Vector2 normalizedPosition, float minX, float maxX, float minY, float maxY){
		float x = RepeatInclusive(normalizedPosition.x, minX, maxX);
		float y = RepeatInclusive(normalizedPosition.y, minY, maxY);
		if(normalizedPosition.x == x && normalizedPosition.y == y) return normalizedPosition;
		return new Vector2(x, y);
	}
	
	public Vector2 RepeatGridPosition(Vector2 gridPosition){
		return RepeatGridPosition(gridPosition, 0, sizeMinusOne.x, 0, sizeMinusOne.y);
	}
	
	public Vector2 RepeatGridPosition(Vector2 gridPosition, float minX, float maxX, float minY, float maxY){
		float x = RepeatInclusive(gridPosition.x, minX, maxX);
		float y = RepeatInclusive(gridPosition.y, minY, maxY);
		if(gridPosition.x == x && gridPosition.y == y) return gridPosition;
		return new Vector2(x, y);
	}
	
	public Vector2Int RepeatGridPoint(Vector2Int gridPoint){
		return RepeatGridPoint(gridPoint, 0, sizeMinusOne.x, 0, sizeMinusOne.y);
	}
	
	public Vector2Int RepeatGridPoint(Vector2Int gridPoint, int minX, int maxX, int minY, int maxY){
		int x = RepeatInclusive(gridPoint.x, minX, maxX);
		int y = RepeatInclusive(gridPoint.y, minY, maxY);
		if(gridPoint.x == x && gridPoint.y == y) return gridPoint;
		return new Vector2Int(x, y);
	}
	



	public Vector2Int[] ValidCardinalDirections(Vector2Int gridPoint){
		return Filter(CardinalDirections(gridPoint).ToList(), IsOnGrid).ToArray();
	}

	public Vector2Int[] ValidOrdinalDirections(Vector2Int gridPoint){
		return Filter(OrdinalDirections(gridPoint).ToList(), IsOnGrid).ToArray();
	}

	public Vector2Int[] ValidCompassDirections(Vector2Int gridPoint){
		return Filter(CompassDirections(gridPoint).ToList(), IsOnGrid).ToArray();
	}
	
	
	
	
	
	// MAP FUNCTIONS
	public Vector2Int[] GetAllGridPoints () {
		Vector2Int[] gridPoints = new Vector2Int[(size.x * size.y)];
		for(int y = 0; y < size.y; y++)
			for(int x = 0; x < size.x; x++)
				gridPoints[GridPointToArrayIndex(x,y)] = new Vector2Int(x,y);
		return gridPoints;
	}

	/// <summary>
	/// Determines whether the point is on the edge of the grid;
	/// </summary>
	/// <returns><c>true</c> if this point is on the edge of the grid; otherwise, <c>false</c>.</returns>
	/// <param name="_point">_point.</param>
	public bool IsEdge (Vector2Int _point) {
		if(_point.x == 0 || _point.x == size.x-1 || _point.y == 0 || _point.y == size.y-1) return true;
		return false;
	}
	
	/// <summary>
	/// Determines whether the point is on a corner of the grid.
	/// </summary>
	/// <returns><c>true</c> if this point is on a corner of the grid; otherwise, <c>false</c>.</returns>
	/// <param name="_point">_point.</param>
	public bool IsCorner (Vector2Int _point) {
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
	/// <param name="gridPoint">SquareGrid point.</param>
	/// <param name="width">Width.</param>
	public static int GridPointToArrayIndex (Vector2Int gridPoint, int width){
		return GridPointToArrayIndex(gridPoint.x, gridPoint.y, width);
	}
	
	public static Vector2Int ArrayIndexToGridPoint (int arrayIndex, int width){
		return new Vector2Int(arrayIndex%width, arrayIndex/width);
	}

	public static Vector2 GridToNormalizedPosition (Vector2 gridPosition, Vector2Int gridSize){
		return new Vector2(gridPosition.x / (gridSize.x - 1), gridPosition.y / (gridSize.y - 1));
	}
	
	public static Vector2 NormalizedToGridPosition (Vector2 normalizedPosition, Vector2Int gridSize){
		return new Vector2(normalizedPosition.x * (gridSize.x-1), normalizedPosition.y * (gridSize.y-1));
	}
	
	public static Vector2Int NormalizedToGridPoint (Vector2 normalizedPosition, Vector2Int gridSize){
		return new Vector2Int(Mathf.RoundToInt(normalizedPosition.x * (gridSize.x - 1)) , Mathf.RoundToInt(normalizedPosition.y * (gridSize.y - 1)) );
	}
	
	public static Vector2Int RandomGridPoint (Vector2Int gridSize) {
		return new Vector2Int (UnityEngine.Random.Range(0, gridSize.x), UnityEngine.Random.Range(0, gridSize.y));
	}
	
	public static Vector2 RandomGridPosition (Vector2Int gridSize) {
		return new Vector2 (UnityEngine.Random.Range(0f, gridSize.x), UnityEngine.Random.Range(0f, gridSize.y));
	}
	
	public static Vector2 RandomNormalizedPosition () {
		return new Vector2 (UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
	}


	/// <summary>
	/// Removes the invalid points in the list as defined by function parameters.
	/// Example usage: List<Vector2Int> validAdjacent = Filter(GetAdjacentPoints(new Vector2Int(0,3), IsOnGrid);
	/// </summary>
	/// <returns>The invalid.</returns>
	/// <param name="allPoints">All points.</param>
	public static List<Vector2Int> Filter(IList<Vector2Int> allPoints, params Func<Vector2Int, bool>[] filters){
		List<Vector2Int> validPoints = new List<Vector2Int>();
		foreach(Vector2Int gridPoint in allPoints) {
			bool valid = true;
			foreach(Func<Vector2Int, bool> filterFunction in filters) {
				if(!filterFunction(gridPoint)) {
					valid = false;
					break;
				}
			}
			if(valid) validPoints.Add(gridPoint);
		}
		return validPoints;
	}


	public virtual void Resize (Vector2Int size) {
		Vector2Int lastSize = this.size;
		this.size = size;
		RaiseResizeEvent(lastSize, size);
	}

	protected virtual void RaiseResizeEvent (Vector2Int lastSize, Vector2Int size) {
		if(OnResize != null)
			OnResize(lastSize, size);
	}


	/// <summary>
	/// Gets the enumerator.
	/// </summary>
	/// <returns>The enumerator.</returns>
	public IEnumerable<Vector2Int> Points() {
		for (int y = 0; y < size.y; y++) {
			for (int x = 0; x < size.x; x++) {
				yield return new Vector2Int(x,y);
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
					if(Intersect(
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


	public RectInt GetPointRectFromNormalizedRect (Rect prospectiveRect) {
		var min = new Vector2Int(Mathf.FloorToInt(prospectiveRect.xMin * size.x), Mathf.FloorToInt(prospectiveRect.yMin * size.y));
		var max = new Vector2Int(Mathf.CeilToInt(prospectiveRect.xMax * size.x), Mathf.CeilToInt(prospectiveRect.yMax * size.y));
		return new RectInt(min.x, min.y, max.x - min.x, max.y - min.y);
	}
	public Rect GetNormalizedRectFromPointRect (RectInt pointRect) {
		return MinMaxRect(
			new Vector2(pointRect.xMin * sizeReciprocal.x, pointRect.yMin * sizeReciprocal.y),
			new Vector2(pointRect.xMax * sizeReciprocal.x, pointRect.yMax * sizeReciprocal.y)
		);
	}

	// --- Helpers kept inline so SquareGrid is dependency-free ---
	static int RepeatInclusive (int val, int min, int max) {
		int range = max - min;
		if (range == 0) return min;
		if (val > max) val -= range * Mathf.CeilToInt((val - max) / range);
		else if (val < min) val += range * Mathf.CeilToInt((-val + min) / range);
		return val;
	}
	static float RepeatInclusive (float val, float min, float max) {
		float range = max - min;
		if (range == 0) return min;
		if (val > max) val -= range * Mathf.CeilToInt((val - max) / range);
		else if (val < min) val += range * Mathf.CeilToInt((-val + min) / range);
		return val;
	}
	static Rect MinMaxRect (Vector2 min, Vector2 max) => Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	static bool Intersect (float r1xMin, float r1xMax, float r1yMin, float r1yMax, float r2xMin, float r2xMax, float r2yMin, float r2yMax, ref Rect output) {
		if (r1xMax < r2xMin || r1xMin > r2xMax || r1yMax < r2yMin || r1yMin > r2yMax) return false;
		output = Rect.MinMaxRect(Mathf.Max(r1xMin, r2xMin), Mathf.Max(r1yMin, r2yMin), Mathf.Min(r1xMax, r2xMax), Mathf.Min(r1yMax, r2yMax));
		return true;
	}

	// Neighbour offsets.
	static readonly Vector2Int[] cardinalOffsets = { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
	static readonly Vector2Int[] ordinalOffsets = { new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 1) };
	static readonly Vector2Int[] compassOffsets = { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1) };
	static Vector2Int[] CardinalDirections (Vector2Int p) => Offsets(cardinalOffsets, p);
	static Vector2Int[] OrdinalDirections (Vector2Int p) => Offsets(ordinalOffsets, p);
	static Vector2Int[] CompassDirections (Vector2Int p) => Offsets(compassOffsets, p);
	static Vector2Int[] Offsets (Vector2Int[] offsets, Vector2Int p) {
		var result = new Vector2Int[offsets.Length];
		for (int i = 0; i < offsets.Length; i++) result[i] = offsets[i] + p;
		return result;
	}
}