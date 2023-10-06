using UnityEngine;
using System.Collections;

[ExecuteAlways]
[RequireComponent(typeof(Canvas))]
public class CanvasWorldScaler : UIMonoBehaviour {
	
	public Vector2 worldScale;
	public Vector2 pixelScale;
	
	public enum ScaleMode {
		StretchToFill,
		ScaleToFitWorldScale,
		ScaleToFitPixelScale
	}
	public ScaleMode scaleMode;

	protected virtual void Update () {
		Vector2 scaleAspect = Vector2.one;
		if(scaleMode == ScaleMode.ScaleToFitPixelScale) {
			if(pixelScale.x > pixelScale.y) {
				scaleAspect.y = Mathf.Min(pixelScale.x, pixelScale.y)/Mathf.Max(pixelScale.x, pixelScale.y);
			} else {
				scaleAspect.x = Mathf.Min(pixelScale.x, pixelScale.y)/Mathf.Max(pixelScale.x, pixelScale.y);
			}
		} else if(scaleMode == ScaleMode.ScaleToFitWorldScale) {
			if(worldScale.x > worldScale.y) {
				scaleAspect.x = Mathf.Min(worldScale.x, worldScale.y)/Mathf.Max(worldScale.x, worldScale.y);
			} else {
				scaleAspect.y = Mathf.Min(worldScale.x, worldScale.y)/Mathf.Max(worldScale.x, worldScale.y);
			}
		}
	
		rectTransform.SetWidth(pixelScale.x);
		rectTransform.SetHeight(pixelScale.y);
		float localScaleX = (worldScale.x / pixelScale.x) * scaleAspect.x;
		float localScaleY = (worldScale.y / pixelScale.y) * scaleAspect.y;
		rectTransform.localScale = new Vector3(localScaleX, localScaleY, rectTransform.localScale.z);
	}
	
	void OnDrawGizmosSelected () {
		if(!enabled) return;
		if(scaleMode != ScaleMode.StretchToFill) {
			Color savedColor = Gizmos.color;
			Gizmos.color = Color.white;
			GizmosX.DrawWireRect(rectTransform.position, rectTransform.rotation, worldScale);
			Gizmos.color = savedColor;
		}
	}
}
