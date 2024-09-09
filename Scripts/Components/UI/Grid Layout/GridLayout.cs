using System;

namespace UnityEngine.UI {
	[RequireComponent(typeof(RectTransform))]
	public class GridLayout : MonoBehaviour, ILayoutElement {
		public RectTransform rectTransform => (RectTransform) transform;

		[Serializable]
		public class GridLayoutAxisSettings {
			[NonSerialized] public GridLayout gridLayout;

			[NonSerialized] public bool isXAxis;

			// Skip the property to avoid expensive checks
			GridLayoutAxisSettings otherAxis => isXAxis ? gridLayout._yAxis : gridLayout._xAxis;
			public float containerSize => isXAxis ? gridLayout.rectTransform.rect.width : gridLayout.rectTransform.rect.height;

			[SerializeField] CellSizeMode _sizeMode = CellSizeMode.FillContainer;

			public CellSizeMode sizeMode {
				get => _sizeMode;
				set => _sizeMode = value;
			}

			[SerializeField] CellCountMode _fillMode = CellCountMode.Defined;

			public CellCountMode fillMode {
				get => _fillMode;
				set => _fillMode = value;
			}

			[SerializeField] float _itemSize = 100;
			[SerializeField] int _cellCount = 3;

			public int cellCount {
				get => _cellCount;
				private set => _cellCount = value;
			}

			[SerializeField] float _aspectRatio = 1;

			public float aspectRatio {
				get => _aspectRatio;
				private set => _aspectRatio = value;
			}

			[SerializeField] float _spacing;

			public float spacing {
				get => _spacing;
				set => _spacing = value;
			}

			[SerializeField] float _offset;

			public float offset {
				get => _offset;
				set => _offset = value;
			}

			[SerializeField] bool _flip;

			public Vector2 margin => isXAxis ? new Vector2(gridLayout.padding.left, gridLayout.padding.right) : new Vector2(gridLayout.padding.bottom, gridLayout.padding.top);

			public void SetTargetCellCount(int newCellCount) => _cellCount = newCellCount;
			public void SetTargetAspectRatio(float newAspectRatio) => _aspectRatio = newAspectRatio;
			public void SetTargetItemSize(float newItemSize) => _itemSize = newItemSize;

			public void ApplySizeToRectTransform() => gridLayout.rectTransform.SetSizeWithCurrentAnchors(isXAxis ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical, GetTotalSize());

			public float GetTotalSize() {
				if (_sizeMode == CellSizeMode.FillContainer) return containerSize;
				return CalculateTotalSize(GetItemSize(), GetCellCount(), _spacing, margin, _offset, otherAxis.GetCellCount());
			}

			public float GetItemSize() {
				if (_sizeMode == CellSizeMode.Defined) return _itemSize;
				else if (_sizeMode == CellSizeMode.FillContainer) return CalculateItemSize(containerSize, _cellCount, _spacing, margin);
				else if (_sizeMode == CellSizeMode.AspectRatio && otherAxis._sizeMode != CellSizeMode.AspectRatio) return _aspectRatio * otherAxis.GetItemSize();
				return 0;
			}

			public int GetCellCount() {
				if (_fillMode == CellCountMode.Defined) return _cellCount;
				else if (_fillMode == CellCountMode.FitContainer) return CalculateCellCount(containerSize, GetItemSize(), _spacing, margin);
				return 0;
			}
			
			public float GetPositionForGridCoord(float index, float otherAxisIndex, float pivot) {
				if (_flip) index = Mathf.Max(0, GetCellCount() - 1) - index;
				return CalculatePositionForGridCoord(index, otherAxisIndex, GetItemSize(), _spacing, margin, _offset, otherAxis.GetCellCount(), pivot);
			}
			public float GetPositionForGridCoord(float index, float otherAxisIndex) => GetPositionForGridCoord(index, otherAxisIndex, 0);
			public float GetCenterPositionForGridCoord(float index, float otherAxisIndex) => GetPositionForGridCoord(index, otherAxisIndex, 0.5f);

			// public float GetPositionForGridCoord(float index) {
			// 	if (_flip) index = Mathf.Max(0, GetCellCount() - 1) - index;
			// 	return CalculatePositionForGridCoord(index, GetItemSize(), _spacing, margin, 0);
			// }
			// public float GetCenterPositionForGridCoord(float index) {
			// 	if (_flip) index = Mathf.Max(0, GetCellCount() - 1) - index;
			// 	return CalculatePositionForGridCoord(index, GetItemSize(), _spacing, margin, 0.5f);
			// }

			public float GetPositionOffsetForGridCoord(float otherAxisIndex) {
				return CalculatePositionOffsetForGridCoord(otherAxisIndex, _offset, otherAxis.GetCellCount());
			}

			public override string ToString() {
				// public float aspectRatio
				// float spacing;
				// float offset;
				// bool _flip;
				return $"[GridLayoutAxisSettings: axis={(isXAxis ? "X" : "Y")}, containerSize={containerSize}, sizeMode={sizeMode}, fillMode={fillMode}, totalSize (calculated)={GetTotalSize()}, itemSize (calculated)={GetItemSize()}, cellCount (calculated)={GetCellCount()}, spacing={spacing}, offset={offset}, flip={_flip}]";
			}
		}


		public RectOffset padding = new();

		[SerializeField] GridLayoutAxisSettings _xAxis = new();

		public GridLayoutAxisSettings xAxis {
			get {
				InitializationCheck();
				return _xAxis;
			}
		}

		[SerializeField] GridLayoutAxisSettings _yAxis = new();

		public GridLayoutAxisSettings yAxis {
			get {
				InitializationCheck();
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
		

		public Vector2Int gridSize => new(xAxis.GetCellCount(), yAxis.GetCellCount());
		public Vector2 itemSize => new(xAxis.GetItemSize(), yAxis.GetItemSize());

		public virtual void CalculateLayoutInputHorizontal() => m_Width = xAxis.GetTotalSize();
		public virtual void CalculateLayoutInputVertical() => m_Height = yAxis.GetTotalSize();

		public virtual float minWidth => m_Width;
		public virtual float minHeight => m_Height;
		public virtual float preferredWidth => m_Width;
		public virtual float preferredHeight => m_Height;
		public virtual float flexibleWidth => -1;
		public virtual float flexibleHeight => -1;
		public virtual int layoutPriority => 1;

		float m_Width;
		float m_Height;

		bool initialized => _xAxis.isXAxis && _yAxis.isXAxis == false && _xAxis.gridLayout == this && _yAxis.gridLayout == this;

		void InitializationCheck() {
			if (!initialized) Init();
		}

		void OnEnable() {
			InitializationCheck();
			Calculate();
		}

		void Reset() {
			InitializationCheck();
			Calculate();
		}

		void OnValidate() {
			Init();
			Calculate();
		}

		void OnRectTransformDimensionsChange() {
			Calculate();
		}

		void Init() {
			_xAxis.isXAxis = true;
			_yAxis.isXAxis = false;
			_xAxis.gridLayout = _yAxis.gridLayout = this;
			Calculate();
		}

		void Calculate() {
			CalculateLayoutInputHorizontal();
			CalculateLayoutInputVertical();
		}

		#region Public Grid Functions
		// Positions are calculated such that a regular grid at 0,0 will be at position 0,0. X extends rightwards, Y extends upwards.
		public Vector2 GetPositionForGridCoord(Vector2 coord) => new (xAxis.GetPositionForGridCoord(coord.x, coord.y), yAxis.GetPositionForGridCoord(coord.y, coord.x));
		public Vector2 GetPositionForGridCoord(Vector2 coord, Vector2 pivot) => new (xAxis.GetPositionForGridCoord(coord.x, coord.y, pivot.x), yAxis.GetPositionForGridCoord(coord.y, coord.x, pivot.y));
		public Vector2 GetCenterPositionForGridCoord(Vector2 coord) => new (xAxis.GetCenterPositionForGridCoord(coord.x, coord.y), yAxis.GetCenterPositionForGridCoord(coord.y, coord.x));
		public Rect GetRectForGridCoord(Vector2Int coord) {
			var localPosition = GetPositionForGridCoord(coord);
			return new Rect(localPosition.x, localPosition.y, xAxis.GetItemSize(), yAxis.GetItemSize());
		}
		
		// Local positions use the RectTransform local space
		public Vector2 GetLocalPositionForGridCoord(Vector2 coord) => GetPositionForGridCoord(coord) + rectTransform.rect.position;
		public Vector2 GetLocalPositionForGridCoord(Vector2 coord, Vector2 pivot) => GetPositionForGridCoord(coord, pivot) + rectTransform.rect.position;
		public Vector2 GetLocalCenterPositionForGridCoord(Vector2 coord) => GetCenterPositionForGridCoord(coord) + rectTransform.rect.position;
		public Rect GetLocalRectForGridCoord(Vector2Int coord) {
			var localPosition = GetLocalPositionForGridCoord(coord);
			return new Rect(localPosition.x, localPosition.y, xAxis.GetItemSize(), yAxis.GetItemSize());
		}
		
		// World space
		public Vector2 GetWorldPositionForGridCoord(Vector2 coord) => transform.TransformPoint(GetLocalPositionForGridCoord(coord));
		public Vector2 GetWorldPositionForGridCoord(Vector2 coord, Vector2 pivot) => transform.TransformPoint(GetLocalPositionForGridCoord(coord, pivot));
		public Vector2 GetWorldCenterPositionForGridCoord(Vector2 coord) => transform.TransformPoint(GetLocalCenterPositionForGridCoord(coord));
		public Rect GetWorldRectForGridCoord(Vector2Int coord) {
			var localRect = GetLocalRectForGridCoord(coord);
			var min = transform.TransformPoint(localRect.min);
			var max = transform.TransformPoint(localRect.max);
			return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
		}

		public Vector2 GetItemSize() {
			return new Vector2(xAxis.GetItemSize(), yAxis.GetItemSize());
		}

		public Vector2 GetTotalSize() {
			return new Vector2(xAxis.GetTotalSize(), yAxis.GetTotalSize());
		}
		#endregion

		public override string ToString() => $"[GridLayout: X={xAxis}, Y={yAxis}]";





		#region Static Grid Functions
		public static int GridCoordToArrayIndex(Vector2Int coord, int numCellsX) => coord.y * numCellsX + coord.x;

		public static Vector2Int ArrayIndexToGridCoord(int arrayIndex, int numCellsX) {
			if (numCellsX == 0) return new Vector2Int(arrayIndex, 0);
			else return new Vector2Int(arrayIndex % numCellsX, Mathf.FloorToInt((float) arrayIndex / numCellsX));
		}

		public static float CalculatePositionForGridCoord(float coord, float otherAxisCoord, float itemSize, float spacing, Vector2 margin, float offset, int otherAxisCellCount, float pivot) {
			return CalculatePositionForGridCoord(coord, itemSize, spacing, margin, pivot) + CalculatePositionOffsetForGridCoord(otherAxisCoord, offset, otherAxisCellCount);
		}
		
		public static float CalculatePositionForGridCoord(float coord, float itemSize, float spacing, Vector2 margin, float pivot) {
			return margin.x + (spacing * coord) + (itemSize * coord);
		}
		
		public static float CalculatePositionOffsetForGridCoord(float otherAxisIndex, float offset, int otherAxisCellCount) {
			var calculatedOffset = offset * otherAxisIndex;
			if (offset < 0) return calculatedOffset - (offset * (otherAxisCellCount - 1));
			else return calculatedOffset;
		}


		// Given a container, a number of items, spacing and margins; calculate the sizes of the items such that they would fill the container
		public static float CalculateItemSize(float containerSize, int numItems, float spacing, Vector2 margin) {
			return numItems == 0 ? 0 : (containerSize - (spacing * (numItems - 1)) - (margin.x + margin.y)) / numItems;
		}

		// Given a container size, items of a certain size, spacing and margins; calculate the maximum number of items that could fit inside the container without going out of bounds.
		public static int CalculateCellCount(float containerSize, float itemSize, float spacing, Vector2 margin) {
			return Mathf.FloorToInt((containerSize - (margin.x + margin.y) - itemSize) / (itemSize + spacing) + 1);
		}

		// Given a number of items with a fixed size, spacing, margins, and an offset to be multiplied by the number of cells on the other axis; calculate the total size of the container
		public static float CalculateTotalSize(float itemSize, int numItems, float spacing, Vector2 margin, float offset, int otherAxisCellCount) {
			if (float.IsNaN(itemSize)) {
				Debug.LogError("Item size is NaN!");
				return 0;
			}

			if (float.IsNaN(spacing)) {
				Debug.LogError("Spacing is NaN!");
				return 0;
			}

			if (float.IsNaN(offset)) {
				Debug.LogError("Offset is NaN!");
				return 0;
			}

			if (numItems == 0) return 0;
			var totalOffset = (otherAxisCellCount - 1) * offset;
			var blockSize = (itemSize * numItems) + (spacing * (numItems - 1));
			return blockSize + Mathf.Abs(totalOffset) + (margin.x + margin.y);
		}
		#endregion
	}
}