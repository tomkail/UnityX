using UnityEngine;

public static class ScaleToContainerUtils {

    // public static Rect GetAxisAlignedBoundingBox (Rect rect, float degrees) {
    //     // rectTransform.GetLocalCorners();
    //     var transformedRect = rect;
        
    //     // var gamma = Mathf.PI / 4;
    //     // var alpha = degrees;
    //     // var beta = gamma - degrees;
    //     // var EA = rect.height * Mathf.Sin(alpha);
    //     // var ED = rect.height * Mathf.Sin(beta);
    //     // var FB = rect.width * Mathf.Sin(alpha);
    //     // var AF = rect.width * Mathf.Sin(beta);

    //     var theta = Mathf.Deg2Rad * degrees;
    //     var x2 = x0+(x-x0)*cos(theta)+(y-y0)*sin(theta);
    //     var y2 = y0-(x-x0)*sin(theta)+(y-y0)*cos(theta);

    //     // transformedRect.width = EA + AF;
    //     // transformedRect.height = ED + FB;
    //     return transformedRect;
    // }

    public static Rect TransformRect(Rect rect, Matrix4x4 m) {
        if (m == null) return rect;

        var topLeft = m.MultiplyPoint3x4(rect.TopLeft());
        var topRight = m.MultiplyPoint3x4(rect.TopRight());
        var bottomRight = m.MultiplyPoint3x4(rect.BottomRight());
        var bottomLeft = m.MultiplyPoint3x4(rect.BottomLeft());

        var left = Mathf.Min(topLeft.x, topRight.x, bottomRight.x, bottomLeft.x);
        var top = Mathf.Min(topLeft.y, topRight.y, bottomRight.y, bottomLeft.y);
        var right = Mathf.Max(topLeft.x, topRight.x, bottomRight.x, bottomLeft.x);
        var bottom = Mathf.Max(topLeft.y, topRight.y, bottomRight.y, bottomLeft.y);
        return new Rect(left, top, right - left, bottom - top);
    }

    public enum ScalingMode {
        // Scale the target until the x dimension fits on the screen exactly, maintaining the content's aspect ratio.
        AspectFitWidthOnly,
        // Scale the target until the y dimension fits on the screen exactly, maintaining the content's aspect ratio.
        AspectFitHeightOnly,
        // Scale the target until one dimension fits on the screen exactly, maintaining the content's aspect ratio. May leave empty space on the screen.
        AspectFit,
        // Scale the target until the target fills the entire screen, maintaining the content's aspect ratio. May crop the content.
        AspectFill,
        // Scale the target until both dimensions fit the screen exactly, ignoring the content's aspect ratio.
        Fill
    }
    
    public static Vector2 Resize(Vector2 containerSize, Vector2 contentSize, ScalingMode scalingMode) {
        return Resize(containerSize, contentSize.x/contentSize.y, scalingMode);
    }
    public static Vector2 Resize(Vector2 containerSize, float contentAspect, ScalingMode scalingMode) {
        if(float.IsNaN(contentAspect)) return containerSize;
        if(scalingMode == ScalingMode.Fill) return containerSize;

        float containerAspect = containerSize.x / containerSize.y;
        if(float.IsNaN(containerAspect)) return containerSize;
        
        bool fillToAtLeastContainerWidth = false;
        bool fillToAtLeastContainerHeight = false;

        if(scalingMode == ScalingMode.AspectFitWidthOnly) fillToAtLeastContainerWidth = true;
        else if(scalingMode == ScalingMode.AspectFitHeightOnly) fillToAtLeastContainerHeight = true;
        else if(scalingMode == ScalingMode.AspectFill) fillToAtLeastContainerWidth = fillToAtLeastContainerHeight = true;
        
        Vector2 destRect = containerSize;
		if(containerSize.x == Mathf.Infinity) {
            destRect.x = containerSize.y * contentAspect;
		} else if(containerSize.y == Mathf.Infinity) {
            destRect.y = containerSize.x / contentAspect;
		}


        if (contentAspect > containerAspect) {
            // wider than high keep the width and scale the height
            var scaledHeight = containerSize.x / contentAspect;
            
            if (fillToAtLeastContainerHeight) {
                float resizePerc = containerSize.y / scaledHeight;
                destRect.x = containerSize.x * resizePerc;
            } else {
                destRect.y = scaledHeight;
            }
        } else {
            // higher than wide – keep the height and scale the width
            var scaledWidth = containerSize.y * contentAspect;

            if (fillToAtLeastContainerWidth) {
                float resizePerc = containerSize.x / scaledWidth;
                destRect.y = containerSize.y * resizePerc;
            } else {
                destRect.x = scaledWidth;
            }
        }

        return destRect;
    }

    // Returns the scale required to make the content fit the container according to the scaling mode.
    public static Vector3 Rescale (Vector2 containerSize, Vector2 contentSize, ScalingMode scalingMode) {
        var resized = ScaleToContainerUtils.Resize(containerSize, contentSize, scalingMode);
        return new Vector3(resized.x/contentSize.x, resized.y/contentSize.y, 1);
    }
    
    
    // Returns UV scale required to make the content fit the container according to the scaling mode.
    public static Vector2 RescaleUVs (Vector2 containerSize, Vector2 contentSize, ScalingMode scalingMode) {
        var resized = ScaleToContainerUtils.Resize(containerSize, contentSize, scalingMode);
        return new Vector2(containerSize.x/resized.x, containerSize.y/resized.y);
    }
    // Returns UV rect where the content fit the container according to the scaling mode, using a pivot point.
    public static Rect RescaleUVs (Vector2 containerSize, Vector2 contentSize, ScalingMode scalingMode, Vector2 pivot) {
        var size = ScaleToContainerUtils.RescaleUVs(containerSize, contentSize, scalingMode);
		return new Rect(-(size.x-1) * pivot.x, - (size.y-1) * pivot.y, size.x, size.y);
    }
}
