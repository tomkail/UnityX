using UnityEngine;
using System.Collections;

[System.Serializable]
public class MouseInputWheel {
	public bool scrolling;
	public Vector2 totalScroll;
	public Vector2 deltaScroll;

	public delegate void OnMouseScrollEvent(Vector2 deltaScroll);
	public event OnMouseScrollEvent OnMouseScroll;

	public void UpdatePosition(){
		if(Input.mousePosition.x > 0 && Input.mousePosition.x < Screen.width && Input.mousePosition.y > 0 && Input.mousePosition.y < Screen.height)
			deltaScroll = Input.mouseScrollDelta;
		else deltaScroll = Vector2.zero;
		if (deltaScroll != Vector2.zero){
			scrolling = true;
			totalScroll += deltaScroll;
			if(OnMouseScroll != null){
				OnMouseScroll(deltaScroll);
			}
		} else {
			scrolling = false;
		}
	}

	public void ResetInput(){
		scrolling = false;
		deltaScroll = Vector2.zero;
	}
}