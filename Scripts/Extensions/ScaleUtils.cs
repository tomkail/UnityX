using UnityEngine;

public static class ScaleUtils {
	
	public static Vector2 ScaleContentToContainer (Vector2 container, Vector2 content, ScaleMode scaleMode) {
		if(scaleMode == ScaleMode.StretchToFill) {
			return container;
		} else if(scaleMode == ScaleMode.ScaleAndCrop) {
			float widthScale = container.x / content.x;
			float heightScale = container.y / content.y;
			float scale = Mathf.Max(widthScale, heightScale);
			return new Vector2(content.x * scale, content.y * scale);
		} else if(scaleMode == ScaleMode.ScaleToFit) {
			float widthScale = container.x / content.x;
			float heightScale = container.y / content.y;
			float scale = Mathf.Min(widthScale, heightScale);
			return new Vector2(content.x * scale, content.y * scale);
		}
		return Vector2.zero;
	}

	public static Rect ScaleContentToContainer (Vector2 containerSize, Rect content, ScaleMode scaleMode) {
		Vector2 newContentSize = ScaleContentToContainer(containerSize, content.size, scaleMode);
		Vector2 offset = (newContentSize-content.size) * 0.5f;
		content.position += offset;
		Debug.Log(offset);
		return content;
	}
}
