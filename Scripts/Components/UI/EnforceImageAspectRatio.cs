using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Uses an AspectRatioFitter to enforce the image aspect ratio
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Image), typeof(AspectRatioFitter))]
public class EnforceImageAspectRatio : MonoBehaviour {

	private AspectRatioFitter _aspectRatioFitter;
	public AspectRatioFitter aspectRatioFitter {
		get {
			if(_aspectRatioFitter == null) _aspectRatioFitter = GetComponent<AspectRatioFitter>();
			return _aspectRatioFitter;
		}
	}

	private Image __image;
	private Image _image {
		get {
			if(__image == null) __image = GetComponent<Image>();
			return __image;
		}
	}

	private Vector2 _lastSize;

	void Update () {
		if(_image.sprite == null) return;
		if(_lastSize == _image.sprite.rect.size) return;
		aspectRatioFitter.aspectRatio = (float)_image.sprite.rect.width/_image.sprite.rect.height;
		_lastSize = _image.sprite.rect.size;
	}
}