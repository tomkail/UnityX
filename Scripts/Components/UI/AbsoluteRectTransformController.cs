using UnityEngine;

// Sets the rect of the attached RectTransform to a viewport space rect, regardless of the pivot, anchors, or (optionally) scale of the RectTransform.
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class AbsoluteRectTransformController : MonoBehaviour {
	public Rect viewportRect = new Rect(0,0,1,1);
	public bool ignoreScale;
	static Vector3[] corners = new Vector3[4];
	DrivenRectTransformTracker drivenRectTransformTracker;
	
	void OnEnable() {
		Refresh();
	}

	void OnDisable() {
		drivenRectTransformTracker.Clear();
	}

	void Update() {
		Refresh();
	}

	void Refresh () {
		drivenRectTransformTracker.Clear();

		RectTransform rectTransform = (RectTransform)transform;
		if(rectTransform.parent == null) return;

		Canvas canvas = GetComponentInParent<Canvas>()?.rootCanvas;
		if(canvas == null) return;
		RectTransform canvasRT = (RectTransform)canvas.transform;
		
		var parent = rectTransform.parent as RectTransform;
		if(parent == null) {
			Debug.LogWarning("Parent of "+this.GetType().Name+" is not null!", this);
			return;
		}

		canvasRT.GetWorldCorners(corners);
		var min = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[0]);
		var max = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[2]);
		var fullScreenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
		var targetScreenRect = Rect.MinMaxRect(fullScreenRect.x + viewportRect.x * fullScreenRect.width, fullScreenRect.y + viewportRect.y * fullScreenRect.height, fullScreenRect.xMax + (viewportRect.xMax-1) * fullScreenRect.width, fullScreenRect.yMax + (viewportRect.yMax-1) * fullScreenRect.height);
		
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, targetScreenRect.min, canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null, out Vector2 minLocalPoint);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, targetScreenRect.max, canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null, out Vector2 maxLocalPoint);
		
		var minWorldPoint = parent.TransformPoint(minLocalPoint);
		var maxWorldPoint = parent.TransformPoint(maxLocalPoint);

		rectTransform.position = new Vector3(Mathf.LerpUnclamped(minWorldPoint.x, maxWorldPoint.x, rectTransform.pivot.x), Mathf.LerpUnclamped(minWorldPoint.y, maxWorldPoint.y, rectTransform.pivot.y), Mathf.Lerp(minWorldPoint.z, maxWorldPoint.z, 0.5f));

		var size = maxLocalPoint-minLocalPoint;
		if(ignoreScale) size = new Vector2(size.x / rectTransform.localScale.x, size.y / rectTransform.localScale.y);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

		
		drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta | DrivenTransformProperties.AnchoredPosition3D);
	}
}