using UnityEditor;

namespace UnityEngine.UI {
    public class ExtendedCanvasScaler : CanvasScaler {
        public bool m_useCameraSizeInsteadOfScreenSize = true;
        public float scaleMultipler = 1;

        const float kLogBase = 2;
        #if UNITY_EDITOR
        // This is a hack to fix an issue where DeviceSim returns the wrong DPI on the first frame in editor (ARGHHH)
        float ScreenDPI {
            get => EditorPrefs.GetFloat("UnityX/ExtendedCanvasScaler/prevDPI", Screen.dpi);
            set => EditorPrefs.SetFloat("UnityX/ExtendedCanvasScaler/prevDPI", Screen.dpi);
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

        // This function is a copy-paste of this file 
        // https://bitbucket.org/Unity-Technologies/ui/src/0155c39e05ca5d7dcc97d9974256ef83bc122586/UnityEngine.UI/UI/Core/Layout/CanvasScaler.cs
        protected override void HandleScaleWithScreenSize() {
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            if (m_useCameraSizeInsteadOfScreenSize && GetComponent<Canvas>().worldCamera != null) {
                screenSize = GetComponent<Canvas>().worldCamera.pixelRect.size;
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

            scaleFactor *= scaleMultipler;

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

            SetScaleFactor((dpi / targetDPI) * scaleMultipler);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * targetDPI / m_DefaultSpriteDPI);
        }
    }
}