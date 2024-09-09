using UnityEngine;
using UnityEngine.UI;

public static class ScaleToContainerUtils {
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
    
    
    // Calculates the new size of the content so that it fits or fills the container as per the scaling mode
    public static Vector2 Resize(Vector2 containerSize, Vector2 contentSize, ScalingMode scalingMode) {
        return Resize(containerSize, contentSize.x/contentSize.y, scalingMode);
    }
    
    public static Vector2 Resize(Vector2 containerSize, float contentAspect, ScalingMode scalingMode) {
        if(scalingMode == ScalingMode.Fill) return containerSize;
        if(float.IsNaN(contentAspect)) return containerSize;

        float containerAspect = containerSize.x / containerSize.y;
        if(float.IsNaN(containerAspect)) return containerSize;
        
        bool fillToAtLeastContainerWidth = false, fillToAtLeastContainerHeight = false;

        switch (scalingMode) {
            case ScalingMode.AspectFitWidthOnly:
                fillToAtLeastContainerWidth = true;
                break;
            case ScalingMode.AspectFitHeightOnly:
                fillToAtLeastContainerHeight = true;
                break;
            case ScalingMode.AspectFill:
                fillToAtLeastContainerWidth = fillToAtLeastContainerHeight = true;
                break;
        }
        
        var destRect = containerSize;
		if(float.IsPositiveInfinity(containerSize.x)) {
            destRect.x = containerSize.y * contentAspect;
		} else if(float.IsPositiveInfinity(containerSize.y)) {
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
    
    public static Vector2Int ResizeInt(Vector2 containerSize, Vector2 contentSize, ScalingMode scalingMode) {
        var resized = Resize(containerSize, contentSize, scalingMode);
        return new Vector2Int(Mathf.RoundToInt(resized.x), Mathf.RoundToInt(resized.y));
    }
    
    public static Vector2Int ResizeInt(Vector2 containerSize, float contentAspect, ScalingMode scalingMode) {
        var resized = Resize(containerSize, contentAspect, scalingMode);
        return new Vector2Int(Mathf.RoundToInt(resized.x), Mathf.RoundToInt(resized.y));
    }

    
    // Returns a scale that can be applied to the content to make it fit the container according to the scaling mode.
    public static Vector3 Rescale (Vector2 containerSize, Vector2 contentSize, ScalingMode scalingMode) {
        var resized = Resize(containerSize, contentSize, scalingMode);
        return new Vector3(resized.x/contentSize.x, resized.y/contentSize.y, 1);
    }
    
    
    // Returns a UV scale that can be applied to the content to make it fit the container according to the scaling mode.
    // UV scaling is the reciprocal of a typical scaling factor.
    public static Vector2 RescaleUVs (Vector2 containerSize, Vector2 contentSize, ScalingMode scalingMode) {
        var resized = Resize(containerSize, contentSize, scalingMode);
        return new Vector2(containerSize.x/resized.x, containerSize.y/resized.y);
    }
    
    // Returns a UV rect that can be applied to the content to make it fit the container according to the scaling mode.
    // Uses a pivot point to determine positioning.
    public static Rect RescaleUVs (Vector2 containerSize, Vector2 contentSize, ScalingMode scalingMode, Vector2 pivot) {
        var size = RescaleUVs(containerSize, contentSize, scalingMode);
		return new Rect(-(size.x-1) * pivot.x, - (size.y-1) * pivot.y, size.x, size.y);
    }

    // Converts the AspectMode used by AspectRatioFitter to the closest matching ScalingMode used by UnityEngine.
    public static ScaleMode ScalingModeToUnityScaleMode(ScalingMode scalingMode) {
        switch (scalingMode) {
            case ScalingMode.AspectFitWidthOnly:
            case ScalingMode.AspectFitHeightOnly:
            case ScalingMode.AspectFill:
                return ScaleMode.ScaleAndCrop;
            case ScalingMode.Fill:
                return ScaleMode.StretchToFill;
            case ScalingMode.AspectFit:
            default:
                return ScaleMode.ScaleToFit;
        }
    }

    // Converts the AspectMode used by AspectRatioFitter to the ScalingMode used by ScaleToContainer.
    public static AspectRatioFitter.AspectMode ScalingModeToAspectRatioFitterMode(ScalingMode scalingMode) {
        switch (scalingMode) {
            case ScalingMode.AspectFitWidthOnly:
                return AspectRatioFitter.AspectMode.WidthControlsHeight;
            case ScalingMode.AspectFitHeightOnly:
                return AspectRatioFitter.AspectMode.HeightControlsWidth;
            case ScalingMode.AspectFit:
                return AspectRatioFitter.AspectMode.FitInParent;
            case ScalingMode.AspectFill:
                return AspectRatioFitter.AspectMode.EnvelopeParent;
            case ScalingMode.Fill:
                return AspectRatioFitter.AspectMode.None;
            default:
                return AspectRatioFitter.AspectMode.None;
        }
    }
}