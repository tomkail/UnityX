using System;
using UnityEngine;

[Serializable]
public class HSBColor {
	public float h;
	public float s;
	public float b;
	public float a;

	public HSBColor(float h, float s, float b, float a) {
		this.h = h;
		this.s = s;
		this.b = b;
		this.a = a;
	}
 
	public HSBColor(float h, float s, float b) {
        this.h = h;
        this.s = s;
        this.b = b;
        a = 1f;
    }
 
    public HSBColor(Color col) {
        HSBColor temp = FromRGBA(col);
        h = temp.h;
        s = temp.s;
        b = temp.b;
        a = temp.a;
    }
 
    // Delegates to Unity's Color.RGBToHSV/HSVToRGB (HSB brightness == HSV value). h is kept in DEGREES
    // (0..360) to match this type's public convention; s/b stay 0..1.
    public static HSBColor FromRGBA(Color color) {
        Color.RGBToHSV(color, out float h, out float s, out float b);
        return new HSBColor(h * 360f, s, b, color.a);
    }

    public static Color ToRGBA(HSBColor hsbColor)
    {
        Color rgb = Color.HSVToRGB(Mathf.Repeat(hsbColor.h, 360f) / 360f, Mathf.Clamp01(hsbColor.s), Mathf.Clamp01(hsbColor.b));
        rgb.a = hsbColor.a;
        return rgb;
    }
 
    public Color ToRGBA()
    {
        return ToRGBA(this);
    }
 
    public override string ToString()
    {
        return "H:" + h + " S:" + s + " B:" + b;
    }
 
    public static HSBColor Lerp(HSBColor a, HSBColor b, float t)
    {
        float h = 0;
		float s = 0;
 
        //check special case black (color.b==0): interpolate neither hue nor saturation!
        //check special case grey (color.s==0): don't interpolate hue!
        if(a.b==0){
            h=b.h;
            s=b.s;
        }else if(b.b==0){
            h=a.h;
            s=a.s;
        }else{
            if(a.s==0){
                h=b.h;
            }else if(b.s==0){
                h=a.h;
            }else{
                // h is in degrees, so LerpAngle directly.
                float angle = Mathf.LerpAngle(a.h, b.h, t);
                while (angle < 0f)
                    angle += 360f;
                while (angle > 360f)
                    angle -= 360f;
                h = angle;
            }
            s=Mathf.Lerp(a.s,b.s,t);
        }
        return new HSBColor(h, s, Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
    }
 
    public static implicit operator HSBColor(Color src) {
        return FromRGBA(src);
    }
    
    public static implicit operator Color(HSBColor src) {
        return src.ToRGBA();
    }
}