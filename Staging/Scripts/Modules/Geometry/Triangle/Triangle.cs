using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityX.Geometry {
	[System.Serializable]
	public struct Triangle {
		public Vector2 a;
		public Vector2 b;
		public Vector2 c;
		public Line ab;
		public Line bc;
		public Line ca;
		// longest side
		public float @base;
		// line extending from the longest side to the opposite point
		public float height;

		public float area;

		public Triangle (Vector2 a, Vector2 b, Vector2 c) {
			this.a = a;
			this.b = b;
			this.c = c;

			ab = new Line(a,b);
			bc = new Line(b,c);
			ca = new Line(c,a);
			
			var abSqrL = ab.sqrLength;
			var bcSqrL = bc.sqrLength;
			var caSqrL = ca.sqrLength;

			if(bcSqrL > abSqrL && bcSqrL > caSqrL) {
				@base = bc.length;
				height = bc.GetClosestDistanceFromLine(a);
			} else if(caSqrL > abSqrL && caSqrL > bcSqrL) {
				@base = ca.length;
				height = ca.GetClosestDistanceFromLine(b);
			} else {
				@base = ab.length;
				height = ab.GetClosestDistanceFromLine(c);
			}

			area = height * @base * 0.5f;
		}

		public bool ContainsPoint (Vector2 p) {
			return ContainsPoint(a,b,c,p);
		}
		
		public static bool ContainsPoint (Vector2 a, Vector2 b, Vector2 c, Vector2 p) {
			float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
			float cCROSSap, bCROSScp, aCROSSbp;
		
			ax = c.x - b.x; ay = c.y - b.y;
			bx = a.x - c.x; by = a.y - c.y;
			cx = b.x - a.x; cy = b.y - a.y;
			apx = p.x - a.x; apy = p.y - a.y;
			bpx = p.x - b.x; bpy = p.y - b.y;
			cpx = p.x - c.x; cpy = p.y - c.y;
		
			aCROSSbp = ax * bpy - ay * bpx;
			cCROSSap = cx * apy - cy * apx;
			bCROSScp = bx * cpy - by * cpx;
		
			return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
		}

		public Vector2 RandomPoint () {
			var r1 = Mathf.Sqrt(Random.Range(0f, 1f));
			var r2 = Random.Range(0f, 1f);
			var m1 = 1 - r1;
			var m2 = r1 * (1 - r2);
			var m3 = r2 * r1;
			
			return (m1 * a) + (m2 * b) + (m3 * c);
		}
	}
}