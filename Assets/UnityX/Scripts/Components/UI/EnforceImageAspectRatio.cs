#define AVPRO_PACKAGE_UNITYUI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
	[FormerlySerializedAs("__graphic")] [SerializeField]
	private Graphic _graphic;
	private Graphic graphic {
		get {
			if(_graphic == null) _graphic = GetComponent<Graphic>();
			return _graphic;
		}
	}

	private float lastAspectRatio;
	
	void Update () {
		if (graphic is Image) {
			var image = graphic as Image;
			if(image.sprite == null) return;
			var newAspectRatio = (float) (image.sprite.rect.width / image.sprite.rect.height);
			if(lastAspectRatio == newAspectRatio) return;
			lastAspectRatio = newAspectRatio;
			aspectRatioFitter.aspectRatio = newAspectRatio;
		} else if (graphic is RawImage) {
			var image = graphic as RawImage;
			if(image.texture == null) return;
			var newAspectRatio = ((float) image.texture.width / image.texture.height);
			if(lastAspectRatio == newAspectRatio) return;
			lastAspectRatio = newAspectRatio;
			aspectRatioFitter.aspectRatio = newAspectRatio;
		}
	}
}