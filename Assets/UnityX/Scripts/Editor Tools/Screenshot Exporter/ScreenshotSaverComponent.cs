using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Screenshot saver component. Used to take screenshots in-game.
/// Unfinished.
/// </summary>
public class ScreenshotSaverComponent : MonoBehaviour {
	
	public KeyCode[] keycodes = new KeyCode[] {KeyCode.F5};
	public float resolutionScaleFactor = 1;
	public ScreenshotExportFormat exportFormat = ScreenshotExportFormat.PNG;
	
	private void OnCaptureScreenshot (Texture2D screenshot) {
		ScreenshotExportSettings exportSettings = null;
		string fileName = string.Format("{0}_{1}", Application.productName, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		exportSettings = new ScreenshotExportSettings(screenshot, Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"/Screenshots", fileName, exportFormat, 0);
		ScreenshotExporter.Export(exportSettings);
	}
	
	private void Update () {
		if(keycodes.IsNullOrEmpty()) return;
		foreach(KeyCode keycode in keycodes) {
			if(Input.GetKeyDown(keycode)) {
				ScreenshotCapturer.CaptureScreenshot(new ScreenshotCapturer.ScreenshotCapturerProperties(Mathf.FloorToInt(Screen.width * resolutionScaleFactor), Mathf.FloorToInt(Screen.height * resolutionScaleFactor)));
			}
		}
	}
	
	private void OnEnable () {
		ScreenshotCapturer.OnCompleteScreenshotCapture += OnCaptureScreenshot;
	}
	
	private void OnDisable () {
		ScreenshotCapturer.OnCompleteScreenshotCapture -= OnCaptureScreenshot;
	}
}
