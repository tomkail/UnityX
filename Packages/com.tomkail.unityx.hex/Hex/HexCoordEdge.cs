using System.Linq;

namespace UnityX.HexGrid {
[System.Serializable]
public struct HexCoordEdge : System.IEquatable<HexCoordEdge> {
    public HexCoordVert start;
    public HexCoordVert end;

    public bool SeparatesCoords (HexCoord coordA, HexCoord coordB) {
		var sharedVerts = HexCoord.GetCornerIndiciesSharedWithOther(coordA, coordB);
        var thisCoord = this;
        return sharedVerts.Length == 2 && sharedVerts.All((vertIndex) => {
            var c = new HexCoordVert(coordA, vertIndex);
            return thisCoord.start == c || thisCoord.end == c;
        });
	}

    // Edges are undirected: {A,B} equals {B,A}. Equality and hash are order-independent so an edge
    // and its reverse are interchangeable as dictionary/hashset keys.
    public bool Equals(HexCoordEdge other) {
        return (start == other.start && end == other.end) || (start == other.end && end == other.start);
    }
    public override bool Equals(object obj) {
        return obj is HexCoordEdge other && Equals(other);
    }
    public override int GetHashCode() {
        return start.GetHashCode() ^ end.GetHashCode();
    }
    public static bool operator == (HexCoordEdge left, HexCoordEdge right) {
        return left.Equals(right);
    }
    public static bool operator != (HexCoordEdge left, HexCoordEdge right) {
        return !left.Equals(right);
    }
}
}