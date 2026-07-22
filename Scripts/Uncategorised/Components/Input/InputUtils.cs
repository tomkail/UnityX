using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputUtils : MonoBehaviour {
    public static Vector2 clampedInputPosition => new Vector2(Mathf.Clamp(Input.mousePosition.x, 0, Screen.width), Mathf.Clamp(Input.mousePosition.y, 0, Screen.height));
    public static bool HoveringOverUI (Vector2 screenPos) {
        #if UNITY_IOS && !UNITY_EDITOR
            return UnityEngine.EventSystems.EventSystem.current != null && Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        #endif
        if(GUIUtility.hotControl != 0) 
            return true;
        if(UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return true;
        // EventSystemX.Raycast(screenPos).isValid;
        return false;
    }
}