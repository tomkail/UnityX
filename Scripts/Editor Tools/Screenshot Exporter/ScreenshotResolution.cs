[System.Serializable]
public class ScreenshotResolution {
	public string name;
	public int width;
	public int height;
	
	public ScreenshotResolution (string name, int width, int height) {
		this.name = name;
		this.width = width;
		this.height = height;
	}
}

public class CommonScreenshotResolutions {
	public static ScreenshotResolution Create1024x768 () {
		return new ScreenshotResolution ("1024x768", 1024, 768);
	}
	
	public static ScreenshotResolution Create1280x720 () {
		return new ScreenshotResolution ("1280x720", 1280, 720);
	}
	
	public static ScreenshotResolution Create1600x900 () {
		return new ScreenshotResolution ("1600x900", 1600, 900);
	}
	
	public static ScreenshotResolution Create1920x1080 () {
		return new ScreenshotResolution ("HD (1920x1080)", 1920, 1080);
	}
	
	public static ScreenshotResolution Create3840x2160 () {
		return new ScreenshotResolution ("4k (3840x2160)", 3840, 2160);
	}
	
	public static ScreenshotResolution Create7680x4320 () {
		return new ScreenshotResolution ("8k (7680x4320)", 7680, 4320);
	}
	
	public static ScreenshotResolution CreateiPhone4 () {
		return new ScreenshotResolution ("iPhone4 (640x960)", 640, 960);
	}
	
	public static ScreenshotResolution CreateiPhone5 () {
		return new ScreenshotResolution ("iPhone5 (640x1136)", 640, 1136);
	}
	
	public static ScreenshotResolution CreateiPhone6 () {
		return new ScreenshotResolution ("iPhone6 (750x1334)", 750, 1334);
	}
}