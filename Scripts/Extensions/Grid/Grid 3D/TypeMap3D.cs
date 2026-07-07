using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

[System.Serializable]
public class TypeMap3D<T> : Grid3D, IEnumerable<TypeMap3DCellInfo<T>> {

	public T[] values;
	public float valuesLengthReciprocal {
		get {
			return 1f/values.Length;
		}
	}
	
	public TypeMap3D (Vector3Int _size) : base (_size) {
		Clear();
	}
	
	public TypeMap3D (Vector3Int _size, T _value) : this (_size) {
		Fill(_value);
	}
	
	public TypeMap3D (Vector3Int _size, T[] _mapArray) : this (_size) {
		Fill(_mapArray);
	}
	
	public TypeMap3D (TypeMap3D<T> _map) : base (_map.size) {
		values = new T[_map.values.Length];
		System.Array.Copy(_map.values, values, _map.values.Length);
	}
	
	public virtual void Clear() {
		values = new T[size.Area()];
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
	/// Gets the value at a normalized (0-1) position, trilinearly interpolated across the volume.
	/// </summary>
	/// <returns>The interpolated value at the normalized position.</returns>
	/// <param name="position">Normalized (0-1) position.</param>
	public T GetValueAtNormalizedPosition(Vector3 position){
		return GetValueAtGridPosition(NormalizedPositionToGridPosition(position));
	}

	/// <summary>
	/// Gets the value at the specified (possibly fractional) grid position, trilinearly interpolating
	/// between the eight grid points surrounding it. Whole positions short-circuit to a direct lookup.
	/// Significantly slower than sampling a grid point or array index directly.
	/// </summary>
	/// <returns>The interpolated value at the grid position.</returns>
	/// <param name="gridPosition">Grid position.</param>
	public T GetValueAtGridPosition(Vector3 gridPosition){
		gridPosition = ClampGridPosition(gridPosition);
		if(gridPosition.x.IsWhole() && gridPosition.y.IsWhole() && gridPosition.z.IsWhole())
			return GetValueAtGridPoint((int)gridPosition.x, (int)gridPosition.y, (int)gridPosition.z);

		// The eight grid points surrounding this position (low/high corner on each axis), clamped to the grid.
		int x0 = Mathf.Clamp(Mathf.FloorToInt(gridPosition.x), 0, sizeMinusOne.x);
		int y0 = Mathf.Clamp(Mathf.FloorToInt(gridPosition.y), 0, sizeMinusOne.y);
		int z0 = Mathf.Clamp(Mathf.FloorToInt(gridPosition.z), 0, sizeMinusOne.z);
		int x1 = Mathf.Clamp(x0 + 1, 0, sizeMinusOne.x);
		int y1 = Mathf.Clamp(y0 + 1, 0, sizeMinusOne.y);
		int z1 = Mathf.Clamp(z0 + 1, 0, sizeMinusOne.z);

		// Fractional distance into the cell on each axis.
		float tx = gridPosition.x - Mathf.Floor(gridPosition.x);
		float ty = gridPosition.y - Mathf.Floor(gridPosition.y);
		float tz = gridPosition.z - Mathf.Floor(gridPosition.z);

		// Trilinear: lerp along x on the four edges, then along y within each z-plane, then along z.
		T x00 = Lerp(GetValueAtGridPoint(x0, y0, z0), GetValueAtGridPoint(x1, y0, z0), tx);
		T x10 = Lerp(GetValueAtGridPoint(x0, y1, z0), GetValueAtGridPoint(x1, y1, z0), tx);
		T x01 = Lerp(GetValueAtGridPoint(x0, y0, z1), GetValueAtGridPoint(x1, y0, z1), tx);
		T x11 = Lerp(GetValueAtGridPoint(x0, y1, z1), GetValueAtGridPoint(x1, y1, z1), tx);

		T z0Plane = Lerp(x00, x10, ty);
		T z1Plane = Lerp(x01, x11, ty);

		return Lerp(z0Plane, z1Plane, tz);
	}
	
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
	public T GetValueAtGridPoint(Vector3Int gridPoint) {
		return GetValueAtGridPoint(gridPoint.x, gridPoint.y, gridPoint.z);
	}

	/// <summary>
	/// Gets multiple values from an array of grid points.
	/// </summary>
	/// <returns>The value at grid point.</returns>
	/// <param name="gridPosition">Grid position.</param>
	public T[] GetValuesAtGridPoints(IList<Vector3Int> gridPoints) {
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
	public T SetValueAtGridPoint(Vector3Int gridPosition, T val){
		return SetValueAtGridPoint(gridPosition.x, gridPosition.y, gridPosition.z, val);
	}
	
	public void SetValueAtGridPoints(IList<Vector3Int> gridPoints, T val){
		foreach(Vector3Int gridPoint in gridPoints)
			SetValueAtGridPoint(gridPoint.x, gridPoint.y, gridPoint.z, val);
	}
	
	public List<Vector3Int> GetGridPointsContainingValue(T val){
		List<Vector3Int> points = new List<Vector3Int>();
		for(int i = 0; i < cellCount; i++)
			if(values[i].Equals(val))
				points.Add(ArrayIndexToGridPoint(i));
		return points;
	}

	/// <summary>
	/// Resize the grid to specified size, optionally offsetting the existing contents simultaneously in order to control the expansion pivot.
	/// Operates silently (does not raise OnChangeGridPoint callbacks).
	/// For example, Resize(size + Vector2Int.one * 2, Vector2Int.one * 2) resizes from the top right, whereas Resize(size + Vector2Int.one, Vector2Int.zero) resizes from the bottom right.
	/// </summary>
	/// <param name="size">Size.</param>
	/// <param name="offset">Offset.</param>
	public virtual void Resize (Vector3Int size, Vector3Int offset) {
		Vector3Int lastSize = this.size;
		this.size = size;

		T[] cachedValues = new T[values.Length];
		System.Array.Copy(values, cachedValues, values.Length);
		values = new T[size.Area()];
		for(int i = 0; i < cachedValues.Length; i++) {
			Vector3Int gridPoint = ArrayIndexToGridPoint(i, lastSize.y, lastSize.z);
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
	public virtual void Offset (Vector3Int offset) {
		T[] cachedValues = new T[values.Length];
		System.Array.Copy(values, cachedValues, values.Length);
		values = new T[size.Area()];
		for(int i = 0; i < cachedValues.Length; i++) {
			Vector3Int gridPoint = ArrayIndexToGridPoint(i);
			gridPoint += offset;
			if(IsOnGrid(gridPoint))
				SetValueAtGridPoint(gridPoint, cachedValues[i]);
		}
	}

//	public TypeMap<T> GetTrimmed (RectInt pointRect) {
//		TypeMap<T> newMap = new TypeMap<T>(new Vector2Int(pointRect.width, pointRect.height));
//		for(int i = 0; i < values.Length; i++) {
//			Vector2Int gridPoint = ArrayIndexToGridPoint(i);
//			Vector2Int relativeGridPoint = new Vector2Int(gridPoint.x - pointRect.x, gridPoint.y - pointRect.y);
//			if(newMap.IsOnGrid(relativeGridPoint)) {
//				int newMapIndex = newMap.GridPointToArrayIndex(relativeGridPoint);
//				newMap[newMapIndex] = values[i];
//			}
//		}
//		return newMap;
//	}

//	public TypeMap<T> GetTrimmed (Rect rect, Vector3Int resolution) {
//		RectInt expandedPointRect = new RectInt(Mathf.FloorToInt(rect.x), Mathf.FloorToInt(rect.y), Mathf.CeilToInt(rect.width), Mathf.CeilToInt(rect.height));
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
		TypeMap3DCellInfo<T> cellInfo = new TypeMap3DCellInfo<T>(0, Vector3Int.zero, default(T));
		for (int z = 0; z < size.z; z++) {
			for (int y = 0; y < size.y; y++) {
				for (int x = 0; x < size.x; x++) {
					int index = GridPointToArrayIndex(x, y, z);
					cellInfo.Set(index, new Vector3Int(x,y,z), values[index]);
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