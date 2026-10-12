using UnityEngine;

namespace UnityX.HexGrid {
[System.Serializable]
public struct HexCoordVert : System.IEquatable<HexCoordVert> {
    public int x, y;
    public HexCoordVert(int _x, int _y) {
		x = _x;
		y = _y;
	}
	public HexCoordVert(Vector2Int point) {
		x = point.x;
		y = point.y;
	}

	public static bool Valid (HexCoord cornerPointTriad1, HexCoord cornerPointTriad2, HexCoord cornerPointTriad3) {
		var sharedIndicies = HexCoord.GetCornerIndiciesSharedWithOthers(cornerPointTriad1, cornerPointTriad2, cornerPointTriad3);
        return sharedIndicies.Length == 1;
	}

	public HexCoordVert (HexCoord cornerPointTriad1, HexCoord cornerPointTriad2, HexCoord cornerPointTriad3) {
		// Debug.Assert(HexCoord.Distance(cornerPointTriad1, cornerPointTriad2) == 1 && HexCoord.Distance(cornerPointTriad1, cornerPointTriad3) == 1 && HexCoord.Distance(cornerPointTriad2, cornerPointTriad3) == 1);
		var sharedIndicies = HexCoord.GetCornerIndiciesSharedWithOthers(cornerPointTriad1, cornerPointTriad2, cornerPointTriad3);
        if(sharedIndicies.Length == 1) {
            var corner = cornerPointTriad1.Corner(sharedIndicies[0]);
            var point = RoundFrom(PositionToHexCornerPoint(corner));
            x = point.x;
            y = point.y;
        } else {
			x = 0; y = 0;
			Debug.LogError("Points not adjacent "+cornerPointTriad1+" "+cornerPointTriad2+" "+cornerPointTriad3);
		}
	}
	
	public HexCoordVert (HexCoord hex, int cornerIndex) {
        var position = hex.Corner(cornerIndex);
		var point = RoundFrom(PositionToHexCornerPoint(position));
		x = point.x;
		y = point.y;
	}

    public static HexCoordVert RoundFrom (Vector2 pos) {
        return new HexCoordVert(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }


	public Vector2 Position () {
		return HexCornerPointToPosition(new Vector2(x,y));
	}

	public static Vector2 HexCornerPointToPosition (Vector2 point) {
		// Vector2 cellSize;
		// pos = Vector2.zero;
		// if(HexCoord.orientation == HexCoord.Orientation.Flat) {
		// 	// cellSize = new Vector2(2f/4f, HexCoord.SQRT3*0.5f);
		// 	// offset = new Vector2(-cellSize.x * 2, -cellSize.y);
		// } else {
		// 	pos += point.x * new Vector2(HexCoord.SQRT3 * 0.5f, 0);
		// 	pos += Mathf.FloorToInt(point.y * 0.5f) * new Vector2(0, 1);
		// 	if(point.x % 2 == 0) {
		// 		pos += new Vector2(0, 1 * 0.5f);
		// 	}
		// }
		Vector2 pos = point.x*Q_XY + point.y*R_XY;
		return pos + offset;
	}

    public static Vector2 PositionToHexCornerPoint (Vector2 position) {
		position -= offset;
        Vector2 vPoint = position.x*X_QR + position.y*Y_QR;
        // Round to the nearest integer corner point (identical to the old Point(Vector2) conversion).
        return new Vector2(Mathf.RoundToInt(vPoint.x), Mathf.RoundToInt(vPoint.y));
	}

    static Vector2 offset {
        get {
            if(HexCoord.orientation == HexCoord.Orientation.Flat) {
                return new Vector2(-0.25f,-HexCoord.SQRT3 * 0.75f);
            } else {
                return new Vector2(-HexCoord.SQRT3 * 0.5f, -1f);
            }    
        }
    }
	
    static Vector2 Q_XY {
		get {
			if(HexCoord.orientation == HexCoord.Orientation.Flat) return Q_XY_Flat;
			else return Q_XY_Pointy;
		}
	}
	static Vector2 R_XY {
		get {
			if(HexCoord.orientation == HexCoord.Orientation.Flat) return R_XY_Flat;
			else return R_XY_Pointy;
		}
	}
	static readonly Vector2 Q_XY_Pointy = new Vector2(HexCoord.SQRT3/2, 0);
	static readonly Vector2 R_XY_Pointy = new Vector2(0, 0.5f);
	static readonly Vector2 Q_XY_Flat = new Vector2(0.75f, HexCoord.SQRT3/4);
	static readonly Vector2 R_XY_Flat = new Vector2(-HexCoord.SQRT3/8, 0.75f/2);

    static Vector2 X_QR {
		get {
			if(HexCoord.orientation == HexCoord.Orientation.Flat) return X_QR_Flat;
			else return X_QR_Pointy;
		}
	}
	static Vector2 Y_QR {
		get {
			if(HexCoord.orientation == HexCoord.Orientation.Flat) return Y_QR_Flat;
			else return Y_QR_Pointy;
		}
	}
	public static readonly Vector2 X_QR_Pointy = new Vector2(HexCoord.SQRT3/1.5f, 0);
	public static readonly Vector2 Y_QR_Pointy = new Vector2(0, 2);
	public static readonly Vector2 X_QR_Flat = new Vector2(1, -HexCoord.SQRT3/1.5f);
	public static readonly Vector2 Y_QR_Flat = new Vector2(HexCoord.SQRT3/3f, 2);






    public static Vector2Int ToPoint(HexCoordVert point) {
		return new Vector2Int(point.x, point.y);
	}

    public Vector2Int ToPoint() {
		return ToPoint(this);
	}

    public static HexCoordVert FromPoint(Vector2Int vector) {
		return new HexCoordVert(vector.x, vector.y);
	}

	public bool Equals(HexCoordVert other) {
		return x == other.x && y == other.y;
	}

	public override bool Equals(System.Object obj) {
		return obj is HexCoordVert other && Equals(other);
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 17;
			hash = hash * 31 + x;
			hash = hash * 31 + y;
			return hash;
		}
	}

	public static bool operator == (HexCoordVert left, HexCoordVert right) {
		return left.x == right.x && left.y == right.y;
	}

	public static bool operator != (HexCoordVert left, HexCoordVert right) {
		return !(left == right);
	}

	public static HexCoordVert operator +(HexCoordVert left, HexCoordVert right) {
		return new HexCoordVert(left.x + right.x, left.y + right.y);
	}

	public static HexCoordVert operator -(HexCoordVert left) {
		return new HexCoordVert(-left.x, -left.y);
	}

	public static HexCoordVert operator -(HexCoordVert left, HexCoordVert right) {
		return new HexCoordVert(left.x - right.x, left.y - right.y);
	}


	public static HexCoordVert operator *(HexCoordVert left, HexCoordVert right) {
		return new HexCoordVert(left.x * right.x, left.y * right.y);
	}


	public static HexCoordVert operator /(HexCoordVert left, HexCoordVert right) {
		return new HexCoordVert(left.x / right.x, left.y / right.y);
	}

	public static implicit operator HexCoordVert(Vector2Int src) {
		return FromPoint(src);
	}

	public static implicit operator Vector2Int(HexCoordVert src) {
		return src.ToPoint();
	}




	public override string ToString() {
		return "(" + x + ", " + y+")";
	}
}
}
