using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace UnityX.UI {
	// Automatically sets the children of this object to be objects in the grid, and applies their positions to the grid.
	// Also sets the size of this recttransform
	[ExecuteAlways]
	[RequireComponent(typeof(GridLayoutElement))]
	public class GridLayoutApplier : MonoBehaviour {
		public RectTransform rectTransform => (RectTransform) transform;
		public GridLayoutElement gridLayout => GetComponent<GridLayoutElement>();

		public AutoFillMode autoFillMode;

		public enum AutoFillMode {
			None,
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
				gridLayout.xAxis.SetTargetCellCount(numValidChildren > 0 ? GridLayoutElement.ArrayIndexToGridCoord(numValidChildren - 1, gridLayout.yAxis.GetCellCount()).y + 1 : 0);
				if (gridLayout.xAxis.sizeMode != GridLayoutElement.CellSizeMode.FillContainer) {
					gridLayout.xAxis.ApplySizeToRectTransform();
					drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
				}
			} else if (autoFillMode == AutoFillMode.YAxis) {
				gridLayout.yAxis.SetTargetCellCount(numValidChildren > 0 ? GridLayoutElement.ArrayIndexToGridCoord(numValidChildren - 1, gridLayout.xAxis.GetCellCount()).y + 1 : 0);
				if (gridLayout.yAxis.sizeMode != GridLayoutElement.CellSizeMode.FillContainer) {
					gridLayout.yAxis.ApplySizeToRectTransform();
					drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
				}
			}

			var cellCountX = gridLayout.xAxis.GetCellCount();
			int i = 0;
			foreach (var child in validChildren) {
				var gridCoordinate = GridLayoutElement.ArrayIndexToGridCoord(i, cellCountX);
				gridLayout.ApplyToRectTransform(child, gridCoordinate);
				i++;
			}
		}
	}
}