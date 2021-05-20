using UnityEngine;

[System.Serializable]
public struct HSLColor {
	public float h;
	public float s;
	public float l;
	public float a;
	
	public HSLColor(float h, float s, float l, float a) {
		this.h = h;
		this.s = s;
		this.l = l;
		this.a = a;
	}
	
	public HSLColor(float h, float s, float l) {
		this.h = h;
		this.s = s;
		this.l = l;
		this.a = 1f;
	}
	
	public HSLColor(Color c) {
		HSLColor temp = FromRGBA(c);
		h = temp.h;
		s = temp.s;
		l = temp.l;
		a = temp.a;
	}
	
	public static HSLColor FromRGBA(Color c) {		
		float h, s, l, a;
		a = c.a;
		
		float cmin = Mathf.Min(Mathf.Min(c.r, c.g), c.b);
		float cmax = Mathf.Max(Mathf.Max(c.r, c.g), c.b);
		
		l = (cmin + cmax) / 2f;
		
		if (cmin == cmax) {
			s = 0;
			h = 0;
		} else {
			float delta = cmax - cmin;
			
			s = (l <= .5f) ? (delta / (cmax + cmin)) : (delta / (2f - (cmax + cmin)));
			
			h = 0;
			
			if (c.r == cmax) {
				h = (c.g - c.b) / delta;
			} else if (c.g == cmax) {
				h = 2f + (c.b - c.r) / delta;
			} else if (c.b == cmax) {
				h = 4f + (c.r - c.g) / delta;
			}
			
			h = Mathf.Repeat(h * 60f, 360f);
		}
		
		return new HSLColor(h, s, l, a);
	}
	
	
	public Color ToRGBA() {
		float r, g, b, a;
		a = this.a;
		
		float m1, m2;
	
	//	Note: there is a typo in the 2nd International Edition of Foley and
	//	van Dam's "Computer Graphics: Principles and Practice", section 13.3.5
	//	(The HLS Color Model). This incorrectly replaces the 1f in the following
	//	line with "l", giving confusing results.
		m2 = (l <= .5f) ? (l * (1f + s)) : (l + s - l * s);
		m1 = 2f * l - m2;
		
		if (s == 0f) {
			r = g = b = l;
		} else {
			r = Value(m1, m2, h + 120f);
			g = Value(m1, m2, h);
			b = Value(m1, m2, h - 120f);
		}
		
		return new Color(r, g, b, a);
	}
	
	static float Value(float n1, float n2, float hue) {
		hue = Mathf.Repeat(hue, 360f);
		
		if (hue < 60f) {
			return n1 + (n2 - n1) * hue / 60f;
		} else if (hue < 180f) {
			return n2;
		} else if (hue < 240f) {
			return n1 + (n2 - n1) * (240f - hue) / 60f;
		} else {
			return n1;
		}
	}

	public static HSLColor Lerp(HSLColor c1, HSLColor c2, float step) {
		return new HSLColor(Mathf.Lerp(c1.h, c2.h, step), Mathf.Lerp(c1.s, c2.s, step), Mathf.Lerp(c1.l, c2.l, step), Mathf.Lerp(c1.a, c2.a, step));
	}
	public static HSLColor MoveTowards(HSLColor c1, HSLColor c2, float maxDelta) {
		return new HSLColor(Mathf.MoveTowards(c1.h, c2.h, maxDelta*180), Mathf.MoveTowards(c1.s, c2.s, maxDelta), Mathf.MoveTowards(c1.l, c2.l, maxDelta), Mathf.MoveTowards(c1.a, c2.a, maxDelta));
	}
	

    public static HSLColor Add(HSLColor left, HSLColor right){
		return new HSLColor(left.h+right.h, left.s+right.s, left.l+right.l, left.a+right.a);
	}

	public static HSLColor Subtract(HSLColor left, HSLColor right){
		return new HSLColor(left.h-right.h, left.s-right.s, left.l-right.l, left.a-right.a);
	}
    
    public override bool Equals(System.Object obj) {
		return obj is HSLColor && this == (HSLColor)obj;
	}

	public bool Equals(HSLColor p) {
		return h == p.h && s == p.s && l == p.l && a == p.a;
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * 31 + h.GetHashCode();
			hash = hash * 31 + s.GetHashCode();
			hash = hash * 31 + l.GetHashCode();
			hash = hash * 31 + a.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (HSLColor left, HSLColor right) {
		return left.Equals(right);
	}

	public static bool operator != (HSLColor left, HSLColor right) {
		return !(left == right);
	}

	public static HSLColor operator +(HSLColor left, HSLColor right) {
		return Add(left, right);
	}

	public static HSLColor operator -(HSLColor left) {
		return new HSLColor(-left.h, -left.s, -left.l, -left.a);
	}

	public static HSLColor operator -(HSLColor left, HSLColor right) {
		return Subtract(left, right);
	}

	public static implicit operator HSLColor(Color src) {
		return FromRGBA(src);
	}
	
	public static implicit operator Color(HSLColor src) {
		return src.ToRGBA();
	}
}