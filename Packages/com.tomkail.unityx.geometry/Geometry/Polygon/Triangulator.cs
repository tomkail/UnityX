using System.Collections.Generic;
using UnityEngine;

// Ear-clipping triangulation for simple 2D polygons, optionally with holes, shipped alongside Polygon so
// UnityX.Geometry is self-contained (no external triangulation dependency). Only the Vector2 path is included — the
// whole project triangulates from Vector2 vertex rings. Swap the body here for a library if you need robustness
// on self-intersecting polygons. Note: _indicesScratch is shared static state, so GenerateIndices
// is not re-entrant or thread-safe (unchanged from the original).
public static class Triangulator {
	public static void GenerateIndices(IList<Vector2> points, List<int> outputIndices) {

		int n = points.Count;
		if (n < 3) return;

		Debug.Assert(outputIndices.Count == 0);

		_indicesScratch.Clear();

		if (SignedArea(points) > 0) {
			for (int v = 0; v < n; v++)
				_indicesScratch.Add(v);
		}
		else {
			for (int v = 0; v < n; v++)
				_indicesScratch.Add((n - 1) - v);
		}

		EarClip(points, outputIndices);
	}

	// Triangulates an outer loop with holes cut out of it. Either winding is accepted for any loop.
	// Indices refer to the concatenation [outer..., holes[0]..., holes[1]..., ...], so callers can keep their own
	// per-vertex data (heights, colours) in the same order. Holes must lie inside the outer loop and not touch each other.
	// Each hole is joined to the outer loop by a zero-width bridge (Eberly, "Triangulation by Ear Clipping", 2002),
	// which turns the shape into one loop the ordinary ear-clipper can handle; the bridge's two ends appear twice in that loop.
	public static void GenerateIndices(IList<Vector2> outer, IList<IList<Vector2>> holes, List<int> outputIndices) {
		if (holes == null || holes.Count == 0) {
			GenerateIndices(outer, outputIndices);
			return;
		}
		if (outer.Count < 3) return;
		Debug.Assert(outputIndices.Count == 0);

		var points = new List<Vector2>(outer);
		var holeStarts = new List<int>();
		foreach (var hole in holes) {
			holeStarts.Add(points.Count);
			points.AddRange(hole);
		}

		// The ear-clipper wants the outer loop counter-clockwise (positive area), which makes holes clockwise.
		_indicesScratch.Clear();
		AddLoop(_indicesScratch, 0, outer.Count, SignedArea(outer) < 0);

		// Bridge the holes rightmost-first, so each bridge only has to see past holes that are already part of the loop.
		var order = new List<int>();
		for (int h = 0; h < holes.Count; h++) if (holes[h].Count >= 3) order.Add(h);
		order.Sort((a, b) => MaxX(holes[b]).CompareTo(MaxX(holes[a])));
		var holeRing = new List<int>();
		foreach (var h in order) {
			holeRing.Clear();
			AddLoop(holeRing, holeStarts[h], holes[h].Count, SignedArea(holes[h]) > 0);
			BridgeHole(points, _indicesScratch, holeRing);
		}

		EarClip(points, outputIndices, bridged: true);
	}

	static void AddLoop (List<int> ring, int start, int count, bool reverse) {
		for (int v = 0; v < count; v++)
			ring.Add(start + (reverse ? (count - 1) - v : v));
	}

	static float MaxX (IList<Vector2> loop) {
		float max = float.MinValue;
		foreach (var p in loop) max = Mathf.Max(max, p.x);
		return max;
	}

	// Splices `hole` into `ring` through a bridge from the hole's rightmost vertex M to a ring vertex P that M can see.
	static void BridgeHole (List<Vector2> points, List<int> ring, List<int> hole) {
		int mi = 0;
		for (int i = 1; i < hole.Count; i++)
			if (points[hole[i]].x > points[hole[mi]].x) mi = i;
		Vector2 M = points[hole[mi]];

		// Cast a ray in +x from M and find the nearest ring edge it hits, at I.
		int pi = -1;
		float bestX = float.MaxValue;
		for (int i = 0; i < ring.Count; i++) {
			Vector2 a = points[ring[i]];
			Vector2 b = points[ring[(i + 1) % ring.Count]];
			if ((a.y > M.y) == (b.y > M.y)) continue;
			float x = a.x + (M.y - a.y) * (b.x - a.x) / (b.y - a.y);
			if (x < M.x || x >= bestX) continue;
			bestX = x;
			pi = a.x > b.x ? i : (i + 1) % ring.Count;
		}
		if (pi == -1) {
			Debug.LogWarning("Triangulator: hole isn't inside the outer loop; ignoring it.");
			return;
		}
		Vector2 I = new Vector2(bestX, M.y);
		Vector2 P = points[ring[pi]];

		// Another ring vertex inside triangle M-I-P could block the view of P. If so, use the one closest in angle
		// to the ray instead (nearest first on ties); nothing can sit between it and M.
		// The x bounds matter when the ray hits a vertex exactly (common on grids): M-I-P is then a line, and the
		// inclusive triangle test would otherwise accept vertices on that line behind M.
		float maxX = Mathf.Max(I.x, P.x);
		float bestAngle = Mathf.Abs(Mathf.Atan2(P.y - M.y, P.x - M.x));
		float bestDistance = (P - M).sqrMagnitude;
		for (int i = 0; i < ring.Count; i++) {
			if (i == pi) continue;
			Vector2 R = points[ring[i]];
			if (R.x <= M.x || R.x > maxX || !InsideTriangleEitherWinding(M, I, P, R)) continue;
			float angle = Mathf.Abs(Mathf.Atan2(R.y - M.y, R.x - M.x));
			float distance = (R - M).sqrMagnitude;
			if (angle < bestAngle || (angle == bestAngle && distance < bestDistance)) {
				bestAngle = angle;
				bestDistance = distance;
				pi = i;
			}
		}

		// ring[..P], hole from M round to M, back to P, ring[P+1..]
		var splice = new List<int>(hole.Count + 2);
		for (int i = 0; i <= hole.Count; i++) splice.Add(hole[(mi + i) % hole.Count]);
		splice.Add(ring[pi]);
		ring.InsertRange(pi + 1, splice);
	}

	// A-B-C has no area and B doubles back on itself (or repeats a neighbour), rather than sitting on a straight run.
	static bool IsSpike (Vector2 A, Vector2 B, Vector2 C) {
		const float tolerance = 1e-6f;
		float cross = (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x);
		if (Mathf.Abs(cross) > tolerance * Mathf.Max(1f, (C - A).sqrMagnitude, (B - A).sqrMagnitude)) return false;
		return Vector2.Dot(B - A, C - B) <= 0;
	}

	// In a bridged loop a vertex appears twice, and an edge leaving its other copy can cut across an ear's new
	// diagonal u-w without putting any vertex inside the ear, which Snip alone can't see.
	static bool DiagonalCrossesRing (IList<Vector2> points, int u, int w, int nv) {
		Vector2 A = points[_indicesScratch[u]];
		Vector2 C = points[_indicesScratch[w]];
		for (int p = 0; p < nv; p++) {
			int q = (p + 1) % nv;
			Vector2 P = points[_indicesScratch[p]];
			Vector2 Q = points[_indicesScratch[q]];
			if (P == A || P == C || Q == A || Q == C) continue;
			if (SegmentsCross(A, C, P, Q)) return true;
		}
		return false;
	}

	// Proper crossing only; touching at an end or running collinear doesn't count.
	static bool SegmentsCross (Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
		float Cross (Vector2 o, Vector2 x, Vector2 y) => (x.x - o.x) * (y.y - o.y) - (x.y - o.y) * (y.x - o.x);
		float d1 = Cross(c, d, a), d2 = Cross(c, d, b), d3 = Cross(a, b, c), d4 = Cross(a, b, d);
		return ((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) && ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0));
	}

	static bool InsideTriangleEitherWinding (Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
		return InsideTriangle(A, B, C, P) || InsideTriangle(A, C, B, P);
	}

	// Clips ears off the loop in _indicesScratch, which must be counter-clockwise.
	static void EarClip (IList<Vector2> points, List<int> outputIndices, bool bridged = false) {
		int nv = _indicesScratch.Count;
		int count = 2 * nv;
		for (int m = 0, v = nv - 1; nv > 2; ) {
			if ((count--) <= 0)
				return;

			int u = v;
			if (nv <= u)
				u = 0;
			v = u + 1;
			if (nv <= v)
				v = 0;
			int w = v + 1;
			if (nv <= w)
				w = 0;

			// Bridges leave zero-width spikes behind as ears are clipped; nothing can ever clip them, so drop the tip
			// without emitting a triangle. Only for bridged loops, so plain polygons triangulate exactly as before.
			if (bridged && IsSpike(points[_indicesScratch[u]], points[_indicesScratch[v]], points[_indicesScratch[w]])) {
				for (int s = v, t = v + 1; t < nv; s++, t++)
					_indicesScratch[s] = _indicesScratch[t];
				nv--;
				count = 2 * nv;
				continue;
			}

			if (Snip(points, u, v, w, nv) && (!bridged || !DiagonalCrossesRing(points, u, w, nv))) {
				int a, b, c, s, t;
				a = _indicesScratch[u];
				b = _indicesScratch[v];
				c = _indicesScratch[w];
				outputIndices.Add(a);
				outputIndices.Add(b);
				outputIndices.Add(c);
				m++;
				for (s = v, t = v + 1; t < nv; s++, t++)
					_indicesScratch[s] = _indicesScratch[t];
				nv--;
				count = 2 * nv;
			}
		}

		outputIndices.Reverse();
	}

	public static float SignedArea (IList<Vector2> points) {
		int n = points.Count;
		float A = 0.0f;
		for (int p = n - 1, q = 0; q < n; p = q++) {
			Vector2 pval = points[p];
			Vector2 qval = points[q];
			A += pval.x * qval.y - qval.x * pval.y;
		}
		return (A * 0.5f);
	}

	static bool Snip (IList<Vector2> points, int u, int v, int w, int n) {
		int p;
		Vector2 A = points[_indicesScratch[u]];
		Vector2 B = points[_indicesScratch[v]];
		Vector2 C = points[_indicesScratch[w]];
		if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
			return false;
		for (p = 0; p < n; p++) {
			if ((p == u) || (p == v) || (p == w))
				continue;
			// A bridged loop visits the bridge's ends twice; the second visit isn't in the way of the first.
			int pIndex = _indicesScratch[p];
			if (pIndex == _indicesScratch[u] || pIndex == _indicesScratch[v] || pIndex == _indicesScratch[w])
				continue;
			Vector2 P = points[pIndex];
			if (InsideTriangle(A, B, C, P))
				return false;
		}
		return true;
	}

	static bool InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
		float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
		float cCROSSap, bCROSScp, aCROSSbp;

		ax = C.x - B.x; ay = C.y - B.y;
		bx = A.x - C.x; by = A.y - C.y;
		cx = B.x - A.x; cy = B.y - A.y;
		apx = P.x - A.x; apy = P.y - A.y;
		bpx = P.x - B.x; bpy = P.y - B.y;
		cpx = P.x - C.x; cpy = P.y - C.y;

		aCROSSbp = ax * bpy - ay * bpx;
		cCROSSap = cx * apy - cy * apx;
		bCROSScp = bx * cpy - by * cpx;

		return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
	}

	static List<int> _indicesScratch = new(256);
}
