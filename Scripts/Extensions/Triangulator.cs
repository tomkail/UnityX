using System.Collections.Generic;
using UnityEngine;

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
	
		int nv = n;
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
	
			if (Snip(points, u, v, w, nv)) {
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
	// These points are assumed to be Vector2s, and are only stored as Vector3s for performance reasons
	public static void GenerateIndices(IList<Vector3> points, List<int> outputIndices) {
	
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
	
		int nv = n;
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
	
			if (Snip(points, u, v, w, nv)) {
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
	
	// These points are assumed to be Vector2s, and are only stored as Vector3s for performance reasons
	public static float SignedArea (IList<Vector3> points) {
		int n = points.Count;
		float A = 0.0f;
		for (int p = n - 1, q = 0; q < n; p = q++) {
			Vector2 pval = points[p];
			Vector2 qval = points[q];
			A += pval.x * qval.y - qval.x * pval.y;
		}
		return (A * 0.5f);
	}
	// These points are assumed to be Vector2s, and are only stored as Vector3s for performance reasons
	public static float Area (IList<Vector3> points) {
		return Mathf.Abs(SignedArea(points));
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
	public static float Area (IList<Vector2> points) {
		return Mathf.Abs(SignedArea(points));
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
			Vector2 P = points[_indicesScratch[p]];
			if (InsideTriangle(A, B, C, P))
				return false;
		}
		return true;
	}
	static bool Snip (IList<Vector3> points, int u, int v, int w, int n) {
		int p;
		Vector2 A = points[_indicesScratch[u]];
		Vector2 B = points[_indicesScratch[v]];
		Vector2 C = points[_indicesScratch[w]];
		if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
			return false;
		for (p = 0; p < n; p++) {
			if ((p == u) || (p == v) || (p == w))
				continue;
			Vector2 P = points[_indicesScratch[p]];
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