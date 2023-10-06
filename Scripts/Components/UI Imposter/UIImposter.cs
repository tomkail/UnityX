using UnityEngine;
using UnityEngine.UI;

// Renders a group of UI elements to a rendertexture. Works by moving the UI elements into a special area, rendering it, and returning the UI elements to where they were before.
[ExecuteAlways]
public class UIImposter : RawImage {
    public RectTransform target;
    [PreviewTexture(100)]
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
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, renderTexture.width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, renderTexture.height);
    }

    void OnDrawGizmos () {
        if(outputParams.sizeMode == UIImposterOutputParams.SizeMode.CustomSize) {
            var c = Gizmos.color;
            Gizmos.color = Color.white.WithAlpha(0.25f);
            
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