using UnityEngine;

public static class RectTransformX {
	static Vector3[] corners = new Vector3[4];

    
	//
    // Summary:
    //     Get the corners of the calculated rectangle in screen space.
    //
    // Parameters:
    //   fourCornersArray:
    //     The array that corners are filled into.
	public static void GetScreenCorners(this RectTransform rectTransform, Canvas canvas, Vector3[] fourCornersArray) {
		rectTransform.GetWorldCorners(corners);

		for (int i = 0; i < 4; i++) {
			// For Canvas mode Screen Space - Overlay there is no Camera; best solution I've found
			// is to use RectTransformUtility.WorldToScreenPoint with a null camera.
			Camera cam = null;
			if(canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
				cam = canvas.worldCamera;
			Vector3 screenCoord = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);

            fourCornersArray[i] = screenCoord;
		}
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
			rects[i] = rectTransforms[i].TransformRectTo(rectTransforms[i].rect, rectTransforms[i].GetComponentInAncestors<Canvas>().GetRectTransform());
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
		rectTransform.position = rectTransform.GetComponentInAncestors<Canvas>().GetRectTransform().TransformPoint(rect.position + Vector2.Scale(rect.size, rectTransform.pivot));
		rectTransform.SetSizeInCanvasSpace(rect.size);
	}

//	public static void SetRectInWorldSpace(this RectTransform rectTransform, Rect rect) {
//		rectTransform.localPosition = rect.position + Vector2.Scale(rect.size, rectTransform.pivot);
//		rectTransform.SetSizeInCanvasSpace(rect.size);
//		Debug.Log(rect+" "+rectTransform.localPosition+" "+rectTransform.anchoredPosition+" "+rectTransform.rect);
//	}


	/// <summary>
	/// Sets the size of the rect transform relative to the canvas, subtracting the effect of the rectTransform's anchors.
	/// SizeDelta is relative to anchors, so if the anchors are not together, the rect transform will have a larger actual width.
	/// </summary>
	/// <param name="trans">Trans.</param>
	/// <param name="newSize">New size.</param>
	public static void SetSizeInCanvasSpace(this RectTransform trans, Vector2 newSize) {
		Vector2 oldSize = trans.rect.size;
		Vector2 deltaSize = newSize - oldSize;
		trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
		trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
	}


	// ------ OLD STUFF


	/// <summary>
	/// Find the size of the parent required to fit the child at a new size, given current anchoring.
	/// When anchors are together, parent is the same as it currently is.
	/// When anchors are at the parent corners, parent needs to grow 1:1 with child.
	/// </summary>
	/// <returns>The required size of the parent on the given axis.</returns>
	/// <param name="size">The desired size of the target RectTransform.</param>
	/// <param name="axis">The axis for the size calculation.</param>
	// Everything under here was built for 80 Days, and may either not work or be unhelpful. If you find something good, comment it up and add it above this line.

	public static float SizeOfParentToFitSize(this RectTransform thisRect, float size, RectTransform.Axis axis) {
		
		int axisIndex = (int)axis;
		float currentSize = thisRect.rect.size[axisIndex];

		float anchorSeparation = thisRect.anchorMax[axisIndex] - thisRect.anchorMin[axisIndex];

		RectTransform parent = thisRect.parent.transform as RectTransform;
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
	/// <param name="rectPosition">Rect position.</param>
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