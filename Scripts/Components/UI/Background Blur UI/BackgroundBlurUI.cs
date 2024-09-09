using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
public class BackgroundBlurUI : UIBehaviour {
    static readonly int KernelSizeProperty = Shader.PropertyToID("_KernelSize");
    static readonly int WeightTextureProperty = Shader.PropertyToID("_WeightTexture");
    static readonly int StrengthProperty = Shader.PropertyToID("_Strength");
    static readonly int StepSizeProperty = Shader.PropertyToID("_StepSize");
    static Shader shader => Shader.Find("Hidden/BackgroundBlurUI");

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

    protected override void OnEnable () {
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

    protected override void OnValidate() {
        blurRadius = Mathf.Clamp(blurRadius, 0, maxBlurRadius);
        
        _isDirty = true;
    }

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
        var sigma = blurRadius/3;
        var finalSigma = finalStrength * sigma;
        if(quality == Quality.Low) stepSize = (int) (Mathf.Lerp(0.125f,1f,finalStrength) * blurRadius * 0.14f);
        else if(quality == Quality.Medium) stepSize = (int) (Mathf.Lerp(0.25f,1f,finalStrength) * blurRadius * 0.06f);
        else if(quality == Quality.High) stepSize = (int) (Mathf.Lerp(0.3f,1f,finalStrength) * blurRadius * 0.03f);
        stepSize = Mathf.Clamp(stepSize, 2, blurRadius);
        
        GenerateWeightTexture(ref weightTexture, blurRadius, finalSigma);
        graphic.materialForRendering.SetFloat(StrengthProperty, finalStrength);
        graphic.materialForRendering.SetInt(KernelSizeProperty, blurRadius);
        graphic.materialForRendering.SetTexture(WeightTextureProperty, weightTexture);
        graphic.materialForRendering.SetInt(StepSizeProperty, Mathf.Clamp(stepSize, 2,blurRadius));
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

        Color[] colors = new Color[size];;
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