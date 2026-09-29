using UnityEngine;
using System.Collections;

// Supported texture formats. Mirrors the names in the TextureFormat enum.
public enum ScreenshotSaverTextureFormat {
	ARGB32,
	RGB24
}

public class ScreenshotSaverTextureFormatUtils {
	
	public static TextureFormat ToTextureFormat (ScreenshotSaverTextureFormat format) {
		switch (format) {
		case ScreenshotSaverTextureFormat.ARGB32:
			return TextureFormat.ARGB32;
		case ScreenshotSaverTextureFormat.RGB24:
			return TextureFormat.RGB24;
		default:
			return TextureFormat.RGB24;
		}
	}
	
	
	public static string FormatToInfoMessage (ScreenshotSaverTextureFormat format) {
		switch (format) {
		case ScreenshotSaverTextureFormat.ARGB32:
			return "Allows transparent backgrounds. Never shows camera background color.";
		case ScreenshotSaverTextureFormat.RGB24:
			return "Displays camera background color. Creates glitches when camera clears background.";
		default:
			return "Unrecognized format.";
		}
	}
}