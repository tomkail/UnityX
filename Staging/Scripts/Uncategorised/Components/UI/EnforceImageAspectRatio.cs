using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Uses an AspectRatioFitter to enforce the image aspect ratio
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(AspectRatioFitter))]
public class EnforceImageAspectRatio : MonoBehaviour {
	AspectRatioFitter _aspectRatioFitter;
	public AspectRatioFitter aspectRatioFitter {
		get {
			if(_aspectRatioFitter == null) _aspectRatioFitter = GetComponent<AspectRatioFitter>();
			return _aspectRatioFitter;
		}
	}
	[SerializeField]
	Graphic _graphic;
	Graphic graphic {
		get {
			if(_graphic == null) _graphic = GetComponent<Graphic>();
			return _graphic;
		}
	}

	void Update () {
		var newAspectRatio = aspectRatioFitter.aspectRatio;
		if (graphic is Image image) {
			if(image.sprite == null) return;
			newAspectRatio = image.sprite.rect.width / image.sprite.rect.height;
		} else {
			if(graphic.mainTexture == null) return;
			newAspectRatio = (float) graphic.mainTexture.width / graphic.mainTexture.height;
		}
		if(aspectRatioFitter.aspectRatio == newAspectRatio) return;
		aspectRatioFitter.aspectRatio = newAspectRatio;
	}
}