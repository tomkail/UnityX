#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI {
    public class ExtendedCanvasScaler : CanvasScaler {
        public bool m_useCameraSizeInsteadOfScreenSize = true;
        [UnityEngine.Serialization.FormerlySerializedAs("scaleMultipler")]
        public float scaleMultiplier = 1;

        const float kLogBase = 2;
        #if UNITY_EDITOR
        // This is a hack to fix an issue where DeviceSim returns the wrong DPI on the first frame in editor (ARGHHH)
        float ScreenDPI {
            get => EditorPrefs.GetFloat("UnityX/ExtendedCanvasScaler/prevDPI", Screen.dpi);
            set => EditorPrefs.SetFloat("UnityX/ExtendedCanvasScaler/prevDPI", value);
        }
#else
        float ScreenDPI => Screen.dpi;
#endif

        public void HandlePublic() {
            Handle();
        }

#if UNITY_EDITOR
        void Update() {
            ScreenDPI = Screen.dpi;
        }
#endif

        // Mirrors CanvasScaler.HandleScaleWithScreenSize from com.unity.ugui 2.5.0
        // (Runtime/UI/Core/Layout/CanvasScaler.cs), with two additions layered on top:
        // the camera-viewport override (m_useCameraSizeInsteadOfScreenSize) and scaleMultiplier.
        // Re-sync this if the ugui package is upgraded.
        protected override void HandleScaleWithScreenSize() {
            Canvas canvas = GetComponent<Canvas>();
            Vector2 screenSize = canvas.renderingDisplaySize;

            // Multiple display support only when not the main display. For display 0 the reported
            // resolution is always the desktops resolution since its part of the display API,
            // so we use the standard none multiple display method. (case 741751)
            int displayIndex = canvas.targetDisplay;
            if (displayIndex > 0 && displayIndex < Display.displays.Length) {
                Display disp = Display.displays[displayIndex];
                screenSize = new Vector2(disp.renderingWidth, disp.renderingHeight);
            }

            // ExtendedCanvasScaler: match the render camera's viewport rather than the display,
            // e.g. a Screen-Space-Camera canvas whose camera uses a sub-viewport (split-screen, letterboxed).
            if (m_useCameraSizeInsteadOfScreenSize && canvas.worldCamera != null) {
                screenSize = canvas.worldCamera.pixelRect.size;
            }

            float scaleFactor = 0;
            switch (m_ScreenMatchMode) {
                case ScreenMatchMode.MatchWidthOrHeight: {
                    // We take the log of the relative width and height before taking the average.
                    // Then we transform it back in the original space.
                    // the reason to transform in and out of logarithmic space is to have better behavior.
                    // If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
                    // In normal space the average would be (0.5 + 2) / 2 = 1.25
                    // In logarithmic space the average is (-1 + 1) / 2 = 0
                    float logWidth = Mathf.Log(screenSize.x / m_ReferenceResolution.x, kLogBase);
                    float logHeight = Mathf.Log(screenSize.y / m_ReferenceResolution.y, kLogBase);
                    float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, m_MatchWidthOrHeight);
                    scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
                    break;
                }
                case ScreenMatchMode.Expand: {
                    scaleFactor = Mathf.Min(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                    break;
                }
                case ScreenMatchMode.Shrink: {
                    scaleFactor = Mathf.Max(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                    break;
                }
            }

            scaleFactor *= scaleMultiplier;

            SetScaleFactor(scaleFactor);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
        }

        ///<summary>
        ///Handles canvas scaling for a constant physical size.
        ///</summary>
        protected override void HandleConstantPhysicalSize() {
            float currentDpi = ScreenDPI;
            float dpi = (currentDpi == 0 ? m_FallbackScreenDPI : currentDpi);
            float targetDPI = 1;
            switch (m_PhysicalUnit) {
                case Unit.Centimeters:
                    targetDPI = 2.54f;
                    break;
                case Unit.Millimeters:
                    targetDPI = 25.4f;
                    break;
                case Unit.Inches:
                    targetDPI = 1;
                    break;
                case Unit.Points:
                    targetDPI = 72;
                    break;
                case Unit.Picas:
                    targetDPI = 6;
                    break;
            }

            SetScaleFactor((dpi / targetDPI) * scaleMultiplier);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * targetDPI / m_DefaultSpriteDPI);
        }
    }
}