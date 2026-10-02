using UnityEngine;

public class LinearGradientRenderer : BaseGradientRenderer {
    [SerializeField]
	float _degrees = 0;
    public float degrees {
		get => _degrees;
		set {
			_degrees = value;
			RefreshMaterialPropertyBlock();
		}
	}

	protected override void PopulateMaterialPropertyBlock () {
        base.PopulateMaterialPropertyBlock();
		propBlock.SetFloat("_Degrees", degrees);
    }
}