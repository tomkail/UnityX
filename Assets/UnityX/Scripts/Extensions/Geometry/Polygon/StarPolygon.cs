using UnityEngine;
using System.Collections;

namespace UnityX.Geometry {

	[System.Serializable]
	/// <summary>
	/// Star polygon. Creates a polygon from the number of points, and the number of points to skip. 
	/// Not all combinations of points/skip create a star polygon.
	/// More information about star polygons 
	/// http://en.wikipedia.org/wiki/Star_polygon
	/// http://en.wikipedia.org/wiki/Schl%C3%A4fli_symbol
	/// </summary>
	public class StarPolygon {
		
		private int numVertices;
		public int NumVertices {
			get {
				return numVertices;
			}
			set {
				numVertices = Mathf.RoundToInt(Mathf.Clamp (value, 3, Mathf.Infinity));
			}
		}
		
		private int skip;
		public int Skip {
			get {
				return skip;
			}
			set {
				skip = Mathf.RoundToInt(Mathf.Clamp (value, 1, Mathf.Infinity));
			}
		}
		
		public float rotation = 0f;
		public float radius = 0.5f;
		public float concaveRadius;
		public Vector2 offset = Vector2.zero;
		
		public StarPolygon (int numVertices, int skip) {
			this.NumVertices = numVertices;
			this.Skip = skip;
			this.concaveRadius = CalculateConcaveRadius();
		}
		
		public StarPolygon (int numVertices, int skip, float rotation) {
			this.NumVertices = numVertices;
			this.Skip = skip;
			this.rotation = rotation;
			this.concaveRadius = CalculateConcaveRadius();
		}
		
		public StarPolygon (int numVertices, int skip, float rotation, float radius) {
			this.NumVertices = numVertices;
			this.Skip = skip;
			this.rotation = rotation;
			this.radius = radius;
			this.concaveRadius = CalculateConcaveRadius();
		}
		
		
		public StarPolygon (int numVertices, int skip, float rotation, float radius, float concaveRadius) {
			this.NumVertices = numVertices;
			this.Skip = skip;
			this.rotation = rotation;
			this.radius = radius;
			this.concaveRadius = concaveRadius;
		}
		
		public Polygon ToPolygon () {
			// If this is a polygon, don't bother with concave points.
			if (Skip == 1) {
				return RegularPolygonToPolygon();
			} else {
				return StarPolygonToPolygon();
			}
		}
		
		public static implicit operator Polygon(StarPolygon src) {
			return src.ToPolygon();
		}
		
		
		private Polygon RegularPolygonToPolygon () {
			Vector2[] vertices = new Vector2[NumVertices];
			for(int i = 0; i < NumVertices; i++) {
				var radians = (i/(float)NumVertices) * Mathf.PI * 2;
				var dir = new Vector2(Mathf.Sin(radians), Mathf.Sin(radians));
				vertices[i] = offset + dir * radius;
			}
			return new Polygon(vertices);
		}
		
		private Polygon StarPolygonToPolygon () {
			int calculatedNumVerts = 2 * NumVertices;
			Vector2[] vertices = new Vector2[calculatedNumVerts];
			for (int i = 0; i < calculatedNumVerts; i += 2) {
				var radians = (i/(float)calculatedNumVerts) * Mathf.PI * 2;
				var dir = new Vector2(Mathf.Sin(radians), Mathf.Sin(radians));
				vertices[i] = offset + dir * radius;
				
				radians = ((i + 1)/(float)calculatedNumVerts) * Mathf.PI * 2;
				dir = new Vector2(Mathf.Sin(radians), Mathf.Sin(radians));
				vertices[i + 1] = offset + dir * concaveRadius;
			}
			return new Polygon(vertices);
		}
		
		// Calculate the inner star radius.
		private float CalculateConcaveRadius() {
		    // For really small numbers of points.
			if (NumVertices < 5) return radius * 0.333f;
	
		    // Calculate angles to key points.
		    float dtheta = 2 * Mathf.PI / NumVertices;
		    float theta00 = -Mathf.PI / 2;
		    float theta01 = theta00 + dtheta * Skip;
		    float theta10 = theta00 + dtheta;
		    float theta11 = theta10 - dtheta * Skip;
	
		    // Find the key points.
			Vector2 pt00 = new Vector2(Mathf.Sin(theta00), Mathf.Cos(theta00));
			Vector2 pt01 = new Vector2(Mathf.Sin(theta01), Mathf.Cos(theta01));
			Vector2 pt10 = new Vector2(Mathf.Sin(theta10), Mathf.Cos(theta10));
			Vector2 pt11 = new Vector2(Mathf.Sin(theta11), Mathf.Cos(theta11));
	
		    // See where the segments connecting the points intersect.
		    bool lines_intersect, segments_intersect;
		    Vector2 intersection, close_p1, close_p2;
		    FindIntersection(pt00, pt01, pt10, pt11, out lines_intersect, out segments_intersect, out intersection, out close_p1, out close_p2);
	
		    // Calculate the distance between the
		    // point of intersection and the center.
		    return radius * Mathf.Sqrt(intersection.x * intersection.x + intersection.y * intersection.y);
		}
	
		private void FindIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out bool lines_intersect, out bool segments_intersect, out Vector2 intersection, out Vector2 close_p1, out Vector2 close_p2) {
		    // Get the segments' parameters.
		    float dx12 = p2.x - p1.x;
		    float dy12 = p2.y - p1.y;
		    float dx34 = p4.x - p3.x;
		    float dy34 = p4.y - p3.y;
	
		    // Solve for t1 and t2
		    float denominator = (dy12 * dx34 - dx12 * dy34);
	
		    float t1 = ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34) / denominator;
		    if (float.IsInfinity(t1)) {
		        // The lines are parallel (or close enough to it).
		        lines_intersect = false;
		        segments_intersect = false;
		        intersection = new Vector2(float.NaN, float.NaN);
		        close_p1 = new Vector2(float.NaN, float.NaN);
		        close_p2 = new Vector2(float.NaN, float.NaN);
		        return;
		    }
		    lines_intersect = true;
	
		    float t2 = ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12) / -denominator;
	
		    // Find the point of intersection.
		    intersection = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);
	
		    // The segments intersect if t1 and t2 are between 0 and 1.
		    segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));
	
		    // Find the closest points on the segments.
		    if (t1 < 0) {
		        t1 = 0;
		    } else if (t1 > 1) {
		        t1 = 1;
		    }
	
		    if (t2 < 0) {
		        t2 = 0;
		    } else if (t2 > 1) {
		        t2 = 1;
		    }
		    
		    close_p1 = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);
		    close_p2 = new Vector2(p3.x + dx34 * t2, p3.y + dy34 * t2);
		}
	}
}