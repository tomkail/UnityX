using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public static class CanvasX {
	public static RectTransform GetRectTransform (this Canvas canvas) {
		Debug.Assert(canvas != null, "Canvas is null!");
		return canvas.transform as RectTransform;
	}

	// Forces the canvas to refresh position from camera position (ARGH why isn't this handled internally?!)
	// This is necessary when converting between screen and canvas camera space when the canvas's camera has moved that frame, or you'll be a frame behind
	public static void RefreshPosition (this Canvas canvas) {
		if(canvas.worldCamera == null) return;
        canvas.enabled = false;
        canvas.enabled = true;
	}

	private static void GetCameraFromCanvas (Canvas canvas, ref Camera camera) {
		if(canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) {
			camera = null;
		} else if(canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceCamera) {
			camera = canvas.rootCanvas.worldCamera;
		} else if(canvas.rootCanvas.renderMode == RenderMode.WorldSpace && camera == null) {
			if(canvas.rootCanvas.worldCamera != null)
				camera = canvas.rootCanvas.worldCamera;
			else
				Debug.LogError("Canvas is in world space, but camera is null and no event camera exists on canvas.");
		}
	}

	// "Canvas Space" is space local to a child of a canvas. 0,0 is in the bottom left.
	// Converts screen position to the canvas position, allowing for a specified camera to be chosen for world space canvases.
	public static Vector2 ScreenToCanvasPoint(this Canvas canvas, Vector2 screenPoint, Camera camera = null) {
		GetCameraFromCanvas(canvas, ref camera);
        var rectTransform = canvas.GetComponent<RectTransform>();
		
		Vector2 canvasSpace = Vector3.zero;
		if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera, out canvasSpace))
			return canvasSpace;// + 0.5f * rectTransform.rect.size;
		else return Vector2.zero;
	}

	static Vector3[] corners = new Vector3[4];
	public static Rect CanvasToScreenRect (this Canvas canvas, RectTransform rectTransform) {
		rectTransform.GetWorldCorners(corners);
		return RectX.MinMaxRect(canvas.WorldToScreenPoint(corners[0]), canvas.WorldToScreenPoint(corners[2]));
	}
	public static Vector2 CanvasToScreenPoint (this Canvas canvas, Vector3 canvasPoint) {
		return canvas.WorldToScreenPoint(canvas.transform.TransformPoint(canvasPoint));
	}

	public static Vector2 CanvasToScreenVector (this Canvas canvas, Vector3 vector) {
		return canvas.WorldToScreenVector(canvas.transform.TransformPoint(vector));
	}


	public static Rect CanvasToViewportRect (this Canvas canvas, RectTransform rectTransform) {
		return ScreenX.ScreenToViewportRect(canvas.CanvasToScreenRect(rectTransform));
	}

	public static Vector2 CanvasToViewportPoint (this Canvas canvas, Vector3 canvasPoint) {
		var screenPoint = canvas.CanvasToScreenPoint(canvasPoint);
		return ScreenX.ScreenToViewportPoint(screenPoint);
	}

	public static Vector2 CanvasToViewportVector (this Canvas canvas, Vector3 vector) {
		var screenPoint = canvas.CanvasToScreenVector(vector);
		return ScreenX.ScreenToViewportPoint(screenPoint);
	}




	public static Vector3 WorldToCanvasPoint(this Canvas canvas, Vector3 position, Camera camera = null) {
		GetCameraFromCanvas(canvas, ref camera);
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, position);
		return ScreenToCanvasPoint(canvas, screenPoint, camera);
	}

	public static Vector2 WorldToScreenPoint(this Canvas canvas, Vector3 position, Camera camera = null) {
		GetCameraFromCanvas(canvas, ref camera);
		return RectTransformUtility.WorldToScreenPoint(camera, position);
	}
	public static Vector2 WorldToScreenVector(this Canvas canvas, Vector3 vector, Camera camera = null) {
		return canvas.WorldToScreenPoint(vector) - canvas.WorldToScreenPoint(canvas.transform.TransformPoint(Vector3.zero));
	}


	/// <summary>
	/// Converts a point in world space to a point in canvas space by converting from world space to screen space using a specified camera. 
	/// </summary>
	/// <returns>The point to local point in rectangle.</returns>
	/// <param name="canvas">Canvas.</param>
	/// <param name="camera">Camera.</param>
	/// <param name="worldPosition">World position.</param>
	public static Vector3? WorldPointToLocalPointInRectangle (this Canvas canvas, Camera camera, RectTransform rectTransform, Vector3 worldPosition) {
		Vector3 screenPoint = camera.WorldToScreenPoint(worldPosition);
		if (screenPoint.z < 0) return null;
		return canvas.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint);
	}
	public static Vector3? WorldPointToLocalPointInRectangle (this Canvas canvas, Camera camera, Vector3 worldPosition) {
		return canvas.WorldPointToLocalPointInRectangle(camera, canvas.GetComponent<RectTransform>(), worldPosition);
	}

	public static Vector3? ScreenPointToLocalPointInRectangle (this Canvas canvas, RectTransform rectTransform, Vector2 screenPoint) {
		Camera camera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
		Vector2 localPosition;
		if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera, out localPosition))
			return localPosition;
		else return null;
	}
	
	public static Vector3? ScreenPointToLocalPointInRectangle (this Canvas canvas, Vector2 screenPoint) {
		return canvas.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPoint);
	}
	
	
	public static Vector3? ScreenPointToCanvasSpace(this Canvas canvas, Vector2 screenPoint) {
		Camera camera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
		Vector3 canvasSpace = Vector3.zero;
        var rectTransform = canvas.GetComponent<RectTransform>();
		if(RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, camera, out canvasSpace))
			return canvasSpace;
		else return null;
	}


    // Taken from https://github.com/Unity-Technologies/uGUI/blob/2019.1/UnityEngine.UI/UI/Core/Selectable.cs
    private static readonly List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();
    public static bool CanvasGroupsAllowInteraction (GameObject gameObject) {
        // Figure out if parent groups allow interaction
        // If no interaction is alowed... then we need
        // to not do that :)
        var groupAllowInteraction = true;
        Transform t = gameObject.transform;
        while (t != null)
        {
            t.GetComponents(m_CanvasGroupCache);
            bool shouldBreak = false;
            for (var i = 0; i < m_CanvasGroupCache.Count; i++)
            {
                // if the parent group does not allow interaction
                // we need to break
                if (!m_CanvasGroupCache[i].interactable)
                {
                    groupAllowInteraction = false;
                    shouldBreak = true;
                }
                // if this is a 'fresh' group, then break
                // as we should not consider parents
                if (m_CanvasGroupCache[i].ignoreParentGroups)
                    shouldBreak = true;
            }
            if (shouldBreak)
                break;

            t = t.parent;
        }
        return groupAllowInteraction;
    }

    // Untested
    public static float CanvasGroupsAlpha (GameObject gameObject) {
        var groupAlpha = 1f;
        Transform t = gameObject.transform;
        while (t != null) {
            t.GetComponents(m_CanvasGroupCache);
            bool shouldBreak = false;
            for (var i = 0; i < m_CanvasGroupCache.Count; i++)
            {
                groupAlpha *= m_CanvasGroupCache[i].alpha;
                
                // if this is a 'fresh' group, then break
                // as we should not consider parents
                if (m_CanvasGroupCache[i].ignoreParentGroups)
                    shouldBreak = true;
            }
            if (shouldBreak)
                break;

            t = t.parent;
        }
        return groupAlpha;
    }
}