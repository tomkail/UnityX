using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Screenshot album.
/// A collection of screenshots.
/// </summary>
public class ScreenshotAlbum {
	
	public List<Texture2D> screenshots = new List<Texture2D>();
	
	public delegate void OnAddScreenshotEvent (Texture2D screenshot);
	public event OnAddScreenshotEvent OnAddScreenshot;
	
	public delegate void OnRemoveScreenshotEvent (Texture2D screenshot);
	public event OnRemoveScreenshotEvent OnRemoveScreenshot;
	
	public void AddScreenshot (Texture2D screenshot) {
		screenshots.Add(screenshot);
		if(OnAddScreenshot != null)
			OnAddScreenshot(screenshot);
	}
	
	public bool RemoveScreenshot (Texture2D screenshot) {
		bool successful = screenshots.Remove(screenshot);
		if(successful) {
			if(OnRemoveScreenshot != null)
				OnRemoveScreenshot(screenshot);
		}
		return successful;
	}
}