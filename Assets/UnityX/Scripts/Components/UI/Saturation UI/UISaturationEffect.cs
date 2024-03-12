using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Graphic))]
public class UISaturationEffect : MonoBehaviour {
    [Range(0, 1)]
    public float saturation = 1;

    Graphic img;
    [SerializeField]
    Material matPrefab;
    [SerializeField, HideInInspector]
    Material mat;

    void Reset() {
        mat = null;
    }
    void OnValidate() {
        img = GetComponent<Graphic>();
        if (mat != null) mat.SetFloat("_Saturation", saturation);
        if(mat == null && matPrefab != null)
            img.material = mat = new Material(matPrefab);
    }

    void OnEnable () {
        img = GetComponent<Graphic>();
        if(mat == null && matPrefab != null)
            img.material = mat = new Material(matPrefab);
    }

    void Update() {
        if (mat != null) mat.SetFloat("_Saturation", saturation);
    }
}
