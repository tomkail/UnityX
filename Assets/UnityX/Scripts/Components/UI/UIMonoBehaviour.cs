using UnityEngine;

public class UIMonoBehaviour : MonoBehaviour {
	public Canvas canvas {
		get {
			return transform.GetComponentInParent<Canvas>();
		}
	}

	public Canvas rootCanvas {
		get {
			return canvas.rootCanvas;
		}
	}

	public RectTransform rectTransform {
		get {
			return (RectTransform) transform;
		}
	}
}
