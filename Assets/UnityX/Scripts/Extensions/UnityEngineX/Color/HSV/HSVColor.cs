using UnityEngine;

[System.Serializable]
public class HSVColor
{
    // human settings
    public float _Hue;
    public float _Saturation;
    public float _Value;

    // derived values
    private float _vsu;
    private float _vsw;
    private float _rr;
    private float _rg;
    private float _rb;
    private float _gr;
    private float _gg;
    private float _gb;
    private float _br;
    private float _bg;
    private float _bb;

    // the colour remains unchanged if these values are used in the hsv shader
    public static HSVColor NoShift = new HSVColor() { _Hue = 0, _Saturation = 1, _Value = 1 };

    public void UseOnHSVMaterial(Material HSVMaterial)
    {
        HSVMaterial.SetFloat("_HueShift", _Hue);
        HSVMaterial.SetFloat("_Sat", _Saturation);
        HSVMaterial.SetFloat("_Val", _Value);
    }

    public void UseOnHSVFastMaterial(Material HSVFastMaterial)
    {
        UseOnHSVMaterial(HSVFastMaterial);
        SetUpHSVFast(HSVFastMaterial);
    }

    public void SetFromHSVMaterial(Material HSVMaterial)
    {
        _Hue = HSVMaterial.GetFloat("_HueShift");
        _Saturation = HSVMaterial.GetFloat("_Sat");
        _Value = HSVMaterial.GetFloat("_Val");
    }

    public Color ShiftRGBColour(Color originalPixel)
    {
        Color shifted = new Color();

        CalculateDerivedValues();


        shifted.r = _rr * originalPixel.r
                + _rg * originalPixel.g
                + _rb * originalPixel.b;

        shifted.g = _gr * originalPixel.r
                + _gg * originalPixel.g
                + _gb * originalPixel.b;

        shifted.b = _br * originalPixel.r
                + _bg * originalPixel.g
                + _bb * originalPixel.b;

        return shifted;
    }

    public void ApplyToTexture(Texture2D source, Texture2D destination)
    {
        for (int x = 0; x < destination.width; x++)
        {
            for (int y = 0; y < destination.height; y++)
            {
                Color originalPixel = source.GetPixel(x, y);

                Color shiftedPixel = ShiftRGBColour(originalPixel);

                destination.SetPixel(x, y, shiftedPixel);
            }
        }

        destination.Apply();
    }

   

    public void SetUpHSVFast(Material HSVFastMaterial)
    {
        CalculateDerivedValues();

        HSVFastMaterial.SetFloat("_VSU", _vsu);
        HSVFastMaterial.SetFloat("_VSW", _vsw);
        HSVFastMaterial.SetFloat("_RR", _rr);
        HSVFastMaterial.SetFloat("_RG", _rg);
        HSVFastMaterial.SetFloat("_RB", _rb);
        HSVFastMaterial.SetFloat("_GR", _gr);
        HSVFastMaterial.SetFloat("_GG", _gg);
        HSVFastMaterial.SetFloat("_GB", _gb);
        HSVFastMaterial.SetFloat("_BR", _br);
        HSVFastMaterial.SetFloat("_BG", _bg);
        HSVFastMaterial.SetFloat("_BB", _bb);
    }

    private void CalculateDerivedValues()
    {
        CalculateVSU();
        CalculateVSW();
        CalculateMatrix();
    }

    private void CalculateVSU()
    {
        _vsu =  _Value * _Saturation * Mathf.Cos(_Hue * 3.14159265f / 180f);
    }

    private void CalculateVSW()
    {
        _vsw = _Value * _Saturation * Mathf.Sin(_Hue * 3.14159265f / 180f);
    }

    private void CalculateMatrix()
    {
        _rr = (.299f * _Value + .701f * _vsu + .168f * _vsw);
        _rg = (.587f * _Value - .587f * _vsu + .330f * _vsw);
        _rb = (.114f * _Value - .114f * _vsu - .497f * _vsw);

        _gr = (.299f * _Value - .299f * _vsu - .328f * _vsw);
        _gg = (.587f * _Value + .413f * _vsu + .035f * _vsw);
        _gb = (.114f * _Value - .114f * _vsu + .292f * _vsw);

        _br = (.299f * _Value - .300f * _vsu + 1.25f * _vsw);
        _bg = (.587f * _Value - .588f * _vsu - 1.05f * _vsw);
        _bb = (.114f * _Value + .886f * _vsu - .203f * _vsw);
    }

}
