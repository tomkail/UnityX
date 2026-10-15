using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteAlways]
public class BackgroundBlurUI : UIBehaviour {
    static readonly int KernelSizeProperty = Shader.PropertyToID("_KernelSize");
    static readonly int WeightTextureProperty = Shader.PropertyToID("_WeightTexture");
    static readonly int StrengthProperty = Shader.PropertyToID("_Strength");
    static readonly int StepSizeProperty = Shader.PropertyToID("_StepSize");
    static readonly int BlurSigmaProperty = Shader.PropertyToID("_BlurSigma");
    static readonly int BlurTapRadiusProperty = Shader.PropertyToID("_BlurTapRadius");
    static Shader _shader;
    static Shader shader => _shader != null ? _shader : (_shader = Shader.Find("Hidden/BackgroundBlurUI"));

    public Graphic graphic => GetComponent<Graphic>();

    [SerializeField, Range(0f,1f)]
    float _strength = 1f;
    public float strength {
        get => _strength;
        set {
            _strength = value;
            _isDirty = true;
        }
    }

    public Quality quality = Quality.Medium;
    public enum Quality {
        Low,
        Medium,
        High
    }
    
    public int blurRadius = 64;
    public int blurSize => 2 * blurRadius + 1;
    const int maxBlurRadius = 511;
    // Increasing this value will decrease the blur quality but increase performance.
    // The lowest value you can use is 2. You normally want to increase it in proportion with the radius.
    int stepSize = 2;
    
    [SerializeField]
    bool _canvasGroupAlphaAffectsStrength;
    public bool canvasGroupAlphaAffectsStrength {
        get => _canvasGroupAlphaAffectsStrength;
        set {
            _canvasGroupAlphaAffectsStrength = value;
            _isDirty = true;
        }
    }
    float canvasGroupAlpha;
    
    Material material;
    Texture2D weightTexture;
    
    bool initialized;
    bool _isDirty;

    static readonly HashSet<BackgroundBlurUI> enabledInstances = new();

    // Whether every enabled blur is drawn after its camera has finished, by a screen space overlay canvas.
    internal static bool AllEnabledOnScreenSpaceOverlayCanvases() {
        foreach (var instance in enabledInstances) {
            var canvas = instance.graphic != null ? instance.graphic.canvas : null;
            if (canvas == null) return false;
            canvas = canvas.rootCanvas;
            var isOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay || (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null);
            if (!isOverlay) return false;
        }
        return true;
    }

    protected override void OnEnable () {
        enabledInstances.Add(this);
#if UNITYX_URP
        if (enabledInstances.Count == 1) BackgroundBlurUIRenderPass.Register();
#endif
        Initialize();
    }

    void Initialize () {
        if (shader == null) {
            Debug.LogError("Could not find shader for UIBlur", this);
            return;
        }
        if(initialized || graphic == null) return;

        material = new Material(shader);
        material.name = $"{material.name} ({nameof(BackgroundBlurUI)} Clone)";
        graphic.material = material;
    
        initialized = true;

        _isDirty = true;
    }

    protected override void OnDisable () {
        enabledInstances.Remove(this);
#if UNITYX_URP
        if (enabledInstances.Count == 0) BackgroundBlurUIRenderPass.Unregister();
#endif
        if(!initialized) return;

        graphic.material = null;
        if (Application.isPlaying) {
            if(material != null) Destroy(material);
            if(weightTexture != null) Destroy(weightTexture);
        } else {
            if(material != null) DestroyImmediate(material);
            if(weightTexture != null) DestroyImmediate(weightTexture);
        }
        material = null;
        weightTexture = null;
        
        initialized = false;
    }

#if UNITY_EDITOR
    protected override void OnValidate() {
        blurRadius = Mathf.Clamp(blurRadius, 0, maxBlurRadius);
        
        _isDirty = true;
    }
#endif

    void LateUpdate () {
        if(!initialized) {
            Initialize();
            if(!initialized) return;
        }

        if (canvasGroupAlphaAffectsStrength) {
            var newCanvasGroupAlpha = graphic.canvasRenderer.GetInheritedAlpha();
            if (canvasGroupAlpha != newCanvasGroupAlpha) {
                canvasGroupAlpha = newCanvasGroupAlpha;
                _isDirty = true;
            }
        }
        if(_isDirty) Refresh();
    }

    void Refresh() {
        graphic.material = material;
        var finalStrength = strength * (canvasGroupAlphaAffectsStrength ? canvasGroupAlpha : 1);
        var sigma = blurRadius/3f;
        var finalSigma = finalStrength * sigma;
        if(quality == Quality.Low) stepSize = (int) (Mathf.Lerp(0.125f,1f,finalStrength) * blurRadius * 0.14f);
        else if(quality == Quality.Medium) stepSize = (int) (Mathf.Lerp(0.25f,1f,finalStrength) * blurRadius * 0.06f);
        else if(quality == Quality.High) stepSize = (int) (Mathf.Lerp(0.3f,1f,finalStrength) * blurRadius * 0.03f);
        stepSize = Mathf.Clamp(stepSize, 2, blurRadius);
        
        graphic.materialForRendering.SetFloat(StrengthProperty, finalStrength);
        if (GraphicsSettings.currentRenderPipeline == null) {
            // The built-in pipeline's shader does a separable blur of the GrabPass texture, weighted by this texture.
            GenerateWeightTexture(ref weightTexture, blurRadius, finalSigma);
            graphic.materialForRendering.SetInt(KernelSizeProperty, blurRadius);
            graphic.materialForRendering.SetTexture(WeightTextureProperty, weightTexture);
            graphic.materialForRendering.SetInt(StepSizeProperty, stepSize);
        } else {
            // The URP shader does a single-pass blur of a mip-mapped copy of the camera colour, (2 * tap radius + 1)^2 taps per pixel.
            graphic.materialForRendering.SetFloat(BlurSigmaProperty, finalSigma);
            graphic.materialForRendering.SetFloat(BlurTapRadiusProperty, quality == Quality.Low ? 2 : quality == Quality.Medium ? 4 : 6);
        }
        graphic.SetMaterialDirty();
        graphic.enabled = finalSigma > 0; 
        _isDirty = false;
    }
    
    
    void GenerateWeightTexture(ref Texture2D weightTexture, int radius, float sigma) {
        int size = 2 * radius + 1; // Total size is always odd to maintain symmetry.

        if (weightTexture == null) {
            weightTexture = new Texture2D(size, 1, TextureFormat.RFloat, false) {
                wrapMode = TextureWrapMode.Clamp,
                name = "BackgroundBlurUI Weight Texture"
            };
        } else if (weightTexture.width != size) {
            weightTexture.Reinitialize(size, 1, TextureFormat.RFloat, false);
            weightTexture.wrapMode = TextureWrapMode.Clamp;
        }

        Color[] colors = new Color[size];
        float sum = 0f;

        // Directly compute Gaussian weights into the colors array
        for (int i = 0; i <= radius; i++) {
            float x = i;
            float gaussian = Mathf.Exp(-(x * x) / (2 * sigma * sigma));
            float weight = gaussian / (Mathf.Sqrt(2.0f * Mathf.PI) * sigma); // Normalize the Gaussian function
            sum += weight * (i == 0 ? 1 : 2); // Account for both sides except the middle
            colors[radius + i] = new Color(weight, 0, 0, 0);
            colors[radius - i] = colors[radius + i]; // Symmetry
        }

        // Normalize weights
        for (int i = 0; i < size; i++) {
            colors[i].r /= sum;
        }

        // Apply all colors at once and upload to GPU
        weightTexture.SetPixels(colors);
        weightTexture.Apply();
    }
}