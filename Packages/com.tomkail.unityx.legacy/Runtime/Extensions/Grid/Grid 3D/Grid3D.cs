using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityX.Geometry;

[System.Serializable]
public class Grid3D {

	/// <summary>
	/// The size of the grid.
	/// </summary>
	public Vector3Int size;
	
	/// <summary>
	/// Gets the number of cells in the grid.
	/// </summary>
	/// <value>The length.</value>
	public int cellCount {
		get {
			return size.Area();
		}
	}
	
	/// <summary>
	/// The size of the grid, minus one.
	/// Many functions calculate starting from zero rather than one, where this value is used instead of size.
	/// </summary>
	public Vector3Int sizeMinusOne {
		get {
			return new Vector3Int(size.x-1, size.y-1, size.z-1);
		}
	}
	
	/// <summary>
	/// The reciprocal of the size of the grid. 
	/// An optimization, used as a multiplier instead of division by the grid size by various functions.
	/// </summary>
	public Vector3 sizeReciprocal {
		get {
			return new Vector3(1f/size.x, 1f/size.y, 1f/size.z);
		}
	}
	
	/// <summary>
	/// The reciprocal of the size of the grid minus one.
	/// An optimization, used as a multiplier instead of division by the grid size minus one by various functions.
	/// </summary>
	public Vector3 sizeMinusOneReciprocal {
		get {
			return new Vector3(1f/sizeMinusOne.x, 1f/sizeMinusOne.y, 1f/sizeMinusOne.z);
		}
	}

	public delegate void OnResizeEvent(Vector3Int lastSize, Vector3Int newSize);
	public event OnResizeEvent OnResize;

	
	public Grid3D (Vector3Int _size) {
		SetSize(_size);
	}
	
	public virtual void SetSize(Vector3Int _size) {
		size = _size;
	}


	public int GridPointToArrayIndex (int x, int y, int z){
		return Grid3D.GridPointToArrayIndex(x, y, z, size.y, size.z);
	}
	
	public int GridPointToArrayIndex (Vector3Int gridPoint){
		return GridPointToArrayIndex(gridPoint.x, gridPoint.y, gridPoint.z);
	}
	
	public int PositionToArrayIndex (Vector3 position){
		return GridPointToArrayIndex(NormalizedPositionToGridPoint(position));
	}
	
	public Vector3Int ArrayIndexToGridPoint (int arrayIndex){
		return Grid3D.ArrayIndexToGridPoint(arrayIndex, size.y, size.z);
	}

	public Vector3 ArrayIndexToNormalizedPosition (int arrayIndex){
		Vector3Int gridPoint = ArrayIndexToGridPoint(arrayIndex);
		return GridPositionToNormalizedPosition(gridPoint);
	}
	
	public bool IsOnGrid(Vector3Int gridPoint){
		return IsOnGrid(gridPoint.x, gridPoint.y, gridPoint.z);
	}
	
	public bool IsOnGrid(int x, int y, int z){
		return (x >= 0 && x < size.x && y >= 0 && y < size.y && z >= 0 && z < size.z);
	}

	public bool IsOnGrid(int index){
		return (index >= 0 && index < cellCount);
	}
	
	
	// RANDOM LOCATION
	
	public Vector3Int RandomGridPoint () {
		return Grid3D.RandomGridPoint(size);
	}
	
	public Vector3 RandomGridPosition () {
		return Grid3D.RandomGridPosition(size);
	}


	//Conversion Functions
	
	public Vector3 GridPositionToNormalizedPosition (Vector3 gridPosition){
		return Grid3D.GridPositionToNormalizedPosition(gridPosition, size);
	}
	
	public Vector3 NormalizedPositionToGridPosition (Vector3 normalizedPosition){
		return Grid3D.NormalizedPositionToGridPosition(normalizedPosition, size);
	}
	
	public Vector3Int NormalizedPositionToGridPoint(Vector3 normalizedPosition) {
		return Grid3D.NormalizedPositionToGridPoint(normalizedPosition, size);
	}
	
	
	//Clamping Functions

	public Vector3 ClampGridPosition(Vector3 gridPosition){
		return ClampGridPosition(gridPosition, 0, sizeMinusOne.x, 0, sizeMinusOne.y, 0, sizeMinusOne.z);
	}
	
	public Vector3 ClampGridPosition(Vector3 gridPosition, int minX, int maxX, int minY, int maxY, int minZ, int maxZ){
		float x = Mathf.Clamp(gridPosition.x, minX, maxX);
		float y = Mathf.Clamp(gridPosition.y, minY, maxY);
		float z = Mathf.Clamp(gridPosition.z, minZ, maxZ);
		if(gridPosition.x == x && gridPosition.y == y && gridPosition.z == z) return gridPosition;
		return new Vector3(x, y, z);
	}

	public void ClampGridPoint(ref int x, ref int y, ref int z){
		ClampGridPoint(ref x, ref y, ref z, 0, sizeMinusOne.x, 0, sizeMinusOne.y, 0, sizeMinusOne.z);
	}
	
	public void ClampGridPoint(ref int x, ref int y, ref int z, int minX, int maxX, int minY, int maxY, int minZ, int maxZ){
		x = Mathf.Clamp(x, minX, maxX);
		y = Mathf.Clamp(y, minY, maxY);
		z = Mathf.Clamp(z, minZ, maxZ);
	}
	
	public Vector3Int ClampGridPoint(Vector3Int gridPoint){
		return ClampGridPoint(gridPoint, 0, sizeMinusOne.x, 0, sizeMinusOne.y, 0, sizeMinusOne.z);
	}
	
	public Vector3Int ClampGridPoint(Vector3Int gridPoint, int minX, int maxX, int minY, int maxY, int minZ, int maxZ){
		int x = Mathf.Clamp(gridPoint.x, minX, maxX);
		int y = Mathf.Clamp(gridPoint.y, minY, maxY);
		int z = Mathf.Clamp(gridPoint.z, minZ, maxZ);
		if(gridPoint.x == x && gridPoint.y == y && gridPoint.z == z) return gridPoint;
		return new Vector3Int(x, y, z);
	}
	
	
	public Vector3 RepeatNormalizedPosition(Vector3 normalizedPosition){
		return RepeatNormalizedPosition(normalizedPosition, 0, 1, 0, 1, 0, 1);
	}
	
	public Vector3 RepeatNormalizedPosition(Vector3 normalizedPosition, float minX, float maxX, float minY, float maxY, float minZ, float maxZ){
		float x = MathX.RepeatInclusive(normalizedPosition.x, minX, maxX);
		float y = MathX.RepeatInclusive(normalizedPosition.y, minY, maxY);
		float z = MathX.RepeatInclusive(normalizedPosition.z, minZ, maxZ);
		if(normalizedPosition.x == x && normalizedPosition.y == y && normalizedPosition.z == z) return normalizedPosition;
		return new Vector3(x, y, z);
	}
	
	public Vector3 RepeatGridPosition(Vector3 gridPosition){
		return RepeatGridPosition(gridPosition, 0, sizeMinusOne.x, 0, sizeMinusOne.y, 0, sizeMinusOne.z);
	}
	
	public Vector3 RepeatGridPosition(Vector3 gridPosition, float minX, float maxX, float minY, float maxY, float minZ, float maxZ){
		float x = MathX.RepeatInclusive(gridPosition.x, minX, maxX);
		float y = MathX.RepeatInclusive(gridPosition.y, minY, maxY);
		float z = MathX.RepeatInclusive(gridPosition.z, minZ, maxZ);
		if(gridPosition.x == x && gridPosition.y == y && gridPosition.z == z) return gridPosition;
		return new Vector3(x, y, z);
	}
	
	public Vector3Int RepeatGridPoint(Vector3Int gridPoint){
		return RepeatGridPoint(gridPoint, 0, sizeMinusOne.x, 0, sizeMinusOne.y, 0, sizeMinusOne.z);
	}
	
	public Vector3Int RepeatGridPoint(Vector3Int gridPoint, int minX, int maxX, int minY, int maxY, int minZ, int maxZ){
		int x = MathX.RepeatInclusive(gridPoint.x, minX, maxX);
		int y = MathX.RepeatInclusive(gridPoint.y, minY, maxY);
		int z = MathX.RepeatInclusive(gridPoint.z, minZ, maxZ);
		if(gridPoint.x == x && gridPoint.y == y && gridPoint.z == z) return gridPoint;
		return new Vector3Int(x, y, z);
	}

	public Vector3Int[] ValidAdjacentDirections(Vector3Int gridPoint){
		return Grid3D.Filter(Grid3D.AdjacentDirections(gridPoint).ToList(), IsOnGrid).ToArray();
	}

	/// <summary>
	/// Converts a grid point to an index of the array.
	/// </summary>
	/// <returns>The point to array index.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="width">Width.</param>
	public static int GridPointToArrayIndex (int x, int y, int z, int height, int depth){
		return (x * height * depth) + (y * depth) + z;
	}
	
	/// <summary>
	/// Converts a grid point to an index of the array.
	/// </summary>
	/// <returns>The point to array index.</returns>
	/// <param name="gridPoint">Grid point.</param>
	/// <param name="width">Width.</param>
	public static int GridPointToArrayIndex (Vector3Int gridPoint, int height, int depth){
		return GridPointToArrayIndex(gridPoint.x, gridPoint.y, gridPoint.z, height, depth);
	}
	
	public static Vector3Int ArrayIndexToGridPoint (int arrayIndex, int height, int depth){
		int x = arrayIndex / (height * depth);
		arrayIndex -= (x * height * depth);
		int y = arrayIndex / depth;
		int z = arrayIndex % depth;
    	return new Vector3Int(x, y, z);
	}

	public static Vector3 GridPositionToNormalizedPosition (Vector3 gridPosition, Vector3Int gridSize){
		return new Vector3(gridPosition.x / (gridSize.x - 1), gridPosition.y / (gridSize.y - 1), gridPosition.z / (gridSize.z - 1));
	}
	
	public static Vector3 NormalizedPositionToGridPosition (Vector3 normalizedPosition, Vector3Int gridSize){
		return new Vector3(normalizedPosition.x * (gridSize.x-1), normalizedPosition.y * (gridSize.y-1), normalizedPosition.z * (gridSize.z-1));
	}
	
	public static Vector3Int NormalizedPositionToGridPoint(Vector3 normalizedPosition, Vector3Int gridSize){
		return new Vector3Int(MathX.RoundToInt(normalizedPosition.x * (gridSize.x - 1)), MathX.RoundToInt(normalizedPosition.y * (gridSize.y - 1)), MathX.RoundToInt(normalizedPosition.z * (gridSize.z - 1)));
	}
	
	public static Vector3Int RandomGridPoint (Vector3Int gridSize) {
		return new Vector3Int (UnityEngine.Random.Range(0, gridSize.x), UnityEngine.Random.Range(0, gridSize.y), UnityEngine.Random.Range(0, gridSize.z));
	}
	
	public static Vector3 RandomGridPosition (Vector3Int gridSize) {
		return new Vector3 (UnityEngine.Random.Range(0f, gridSize.x), UnityEngine.Random.Range(0f, gridSize.y), UnityEngine.Random.Range(0f, gridSize.z));
	}
	
	public static Vector3 RandomNormalizedPosition () {
		return new Vector3 (UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
	}

	public static Vector3Int[] AdjacentDirections(){
		Vector3Int[] points = new Vector3Int[6];
		points[0] = new Vector3Int(0, 1, 0);
		points[1] = new Vector3Int(1, 0, 0);
		points[2] = new Vector3Int(0, -1, 0);
		points[3] = new Vector3Int(-1, 0, 0);
		points[4] = new Vector3Int(0, 0, 1);
		points[5] = new Vector3Int(0, 0, -1);
		return points;
	}

	public static Vector3Int[] AdjacentDirections(Vector3Int gridPoint){
		Vector3Int[] points = AdjacentDirections();
		for(int i = 0; i < points.Length; i++) points[i] += gridPoint;
		return points;
	}

	/// <summary>
	/// Removes the invalid points in the list as defined by function parameters.
	/// Example usage: List<Vector2Int> validAdjacent = SquareGrid.Filter(GetAdjacentPoints(new Vector2Int(0,3), IsOnGrid);
	/// </summary>
	/// <returns>The invalid.</returns>
	/// <param name="allPoints">All points.</param>
	public static List<Vector3Int> Filter(IList<Vector3Int> allPoints, params Func<Vector3Int, bool>[] filters){
		List<Vector3Int> validPoints = new List<Vector3Int>();
		foreach(Vector3Int gridPoint in allPoints) {
			bool valid = true;
			foreach(Func<Vector3Int, bool> filterFunction in filters) {
				if(!filterFunction(gridPoint)) {
					valid = false;
					break;
				}
			}
			if(valid) validPoints.Add(gridPoint);
		}
		return validPoints;
	}


	public virtual void Resize (Vector3Int size) {
		Vector3Int lastSize = this.size;
		this.size = size;
		RaiseResizeEvent(lastSize, size);
	}

	protected virtual void RaiseResizeEvent (Vector3Int lastSize, Vector3Int size) {
		if(OnResize != null)
			OnResize(lastSize, size);
	}
}
