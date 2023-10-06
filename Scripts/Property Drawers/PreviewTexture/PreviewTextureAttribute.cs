using UnityEngine;

public class PreviewTextureAttribute : PropertyAttribute {

	public ScaleMode scaleMode = ScaleMode.ScaleToFit;
	public int width = 32;
	public int height = 32;

	public PreviewTextureAttribute () {}

	public PreviewTextureAttribute (int size, ScaleMode scaleMode = ScaleMode.ScaleToFit) {
		this.width = size;
		this.height = size;
		this.scaleMode = scaleMode;
	}

	public PreviewTextureAttribute (int width, int height, ScaleMode scaleMode = ScaleMode.ScaleToFit) {
		this.width = width;
		this.height = height;
		this.scaleMode = scaleMode;
	}
}