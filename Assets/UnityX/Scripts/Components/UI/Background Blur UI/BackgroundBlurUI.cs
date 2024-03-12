using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class BackgroundBlurUI : MonoBehaviour {
    static readonly int BlurSigma = Shader.PropertyToID("_BlurSigma");
    Shader shader => Shader.Find("Hidden/BackgroundBlurUI");

    public Graphic graphic => GetComponent<Graphic>();
    
    [SerializeField]
    Quality _quality = Quality.MEDIUM_KERNEL;
    public Quality quality {
        get => _quality;
        set {
            _quality = value;
            _isDirty = true;
        }
    }

    static Quality[] _qualities;
    static Quality[] qualities {
        get {
            if(_qualities == null) _qualities = (Quality[]) Enum.GetValues(typeof(Quality));
            return _qualities;
        }
    }
    public enum Quality
    {
        LITTLE_KERNEL = 7,
        MEDIUM_KERNEL = 35,
        BIG_KERNEL = 127
    }
    [SerializeField, Range(0f,1f)]
    float _strength = 1f;
    public float strength {
        get => _strength;
        set {
            _strength = value;
            _isDirty = true;
        }
    }
    
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
    
    bool initialized;
    bool _isDirty;

    void OnEnable () {
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

    void OnDisable () {
        if(!initialized) return;

        graphic.material = null;
        if(material != null) {
            if(Application.isPlaying) Destroy(material);
            else DestroyImmediate(material);
            material = null;
        }
        initialized = false;
    }

    void OnValidate() {
        _isDirty = true;
    }

    void LateUpdate () {
        if(!initialized) {
            Initialize();
            if(!initialized) return;
        }

        if (canvasGroupAlphaAffectsStrength) {
            var newCanvasGroupAlpha = CanvasGroupsAlpha(gameObject);
            if (canvasGroupAlpha != newCanvasGroupAlpha) {
                canvasGroupAlpha = newCanvasGroupAlpha;
                _isDirty = true;
            }
        }
        if(_isDirty) Refresh();
    }

    void Refresh() {
        graphic.material = material;
        var sigma = strength * (int) quality * 0.5f * (canvasGroupAlphaAffectsStrength ? canvasGroupAlpha : 1);
        graphic.materialForRendering.SetFloat(BlurSigma, sigma);
        foreach (Quality q in qualities) {
            if(quality == q) graphic.materialForRendering.EnableKeyword(q.ToString());
            else graphic.materialForRendering.DisableKeyword(q.ToString());
        }
        graphic.SetMaterialDirty();
        graphic.enabled = sigma > 0; 
        _isDirty = false;
    }
    
    static readonly List<CanvasGroup> m_CanvasGroupCache = new();
    static float CanvasGroupsAlpha (GameObject gameObject) {
        var groupAlpha = 1f;
        Transform t = gameObject.transform;
        while (t != null) {
            t.GetComponents(m_CanvasGroupCache);
            bool shouldBreak = false;
            for (var i = 0; i < m_CanvasGroupCache.Count; i++)
            {
                groupAlpha *= m_CanvasGroupCache[i].alpha;
                
                // if this is a 'fresh' group, then break
                // as we should not consider parents
                if (m_CanvasGroupCache[i].ignoreParentGroups)
                    shouldBreak = true;
            }
            if (shouldBreak)
                break;

            t = t.parent;
        }
        return groupAlpha;
    }
}