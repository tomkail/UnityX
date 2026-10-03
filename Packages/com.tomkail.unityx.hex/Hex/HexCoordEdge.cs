using System.Linq;
using UnityEngine;

namespace UnityX.HexGrid {
[System.Serializable]
public struct HexCoordEdge : System.IEquatable<HexCoordEdge> {
    public HexCoordVert start;
    public HexCoordVert end;

    public HexCoordEdge (HexCoordVert start, HexCoordVert end) {
        this.start = start;
        this.end = end;
    }

    // Edge `edgeIndex` of `coord`: the side facing Neighbor(edgeIndex), running between its two flanking corners.
    public HexCoordEdge (HexCoord coord, int edgeIndex) {
        var corners = HexCoord.CornersSharingEdgeIndex(edgeIndex);
        start = new HexCoordVert(coord, corners[0]);
        end = new HexCoordVert(coord, corners[1]);
    }

    // The edge shared by two neighbouring cells.
    public static HexCoordEdge Between (HexCoord a, HexCoord b) {
        var edgeIndex = HexCoord.EdgeIndexBetween(a, b);
        if(edgeIndex == -1) throw new System.ArgumentException($"{a} and {b} aren't neighbours, so share no edge");
        return new HexCoordEdge(a, edgeIndex);
    }

    public Vector2 Midpoint () {
        return (start.Position() + end.Position()) * 0.5f;
    }

    // The two cells either side of this edge, in no particular order. Steps half a unit off the midpoint, square to
    // the edge, each way: cell centres sit sqrt(3)/2 from the midpoint, so both steps land well inside their cell.
    public HexCoord[] GetCoords () {
        var along = end.Position() - start.Position();
        var across = new Vector2(-along.y, along.x).normalized * 0.5f;
        var midpoint = Midpoint();
        return new HexCoord[2] { HexCoord.AtPosition(midpoint + across), HexCoord.AtPosition(midpoint - across) };
    }

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