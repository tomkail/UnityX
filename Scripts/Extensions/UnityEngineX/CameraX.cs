using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension methods for the UnityEngine.Camera class, and helper methods for working with cameras in general.
/// </summary>
public static class CameraX {

	#region Extensions for UnityEngine.Camera

	public static Rect ViewportToScreenRect (this Camera camera, Rect rect) {
		var min = camera.ViewportToScreenPoint(rect.min);
		var max = camera.ViewportToScreenPoint(rect.max);
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}
	public static Vector3 ViewportToScreenVector (this Camera camera, Vector2 vector) {
		return camera.ViewportToScreenPoint(vector) - camera.ViewportToScreenPoint(Vector2.zero);
	}

	public static Vector3 ViewportToWorldVector (this Camera camera, Vector2 vector, float distance) {
		return camera.ViewportToWorldPoint(new Vector3(0,0,distance)) - camera.ViewportToWorldPoint(new Vector3(vector.x, vector.y, distance));
	}


	public static Rect ScreenToViewportRect (this Camera camera, Rect rect) {
		var min = camera.ScreenToViewportPoint(rect.min);
		var max = camera.ScreenToViewportPoint(rect.max);
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}
	public static Vector3 ScreenToWorldVector (this Camera camera, Vector2 vector, float distance) {
		return camera.ScreenToWorldPoint(new Vector3(0,0,distance)) - camera.ScreenToWorldPoint(new Vector3(vector.x, vector.y, distance));
	}
	public static Vector3 ScreenToViewportVector (this Camera camera, Vector2 vector) {
		return camera.ScreenToViewportPoint(vector) - camera.ScreenToViewportPoint(Vector2.zero);
	}


	public static Vector2 WorldToViewportVector (this Camera camera, Vector3 vector) {
		return camera.WorldToViewportPoint(Vector2.zero) - camera.WorldToViewportPoint(vector);
	}

	public static Rect WorldToViewportRect (this Camera camera, Rect worldRect, float distance) {
		Vector3 worldTopLeft = camera.WorldToViewportPoint(new Vector3(worldRect.x, worldRect.y, distance));
		Vector3 worldBottomRight = camera.WorldToViewportPoint(new Vector3(worldRect.x + worldRect.width, worldRect.y + worldRect.height, distance));
		return Rect.MinMaxRect(worldTopLeft.x, worldTopLeft.y, worldBottomRight.x, worldBottomRight.y);
	}

	public static Vector2[] ScreenToViewportPoints (this Camera camera, Vector2[] screenPoints) {
		Vector2[] viewportPoints = new Vector2[screenPoints.Length];
        for(int i = 0; i < viewportPoints.Length; i++) viewportPoints[i] = camera.ScreenToViewportPoint(screenPoints[i]);
		return viewportPoints;
	}
	
	public static Vector2[] WorldToScreenPoints (this Camera camera, Vector3[] worldPoints) {
		Vector2[] screenPoints = new Vector2[worldPoints.Length];
		camera.WorldToScreenPoints(worldPoints, ref screenPoints);
		return screenPoints;
	}
	
	public static void WorldToScreenPoints (this Camera camera, Vector3[] worldPoints, ref Vector2[] screenPoints) {
		if(screenPoints == null || screenPoints.Length != worldPoints.Length) screenPoints = new Vector2[worldPoints.Length];
        for(int i = 0; i < screenPoints.Length; i++) screenPoints[i] = camera.WorldToScreenPoint(worldPoints[i]);
	}

	public static Rect WorldToScreenRect (this Camera camera, Bounds worldBounds) {
		Vector2[] screenSpaceTargetPoints = camera.WorldToScreenPoints(GetVerticesFromBounds(worldBounds));
		Rect rect = new Rect(screenSpaceTargetPoints[0].x, screenSpaceTargetPoints[0].y, 0, 0);
		for(int i = 1; i < screenSpaceTargetPoints.Length; i++) {
			var xMin = Mathf.Min (rect.xMin, screenSpaceTargetPoints[i].x);
			var xMax = Mathf.Max (rect.xMax, screenSpaceTargetPoints[i].x);
			var yMin = Mathf.Min (rect.yMin, screenSpaceTargetPoints[i].y);
			var yMax = Mathf.Max (rect.yMax, screenSpaceTargetPoints[i].y);
			rect = Rect.MinMaxRect (xMin, yMin, xMax, yMax);
		}
		return rect;
	}

	public static Vector3[] GetVerticesFromBounds(Bounds bounds) {
		var min = bounds.min;
		var max = bounds.max;
		return new Vector3[8]{min, max, new(min.x, min.y, max.z), new(min.x, max.y, min.z), new(max.x, min.y, min.z), new(min.x, max.y, max.z), new(max.x, min.y, max.z), new(max.x, max.y, min.z)};
	}
	
	public static Rect WorldToScreenRect (this Camera camera, Rect worldRect, float distance) {
		Vector3 worldTopLeft = camera.WorldToScreenPoint(new Vector3(worldRect.x, worldRect.y, distance));
		Vector3 worldBottomRight = camera.WorldToScreenPoint(new Vector3(worldRect.x + worldRect.width, worldRect.y + worldRect.height, distance));
		return Rect.MinMaxRect(worldTopLeft.x, worldTopLeft.y, worldBottomRight.x, worldBottomRight.y);
	}
	
    public static Vector2[] WorldToViewportPoints (this Camera camera, IList<Vector3> input) {
		Vector2[] output = new Vector2[input.Count];
		for(int i = 0; i < output.Length; i++) {
			output[i] = camera.WorldToViewportPoint(input[i]);
		}
		return output;
	}
    
    
	public enum ScalingMode {
		// The target rect fits the camera rect exactly on the x axis.
		AspectFitWidthOnly,
		// The target rect fits the camera rect exactly on the y axis.
		AspectFitHeightOnly,
		// The target rect fits the camera rect exactly on the smallest axis. May leave empty space in the camera rect.
		AspectFit,
		// The target rect fills the entire camera rect. May crop the target.
		AspectFill
	}
	public static float CalculateOrthographicSize(float cameraRectAspect, Vector2 boundingBoxSize, ScalingMode scalingMode) {
		float targetRectAspect = boundingBoxSize.x / boundingBoxSize.y;
		switch (scalingMode) {
			case ScalingMode.AspectFitWidthOnly:
				return boundingBoxSize.x / (2 * cameraRectAspect);
			case ScalingMode.AspectFitHeightOnly:
				return boundingBoxSize.y / 2;
			case ScalingMode.AspectFit:
				if (cameraRectAspect >= targetRectAspect) return boundingBoxSize.y / 2;
				else return boundingBoxSize.x / (2 * cameraRectAspect);
			case ScalingMode.AspectFill:
				if (cameraRectAspect >= targetRectAspect) return boundingBoxSize.x / (2 * cameraRectAspect);
				else return boundingBoxSize.y / 2;
			default:
				Debug.LogWarning($"Unhandled scaling mode {scalingMode}");
				return 1;
		}
	}

	/// <summary>
	/// Focuses on bounds
	/// </summary>
	/// <param name="camera">Cmera.</param>
	/// <param name="go">Go.</param>
	public static void FocusOnBounds(this Camera camera, Bounds bounds) {
		// Get the radius of a sphere circumscribing the bounds
		float radius = bounds.size.magnitude / 2f;
		// Use the smaller FOV as it limits what would get cut off by the frustum        
		float fov = Mathf.Min(camera.fieldOfView, camera.GetHorizontalFieldOfView());
		float dist = radius / (Mathf.Sin(fov * 0.5f * Mathf.Deg2Rad));
		
		camera.transform.localPosition = new Vector3(camera.transform.localPosition.x, camera.transform.localPosition.y, dist);
		if (camera.orthographic)
			camera.orthographicSize = radius;
		
		// Frame the object hierarchy
		camera.transform.LookAt(bounds.center);
	}

	/// <summary>
	/// Gets the horizontal field of view of a camera.
	/// </summary>
	/// <returns>The horizontal field of view.</returns>
	/// <param name="camera">Camera.</param>
	public static float GetHorizontalFieldOfView (this Camera camera) {
		return GetHorizontalFieldOfView(camera.fieldOfView, camera.aspect);
	}

	/// <summary>
	/// Gets the frustrum height at a distance.
	/// </summary>
	/// <returns>The frustrum height at a distance.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="distance">Distance.</param>
	public static float GetFrustrumHeightAtDistance (this Camera camera, float distance) {
		return GetFrustrumHeightAtDistance(distance, camera.fieldOfView);
	}

	/// <summary>
	/// Gets the frustrum width at a distance.
	/// </summary>
	/// <returns>The frustrum width at a distance.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="distance">Distance.</param>
	public static float GetFrustrumWidthAtDistance (this Camera camera, float distance) {
		return GetFrustrumWidthAtDistance(distance, camera.fieldOfView, camera.aspect);
	}

	/// <summary>
	/// Gets the distance from the camera where frustrum height equals the input value.
	/// </summary>
	/// <returns>The distance at frustrum height.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="frustumHeight">Frustum height.</param>
	public static float GetDistanceAtFrustrumHeight (this Camera camera, float frustumHeight) {
		return GetDistanceAtFrustrumHeight(frustumHeight, camera.fieldOfView);
	}

	/// <summary>
	/// Gets the distance from the camera where frustrum width equals the input value.
	/// </summary>
	/// <returns>The distance at frustrum width.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="frustumWidth">Frustum width.</param>
	public static float GetDistanceAtFrustrumWidth (this Camera camera, float frustumWidth) {
		return GetDistanceAtFrustrumWidth(frustumWidth, camera.fieldOfView, camera.aspect);
	}

	/// <summary>
	/// Calculates the FOV angle at a specified width and distance.
	/// </summary>
	/// <returns>The FOV angle at width and distance.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="frustumWidth">Frustum width.</param>
	/// <param name="distance">Distance.</param>
	public static float GetFOVAngleAtWidthAndDistance (this Camera camera, float frustumWidth, float distance) {
		return GetFOVAngleAtWidthAndDistance(frustumWidth, distance, camera.aspect);
	}

	/// <summary>
	/// Get the frustrum height from a given width using the camera aspect ratio.
	/// </summary>
	/// <returns>The frustum width to frustum height.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="frustumWidth">Frustum width.</param>
	public static float ConvertFrustumWidthToFrustumHeight (this Camera camera, float frustumWidth) {
		return ConvertFrustumWidthToFrustumHeight(frustumWidth, camera.aspect);
	}

	/// <summary>
	/// Get the frustrum width from a given height using the camera aspect ratio.
	/// </summary>
	/// <returns>The frustum height to frustum width.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="frustumHeight">Frustum height.</param>
	public static float ConvertFrustumHeightToFrustumWidth (this Camera camera, float frustumHeight) {
		return ConvertFrustumHeightToFrustumWidth(frustumHeight, camera.aspect);
	}
	
	
	#endregion
	
	
	
	
	#region Camera Helper Functions
	
	/// <summary>
	/// Gets the horizontal field of view of a camera.
	/// </summary>
	/// <returns>The horizontal field of view.</returns>
	/// <param name="camera">Camera.</param>
	public static float GetHorizontalFieldOfView (float verticalFieldOfView, float aspectRatio) {
		return 2f * Mathf.Atan(Mathf.Tan(verticalFieldOfView * Mathf.Deg2Rad * 0.5f) * aspectRatio) * Mathf.Rad2Deg;
	}


	/// <summary>
	/// Gets the frustrum height at a distance.
	/// </summary>
	/// <returns>The frustrum height at a distance.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="distance">Distance.</param>
	public static float GetFrustrumHeightAtDistance (float distance, float fieldOfView) {
		return 2f * distance * Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
	}

	/// <summary>
	/// Gets the frustrum width at a distance.
	/// </summary>
	/// <returns>The frustrum width at a distance.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="distance">Distance.</param>
	public static float GetFrustrumWidthAtDistance (float distance, float fieldOfView, float aspectRatio) {
		return ConvertFrustumHeightToFrustumWidth(GetFrustrumHeightAtDistance(distance, fieldOfView), aspectRatio);
	}

	
	public static float GetDistanceAtFrustrumHeight (float frustumHeight, float fieldOfView) {
		return frustumHeight * 0.5f / Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
	}
	
	public static float GetDistanceAtFrustrumWidth (float frustumWidth, float fieldOfView, float aspectRatio) {
		return GetDistanceAtFrustrumHeight(ConvertFrustumWidthToFrustumHeight(frustumWidth, aspectRatio), fieldOfView);
	}
	
	
	public static float GetFOVAngleAtHeightAndDistance (float frustumHeight, float distance) {
		return 2f * Mathf.Atan(frustumHeight * 0.5f / distance) * Mathf.Rad2Deg;
	}
	
	public static float GetFOVAngleAtWidthAndDistance (float frustumWidth, float distance, float aspectRatio) {
		return GetFOVAngleAtHeightAndDistance(ConvertFrustumWidthToFrustumHeight(frustumWidth, aspectRatio), distance);
	}
	
	
	public static float ConvertFrustumWidthToFrustumHeight (float frustumWidth, float aspectRatio) {
		return frustumWidth / aspectRatio;
	}
	
	public static float ConvertFrustumHeightToFrustumWidth (float frustumHeight, float aspectRatio) {
		return frustumHeight * aspectRatio;
	}

	/// <summary>
	/// Similar to Camera's WorldToViewport, except that when a position is offscreen (including behind the camera),
	/// it clamps to the edge. Additionally, you can have the position clamp to the middle of the screen in Y to prevent
	/// positions going off the top and bottom of the screen (useful for primarily XZ based games). A margin allows you
	/// to clamp to a particular position just off or on screen. A positive margin is inside the screen, negative is outside.
	/// </summary>
	public static Vector3 WorldToViewportPointClamped(this Camera camera, Vector3 worldPosition, bool allowOffscreenY = true, float margin = -0.05f) {
		Debug.Assert(camera != null, "Camera is null!");
		var viewportPos = camera.WorldToViewportPoint(worldPosition);

		var viewportWithMargin = new Rect(margin, margin, 1.0f - 2*margin, 1.0f - 2*margin);

		// XY is inverted behind camera - invert back
		if( viewportPos.z < 0.0f ) {
			viewportPos.x = 1.0f-viewportPos.x;
			viewportPos.y = 1.0f-viewportPos.y;
		} 
			
		// Clamp vector to margin region.
		//  - In front of camera: only clamp when outside
		//  - Behind camera: ALWAYS clamp direct onto edge of viewport
		// Behind camera? Push to screen edge - clamp outwards.
		var rectCentre = new Vector2(0.5f, 0.5f);
		if( !viewportWithMargin.Contains(new Vector2(viewportPos.x, viewportPos.y)) || viewportPos.z < 0.0f ) {
			var posFromViewportCentre = new Vector2(viewportPos.x, viewportPos.y) - rectCentre;
			if( !allowOffscreenY ) posFromViewportCentre.y = 0.0f;
			var viewportPosXY = SplatVector(viewportWithMargin, posFromViewportCentre);
			viewportPos.x = viewportPosXY.x;
			viewportPos.y = viewportPosXY.y;
		}

		// Splats the vector in the rect.
		// Returns the point on the edge of the rect as if you'd fired a ray from the center in the vector direction
		Vector2 SplatVector(Rect rect, Vector2 vector)  {
			// Degenerate cases
			if( vector == Vector2.zero)
				return rect.center;

			float vecAspect = Mathf.Abs(vector.x / vector.y);
			float rectAspect = rect.size.x / rect.size.y;

			// Clamp to sides
			float scale;
			if( vecAspect > rectAspect ) {
				scale = Mathf.Abs((0.5f*rect.size.x) / vector.x);
			} 

			// Clamp to top/bottom
			else {
				scale = Mathf.Abs((0.5f*rect.size.y) / vector.y);
			}

			return (scale * vector) + rect.center;
		}

		return viewportPos;
	}
	
	#endregion
}
