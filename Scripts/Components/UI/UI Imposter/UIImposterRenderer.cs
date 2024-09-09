using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public struct UIImposterOutputParams {
    // Determines the size of the output RenderTexture.
    public SizeMode sizeMode;
    public enum SizeMode {
        // Match the size of the target's in the canvas.
        OriginalCanvasSize,
        // Match the size of the target's size on screen.
        OriginalScreenSize,
        // Use a custom size
        CustomSize,
    }
    
    // When using a custom size, this is the size of the output RenderTexture.
    public Vector2 customContainerSize;
    // When using a custom size, this is how the content should scale to fit in the RenderTexture.
    public ScalingMode containerScalingMode;

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
}

// Renders a UI element/hierarchy to a rendertexture. This is handy for when you want to render the same element multiple times without the overhead of duplicating the object.
// Works by moving the target into a new canvas, setting up the camera to frame the target, and rendering it.
// Current Issues:
// Can cause issues when used on the root object in a PrefabStage. 
public static class UIImposterRenderer {
    static Camera camera;
    static Canvas canvas;

    static Vector3[] worldCorners = new Vector3[4];
    static Vector3[] canvasRelativeCorners = new Vector3[4];


    // Creates an imposter from a RectTransform, returning a newly created RenderTexture.
    public static RenderTexture Render(RectTransform target, UIImposterOutputParams outputParams) {
        return Render(target, outputParams, new RenderTextureDescriptor(0, 0, RenderTextureFormat.Default, 0));
    }
    public static RenderTexture Render (RectTransform target, UIImposterOutputParams outputParams, RenderTextureDescriptor renderTextureDescriptor) {
        RenderTexture renderTexture = null;
        Render(target, outputParams, ref renderTexture);
        return renderTexture;
    }

    public static void Render(RectTransform target, UIImposterOutputParams outputParams, ref RenderTexture renderTexture) {
        Render(target, outputParams, ref renderTexture, new RenderTextureDescriptor(0, 0, RenderTextureFormat.Default, 0));
    }

    // Creates an imposter from a RectTransform, using an existing RenderTexture or setting the instance to a newly created one if null.
    public static void Render (RectTransform target, UIImposterOutputParams outputParams, ref RenderTexture renderTexture, RenderTextureDescriptor renderTextureDescriptor) {
        if(target == null) return;
        // We currently delete these once we're done rendering, but we might just as happily keep them around.
        if(camera == null) {
            camera = new GameObject("UI Imposter Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Depth;
            camera.hideFlags = HideFlags.HideAndDontSave;
            camera.orthographic = true;
        }
        if(canvas == null) {
            canvas = new GameObject("UI Imposter Canvas").AddComponent<Canvas>();
            canvas.hideFlags = HideFlags.HideAndDontSave;
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
        }

        // Setup
        var epsilon = 0.01f;
        var targetCanvas = target.GetComponentInParent<Canvas>().rootCanvas;

        // Cache the bits we're changing so we can restore them once we're done.
        var originalParent = target.parent;
        var originalIndex = target.GetSiblingIndex();
        var originalPos = target.localPosition;
        var originalScale = target.localScale;


        // Get the world bounds of the target
        
        target.GetWorldCorners(worldCorners);
        var worldBounds = CreateEncapsulating(worldCorners);

        // A potential upgrade is a scaling mode for the content, where the container size is constant. We'd do this by changing the size of the world bounds to fit the container here.
        // if(true) {
        //     worldBounds = Vector2Int.CeilToInt(Resize(outputParams.customContainerSize, worldBounds.size, outputParams.scalingMode));
        // }

        // Make the camera/canvas match that of the target
        camera.transform.rotation = targetCanvas.transform.rotation;
        camera.transform.position = worldBounds.center;
        camera.orthographicSize = worldBounds.extents.y;
        
        canvas.transform.position = targetCanvas.transform.position;
        canvas.transform.rotation = targetCanvas.transform.rotation;
        canvas.transform.localScale = targetCanvas.transform.localScale;
        ((RectTransform)canvas.transform).sizeDelta = ((RectTransform)targetCanvas.transform).rect.size;

        // Set the size of the output RenderTexture
        Vector2Int renderTextureSize = Vector2Int.zero;
        if(outputParams.sizeMode == UIImposterOutputParams.SizeMode.OriginalCanvasSize) {
            for(int i = 0; i < worldCorners.Length; i++) canvasRelativeCorners[i] = targetCanvas.transform.InverseTransformPoint(worldCorners[i]);
            var localBounds = CreateEncapsulating(canvasRelativeCorners);
            renderTextureSize = Vector2Int.CeilToInt(localBounds.size);
        } else if(outputParams.sizeMode == UIImposterOutputParams.SizeMode.OriginalScreenSize) {
            Vector2[] screenCorners = new Vector2[4];
            Camera cam = targetCanvas.renderMode == RenderMode.ScreenSpaceCamera || targetCanvas.renderMode == RenderMode.WorldSpace ? targetCanvas.worldCamera : null;
            for (int i = 0; i < 4; i++) screenCorners[i] = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[i]);
            var screenRect = CreateEncapsulating(screenCorners);
            renderTextureSize = Vector2Int.CeilToInt(Resize(screenRect.size, worldBounds.size, outputParams.containerScalingMode));
        } else {
            renderTextureSize = Vector2Int.CeilToInt(Resize(outputParams.customContainerSize, worldBounds.size, outputParams.containerScalingMode));
        }
        
        // Move the target to the new canvas
        target.SetParent(canvas.transform);
        target.position = camera.transform.position + Vector3.forward * Mathf.Max(epsilon*2, worldBounds.extents.z);
        var pivotOffset = (target.pivot - Vector2.one * 0.5f) * target.rect.size;
        target.anchoredPosition += pivotOffset;

        // Set the camera to only render around our target
        // Note that this means that only flat UI will be renderered. We'll need to edit this to support UI that uses the Z axis.
        camera.nearClipPlane = epsilon;
        camera.farClipPlane = (Mathf.Max(epsilon*2, worldBounds.extents.z)*2)+epsilon;
        
        if (renderTextureSize.x <= 0 || renderTextureSize.y <= 0) {
            Debug.LogWarning($"UIImposterRenderer: Target size is {renderTextureSize}, so not creating RenderTexture.");
        } else {
            FilterMode rtFilterMode = FilterMode.Bilinear;
            
            renderTextureDescriptor.width = renderTextureSize.x;
            renderTextureDescriptor.height = renderTextureSize.y;
            
            if (renderTexture == null) {
                renderTexture = new RenderTexture (renderTextureDescriptor);
                renderTexture.filterMode = rtFilterMode;
            } else if(!RenderTextureDescriptorsMatch(renderTexture.descriptor, renderTextureDescriptor)) {
                rtFilterMode = renderTexture.filterMode;
                
                if(RenderTexture.active == renderTexture) RenderTexture.active = null;
                renderTexture.Release();

                renderTexture.descriptor = renderTextureDescriptor;
                renderTexture.Create();
                renderTexture.filterMode = rtFilterMode;
            }
        }


        // Render
        camera.targetTexture = renderTexture;
        camera.Render();
        camera.targetTexture = null;

        // Restore to original position
        target.SetParent(originalParent);
        target.SetSiblingIndex(originalIndex);
        target.localScale = originalScale;
        target.localPosition = originalPos;

        // Destroy renderer. This is optional, but it's a good idea to do this if you're not going to be using the renderer again.
        if(Application.isPlaying) {
            Object.Destroy(camera.gameObject);
            camera = null;
            Object.Destroy(canvas.gameObject);
            canvas = null;
        } else {
            Object.DestroyImmediate(camera.gameObject);
            Object.DestroyImmediate(canvas.gameObject);
        }
    }


    
    static Vector2 Resize(Vector2 containerSize, Vector2 contentSize, UIImposterOutputParams.ScalingMode scalingMode) {
        return Resize(containerSize, contentSize.x/contentSize.y, scalingMode);
    }
    static Vector2 Resize(Vector2 containerSize, float contentAspect, UIImposterOutputParams.ScalingMode scalingMode) {
        if(float.IsNaN(contentAspect)) return containerSize;
        if(scalingMode == UIImposterOutputParams.ScalingMode.Fill) return containerSize;

        float containerAspect = containerSize.x / containerSize.y;
        if(float.IsNaN(containerAspect)) return containerSize;
        
        bool fillToAtLeastContainerWidth = false;
        bool fillToAtLeastContainerHeight = false;

        if(scalingMode == UIImposterOutputParams.ScalingMode.AspectFitWidthOnly) fillToAtLeastContainerWidth = true;
        else if(scalingMode == UIImposterOutputParams.ScalingMode.AspectFitHeightOnly) fillToAtLeastContainerHeight = true;
        else if(scalingMode == UIImposterOutputParams.ScalingMode.AspectFill) fillToAtLeastContainerWidth = fillToAtLeastContainerHeight = true;
        
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

    static Bounds CreateEncapsulating (Vector3[] vectors) {
        if(vectors == null) return new Bounds(Vector3.zero, Vector3.zero);
        var count = vectors.Length;
        if(count == 0) return new Bounds(Vector3.zero, Vector3.zero);
        Vector3 min = vectors[0];
        Vector3 max = vectors[0];
        foreach(var vector in vectors) {
            if(vector.x < min.x) min.x = vector.x;
            else if(vector.x > max.x) max.x = vector.x;

            if(vector.y < min.y) min.y = vector.y;
            else if(vector.y > max.y) max.y = vector.y;

            if(vector.z < min.z) min.z = vector.z;
            else if(vector.z > max.z) max.z = vector.z;
        }
        
        var size = max - min;
        var center = min + size * 0.5f;
        return new Bounds(center, size);
    }
    
    static Rect CreateEncapsulating (params Vector2[] vectors) {
        float xMin = vectors[0].x;
        float xMax = vectors[0].x;
        float yMin = vectors[0].y;
        float yMax = vectors[0].y;
        for(int i = 1; i < vectors.Length; i++) {
            var vector = vectors[i];
            xMin = Mathf.Min (xMin, vector.x);
            xMax = Mathf.Max (xMax, vector.x);
            yMin = Mathf.Min (yMin, vector.y);
            yMax = Mathf.Max (yMax, vector.y);
        }
        return Rect.MinMaxRect (xMin, yMin, xMax, yMax);
    }
            
    static bool RenderTextureDescriptorsMatch(RenderTextureDescriptor descriptorA, RenderTextureDescriptor descriptorB) {
        if (descriptorA.depthBufferBits != descriptorB.depthBufferBits) return false;
        if (descriptorA.width != descriptorB.width) return false;
        if (descriptorA.height != descriptorB.height) return false;
        if (descriptorA.depthStencilFormat != descriptorB.depthStencilFormat) return false;
        if (descriptorA.enableRandomWrite != descriptorB.enableRandomWrite) return false;
        if (descriptorA.colorFormat != descriptorB.colorFormat) return false;
        if (descriptorA.dimension != descriptorB.dimension) return false;
        return true;
    }
}