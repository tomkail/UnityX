using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class GridLayoutItem : MonoBehaviour {
	public RectTransform rectTransform => (RectTransform)transform;

	public GridLayout gridLayout;
	public Vector2 gridCoordinate;

	DrivenRectTransformTracker drivenRectTransformTracker;
	
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

		
		var size = gridLayout.GetItemSize();
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

		rectTransform.position = gridLayout.GetWorldPositionForGridCoord(gridCoordinate);
		rectTransform.anchoredPosition += new Vector2(size.x * (rectTransform.pivot.x), size.y * (rectTransform.pivot.y));

		drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta | DrivenTransformProperties.AnchoredPosition3D);
	}
}