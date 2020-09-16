using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

[System.Serializable]
public class TypeMap3D<T> : Grid3D, IEnumerable<TypeMap3DCellInfo<T>> {

	[System.NonSerialized]
	public T[] values;
	public float valuesLengthReciprocal {
		get {
			return 1f/values.Length;
		}
	}
	
	public TypeMap3D (Point3 _size) : base (_size) {
		Clear();
		values = new T[size.area];
	}
	
	public TypeMap3D (Point3 _size, T _value) : this (_size) {
		Fill(_value);
	}
	
	public TypeMap3D (Point3 _size, T[] _mapArray) : this (_size) {
		Fill(_mapArray);
	}
	
	public TypeMap3D (TypeMap3D<T> _map) : base (_map.size) {
		values = new T[_map.values.Length];
		System.Array.Copy(_map.values, values, _map.values.Length);
	}
	
	public virtual void Clear() {
		values = new T[size.area];
	}
	
	/// <summary>
	/// Calculates additional properties from the map. 
	/// For example, a map might store the largest value, or the average value. 
	/// These values are expensive to calculate all the time or via a getter, so this function can be called when needed by the user.
	/// </summary>
	public virtual void CalculateMapProperties() {}
	
	/// <summary>
	/// Fill the map with a value.
	/// </summary>
	/// <param name="_value">_value.</param>
	public virtual void Fill(T _value) {
		for(int i = 0; i < values.Length; i++) {
			values[i] = _value;
		}
	}
	
	/// <summary>
	/// Fill the map with values.
	/// </summary>
	/// <param name="_mapArray">_map array.</param>
	public virtual void Fill(T[] _mapArray) {
		values = _mapArray;
	}
	
	/// <summary>
	/// Gets the value at normalized position.
	/// </summary>
	/// <returns>The value at normalized position.</returns>
	/// <param name="position">Position.</param>
	public T GetValueAtNormalizedPosition(Vector3 position){
		return GetValueAtNormalizedPosition(position.XZ());
	}
	/*
	/// <summary>
	/// Gets the value at normalized position.
	/// </summary>
	/// <returns>The value at normalized position.</returns>
	/// <param name="position">Position.</param>
	public T GetValueAtNormalizedPosition(Vector2 position){
		return GetValueAtGridPosition(NormalizedPositionToGridPosition(position));
	}
	
	/// <summary>
	/// Gets the value at the specified grid position.
	/// Interpolates between the 4 points that this position falls between based on distance from the points.
	/// Note that this is significantly slower than obtaining values via a grid point, or directly from an array index.
	/// </summary>
	/// <returns>The value at grid position.</returns>
	/// <param name="gridPosition">Grid position.</param>

	public T GetValueAtGridPosition(Vector3 gridPosition){
		gridPosition = ClampGridPosition(gridPosition);
		if(gridPosition.x.IsWhole() && gridPosition.y.IsWhole()) {
			return GetValueAtGridPoint((int)gridPosition.x, (int)gridPosition.y);
		}

		int left = Mathf.FloorToInt(gridPosition.x);
		int right = left+1;
		int bottom = Mathf.FloorToInt(gridPosition.y);
		int top = bottom+1;
		
		Vector2 offset = new Vector2(gridPosition.x - Mathf.Floor(gridPosition.x), gridPosition.y - Mathf.Floor(gridPosition.y), gridPosition.z - Mathf.Floor(gridPosition.z));
		
		left = Mathf.Clamp(left, 0, sizeMinusOne.x);
		right = Mathf.Clamp(right, 0, sizeMinusOne.x);
		bottom = Mathf.Clamp(bottom, 0, sizeMinusOne.y);
		top = Mathf.Clamp(top, 0, sizeMinusOne.y);

		T topLeftValue = GetValueAtGridPoint(left, top);
		T topRightValue = GetValueAtGridPoint(right, top);
		T bottomLeftValue = GetValueAtGridPoint(left, bottom);
		T bottomRightValue = GetValueAtGridPoint(right, bottom);

		T x1 = Lerp(bottomLeftValue, bottomRightValue, offset.x);
		T x2 = Lerp(topLeftValue, topRightValue, offset.x);
		
		T xValue = Lerp(x1, x2, offset.y);
		return xValue;
	}
	*/
	
	/// <summary>
	/// Gets the value at grid point.
	/// </summary>
	/// <returns>The value at grid point.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public T GetValueAtGridPoint(int x, int y, int z) {
		return values[GridPointToArrayIndex(x, y, z)];
	}
	
	/// <summary>
	/// Gets the value at grid point.
	/// </summary>
	/// <returns>The value at grid point.</returns>
	/// <param name="gridPosition">Grid position.</param>
	public T GetValueAtGridPoint(Point3 gridPoint) {
		return GetValueAtGridPoint(gridPoint.x, gridPoint.y, gridPoint.z);
	}

	/// <summary>
	/// Gets multiple values from an array of grid points.
	/// </summary>
	/// <returns>The value at grid point.</returns>
	/// <param name="gridPosition">Grid position.</param>
	public T[] GetValuesAtGridPoints(IList<Point3> gridPoints) {
		T[] values = new T[gridPoints.Count];
		for(int i = 0; i < gridPoints.Count; i++) {
			values[i] = GetValueAtGridPoint(gridPoints[i]);
		}
		return values;
	}

	/// <summary>
	/// Sets the value at grid point.
	/// </summary>
	/// <returns>The value at grid point.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="val">Value.</param>
	public T SetValueAtGridPoint(int x, int y, int z, T val) {
		return values[GridPointToArrayIndex(x, y, z)] = val;
	}
	
	/// <summary>
	/// Sets the value at grid point.
	/// </summary>
	/// <returns>The value at grid point.</returns>
	/// <param name="gridPosition">Grid position.</param>
	/// <param name="val">Value.</param>
	public T SetValueAtGridPoint(Point3 gridPosition, T val){
		return SetValueAtGridPoint(gridPosition.x, gridPosition.y, gridPosition.z, val);
	}
	
	public void SetValueAtGridPoints(IList<Point3> gridPoints, T val){
		foreach(Point3 gridPoint in gridPoints)
			SetValueAtGridPoint(gridPoint.x, gridPoint.y, gridPoint.z, val);
	}
	
	public List<Point3> GetGridPointsContainingValue(T val){
		List<Point3> points = new List<Point3>();
		for(int i = 0; i < cellCount; i++)
			if(values[i].Equals(val))
				points.Add(ArrayIndexToGridPoint(i));
		return points;
	}

	/// <summary>
	/// Resize the grid to specified size, optionally offsetting the existing contents simultaniously in order to control the expansion pivot.
	/// Operates silently, avoiding OnChangeGridPoint calls from entities.
	/// For example, Resize(size + Point.one * 2, Point.one * 2) resizes from the top right, whereas Resize(size + Point.one, Point.zero) resizes from the bottom right.
	/// </summary>
	/// <param name="size">Size.</param>
	/// <param name="offset">Offset.</param>
	public virtual void Resize (Point3 size, Point3 offset) {
		DebugX.LogList(values);
		Point3 lastSize = this.size;
		this.size = size;

		T[] cachedValues = new T[values.Length];
		System.Array.Copy(values, cachedValues, values.Length);
		values = new T[size.area];
		for(int i = 0; i < cachedValues.Length; i++) {
			Point3 gridPoint = ArrayIndexToGridPoint(i, lastSize.x, lastSize.y);
			gridPoint += offset;
			if(IsOnGrid(gridPoint))
				SetValueAtGridPoint(gridPoint, cachedValues[i]);
		}

		RaiseResizeEvent(lastSize, size);
	}

	/// <summary>
	/// Offset the values.
	/// </summary>
	/// <param name="offset">Offset.</param>
	public virtual void Offset (Point3 offset) {
		T[] cachedValues = new T[values.Length];
		System.Array.Copy(values, cachedValues, values.Length);
		values = new T[size.area];
		for(int i = 0; i < cachedValues.Length; i++) {
			Point3 gridPoint = ArrayIndexToGridPoint(i);
			gridPoint += offset;
			if(IsOnGrid(gridPoint))
				SetValueAtGridPoint(gridPoint, cachedValues[i]);
		}
	}

//	public TypeMap<T> GetTrimmed (PointRect pointRect) {
//		TypeMap<T> newMap = new TypeMap<T>(new Point(pointRect.width, pointRect.height));
//		for(int i = 0; i < values.Length; i++) {
//			Point gridPoint = ArrayIndexToGridPoint(i);
//			Point relativeGridPoint = new Point(gridPoint.x - pointRect.x, gridPoint.y - pointRect.y);
//			if(newMap.IsOnGrid(relativeGridPoint)) {
//				int newMapIndex = newMap.GridPointToArrayIndex(relativeGridPoint);
//				newMap[newMapIndex] = values[i];
//			}
//		}
//		return newMap;
//	}

//	public TypeMap<T> GetTrimmed (Rect rect, Point3 resolution) {
//		PointRect expandedPointRect = new PointRect(Mathf.FloorToInt(rect.x), Mathf.FloorToInt(rect.y), Mathf.CeilToInt(rect.width), Mathf.CeilToInt(rect.height));
//		TypeMap<T> expandedMap = GetTrimmed(expandedPointRect);
//		TypeMap<T> heightMap = new TypeMap<T>(resolution);
//		foreach(var cellInfo in heightMap) {
//			expandedMap.GetValueAtGridPosition(cellInfo.point);
//		}
//		return expandedMap;
//	}
	
	protected virtual T Lerp (T a, T b, float l) {
		return default(T);
	}

	/// <summary>
	/// Gets the enumerator.
	/// </summary>
	/// <returns>The enumerator.</returns>
	IEnumerator<TypeMap3DCellInfo<T>> IEnumerable<TypeMap3DCellInfo<T>>.GetEnumerator() {
		TypeMap3DCellInfo<T> cellInfo = new TypeMap3DCellInfo<T>(0, Point3.zero, default(T));
		for (int z = 0; z < size.z; z++) {
			for (int y = 0; y < size.y; y++) {
				for (int x = 0; x < size.x; x++) {
					int index = GridPointToArrayIndex(x, y, z);
					cellInfo.Set(index, new Point3(x,y,z), values[index]);
					yield return cellInfo;
				}
		    }
		}
    }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>The enumerator.</returns>
    IEnumerator IEnumerable.GetEnumerator() {
		for (int z = 0; z < size.z; z++) {
			for (int y = 0; y < size.y; y++) {
				for (int x = 0; x < size.x; x++) {
					yield return null;
				}
		    }
		}
    }

	/// <summary>
	/// Array operator.
	/// </summary>
	/// TODO - Make map array protected, replace with this.
	public T this[int key] {
		get {
			return values[key];
		} set {
			values[key] = value;
		}
	}

	public override string ToString () {
		return string.Format ("[TypeMap: size={0}, values={1}]", size, values);
	}
}