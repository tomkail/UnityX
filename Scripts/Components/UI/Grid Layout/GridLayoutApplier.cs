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
			if (autoFillMode == AutoFillMode.XAxis) {
				gridLayout.xAxis.SetTargetCellCount(GridLayout.ArrayIndexToGridCoord(numValidChildren - 1, gridLayout.yAxis.GetCellCount()).y + 1);
			} else if (autoFillMode == AutoFillMode.YAxis) {
				gridLayout.yAxis.SetTargetCellCount(GridLayout.ArrayIndexToGridCoord(numValidChildren - 1, gridLayout.xAxis.GetCellCount()).y + 1);
			}
			// else if(autoFillMode == AutoFillMode.Auto) {
			//     if(gridLayout.xAxis.fillMode == GridLayout.CellCountMode.Defined) {

			//     }
			// }

			if (autoFillMode == AutoFillMode.XAxis) {
				if (gridLayout.xAxis.sizeMode != GridLayout.CellSizeMode.FillContainer) {
					gridLayout.xAxis.ApplySizeToRectTransform();
					drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
				}
			} else if (autoFillMode == AutoFillMode.YAxis) {
				if (gridLayout.yAxis.sizeMode != GridLayout.CellSizeMode.FillContainer) {
					gridLayout.yAxis.ApplySizeToRectTransform();
					drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
				}
			}

			var size = gridLayout.GetItemSize();
			var cellCountX = gridLayout.xAxis.GetCellCount();
			int i = 0;
			foreach (var child in validChildren) {
				var gridCoordinate = GridLayout.ArrayIndexToGridCoord(i, cellCountX);

				child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
				child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

				child.position = gridLayout.GetWorldPositionForGridCoord(gridCoordinate);
				var pivot = child.pivot;
				child.anchoredPosition += new Vector2(size.x * (pivot.x), size.y * (pivot.y));

				i++;
			}
		}
	}
}