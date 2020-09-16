using UnityEngine;
using System.Collections;

public class UIMonoBehaviour : MonoBehaviour {
	public Canvas canvas {
		get {
			return transform.GetComponentInAncestors<Canvas>();
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
