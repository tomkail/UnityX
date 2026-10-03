using System.Collections.Generic;

namespace UnityX.HexGrid {
// Edge lookup from the cell side. Edge i is the side facing Neighbor(i), so it's shared with that neighbour's
// edge i+3. See HexCoordEdge for going from an edge back to its cells.
public partial struct HexCoord {

	/// <summary>
	/// The edge facing direction <paramref name="edgeIndex"/>, shared with <c>Neighbor(edgeIndex)</c>.
	/// </summary>
	public HexCoordEdge Edge(int edgeIndex) {
		return new HexCoordEdge(this, edgeIndex);
	}

	/// <summary>
	/// Enumerate this hex's six edges, indexed by the direction each faces.
	/// </summary>
	public IEnumerable<HexCoordEdge> Edges(int first = 0) {
		for (int i = 0; i < 6; i++)
			yield return Edge(first + i);
	}

	/// <summary>
	/// Index of the edge of <paramref name="a"/> shared with <paramref name="b"/>, or -1 if they aren't neighbours.
	/// </summary>
	public static int EdgeIndexBetween(HexCoord a, HexCoord b) {
		if (Distance(a, b) != 1) return -1;
		return a.NeighborIndexOf(b - a);
	}
}
}
