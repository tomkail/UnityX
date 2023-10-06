using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Screenshot saver. Allows developers or players to create screenshots.
/// </summary>
public class ScreenshotCapturer {
	
	public class ScreenshotCapturerProperties {
		public int width;
		public int height;
		public IList<Camera> cameras;
		public ScreenshotSaverTextureFormat textureFormat = ScreenshotSaverTextureFormat.RGB24;
		public System.Action<Texture2D> onComplete;

		public ScreenshotCapturerProperties (int width, int height) {
			this.width = width;
			this.height = height;
			this.cameras = Camera.allCameras;
		}

		public ScreenshotCapturerProperties (int width, int height, IList<Camera> cameras) {
			this.width = width;
			this.height = height;
			this.cameras = cameras;
		}

		public ScreenshotCapturerProperties (int width, int height, ScreenshotSaverTextureFormat textureFormat) {
			this.width = width;
			this.height = height;
			this.textureFormat = textureFormat;
			this.cameras = Camera.allCameras;
		}

		public ScreenshotCapturerProperties (int width, int height, IList<Camera> cameras, ScreenshotSaverTextureFormat textureFormat) {
			this.width = width;
			this.height = height;
			this.cameras = cameras;
			this.textureFormat = textureFormat;
		}

		public ScreenshotCapturerProperties (int width, int height, System.Action<Texture2D> onComplete) {
			this.width = width;
			this.height = height;
			this.cameras = Camera.allCameras;
		}

		public ScreenshotCapturerProperties (int width, int height, IList<Camera> cameras, System.Action<Texture2D> onComplete) {
			this.width = width;
			this.height = height;
			this.cameras = cameras;
			this.onComplete = onComplete;
		}

		public ScreenshotCapturerProperties (int width, int height, ScreenshotSaverTextureFormat textureFormat, System.Action<Texture2D> onComplete) {
			this.width = width;
			this.height = height;
			this.cameras = Camera.allCameras;
			this.textureFormat = textureFormat;
			this.onComplete = onComplete;
		}

		public ScreenshotCapturerProperties (int width, int height, IList<Camera> cameras, ScreenshotSaverTextureFormat textureFormat, System.Action<Texture2D> onComplete) {
			this.width = width;
			this.height = height;
			this.cameras = cameras;
			this.textureFormat = textureFormat;
			this.onComplete = onComplete;
		}
	}

	private static bool _capturingScreenshot;
	public static bool capturingScreenshot { 
		get { 
			return _capturingScreenshot;
		} private set {
			_capturingScreenshot = value;
		}
	}
	
	public delegate void OnCompleteScreenshotCaptureEvent (Texture2D screenshot);
	public static event OnCompleteScreenshotCaptureEvent OnCompleteScreenshotCapture;
	
	public static void CaptureScreenshot(ScreenshotCapturerProperties properties) {
		CoroutineHelper.Execute(CaptureScreenshotCoroutine(properties));
	}
	
	struct SavedCameraProperties {
		public bool enabled;
		public RenderTexture targetTexture;

		public SavedCameraProperties (Camera camera) {
			enabled = camera.enabled;
			targetTexture = camera.targetTexture;
		}

		public void ApplyTo (Camera camera) {
			camera.enabled = enabled;
			camera.targetTexture = targetTexture;
		}
	}
	public static IEnumerator CaptureScreenshotCoroutine (ScreenshotCapturerProperties properties) {
        List<Camera> _cameras = properties.cameras.ToList();
		_cameras.RemoveNull();

		if(capturingScreenshot || properties.width <= 0 || properties.height <= 0 || _cameras.IsNullOrEmpty()) {
			if(capturingScreenshot) Debug.LogError("Could not capture screenshot because a screenshot is currently being captured by the same ScreenshotSaver.");
			else if(properties.width <= 0) Debug.LogError("Could not capture screenshot because width is "+properties.width+".");
			else if(properties.height <= 0) Debug.LogError("Could not capture screenshot because height is "+properties.height+".");
			else if(_cameras.IsNullOrEmpty()) Debug.LogError("Could not capture screenshot because camera array contains no cameras.");
			yield break;
		}

        capturingScreenshot = true;

		yield return new WaitForEndOfFrame();
		
		RenderTexture rt = new RenderTexture(properties.width, properties.height, 24, RenderTextureFormat.ARGB32);
		
		Canvas.ForceUpdateCanvases();

		SavedCameraProperties[] savedProperties = new SavedCameraProperties[_cameras.Count];
		for (int i = 0; i < _cameras.Count; i++) {
			Camera cam = _cameras [i];
			savedProperties[i] = new SavedCameraProperties(cam);
			if (!savedProperties[i].enabled)
				continue;
			cam.targetTexture = rt;
			cam.enabled = false;
			cam.Render ();
			cam.enabled = true;
			cam.targetTexture = null;
		}

		for (int i = 0; i < _cameras.Count; i++) {
			savedProperties[i].ApplyTo(_cameras[i]);
		}

		RenderTexture savedActiveRenderTexture = RenderTexture.active;
		RenderTexture.active = rt;
		
		Texture2D screenshot = new Texture2D(properties.width, properties.height, ScreenshotSaverTextureFormatUtils.ToTextureFormat(properties.textureFormat), false);
		screenshot.ReadPixels(new Rect(0, 0, properties.width, properties.height), 0, 0);

		RenderTexture.active = savedActiveRenderTexture;

		rt.Release();
		MonoBehaviour.Destroy(rt);
		rt = null;

		capturingScreenshot = false;
        
		if(properties.onComplete != null) properties.onComplete(screenshot);
		if(OnCompleteScreenshotCapture != null) {
			OnCompleteScreenshotCapture(screenshot);
		}
	}
}