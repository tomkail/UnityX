using UnityEngine;
using UnityEngine.EventSystems;

// Sets the rect of the attached RectTransform to a viewport space rect, regardless of the pivot, anchors, or (optionally) scale of the RectTransform.
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class AbsoluteRectTransformController : UIBehaviour {
	public Rect viewportRect = new(0,0,1,1);
	public bool ignoreSafeArea;
	public bool ignoreScale;
	static Vector3[] canvasWorldCorners = new Vector3[4];
	DrivenRectTransformTracker drivenRectTransformTracker;

	bool _refreshing;
	
	protected override void OnEnable() {
		base.OnEnable();
		if(Application.isPlaying)
			Refresh();
	}

	protected override void OnDisable() {
		base.OnDisable();
		drivenRectTransformTracker.Clear();
	}

	protected override void OnRectTransformDimensionsChange() {
		if (isActiveAndEnabled && !_refreshing) {
			Refresh();
		}
		base.OnRectTransformDimensionsChange();
	}
	
	void Update() {
		Refresh();
	}
	
	void LateUpdate() {
		Refresh();
	}

	void Refresh () {
		_refreshing = true;
		drivenRectTransformTracker.Clear();

		RectTransform rectTransform = (RectTransform)transform;
		if(rectTransform.parent == null) return;

		Canvas canvas = GetComponentInParent<Canvas>()?.rootCanvas;
		if(canvas == null) return;
		RectTransform canvasRT = (RectTransform)canvas.transform;
		
		var parent = rectTransform.parent as RectTransform;
		if(parent == null) {
			Debug.LogWarning("Parent of "+GetType().Name+" is not null!", this);
			return;
		}

		canvasRT.GetWorldCorners(canvasWorldCorners);
		var canvasScreenMin = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, canvasWorldCorners[0]);
		var canvasScreenMax = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, canvasWorldCorners[2]);
		var canvasScreenRect = Rect.MinMaxRect(canvasScreenMin.x, canvasScreenMin.y, canvasScreenMax.x, canvasScreenMax.y);
		if (!ignoreSafeArea) {
			canvasScreenRect = Rect.MinMaxRect(Mathf.Max(canvasScreenRect.xMin, Screen.safeArea.xMin), Mathf.Max(canvasScreenRect.yMin, Screen.safeArea.yMin), Mathf.Min(canvasScreenRect.xMax, Screen.safeArea.xMax), Mathf.Min(canvasScreenRect.yMax, Screen.safeArea.yMax));
		}
		
		var targetScreenRect = Rect.MinMaxRect(canvasScreenRect.x + viewportRect.x * canvasScreenRect.width, canvasScreenRect.y + viewportRect.y * canvasScreenRect.height, canvasScreenRect.xMax + (viewportRect.xMax-1) * canvasScreenRect.width, canvasScreenRect.yMax + (viewportRect.yMax-1) * canvasScreenRect.height);
		
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, targetScreenRect.min, canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null, out Vector2 minLocalPoint);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, targetScreenRect.max, canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null, out Vector2 maxLocalPoint);
		
		var minWorldPoint = parent.TransformPoint(minLocalPoint);
		var maxWorldPoint = parent.TransformPoint(maxLocalPoint);

		var newPosition = new Vector3(Mathf.LerpUnclamped(minWorldPoint.x, maxWorldPoint.x, rectTransform.pivot.x), Mathf.LerpUnclamped(minWorldPoint.y, maxWorldPoint.y, rectTransform.pivot.y), Mathf.Lerp(minWorldPoint.z, maxWorldPoint.z, 0.5f));
		if(rectTransform.position != newPosition)
			rectTransform.position = newPosition;

		var size = maxLocalPoint-minLocalPoint;
		if(ignoreScale) size = new Vector2(size.x / rectTransform.localScale.x, size.y / rectTransform.localScale.y);
		var currentSize = rectTransform.rect.size;
		if(currentSize.x != size.x) rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
		if(currentSize.y != size.y) rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

		
		drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta | DrivenTransformProperties.AnchoredPosition3D);
		_refreshing = false;
	}
}
