using UnityEngine;
using System.Collections;

public class UIMonoBehaviour : MonoBehaviour {
	public Canvas canvas {
		get {
			return transform.GetComponentInParent<Canvas>();
		}
	}

	public Canvas rootCanvas {
		get {
			var c = canvas;
			return c == null ? null : c.rootCanvas;
		}
	}

	public RectTransform rectTransform {
		get {
			return (RectTransform) transform;
		}
	}
}
