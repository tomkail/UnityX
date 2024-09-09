using UnityEngine;

public static class RectTransformX {
	static Vector3[] corners = new Vector3[4];

	public static Canvas GetRootCanvas(this RectTransform rectTransform) {
		return rectTransform.GetComponentInParent<Canvas>(true).rootCanvas;
	}

	public static Camera GetCanvasEventCamera(this RectTransform rectTransform) {
		var canvas = rectTransform.GetRootCanvas();
		var renderMode = canvas.renderMode;
		if (renderMode == RenderMode.ScreenSpaceOverlay || (renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null))
			return null;
		return canvas.worldCamera ? canvas.worldCamera : Camera.main;
	}

	// Gets the distance between two rect transforms, in the space of the first rect transform.
	public static float GetClosestDistanceBetweenRectTransforms(RectTransform rectTransform, RectTransform otherRectTransform) {
		var otherScreenRect = otherRectTransform.GetScreenRect(rectTransform.GetRootCanvas());
		return rectTransform.GetClosestDistanceToScreenRect(otherScreenRect);
	}

	public static float GetClosestDistanceToScreenRect(this RectTransform rectTransform, Rect screenRect) {
		var camera = rectTransform.GetCanvasEventCamera();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenRect.BottomLeft(), camera, out var localBottomLeft);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenRect.TopRight(), camera, out var localTopRight);
		var localRect = RectX.CreateEncapsulating(localBottomLeft, localTopRight);
		return RectX.GetClosestDistance(rectTransform.rect, localRect) * (RectX.Intersects(rectTransform.rect, localRect) ? -1 : 1);
		// return RectX.SignedDistance(rectTransform.rect, localRect);
	}

	public static bool ScreenPointToNormalizedPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 normalizedPosition) {
		normalizedPosition = default;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, cam, out var localPosition)) return false;
		var r = rect.rect;
		normalizedPosition = new Vector2((localPosition.x - r.x) / r.width, (localPosition.y - r.y) / r.height);
		normalizedPosition += rect.pivot - (Vector2.one * 0.5f);
		return true;
	}


	public static Vector2 LocalToScreenPosition(this RectTransform rectTransform, Vector2 localPos) {
		var worldPos = rectTransform.TransformPoint(localPos);
		return RectTransformUtility.WorldToScreenPoint(rectTransform.GetCanvasEventCamera(), worldPos);
	}

	public static Vector2 LocalToScreenVector(this RectTransform rectTransform, Vector2 localPos) {
		return LocalToScreenPosition(rectTransform, localPos) - LocalToScreenPosition(rectTransform, Vector2.zero);
	}

	public static Vector3[] GetWorldCorners(this RectTransform rectTransform) {
		rectTransform.GetWorldCorners(corners);
		return corners;
	}

//
    // Summary:
    //     Get the corners of the calculated rectangle in screen space.
    //
    // Parameters:
    //   fourCornersArray:
    //     The array that corners are filled into.
	public static void GetScreenCorners(this RectTransform rectTransform, Vector3[] fourCornersArray) {
        rectTransform.GetScreenCorners(rectTransform.GetRootCanvas(), fourCornersArray);
	}
	public static void GetScreenCorners(this RectTransform rectTransform, Canvas canvas, Vector3[] fourCornersArray) {
		rectTransform.GetWorldCorners(corners);
		for (int i = 0; i < 4; i++) fourCornersArray[i] = RectTransformUtility.WorldToScreenPoint(GetCanvasRenderCamera(canvas), corners[i]);
	}

	public static Rect GetScreenRect(this RectTransform rectTransform) {
        return rectTransform.GetScreenRect(rectTransform.GetRootCanvas());
	}
	public static Rect GetScreenRect(this RectTransform rectTransform, Canvas canvas) {
		rectTransform.GetScreenCorners(canvas, corners);
		float xMin = float.PositiveInfinity;
		float xMax = float.NegativeInfinity;
		float yMin = float.PositiveInfinity;
		float yMax = float.NegativeInfinity;
		for (int i = 0; i < 4; i++) {
            var screenCoord = corners[i];
			if (screenCoord.x < xMin)
				xMin = screenCoord.x;
			if (screenCoord.x > xMax)
				xMax = screenCoord.x;
			if (screenCoord.y < yMin)
				yMin = screenCoord.y;
			if (screenCoord.y > yMax)
				yMax = screenCoord.y;
		}
		return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
	}

	public static Rect GetScreenRectIgnoringScale(this RectTransform rectTransform) {
        return rectTransform.GetScreenRectIgnoringScale(rectTransform.GetRootCanvas());
	}
	public static Rect GetScreenRectIgnoringScale (this RectTransform rectTransform, Canvas canvas) {
		Rect tmpRect = rectTransform.rect;
		Matrix4x4 localToWorldMatrix = Matrix4x4.TRS(rectTransform.position, rectTransform.rotation, Vector3.one);
		var min = RectTransformUtility.WorldToScreenPoint(GetCanvasRenderCamera(canvas), localToWorldMatrix.MultiplyPoint(tmpRect.min));
		var max = RectTransformUtility.WorldToScreenPoint(GetCanvasRenderCamera(canvas), localToWorldMatrix.MultiplyPoint(tmpRect.max));
		return RectX.CreateEncapsulating(min, max);
	}
	public static Rect GetScreenRectIgnoringRotation(this RectTransform rectTransform) {
        return rectTransform.GetScreenRectIgnoringRotation(rectTransform.GetRootCanvas());
	}
	public static Rect GetScreenRectIgnoringRotation(this RectTransform rectTransform, Canvas canvas) {
		Rect tmpRect = rectTransform.rect;
		Matrix4x4 localToWorldMatrix = Matrix4x4.TRS(rectTransform.position, Quaternion.identity, rectTransform.lossyScale);
		var min = RectTransformUtility.WorldToScreenPoint(GetCanvasRenderCamera(canvas), localToWorldMatrix.MultiplyPoint(tmpRect.min));
		var max = RectTransformUtility.WorldToScreenPoint(GetCanvasRenderCamera(canvas), localToWorldMatrix.MultiplyPoint(tmpRect.max));
		return RectX.CreateEncapsulating(min, max);
	}
	

	public static Bounds GetScaledBounds(this RectTransform rectTransform, Transform relativeTo) {
		rectTransform.GetWorldCorners(corners);
		var viewWorldToLocalMatrix = relativeTo.worldToLocalMatrix;
		var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

		for (int j = 0; j < 4; j++) {
			Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
			vMin = Vector3.Min(v, vMin);
			vMax = Vector3.Max(v, vMax);
		}

		var bounds = new Bounds(vMin, Vector3.zero);
		bounds.Encapsulate(vMax);
		return bounds;
	}

	// Moves and resizes (will not affect pivot or anchors) of the RectTransform to encapsulate other rect transforms.
	public static void ResizeToEncapsulateRectTransforms (this RectTransform rectTransform, params RectTransform[] rectTransforms) {
		Rect combinedRect = rectTransform.GetRectEncapsulatingRectTransformsInCanvasSpace(rectTransforms);
		rectTransform.SetRectInCanvasSpace(combinedRect);
	}

	public static Rect GetRectEncapsulatingRectTransformsInCanvasSpace (this RectTransform rectTransform, params RectTransform[] rectTransforms) {
		Rect[] rects = new Rect[rectTransforms.Length];
		for (int i = 0; i < rectTransforms.Length; i++) {
			rects[i] = rectTransforms[i].TransformRectTo(rectTransforms[i].rect, rectTransforms[i].GetComponentInParent<Canvas>().GetRectTransform());
		}
		return RectX.CreateEncapsulating(rects);
	}

	public static Rect GetRectEncapsulatingRectTransformsInWorldSpace (params RectTransform[] rectTransforms) {
		Rect[] rects = new Rect[rectTransforms.Length];
		for (int i = 0; i < rectTransforms.Length; i++) {
			rects[i] = rectTransforms[i].TransformRect(rectTransforms[i].rect);
		}
		return RectX.CreateEncapsulating(rects);
	}

	
	
	public static Vector2 ScreenToLocalVectorInRectangle(RectTransform rt, Vector2 screenVector) {
		return ScreenToLocalVectorInRectangle(rt, screenVector, rt.GetCanvasEventCamera());
	}
	public static Vector2 ScreenToLocalVectorInRectangle(RectTransform rt, Vector2 screenVector, Camera camera) {
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Vector2.zero, camera, out var localBottomLeft);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenVector, camera, out var localTopRight);
		return localTopRight - localBottomLeft;
	}


	public static void ScreenRectToLocalRectInRectangle(RectTransform rt, Rect screenRect, out Rect rect) {
		var camera = rt.GetCanvasEventCamera();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenRect.BottomLeft(), camera, out var localBottomLeft);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenRect.TopRight(), camera, out var localTopRight);
		rect = RectX.CreateEncapsulating(localBottomLeft, localTopRight);
	}
	public static void ScreenRectToLocalRectInRectangle(RectTransform rt, Rect screenRect, Camera camera, out Rect rect) {
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenRect.BottomLeft(), camera, out var localBottomLeft);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenRect.TopRight(), camera, out var localTopRight);
		rect = RectX.CreateEncapsulating(localBottomLeft, localTopRight);
	}
	
	/// <summary>
	/// Transforms a rect from local to world space
	/// </summary>
	/// <returns>The rect.</returns>
	/// <param name="rectTransform">Rect transform.</param>
	/// <param name="rect">Rect.</param>
	public static Rect TransformRect (this RectTransform rectTransform, Rect rect) {
		return RectX.MinMaxRect (rectTransform.TransformPoint (rect.min), rectTransform.TransformPoint (rect.max));
	}

	/// <summary>
	/// Transforms a rect from world to local space
	/// </summary>
	/// <returns>The transform rect.</returns>
	/// <param name="rectTransform">Rect transform.</param>
	/// <param name="rect">Rect.</param>
	public static Rect InverseTransformRect (this RectTransform rectTransform, Rect rect) {
		return RectX.MinMaxRect (rectTransform.InverseTransformPoint (rect.min), rectTransform.InverseTransformPoint (rect.max));
	}

	public static Rect TransformRectTo (this RectTransform rectTransform, Rect rect, Transform otherRectTransform) {
		return RectX.MinMaxRect (rectTransform.TransformPointTo (rect.min, otherRectTransform), rectTransform.TransformPointTo (rect.max, otherRectTransform));
	}

	// "Canvas" Size can be thought of as similar to world space (although it isn't since it's actually relative to the canvas)
	// It currently doesn't handle the transform scale property, since it hasn't yet come up, and arguably is useful to keep, since scale tends to be used for effects.
	// Actually, is it more like "Sibling space"?

	/// <summary>
	/// Manually sets the getter-only rect property in a recttransform.
	/// Rect is in local space.
	/// </summary>
	/// <param name="rectTransform">Rect transform.</param>
	/// <param name="rect">Rect.</param>
	public static void SetRectInCanvasSpace(this RectTransform rectTransform, Rect rect) {
		rectTransform.position = rectTransform.GetComponentInParent<Canvas>().GetRectTransform().TransformPoint(rect.position + Vector2.Scale(rect.size, rectTransform.pivot));
		rectTransform.SetSizeInCanvasSpace(rect.size);
	}

//	public static void SetRectInWorldSpace(this RectTransform rectTransform, Rect rect) {
//		rectTransform.localPosition = rect.position + Vector2.Scale(rect.size, rectTransform.pivot);
//		rectTransform.SetSizeInCanvasSpace(rect.size);
//		Debug.Log(rect+" "+rectTransform.localPosition+" "+rectTransform.anchoredPosition+" "+rectTransform.rect);
//	}

	// Set the size of the rect transform. 
	public static void SetSizeWithCurrentAnchors(this RectTransform rectTransform, Vector2 size) {
		RectTransform parent = (RectTransform)rectTransform.parent;
		var parentSize = !(bool) (Object) parent ? Vector2.zero : parent.rect.size;
		var anchor = rectTransform.anchorMax - rectTransform.anchorMin;
		var anchoredParentSize = parentSize * anchor;
		rectTransform.sizeDelta = size - anchoredParentSize;
	}
	
	// Set the size of the rect transform, allowing for a custom pivot for the expansion.
	public static void SetSizeWithCurrentAnchors(this RectTransform rectTransform, Vector2 size, Vector2 pivot) {
		var sizeDelta = size - rectTransform.rect.size;
		
		RectTransform parent = (RectTransform)rectTransform.parent;
		var parentSize = !(bool) (Object) parent ? Vector2.zero : parent.rect.size;
		var anchor = rectTransform.anchorMax - rectTransform.anchorMin;
		var anchoredParentSize = parentSize * anchor;
		rectTransform.sizeDelta = size - anchoredParentSize;

		var pivotOffset = rectTransform.pivot - pivot;
		rectTransform.anchoredPosition += pivotOffset * sizeDelta;
	}
	
	
	
	// Sets the world position of an edge of a rect transform by changing its position. The size of the rect transform is unaffected.
	public static void SetWorldPositionOfEdgeByTranslation(RectTransform rectTransform, RectTransform.Edge edge, float targetWorldPosition) {
		var axis = EdgeToAxis(edge);
		var pivotEdge = EdgeToPivot(edge);
		
		var targetWorldPositionVector = new Vector3 {
			[(int) axis] = targetWorldPosition
		};

		// Get the difference between the current and target positions in local space.
		var currentEdgePosition = Rect.NormalizedToPoint(rectTransform.rect, pivotEdge)[(int) axis];
		var targetEdgePosition = rectTransform.InverseTransformPoint(targetWorldPositionVector)[(int) axis];
		var delta = targetEdgePosition - currentEdgePosition;
		
		var translation = Vector2.zero;
		translation[(int) axis] += delta;
		rectTransform.anchoredPosition += translation;
	}
	
	// Sets the world position of an edge of a rect transform by changing its size. The positions of other edges are unaffected.
	public static void SetWorldPositionOfEdgeByExpansion(RectTransform rectTransform, RectTransform.Edge edge, float targetWorldPosition) {
		var axis = EdgeToAxis(edge);
		var pivotEdge = EdgeToPivot(edge);
		
		var targetWorldPositionVector = new Vector3 {
			[(int) axis] = targetWorldPosition
		};
		
		// Get the difference between the current and target positions in local space.
		var currentEdgePosition = Rect.NormalizedToPoint(rectTransform.rect, pivotEdge)[(int) axis];
		var targetEdgePosition = rectTransform.InverseTransformPoint(targetWorldPositionVector)[(int) axis];
		var delta = targetEdgePosition - currentEdgePosition;
		if(pivotEdge[(int) axis] < 0.5f) delta = -delta;
		
		// Get the new target size of the rect transform
		var newSize = rectTransform.rect.size;
		newSize[(int) axis] += delta;
		
		// Fixing the opposite edge, set the size of the rect transform
		pivotEdge[(int) axis] = 1f-pivotEdge[(int) axis];
		rectTransform.SetSizeWithCurrentAnchors(newSize, pivotEdge);
	}
	
	/// <summary>
	/// Sets the size of the rect transform relative to the canvas, subtracting the effect of the rectTransform's anchors.
	/// SizeDelta is relative to anchors, so if the anchors are not together, the rect transform will have a larger actual width.
	/// </summary>
	/// <param name="trans">Trans.</param>
	/// <param name="newSize">New size.</param>
	public static void SetSizeInCanvasSpace(this RectTransform trans, Vector2 newSize) {
		Vector2 oldSize = trans.rect.size;
		Vector2 deltaSize = newSize - oldSize;
		var pivot = trans.pivot;
		trans.offsetMin -= new Vector2(deltaSize.x * pivot.x, deltaSize.y * pivot.y);
		trans.offsetMax += new Vector2(deltaSize.x * (1f - pivot.x), deltaSize.y * (1f - pivot.y));
	}

	
	
	
	// Add this to convert a local position to an anchored position
	public static Vector2 GetLocalToAnchoredPositionOffset(this RectTransform rectTransform) {
		var parentRT = (RectTransform) rectTransform.parent;
		var pivotAnchor = new Vector2(Mathf.LerpUnclamped(rectTransform.anchorMin.x, rectTransform.anchorMax.x, rectTransform.pivot.x), Mathf.LerpUnclamped(rectTransform.anchorMin.y, rectTransform.anchorMax.y, rectTransform.pivot.y));
		return -parentRT.rect.size * (pivotAnchor - parentRT.pivot);
	}
	
	// Add this to convert a local position to an anchored position
	public static Vector2 GetAnchoredToLocalPositionOffset(this RectTransform rectTransform) {
		return -rectTransform.GetLocalToAnchoredPositionOffset();
	}
	
	
	
	// Returns the anchored position required to make a custom pivot point for a rect transform move to the specified anchor point on the target.
	public static Vector2 GetAnchoredPositionForTargetAnchorAndPivot(this RectTransform content, RectTransform target, Vector2 targetAnchor, Vector2 contentPivot) {
		var targetRect = target.rect;
		var localTargetAnchorPosition = new Vector2(Mathf.LerpUnclamped(targetRect.x, targetRect.xMax, targetAnchor.x), Mathf.LerpUnclamped(targetRect.y, targetRect.yMax, targetAnchor.y));
		var worldTargetAnchorPosition = target.TransformPoint(localTargetAnchorPosition);
		return content.GetAnchoredPositionForWorldPointAndPivot(worldTargetAnchorPosition, contentPivot);
	}
	
	// Returns the anchored position required to make a custom pivot point for a rect transform move to a given world point.
	public static Vector2 GetAnchoredPositionForWorldPointAndPivot(this RectTransform content, Vector3 worldPos, Vector2 contentPivot) {
		var parent = content.parent as RectTransform;
		var localPointInContainer = (Vector2)parent.InverseTransformPoint(worldPos);
		var anchoredPositionOffset = content.GetLocalToAnchoredPositionOffset();
		var pivotOffset = (content.pivot - contentPivot) * content.rect.size;
		return localPointInContainer + anchoredPositionOffset + pivotOffset;
	}
	
	
    // Returns the given local position for the rectTransform, clamped within a screen space rect. 
	public static Vector2 GetClampedLocalPositionInsideScreenRect(RectTransform rectTransform, Vector2 localPosition, Rect screenRect, Camera camera) {
		ScreenRectToLocalRectInRectangle((RectTransform)rectTransform.parent, screenRect, camera, out var localContainerRect);
		var rectSize = rectTransform.rect.size;
		var pivotOffset = rectSize * (rectTransform.pivot);
		localContainerRect = new Rect(localContainerRect.x+pivotOffset.x, localContainerRect.y+pivotOffset.y, localContainerRect.width-rectSize.x, localContainerRect.height-rectSize.y);
		return localContainerRect.ClosestPoint(localPosition);
	}
    
	// Returns the given anchored position for the rectTransform, clamped within a screen space rect.
	public static Vector2 GetClampedAnchoredPositionInsideScreenRect(RectTransform rectTransform, Vector2 anchoredPosition, Rect screenRect, Camera camera) {
		return GetClampedLocalPositionInsideScreenRect(rectTransform, anchoredPosition+rectTransform.GetAnchoredToLocalPositionOffset(), screenRect, camera)+rectTransform.GetLocalToAnchoredPositionOffset();
	}
	
	
	
	
	static Camera GetCanvasRenderCamera(Canvas canvas) {
		if(canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
		return canvas.rootCanvas.worldCamera;
	}
	
	
	
	

	// Utilities for working with RectTransform.Axis and RectTransform.Edge
	public static RectTransform.Axis GetOtherAxis(RectTransform.Axis axis) => axis == RectTransform.Axis.Horizontal ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;
	public static RectTransform.Edge GetOppositeEdge(RectTransform.Edge edge) => (int)edge < 2 ? (RectTransform.Edge)(1 - (int)edge) : (RectTransform.Edge)(5 - (int)edge);
	public static RectTransform.Axis EdgeToAxis(RectTransform.Edge edge) => edge is RectTransform.Edge.Bottom or RectTransform.Edge.Top ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;
	public static Vector2 EdgeToPivot(RectTransform.Edge edge, bool invert = false, float otherAxisValue = 0.5f) {
		if (edge == RectTransform.Edge.Left) return new Vector2(invert ? 1 : 0, otherAxisValue);
		else if (edge == RectTransform.Edge.Right) return new Vector2(invert ? 0 : 1, otherAxisValue);
		else if (edge == RectTransform.Edge.Top) return new Vector2(otherAxisValue, invert ? 0 : 1);
		else if (edge == RectTransform.Edge.Bottom) return new Vector2(otherAxisValue, invert ? 1 : 0);
		return Vector2.zero;
	}
	
	
	
	
	
	
	
	
	
	
	
	
	

	// ------ OLD STUFF
	// Everything under here was built for 80 Days, and may either not work or be unhelpful. If you find something good, comment it up and add it above this line.


	/// <summary>
	/// Find the size of the parent required to fit the child at a new size, given current anchoring.
	/// When anchors are together, parent is the same as it currently is.
	/// When anchors are at the parent corners, parent needs to grow 1:1 with child.
	/// </summary>
	/// <returns>The required size of the parent on the given axis.</returns>
	/// <param name="thisRect"></param>
	/// <param name="size">The desired size of the target RectTransform.</param>
	/// <param name="axis">The axis for the size calculation.</param>
	public static float SizeOfParentToFitSize(this RectTransform thisRect, float size, RectTransform.Axis axis) {
		int axisIndex = (int)axis;
		float currentSize = thisRect.rect.size[axisIndex];

		float anchorSeparation = thisRect.anchorMax[axisIndex] - thisRect.anchorMin[axisIndex];

		RectTransform parent = (RectTransform)thisRect.parent.transform;
		float parentSize = parent.rect.size[axisIndex];

		float toParent = parentSize - currentSize;
		float newParent = size + anchorSeparation * toParent;
		
		return newParent;
	}
	
	/// <summary>
	/// Returns the anchor of a RectTransform as a rect.
	/// </summary>
	/// <param name="rectTransform">Rect transform.</param>
	public static Rect Anchor(this RectTransform rectTransform) {
		return Rect.MinMaxRect(rectTransform.anchorMin.x, rectTransform.anchorMin.y, rectTransform.anchorMax.x, rectTransform.anchorMax.y);
	}

	/// <summary>
	/// Gets the anchor of the specified rectTransform at a Vector. Eg (0,0) returns anchorMin, (1,1) returns anchorMax.
	/// </summary>
	/// <param name="rectTransform">Rect transform.</param>
	/// <param name="normalizedRectCoordinates"></param>
	public static Vector2 AnchorPosition(this RectTransform rectTransform, Vector2 normalizedRectCoordinates) {
		return Rect.NormalizedToPoint(Anchor(rectTransform), normalizedRectCoordinates);
	}
	
	/// <summary>
	/// The center of a RectTransform's anchors. 
	/// Shorthand for rectTransform.AnchorPosition(new Vector2(0.5f, 0.5f)).
	/// </summary>
	/// <returns>The center.</returns>
	/// <param name="rectTransform">Rect transform.</param>
	public static Vector2 AnchorCenter(this RectTransform rectTransform) {
		return rectTransform.AnchorPosition(new Vector2(0.5f, 0.5f));
	}

	public static void SetAnchors(this RectTransform trans, Vector2 aVec) {
		trans.anchorMin = aVec;
		trans.anchorMax = aVec;
	}

	public static void SetPivotAndAnchors(this RectTransform trans, Vector2 aVec) {
		trans.pivot = aVec;
		trans.anchorMin = aVec;
		trans.anchorMax = aVec;
	}
	
	public static Vector2 GetSize(this RectTransform trans) {
		return trans.rect.size;
	}
	public static float GetWidth(this RectTransform trans) {
		return trans.rect.width;
	}
	public static float GetHeight(this RectTransform trans) {
		return trans.rect.height;
	}
	
	public static void SetPositionOfPivot(this RectTransform trans, Vector2 newPos) {
		trans.localPosition = new Vector3(newPos.x, newPos.y, trans.localPosition.z);
	}
	
	public static void SetLeftBottomPosition(this RectTransform trans, Vector2 newPos) {
		trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
	}
	public static void SetLeftTopPosition(this RectTransform trans, Vector2 newPos) {
		trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
	}
	public static void SetRightBottomPosition(this RectTransform trans, Vector2 newPos) {
		trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
	}
	public static void SetRightTopPosition(this RectTransform trans, Vector2 newPos) {
		trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
	}
	

	public static void SetWidth(this RectTransform trans, float newSize) {
		SetSizeInCanvasSpace(trans, new Vector2(newSize, trans.rect.size.y));
	}
	public static void SetHeight(this RectTransform trans, float newSize) {
		SetSizeInCanvasSpace(trans, new Vector2(trans.rect.size.x, newSize));
	}
}