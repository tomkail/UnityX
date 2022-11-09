using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
[RequireComponent(typeof(GridLayout))]
public class GridLayoutApplier : MonoBehaviour {
	public RectTransform rectTransform => (RectTransform)transform;
	public GridLayout gridLayout => GetComponent<GridLayout>();

	public AutoFillMode autoFillMode;
	public enum AutoFillMode {
		None,
		// Auto,
		XAxis,
		YAxis
	}
	DrivenRectTransformTracker drivenRectTransformTracker;
	IEnumerable<RectTransform> GetValidChildren() {
		foreach(Transform child in transform) {
			if(child.gameObject.activeInHierarchy && child is RectTransform)
				yield return (RectTransform)child;
		}
	}
	
	void OnEnable() {
		Refresh();
	}

	void OnDisable() {
		drivenRectTransformTracker.Clear();
	}

	void Update () {
		Refresh();
	}

	void Refresh () {
		drivenRectTransformTracker.Clear();

		var numValidChildren = GetValidChildren().Count();
		if(autoFillMode == AutoFillMode.XAxis) {
			gridLayout.xAxis.SetTargetCellCount(GridLayout.ArrayIndexToGridCoord(numValidChildren, gridLayout.yAxis.cellCount).y+1);
		} else if(autoFillMode == AutoFillMode.YAxis) {
			gridLayout.yAxis.SetTargetCellCount(GridLayout.ArrayIndexToGridCoord(numValidChildren, gridLayout.xAxis.cellCount).y+1);
		}
		// else if(autoFillMode == AutoFillMode.Auto) {
		//     if(gridLayout.xAxis.fillMode == GridLayout.CellCountMode.Defined) {

		//     }
		// }

		gridLayout.xAxis.ApplySizeToRectTransform();
		gridLayout.yAxis.ApplySizeToRectTransform();

		var size = gridLayout.GetItemSize();
		int i = 0;

		foreach(RectTransform child in GetValidChildren()) {
			var gridCoordinate = GridLayout.ArrayIndexToGridCoord(i, gridLayout.xAxis.GetCellCount());
		
			child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
			child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

			child.position = gridLayout.GetWorldPositionForGridCoord(gridCoordinate);
			child.anchoredPosition += new Vector2(size.x * (child.pivot.x), size.y * (child.pivot.y));

			i++;
		}

		drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta);
	}
}