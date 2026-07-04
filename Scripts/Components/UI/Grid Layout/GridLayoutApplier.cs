using System.Collections.Generic;

namespace UnityEngine.UI {
	// Automatically sets the children of this object to be objects in the grid, and applies their positions to the grid.
	// Also sets the size of this recttransform
	[ExecuteAlways]
	[RequireComponent(typeof(GridLayout))]
	public class GridLayoutApplier : MonoBehaviour {
		public RectTransform rectTransform => (RectTransform) transform;
		public GridLayout gridLayout => GetComponent<GridLayout>();

		public AutoFillMode autoFillMode;

		public enum AutoFillMode {
			None,

			// Auto,
			XAxis,
			YAxis
		}

		DrivenRectTransformTracker drivenRectTransformTracker;

		void OnEnable() {
			if (Application.isPlaying)
				Refresh();
		}

		void OnDisable() {
			drivenRectTransformTracker.Clear();
		}

		void Update() {
			Refresh();
		}

		List<RectTransform> validChildren = new();

		public void Refresh() {
			drivenRectTransformTracker.Clear();

			validChildren.Clear();
			foreach (Transform child in transform)
				if (child.gameObject.activeInHierarchy && child is RectTransform childRT)
					validChildren.Add(childRT);

			var numValidChildren = validChildren.Count;
			// With no children ArrayIndexToGridCoord(-1, ...) would give a negative cell count.
			if (autoFillMode == AutoFillMode.XAxis) {
				gridLayout.xAxis.SetTargetCellCount(numValidChildren > 0 ? GridLayout.ArrayIndexToGridCoord(numValidChildren - 1, gridLayout.yAxis.GetCellCount()).y + 1 : 0);
				if (gridLayout.xAxis.sizeMode != GridLayout.CellSizeMode.FillContainer) {
					gridLayout.xAxis.ApplySizeToRectTransform();
					drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
				}
			} else if (autoFillMode == AutoFillMode.YAxis) {
				gridLayout.yAxis.SetTargetCellCount(numValidChildren > 0 ? GridLayout.ArrayIndexToGridCoord(numValidChildren - 1, gridLayout.xAxis.GetCellCount()).y + 1 : 0);
				if (gridLayout.yAxis.sizeMode != GridLayout.CellSizeMode.FillContainer) {
					gridLayout.yAxis.ApplySizeToRectTransform();
					drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
				}
			}

			var cellCountX = gridLayout.xAxis.GetCellCount();
			int i = 0;
			foreach (var child in validChildren) {
				var gridCoordinate = GridLayout.ArrayIndexToGridCoord(i, cellCountX);
				gridLayout.ApplyToRectTransform(child, gridCoordinate);
				i++;
			}
		}
	}
}