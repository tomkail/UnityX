using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public abstract class BaseGradientRenderer : MonoBehaviour {
    public MeshRenderer meshRenderer => GetComponent<MeshRenderer>();

    [Space]
    [SerializeField]
	Color _startColor = Color.white;
    public Color startColor {
		get => _startColor;
		set {
			_startColor = value;
			RefreshMaterialPropertyBlock();
		}
	}
    
	[SerializeField]
	Color _endColor = Color.black;
    public Color endColor {
		get => _endColor;
		set {
			_endColor = value;
			RefreshMaterialPropertyBlock();
		}
	}
    
	[SerializeField]
	float _startDistance = 0f;
    public float startDistance {
		get => _startDistance;
		set {
			_startDistance = value;
			RefreshMaterialPropertyBlock();
		}
	}
    
	[SerializeField]
	float _endDistance = 1f;
    public float endDistance {
		get => _endDistance;
		set {
			_endDistance = value;
			RefreshMaterialPropertyBlock();
		}
	}

	[SerializeField]
	float _offsetX = 0f;
    public float offsetX {
		get => _offsetX;
		set {
			_offsetX = value;
			RefreshMaterialPropertyBlock();
		}
	}
    
	[SerializeField]
	float _offsetY = 0f;
    public float offsetY {
		get => _offsetY;
		set {
			_offsetY = value;
			RefreshMaterialPropertyBlock();
		}
	}
    

	[SerializeField]
	float _power = 1;
    public float power {
		get => _power;
		set {
			_power = value;
			RefreshMaterialPropertyBlock();
		}
	}

    MaterialPropertyBlock _propBlock = null;
    protected MaterialPropertyBlock propBlock {
        get {
            if(_propBlock == null) _propBlock = new MaterialPropertyBlock();
            return _propBlock;
        }
    }
	
	void Update() {
		RefreshMaterialPropertyBlock();
	}
	#if UNITY_EDITOR
	void OnValidate() {
		RefreshMaterialPropertyBlock();
	}
	#endif

    protected void RefreshMaterialPropertyBlock () {
        if(meshRenderer != null) {
            meshRenderer.GetPropertyBlock(propBlock);
            PopulateMaterialPropertyBlock();
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }
    protected virtual void PopulateMaterialPropertyBlock () {
        propBlock.SetColor("_StartColor", startColor);
        propBlock.SetColor("_EndColor", endColor);
        propBlock.SetFloat("_StartDistance", startDistance);
        propBlock.SetFloat("_EndDistance", endDistance);
        propBlock.SetFloat("_OffsetX", offsetX);
        propBlock.SetFloat("_OffsetY", offsetY);
        propBlock.SetFloat("_Power", power);
    }
}
