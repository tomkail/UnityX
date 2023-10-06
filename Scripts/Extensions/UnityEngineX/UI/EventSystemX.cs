using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class EventSystemX {
	static List<RaycastResult> staticResults;
	public static RaycastResult Raycast (Vector2 screenPos) {
		RaycastAllNonAlloc (screenPos, ref staticResults);
		return staticResults.FirstOrDefault();
	}
	
	public static RaycastResult Raycast (Vector2 screenPos, LayerMask layerMask) {
		return RaycastAll (screenPos, layerMask).FirstOrDefault();
	}
	
	public static bool Raycast (Vector2 screenPos, out RaycastResult raycastResult) {
		raycastResult = Raycast(screenPos);
		return raycastResult.isValid;
	}
	
	public static bool Raycast (Vector2 screenPos, LayerMask layerMask, out RaycastResult raycastResult) {
		raycastResult = Raycast (screenPos, layerMask);
		return raycastResult.isValid;
	}

	// Returns true if the graphic was hit
	public static bool Raycast (Vector2 screenPos, Graphic graphic) {
		return RaycastAll(screenPos).Any(x => x.gameObject == graphic.gameObject);
	}

	// Returns true if the graphic was hit
	public static bool Raycast (Vector2 screenPos, Graphic graphic, out RaycastResult raycastResult) {
		var results = RaycastAll(screenPos);
		int index = results.IndexOf(x => x.gameObject == graphic.gameObject);
		if(index == -1) {
			raycastResult = new RaycastResult();
			return false;
		} else {
			raycastResult = results[index];
			return true;
		}
	}
	
	/// <summary>
	/// Performs a raycast through the entire scene including the UI. If no UI element is hit then elements
	/// in the 3D scene will be tested.
	/// This is expensive! 
	/// </summary>
	/// <returns>A list of elements we hit. Closest hit first.</returns>
	/// <param name="screenPos">The screen position at which the cast should be performed.</param>
	public static List<RaycastResult> RaycastAll (Vector2 screenPos) {
		if(EventSystem.current == null) {
			Debug.LogWarning("Tried to raycast into the event system, but no event system exists.");
			return null;
		}

		PointerEventData eventData = new PointerEventData (null);
		eventData.position = screenPos;
		List<RaycastResult> hits = new List<RaycastResult> ();
		EventSystem.current.RaycastAll (eventData, hits);
		return hits;
	}
	public static void RaycastAllNonAlloc (Vector2 screenPos, ref List<RaycastResult> hits) {
		if(hits == null) hits = new List<RaycastResult> ();
		if(EventSystem.current == null) {
			Debug.LogWarning("Tried to raycast into the event system, but no event system exists.");
			hits.Clear();
			return;
		}

		PointerEventData eventData = new PointerEventData (null);
		eventData.position = screenPos;
		EventSystem.current.RaycastAll (eventData, hits);
	}
	
	/// <summary>
	/// Performs a raycast through the entire scene including the UI. If no UI element is hit then elements
	/// in the 3D scene will be tested.
	/// </summary>
	/// <returns>A list of elements we hit. Closest hit first.</returns>
	/// <param name="screenPos">The screen position at which the cast should be performed.</param>
	public static List<RaycastResult> RaycastAll (Vector2 screenPos, LayerMask layerMask) {
		if(EventSystem.current == null) {
			Debug.LogWarning("Tried to raycast into the event system, but no event system exists.");
			return null;
		}
		
		PointerEventData eventData = new PointerEventData (EventSystem.current);
		eventData.position = screenPos;
		return EventSystem.current.RaycastAll (eventData, layerMask);
	}
	
	/// <summary>
	/// Raycast into the scene using all configured BaseRaycasters, using a layer mask.
	/// </summary>
	/// <param name="eventSystem">Event system.</param>
	/// <param name="eventData">Event data.</param>
	/// <param name="layerMask">Layer mask.</param>
	/// <param name="raycastResults">Raycast results.</param>
	public static List<RaycastResult> RaycastAll(this EventSystem eventSystem, PointerEventData eventData, LayerMask layerMask) {
		List<RaycastResult> hits = new List<RaycastResult> ();
		eventSystem.RaycastAll (eventData, hits);
		return hits.Where(hit => layerMask.Includes(hit.gameObject.layer)).ToList();
	}



	// HACKSSSSS! THIS IS PROBABLY NOT HOW YOU'RE MEANT TO USE THIS BUT MEH
	public static void Enter (GameObject focusedOption) {
		var eventData = new PointerEventData(null);
        eventData.button = PointerEventData.InputButton.Left;
        if(focusedOption == null) {
            eventData.hovered = new List<GameObject>() {};
            ExecuteEvents.ExecuteHierarchy(null, eventData, ExecuteEvents.pointerEnterHandler);
        } else {
            eventData.hovered = new List<GameObject>() {focusedOption};
			ExecuteEvents.ExecuteHierarchy(focusedOption, eventData, ExecuteEvents.pointerEnterHandler);
        }
	}

	public static void Exit (GameObject focusedOption) {
		var eventData = new PointerEventData(null);
        eventData.button = PointerEventData.InputButton.Left;
        if(focusedOption == null) {
            eventData.hovered = new List<GameObject>() {};
            ExecuteEvents.ExecuteHierarchy(null, eventData, ExecuteEvents.pointerExitHandler);
        } else {
            eventData.hovered = new List<GameObject>() {focusedOption};
			ExecuteEvents.ExecuteHierarchy(focusedOption, eventData, ExecuteEvents.pointerExitHandler);
        }
	}
    
	public static void Click (GameObject focusedOption) {
		var eventData = new PointerEventData(null);
        eventData.eligibleForClick = true;
        eventData.button = PointerEventData.InputButton.Left;
        if(focusedOption == null) {
            eventData.hovered = new List<GameObject>() {};
            ExecuteEvents.ExecuteHierarchy(null, eventData, ExecuteEvents.pointerClickHandler);
        } else {
            eventData.hovered = new List<GameObject>() {focusedOption};
            ExecuteEvents.ExecuteHierarchy(focusedOption, eventData, ExecuteEvents.pointerClickHandler);
        }
	}
}
