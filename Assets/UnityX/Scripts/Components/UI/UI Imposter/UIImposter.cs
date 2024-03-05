using UnityEngine;
using UnityEngine.UI;

// Made by @tomkail
// Renders a group of UI elements to a rendertexture. Works by moving the UI elements into a special area, rendering it, and returning the UI elements to where they were before.
[ExecuteAlways]
public class UIImposter : RawImage {
    public RectTransform target;
    public RenderTexture renderTexture;
    public UIImposterOutputParams outputParams;

    public UpdateMode updateMode;
    public enum UpdateMode {
        Manual,
        Render,
        RenderAndResize
    }
    
    void Update () {
        if(updateMode == UpdateMode.Render || updateMode == UpdateMode.RenderAndResize) Render();
        if(updateMode == UpdateMode.RenderAndResize) ResizeToFit();
    }

    public void Render () {
        UIImposterRenderer.Render(target, outputParams, ref renderTexture);
        texture = renderTexture;
    }
    public void ResizeToFit () {
        if (outputParams.sizeMode == UIImposterOutputParams.SizeMode.OriginalScreenSize) {
            var scaleFactor = CanvasToScreenVector(canvas, Vector2.one);
            scaleFactor = new Vector2(1f / scaleFactor.x, 1f / scaleFactor.y);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, renderTexture.width * scaleFactor.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, renderTexture.height * scaleFactor.y);
        } else {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, renderTexture.width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, renderTexture.height);
        }
        Vector2 CanvasToScreenVector (Canvas canvas, Vector3 vector) {
            Camera camera = null;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && (canvas.renderMode != RenderMode.ScreenSpaceCamera || canvas.worldCamera != null))
                camera = canvas.worldCamera;
            return RectTransformUtility.WorldToScreenPoint(camera, canvas.transform.TransformPoint(vector)) - RectTransformUtility.WorldToScreenPoint(camera, canvas.transform.TransformPoint(Vector3.zero));
        }
    }

    void OnDrawGizmos () {
        if(outputParams.sizeMode == UIImposterOutputParams.SizeMode.CustomSize) {
            var c = Gizmos.color;
            Gizmos.color = new Color(1,1,1,0.25f);
            
            var pivotOffset = (rectTransform.pivot - Vector2.one * 0.5f) * rectTransform.rect.size;
            var min = rectTransform.TransformPoint(new Vector2(-outputParams.customContainerSize.x * 0.5f, -outputParams.customContainerSize.y * 0.5f) - pivotOffset);
            var max = rectTransform.TransformPoint(new Vector2(outputParams.customContainerSize.x * 0.5f, outputParams.customContainerSize.y * 0.5f) - pivotOffset);
            
            var topLeft = new Vector3(min.x, max.y, min.z);
            var topRight = max;
            var bottomLeft = min;
            var bottomRight = new Vector3(max.x, min.y, min.z);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
            
            Gizmos.color = c;
        }
    }
}