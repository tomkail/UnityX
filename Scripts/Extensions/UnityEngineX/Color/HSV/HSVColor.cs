using UnityEngine;

[System.Serializable]
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
        this.a = 1f;
    }
 
    public HSVColor(Color col) {
        HSVColor temp = FromRGBA(col);
        h = temp.h;
        s = temp.s;
        v = temp.v;
        a = temp.a;
    }
 
    public static HSVColor FromRGBA(Color color) {
        HSVColor ret = new HSVColor(0f, 0f, 0f, color.a);
 
        float r = color.r;
        float g = color.g;
        float b = color.b;
 
        float max = Mathf.Max(r, Mathf.Max(g, b));
 
        if (max <= 0)
        {
            return ret;
        }
 
        float min = Mathf.Min(r, Mathf.Min(g, b));
        float dif = max - min;
 
        if (max > min)
        {
            if (g == max)
            {
                ret.h = (b - r) / dif * 60f + 120f;
            }
            else if (b == max)
            {
                ret.h = (r - g) / dif * 60f + 240f;
            }
            else if (b > g)
            {
                ret.h = (g - b) / dif * 60f + 360f;
            }
            else
            {
                ret.h = (g - b) / dif * 60f;
            }
            if (ret.h < 0)
            {
                ret.h = ret.h + 360f;
            }
        }
        else
        {
            ret.h = 0;
        }
 
//        ret.h *= 1f / 360f;
//		ret.h = Mathf.Repeat(ret.h, 360f);
        ret.s = (dif / max) * 1f;
        ret.v = max;
 
        return ret;
    }
 
    public static Color ToRGBA(HSVColor hsbColor)
    {
        float r = hsbColor.v;
        float g = hsbColor.v;
        float b = hsbColor.v;
        if (hsbColor.s != 0)
        {
            float max = hsbColor.v;
            float dif = hsbColor.v * hsbColor.s;
            float min = hsbColor.v - dif;
 
//			float h = hsbColor.h; // * 360f;
			float h = Mathf.Repeat(hsbColor.h, 360f);
            if (h < 60f)
            {
                r = max;
                g = h * dif / 60f + min;
                b = min;
            }
            else if (h < 120f)
            {
                r = -(h - 120f) * dif / 60f + min;
                g = max;
                b = min;
            }
            else if (h < 180f)
            {
                r = min;
                g = max;
                b = (h - 120f) * dif / 60f + min;
            }
            else if (h < 240f)
            {
                r = min;
                g = -(h - 240f) * dif / 60f + min;
                b = max;
            }
            else if (h < 300f)
            {
                r = (h - 240f) * dif / 60f + min;
                g = min;
                b = max;
            }
            else if (h <= 360f)
            {
                r = max;
                g = min;
                b = -(h - 360f) * dif / 60 + min;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }
        }
 
        return new Color(Mathf.Clamp01(r),Mathf.Clamp01(g),Mathf.Clamp01(b),hsbColor.a);
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
                // works around bug with LerpAngle
                float angle = Mathf.LerpAngle(a.h * 360f, b.h * 360f, t);
                while (angle < 0f)
                    angle += 360f;
                while (angle > 360f)
                    angle -= 360f;
//                h=angle/360f;
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
    
    public override bool Equals(System.Object obj) {
        return obj is HSVColor && this == (HSVColor)obj;
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