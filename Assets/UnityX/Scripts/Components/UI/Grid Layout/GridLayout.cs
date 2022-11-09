using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class GridLayout : MonoBehaviour, ILayoutElement {
	public RectTransform rectTransform => (RectTransform)transform;

	[System.Serializable]
	public class GridLayoutAxisSettings {
		[System.NonSerialized]
		public GridLayout gridLayout;
		[System.NonSerialized]
		public bool isXAxis;
		GridLayoutAxisSettings otherAxis => isXAxis ? gridLayout.yAxis : gridLayout.xAxis;
		public float containerSize => isXAxis ? gridLayout.rectTransform.rect.width : gridLayout.rectTransform.rect.height;
		
		[SerializeField]
		CellSizeMode _sizeMode = CellSizeMode.FillContainer;
		public CellSizeMode sizeMode {
			get => _sizeMode;
			set => _sizeMode = value;
		}
		[SerializeField]
		CellCountMode _fillMode = CellCountMode.Defined;
		public CellCountMode fillMode {
			get => _fillMode;
			set => _fillMode = value;
		}

		[SerializeField]
		float _itemSize = 100;
		[SerializeField]
		int _cellCount = 3;
		public int cellCount {
			get => _cellCount;
			private set => _cellCount = value;
		}
		[SerializeField]
		float _aspectRatio = 1;
		public float aspectRatio {
			get => _aspectRatio;
			private set => _aspectRatio = value;
		}
		[SerializeField]
		float _spacing;
		public float spacing {
			get => _spacing;
			set => _spacing = value;
		}
		[SerializeField]
		float _offset;
		public float offset {
			get => _offset;
			set => _offset = value;
		}
		[SerializeField]
		bool _flip;
		
		
		public void SetTargetCellCount (int newCellCount) {
			_cellCount = newCellCount;
		}
		
		public void SetTargetAspectRatio (float aspectRatio) {
			_aspectRatio = aspectRatio;
		}
		public void SetTargetItemSize (float newItemSize) {
			_itemSize = newItemSize;
		}
	
		public void ApplySizeToRectTransform () {
			gridLayout.rectTransform.SetSizeWithCurrentAnchors(isXAxis ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical, GetTotalSize());
		}

		public float GetTotalSize () {
			if(_sizeMode == CellSizeMode.FillContainer) return containerSize;
			return CalculateTotalSize(GetItemSize(), GetCellCount(), _spacing, _offset, otherAxis.GetCellCount());
		}

		public float GetItemSize () {
			if(_sizeMode == CellSizeMode.Defined) return _itemSize;
			else if(_sizeMode == CellSizeMode.FillContainer) return CalculateItemSize(containerSize, _cellCount, _spacing);
			else if(_sizeMode == CellSizeMode.AspectRatio && otherAxis._sizeMode != CellSizeMode.AspectRatio) return _aspectRatio * otherAxis.GetItemSize();

			// if(_axisMode == Calculation.CellCount) return CalculateItemSize(containerSize, _cellCount, _spacing);
			// else if(_axisMode == Calculation.CellSize || _axisMode == Calculation.CellCountAndCellSize) return _itemSize;
			// else if((_axisMode == Calculation.AspectRatio || _axisMode == Calculation.AspectRatioAndCellCount) && !(otherAxis._axisMode == Calculation.AspectRatio || otherAxis._axisMode == Calculation.AspectRatioAndCellCount)) return _aspectRatio * otherAxis.GetItemSize();
			return 0;
		}
		
		public int GetCellCount () {
			if(_fillMode == CellCountMode.Defined) return _cellCount;
			else if(_fillMode == CellCountMode.FitContainer) return Mathf.FloorToInt((containerSize - GetItemSize()) / (GetItemSize() + _spacing) + 1);

			// if(_axisMode == Calculation.CellCount || _axisMode == Calculation.CellCountAndCellSize || _axisMode == Calculation.AspectRatioAndCellCount) return _cellCount;
			// else if(_axisMode == Calculation.CellSize || _axisMode == Calculation.AspectRatio) return Mathf.FloorToInt(containerSize / (GetItemSize() + _spacing));
			return 0;
		}

		public float GetLocalPositionForGridCoord (float index, float otherAxisIndex) {
			if(_flip) index = Mathf.Max(0, GetCellCount() - 1) - index;
			return CalculatePositionForGridCoord(index, otherAxisIndex, GetItemSize(), _spacing, _offset, otherAxis.GetCellCount());
		}
		public float GetLocalPositionForGridCoord (float index) {
			if(_flip) index = Mathf.Max(0, GetCellCount() - 1) - index;
			return CalculatePositionForGridCoord(index, GetItemSize(), _spacing);
		}

		public float GetLocalCenterPositionForGridCoord (float index, float otherAxisIndex) {
			if(_flip) index = Mathf.Max(0, GetCellCount() - 1) - index;
			return CalculateCenterPositionForGridCoord(index, otherAxisIndex, GetItemSize(), _spacing, _offset, otherAxis.GetCellCount());
		}
		public float GetLocalCenterPositionForGridCoord (float index) {
			if(_flip) index = Mathf.Max(0, GetCellCount() - 1) - index;
			return CalculateCenterPositionForGridCoord(index, GetItemSize(), _spacing);
		}

		public float GetOffsetPositionForGridCoord (float otherAxisIndex) {
			return CalculateOffsetForGridCoord(otherAxisIndex, _offset, otherAxis.GetCellCount());
		}

		public override string ToString () {
			// public float aspectRatio
			// float spacing;
			// float offset;
			// bool _flip;
			return string.Format ("[GridLayoutAxisSettings: axis={0}, containerSize={1}, sizeMode={2}, fillMode={3}, totalSize (calculated)={4}, itemSize (calculated)={5}, cellCount (calculated)={6}, spacing={7}, offset={8}, flip={9}]", isXAxis?"X":"Y", containerSize, sizeMode, fillMode, GetTotalSize(), GetItemSize(), GetCellCount(), spacing, offset, _flip);
		}
	}

	[SerializeField]
	GridLayoutAxisSettings _xAxis = new GridLayoutAxisSettings();
	public GridLayoutAxisSettings xAxis {
		get {
			if(!initialized) Init();
			return _xAxis;
		}
	}
	
	[SerializeField]
	GridLayoutAxisSettings _yAxis = new GridLayoutAxisSettings();
	public GridLayoutAxisSettings yAxis {
		get {
			if(!initialized) Init();
			return _yAxis;
		}
	}
	
	public enum CellCountMode {
		Defined,
		FitContainer,
	}
	public enum CellSizeMode {
		Defined,
		AspectRatio,
		FillContainer,
	}



	public Vector2Int gridSize {
		get {
			return new Vector2Int(
				xAxis.GetCellCount(),
				yAxis.GetCellCount()
			);
		}
	}
	public Vector2 itemSize {
		get {
			return new Vector2(
				xAxis.GetItemSize(),
				yAxis.GetItemSize()
			);
		}
	}


	public virtual void CalculateLayoutInputHorizontal() {
		m_Width = xAxis.GetTotalSize();
	}

	public virtual void CalculateLayoutInputVertical() {
		m_Height = yAxis.GetTotalSize();
	}
	
	public virtual float minWidth { get { return m_Width; } }
	public virtual float minHeight { get { return m_Height; } }
	public virtual float preferredWidth { get { return m_Width; } }
	public virtual float preferredHeight { get { return m_Height; } }
	public virtual float flexibleWidth { get { return -1; } }
	public virtual float flexibleHeight { get { return -1; } }
	public virtual int layoutPriority { get { return 1; } }
	float m_Width;
	float m_Height;

	bool initialized {
		get {
			return _xAxis.isXAxis == true && _yAxis.isXAxis == false && _xAxis.gridLayout == this && _yAxis.gridLayout == this;
		}
	}

	void OnEnable () {
		if(!initialized) Init();
		Calculate();
	}
	
	void Reset () {
		if(!initialized) Init();
		Calculate();
	}

	void OnValidate () {
		Init();
		Calculate();
	}
	
	void OnRectTransformDimensionsChange () {
		Calculate();
	}

	void Init () {
		_xAxis.isXAxis = true;
		_yAxis.isXAxis = false;
		_xAxis.gridLayout = _yAxis.gridLayout = this;
		Calculate();
	}

	void Calculate () {
		CalculateLayoutInputHorizontal();
		CalculateLayoutInputVertical();
	}
	
	public Vector2 GetLocalPositionForGridCoord (Vector2 coord) {
		return new Vector2(xAxis.GetLocalPositionForGridCoord(coord.x, coord.y), yAxis.GetLocalPositionForGridCoord(coord.y, coord.x));
	}
	public Vector2 GetLocalCenterPositionForGridCoord (Vector2 coord) {
		return new Vector2(xAxis.GetLocalCenterPositionForGridCoord(coord.x, coord.y), yAxis.GetLocalCenterPositionForGridCoord(coord.y, coord.x));
	}
	public Rect GetLocalRectForGridCoord (Vector2Int coord) {
		var localPosition = GetLocalPositionForGridCoord(coord);
		return new Rect(
			rectTransform.rect.x + localPosition.x,
			rectTransform.rect.y + localPosition.y,
			xAxis.GetItemSize(),
			yAxis.GetItemSize()
		);
	}
	public Vector2 GetWorldPositionForGridCoord (Vector2 coord) {
		return transform.TransformPoint(rectTransform.rect.position + GetLocalPositionForGridCoord(coord));
	}
	public Vector2 GetWorldCenterPositionForGridCoord (Vector2 coord) {
		return transform.TransformPoint(rectTransform.rect.position + GetLocalCenterPositionForGridCoord(coord));
	}
	public Rect GetWorldRectForGridCoord (Vector2Int coord) {
		var localRect = GetLocalRectForGridCoord(coord);
		var min = transform.TransformPoint(localRect.min);
		var max = transform.TransformPoint(localRect.max);
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}
	public Vector2 GetItemSize () {
		return new Vector2(xAxis.GetItemSize(), yAxis.GetItemSize());
	}
	public Vector2 GetTotalSize () {
		return new Vector2(xAxis.GetTotalSize(), yAxis.GetTotalSize());
	}

	public override string ToString () {
		return string.Format ("[GridLayout: X={0}, Y={1}]", xAxis.ToString(), yAxis.ToString());
	}
	
	public static int GridCoordToArrayIndex (Vector2Int coord, int numCellsX){
		return coord.y * numCellsX + coord.x;
	}
	public static Vector2Int ArrayIndexToGridCoord (int arrayIndex, int numCellsX){
		if(numCellsX == 0) return new Vector2Int(arrayIndex, 0);
		else return new Vector2Int(arrayIndex%numCellsX, Mathf.FloorToInt((float)arrayIndex/numCellsX));
	}
	
	
	public static float CalculatePositionForGridCoord (float coord, float otherAxisCoord, float itemSize, float spacing, float offset, int otherAxisCellCount) {
		return CalculatePositionForGridCoord(coord, itemSize, spacing) + CalculateOffsetForGridCoord(otherAxisCoord, offset, otherAxisCellCount);
	}
	public static float CalculateCenterPositionForGridCoord (float coord, float otherAxisCoord, float itemSize, float spacing, float offset, int otherAxisCellCount) {
		return CalculateCenterPositionForGridCoord(coord, itemSize, spacing) + CalculateOffsetForGridCoord(otherAxisCoord, offset, otherAxisCellCount);
	}
	public static float CalculatePositionForGridCoord (float coord, float itemSize, float spacing) {
		return (spacing * coord) + (itemSize * coord);
	}
	public static float CalculateCenterPositionForGridCoord (float coord, float itemSize, float spacing) {
		return CalculatePositionForGridCoord(coord, itemSize, spacing) + (itemSize / 2);
	}

	public static float CalculateOffsetForGridCoord (float otherAxisIndex, float offset, int otherAxisCellCount) {   
		var calculatedOffset = offset * otherAxisIndex;
		if(offset < 0) return calculatedOffset - (offset * (otherAxisCellCount-1));
		else return calculatedOffset;
	}
	public static float CalculateItemSize (float containerSize, int numItems, float spacing = 0) {
		return numItems == 0 ? 0 : (containerSize - (spacing * (numItems - 1))) / numItems;
	}
	public static float CalculateTotalSize (float itemSize, int numItems, float spacing = 0, float offset = 0, int otherAxisCellCount = 0) {
		if(float.IsNaN(itemSize)) {
			Debug.LogError("Item size is NaN!");
			return 0;
		}
		if(float.IsNaN(spacing)) {
			Debug.LogError("Spacing is NaN!");
			return 0;
		}
		if(float.IsNaN(offset)) {
			Debug.LogError("Offset is NaN!");
			return 0;
		}
		if(numItems == 0) return 0;
		var totalOffset = (otherAxisCellCount-1) * offset;
		var blockSize = (itemSize * numItems) + (spacing * (numItems - 1));
		return blockSize + Mathf.Abs(totalOffset);
	}
}