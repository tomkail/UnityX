using UnityEngine;
using System.Collections;

namespace UnityX.Geometry {

	/// <summary>
	/// Regular polygon.
	/// </summary>
	[System.Serializable]
	public class RegularPolygon {
	
		private int numVertices;
		public int NumVertices {
			get {
				return numVertices;
			}
			set {
				numVertices = Mathf.RoundToInt(Mathf.Clamp (value, 3, Mathf.Infinity));
			}
		}

		public float rotation = 0f;
		public float radius = 0.5f;
		public Vector2 offset = Vector2.zero;
		
		public RegularPolygon (int numVertices) {
			this.NumVertices = numVertices;
		}
		
		public RegularPolygon (int numVertices, float rotation) {
			this.NumVertices = numVertices;
			this.rotation = rotation;
		}
		
		public RegularPolygon (int numVertices, float rotation, float radius) {
			this.NumVertices = numVertices;
			this.rotation = rotation;
			this.radius = radius;
		}
		
		public Vector2[] ToPolygonVerts () {
			Vector2[] vertices = new Vector2[NumVertices];
			for(int i = 0; i < NumVertices; i++) {
				var radians = i/(float)NumVertices * Mathf.PI * 2;
				radians += Mathf.Deg2Rad * rotation;
				vertices[i] = offset + new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
			}
			return vertices;
		}
		public Polygon ToPolygon () {
			return new Polygon(ToPolygonVerts());
		}
		
		public static implicit operator Polygon(RegularPolygon src) {
			return src.ToPolygon();
		}
	}
}