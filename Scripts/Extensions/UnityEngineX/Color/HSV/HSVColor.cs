using System;
using UnityEngine;
using Object = System.Object;

[Serializable]
public class HSVColor {
	public float h;
	public float s;
	public float v;
	public float a;

	public HSVColor(float h, float s, float v, float a) {
		this.h = h;
		this.s = s;
		this.v = v;
		this.a = a;
	}
 
	public HSVColor(float h, float s, float v) {
        this.h = h;
        this.s = s;
        this.v = v;
        a = 1f;
    }
 
    public HSVColor(Color col) {
        HSVColor temp = FromRGBA(col);
        h = temp.h;
        s = temp.s;
        v = temp.v;
        a = temp.a;
    }
 
    // Delegates to Unity's Color.RGBToHSV/HSVToRGB (authoritative) rather than a hand-rolled conversion.
    // h is kept in DEGREES (0..360) to match this type's public convention; s/v stay 0..1.
    public static HSVColor FromRGBA(Color color) {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return new HSVColor(h * 360f, s, v, color.a);
    }

    public static Color ToRGBA(HSVColor hsvColor)
    {
        Color rgb = Color.HSVToRGB(Mathf.Repeat(hsvColor.h, 360f) / 360f, Mathf.Clamp01(hsvColor.s), Mathf.Clamp01(hsvColor.v));
        rgb.a = hsvColor.a;
        return rgb;
    }
 
    public Color ToRGBA()
    {
        return ToRGBA(this);
    }
 
    public override string ToString()
    {
        return "H:" + h + " S:" + s + " B:" + v;
    }
 
    public static HSVColor Lerp(HSVColor a, HSVColor b, float t)
    {
        float h = 0;
		float s = 0;
 
        //check special case black (color.b==0): interpolate neither hue nor saturation!
        //check special case grey (color.s==0): don't interpolate hue!
        if(a.v==0){
            h=b.h;
            s=b.s;
        }else if(b.v==0){
            h=a.h;
            s=a.s;
        }else{
            if(a.s==0){
                h=b.h;
            }else if(b.s==0){
                h=a.h;
            }else{
                // h is in degrees, so LerpAngle directly (the old code multiplied by 360 and then
                // never assigned the result, so Lerp always returned hue 0 / red).
                float angle = Mathf.LerpAngle(a.h, b.h, t);
                while (angle < 0f)
                    angle += 360f;
                while (angle > 360f)
                    angle -= 360f;
                h = angle;
            }
            s=Mathf.Lerp(a.s,b.s,t);
        }
        return new HSVColor(h, s, Mathf.Lerp(a.v, b.v, t), Mathf.Lerp(a.a, b.a, t));
    }
    
    public static HSVColor MoveTowards(HSVColor c1, HSVColor c2, float maxDelta) {
        return new HSVColor(Mathf.MoveTowards(c1.h, c2.h, maxDelta*180), Mathf.MoveTowards(c1.s, c2.s, maxDelta), Mathf.MoveTowards(c1.v, c2.v, maxDelta), Mathf.MoveTowards(c1.a, c2.a, maxDelta));
    }
	

    public static HSVColor Add(HSVColor left, HSVColor right){
        return new HSVColor(left.h+right.h, left.s+right.s, left.v+right.v, left.a+right.a);
    }

    public static HSVColor Subtract(HSVColor left, HSVColor right){
        return new HSVColor(left.h-right.h, left.s-right.s, left.v-right.v, left.a-right.a);
    }
    
    public override bool Equals(Object obj) {
        return obj is HSVColor color && this == color;
    }

    public bool Equals(HSVColor p) {
        return h == p.h && s == p.s && v == p.v && a == p.a;
    }

    public override int GetHashCode() {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 27;
            hash = hash * 31 + h.GetHashCode();
            hash = hash * 31 + s.GetHashCode();
            hash = hash * 31 + v.GetHashCode();
            hash = hash * 31 + a.GetHashCode();
            return hash;
        }
    }

    public static bool operator == (HSVColor left, HSVColor right) {
        return left.Equals(right);
    }

    public static bool operator != (HSVColor left, HSVColor right) {
        return !(left == right);
    }

    public static HSVColor operator +(HSVColor left, HSVColor right) {
        return Add(left, right);
    }

    public static HSVColor operator -(HSVColor left) {
        return new HSVColor(-left.h, -left.s, -left.v, -left.a);
    }

    public static HSVColor operator -(HSVColor left, HSVColor right) {
        return Subtract(left, right);
    }

    public static implicit operator HSVColor(Color src) {
        return FromRGBA(src);
    }
	
    public static implicit operator Color(HSVColor src) {
        return src.ToRGBA();
    }
}