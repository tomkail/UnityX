using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UnityX.Geometry
{

    [System.Serializable]
	public class Polygon {
		
		/// <summary>
		/// The vertices.
		/// </summary>
		[SerializeField]
		Vector2[] _vertices;
		public Vector2[] vertices {
			get {
				return _vertices;
			} set {
				Debug.Assert(value != null);
				_vertices = value;
			}
		}

		/// <summary>
		///  Centre (average of all points)
		/// </summary>
		/// <value>The center.</value>
		public Vector2 center {
			get {
				if( _vertices == null || _vertices.Length == 0 ) return Vector2.zero;
				Vector2 _center = Vector2.zero;
				for(int i = 0; i < _vertices.Length; i++) _center += _vertices[i];
				_center.x /= _vertices.Length;
				_center.y /= _vertices.Length;
				return _center;
			}
		}

		public bool GetIsClockwise () {
			float x = 0;
			Vector2 current = _vertices[_vertices.Length-1];
			Vector2 next = _vertices[0];
			x += (next.x-current.x) * (next.y+current.y);
			for (int i = 1; i < _vertices.Length; i++) {
				current = next;
				next = _vertices[i];
				x += (next.x-current.x) * (next.y+current.y);
			}
			return x >= 0;
		}

		public void ReverseDirection () {
			System.Array.Reverse(_vertices);
		}
		
		/// <summary>
		///  Centroid (the place where the perpendiculars from each vertex intersect)
		/// </summary>
		Vector2 centroid {
			get { 
				float computedArea = 0.0f;
				float x = 0.0f;
				float y = 0.0f;

				for (int i = 0; i < _vertices.Length - 1; i++) {
					var a = _vertices [i];
					var b = i == _vertices.Length ? _vertices [0] : _vertices [i + 1];
					var f = a.x * b.y - b.x * a.y;
					x += (a.x + b.x) * f;
					y += (a.y + b.y) * f;
					computedArea += f * 3;
				}
				if (computedArea == 0)
					return _vertices [0];
				return 
					new Vector2 (x / computedArea, y / computedArea);
			}
		}

		struct QuadCell {
			public Vector2 centre; // cell center
			public float halfSize; // half the cell size
			public float distanceFromPoly; 
			public float maxDistanceFromPoly; 
		}

		QuadCell MakeQuadCell(float x, float y, float halfHeight) {
			return MakeQuadCell (new Vector2 (x, y), halfHeight);
		}

		QuadCell MakeQuadCell(Vector2 centre, float halfHeight ) {
			var qc = new QuadCell();
			qc.centre = centre;
			qc.halfSize = halfHeight;

			qc.distanceFromPoly = MinSignedDistanceFromPointToPolygon(qc.centre); // distance from cell center to polygon
			qc.maxDistanceFromPoly = qc.distanceFromPoly + qc.halfSize * Mathf.Sqrt(2); // max distance to polygon within a cell

			return qc;
		}

		// min signed distance from point to polygon outline (negative if point is outside)
		public float MinSignedDistanceFromPointToPolygon(Vector2 point) {
			
			var minDistSq = float.MaxValue;
				
			for (int i = 0;  i < _vertices.Length ; i++) {
				minDistSq = Mathf.Min(minDistSq, PointToLineSegmentSquaredDistance(point, _vertices[i], 
					(i == _vertices.Length - 1) ? _vertices[0] : _vertices[i+1]));
			}

			return (ContainsPoint(point) ? 1 : -1) * Mathf.Sqrt(minDistSq);
		}

		// get squared distance from a point to a segment
		float PointToLineSegmentSquaredDistance(Vector2 point, Vector2 a, Vector2 b) {

			var x = a.x;
			var y = a.y;
			var dx = b.x - a.x;
			var dy = b.y - a.y;

			if (dx != 0 || dy != 0) {

				var t = ((point.x - a.x) * dx + (point.y - a.y) * dy) / (dx * dx + dy * dy);

				if (t > 1) {
					x = b.x;
					y = b.y;

				} else if (t > 0) {
					x += dx * t;
					y += dy * t;
				}
			}

			dx = point.x - x;
			dy = point.y - y;

			return dx * dx + dy * dy;
		}





		Vector2 _poleOfInaccessibility = new Vector2(0, 0); 

		/// <summary>
		/// The point within the poly which is the hardest to reach from all of its boundaries.
		/// See https://github.com/mapbox/polylabel
		/// </summary>
		/// <value>The pole of inaccessibility.</value>
		public Vector2 poleOfInaccessibility {
			get {
				if (_poleOfInaccessibility.magnitude == 0.0f) {
					_poleOfInaccessibility = computePoleOfInaccesibility();
				}
				return  _poleOfInaccessibility;
			}
		}

		Vector2 computePoleOfInaccesibility() {
			if (_vertices.Length == 0) {
				return new Vector2(0,0);
			}

			// find the bounding box of the outer ring
			float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MaxValue, maxY = float.MaxValue;
			for (var i = 0; i < _vertices.Length; i++) {
			var p = _vertices[i];
				if (i == 0 || p [0] < minX) minX = p[0];
				if (i == 0 || p[1] < minY) minY = p[1];
				if (i == 0 || p[0] > maxX) maxX = p[0];
				if (i == 0 || p[1] > maxY) maxY = p[1];
			}

			float width = maxX - minX;
			float height = maxY - minY;
			float cellSize = Mathf.Min(width, height);
			float h = cellSize / 2.0f;

			float precision = Mathf.Min(1.0f, h / 50);

			if (cellSize == 0) return new Vector2(minX + width / 2, minY + height / 2);

			var cellQueue = new List<QuadCell>();

			// cover polygon with initial cells
			for (float x = minX; x < maxX; x += cellSize) {
				for (float y = minY; y < maxY; y += cellSize) {
					cellQueue.Add(MakeQuadCell(x + h, y + h, h));
				}
			}

			// take zero-size centroid cell as the first best guess. Might be outside region.
			var bestCell = MakeQuadCell(centroid, 0);

			// special case for rectangular polygons
			var bboxCell = MakeQuadCell(minX + width / 2, minY + height / 2, 0);
			if (bboxCell.distanceFromPoly > bestCell.distanceFromPoly) bestCell = bboxCell;

			int numProbes = cellQueue.Count;

			while (cellQueue.Count > 0 && numProbes < 1000) {
				// pick the most promising cell from the queue
				var bestIndex = -1;
				for (var i = 0; i < cellQueue.Count; i++) {
					if (bestIndex == -1 || cellQueue [i].maxDistanceFromPoly > cellQueue [bestIndex].maxDistanceFromPoly)
						bestIndex = i;
				}
				var topCell = cellQueue [bestIndex];
				cellQueue.RemoveAt(bestIndex); // pop the one with MAX D 

				// update the best cell if we've found a better one
				if (topCell.distanceFromPoly > bestCell.distanceFromPoly) {
					bestCell = topCell;
				}

				// do not drill down further if there's no chance of a better solution
				if (topCell.maxDistanceFromPoly - bestCell.distanceFromPoly <= precision) continue;

				// split the cell into four cells
				h = topCell.halfSize / 2;
				cellQueue.Add(MakeQuadCell(topCell.centre.x - h, topCell.centre.y - h, h));
				cellQueue.Add(MakeQuadCell(topCell.centre.x + h, topCell.centre.y - h, h));
				cellQueue.Add(MakeQuadCell(topCell.centre.x - h, topCell.centre.y + h, h));
				cellQueue.Add(MakeQuadCell(topCell.centre.x + h, topCell.centre.y + h, h));
				numProbes += 4;
			}

			return bestCell.centre;


		}


		/// <summary>
		/// Gets the vert count.
		/// </summary>
		/// <value>The vert count.</value>
		public int VertCount {
			get {
				if(_vertices.IsNullOrEmpty()) return 0;
				return _vertices.Length;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the first and end points are equal.
		/// This is useful if your polygons are not draw repeating by default.
		/// </summary>
		/// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
		public bool connected {
			get {
				return (!_vertices.IsNullOrEmpty() && _vertices[0] == _vertices[_vertices.Length-1]);
			}
		}
		
		/// <summary>
		/// Determines whether this instance is valid.
		/// </summary>
		/// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
		public bool IsValid () {
			if(_vertices.IsNullOrEmpty()) 
				return false;
			if (connected)
			{
				return _vertices.Length >= 3;
			}
			else
			{
				return _vertices.Length >= 2;
			}
		}
		
		public static Polygon triangle {
			get {
				return new RegularPolygon(3).ToPolygon();
			}
		}
		
		public static Polygon square {
			get {
				return new RegularPolygon(4).ToPolygon();
			}
		}
		
		public static Polygon pentagon {
			get {
				return new RegularPolygon(5).ToPolygon();
			}
		}
		
		public static Polygon hexagon {
			get {
				return new RegularPolygon(6).ToPolygon();
			}
		}
		
		public Polygon (params Vector2[] _vertices) {
			this.vertices = _vertices;
		}
		
		public Polygon (Polygon _polygon) {
			vertices = (Vector2[]) _polygon.vertices.Clone();
		}

		public static Polygon MakeConvexHull(List<Vector2> points) {
			return new Polygon(MakeConvexHullPoints(points).ToArray());
		}

		public Rect GetRect () {
			Rect rect = new Rect();
			if(!IsValid()) 
				return rect;
			return RectX.CreateEncapsulating(_vertices);
		}

		public void Move (Vector2 vector) {
			if(vector == Vector2.zero) return;
			for(int i = 0; i < _vertices.Length; i++) {
				_vertices[i] = _vertices[i] + vector;
			}
		}

		/// <summary>
		/// Returns a scaled polygon.
		/// </summary>
		/// <param name="_polygonDefinition">_polygon definition.</param>
		/// <param name="_scaleModifier">_scale modifier.</param>
		public static Polygon Scale (Polygon _polygonDefinition, Polygon _scaleModifier) {
			Polygon newPolygonDefinition = new Polygon(_polygonDefinition);
			if(newPolygonDefinition.vertices.Length > _scaleModifier.vertices.Length) 
				DebugX.Log(null, "Cannot Scale PolygonDefinition because the input modifier does not have enough vertices. It has "+_scaleModifier.vertices.Length+". It requires at least "+_polygonDefinition.vertices.Length+".");
			for(int i = 0; i < _polygonDefinition.vertices.Length; i++) {
				Vector2.Scale(newPolygonDefinition.vertices[i], _scaleModifier.vertices[i]);
			}
	
			return newPolygonDefinition;
		}
		
		/// <summary>
		/// Returns a scaled polygon.
		/// </summary>
		/// <param name="_polygonDefinition">_polygon definition.</param>
		/// <param name="_scaleModifier">_scale modifier.</param>
		public static Polygon Scale (Polygon _polygonDefinition, Vector2 _scaleModifier) {
			if(_scaleModifier == Vector2.one) return _polygonDefinition;
			
			Polygon newPolygonDefinition = new Polygon(_polygonDefinition);
			for(int i = 0; i < _polygonDefinition.vertices.Length; i++) {
				Vector2.Scale(newPolygonDefinition.vertices[i], _scaleModifier);
			}
			return newPolygonDefinition;
		}
		
		/// <summary>
		/// Returns a scaled polygon.
		/// </summary>
		/// <param name="_polygonDefinition">_polygon definition.</param>
		/// <param name="_scaleModifier">_scale modifier.</param>
		public static Polygon Scale (Polygon _polygonDefinition, float _scaleModifier) {
			if(_scaleModifier == 1) return _polygonDefinition;
			
			Polygon newPolygonDefinition = new Polygon(_polygonDefinition);
			for(int i = 0; i < _polygonDefinition.vertices.Length; i++) {
				newPolygonDefinition.vertices[i] = _polygonDefinition.vertices[i] * _scaleModifier;
			}
			return newPolygonDefinition;
		}
		
		/// <summary>
		/// Expands the polygon making it wider or thinner.
		/// </summary>
		/// <param name="_polygonDefinition">_polygon definition.</param>
		/// <param name="_scaleModifier">_scale modifier.</param>
		public void Expand (float _expansion) {
			if(_expansion == 0) return;
			
			for(int i = 0; i < vertices.Length; i++) {
				vertices[i] = vertices[i].normalized * (vertices[i].magnitude + _expansion);
			}
		}
		
		public float GetTotalLength () {
			var length = 0f;
			foreach(var line in GetLines()) length += line.length;
			return length;
		}
		
		public Vector2 GetRegularEdgePosition (float normalizedEdgeLength) {
			normalizedEdgeLength %= 1; 
			var edgeLength = GetTotalLength() * normalizedEdgeLength;
			// var edgeIndex = Mathf.FloorToInt(edgeLength);
			// var edgeArcLength = edgeIndex-edgeLength;

			var length = 0f;
			foreach(var line in GetLines()) {
				var endLength = length + line.length;
				if(endLength > edgeLength) {
					var l = Mathf.InverseLerp(length, endLength, edgeLength);
					return Vector2.Lerp(line.start, line.end, l);
				}
				else length = endLength;
			}
			return vertices.Last();
		}
		/// <summary>
		/// Gets the vertex.
		/// </summary>
		/// <returns>The vertex.</returns>
		/// <param name="edgeIndex">Edge index.</param>
		public Vector2 GetVertex (int edgeIndex) {
			return vertices[GetRepeatingVertexIndex(edgeIndex)];
		}
		
		/// <summary>
		/// Gets the vertex.
		/// </summary>
		/// <returns>The vertex.</returns>
		/// <param name="edgeIndex">Edge index.</param>
		public Vector2 GetBestVertex (float edgeDistance) {
			return GetVertex(Mathf.RoundToInt (edgeDistance));
		}
		
		
		/// <summary>
		/// Gets the edge position from the distance around the shape.
		/// Whole numbers are vertices.
		/// </summary>
		/// <returns>The edge position.</returns>
		/// <param name="edgeIndex">Edge index.</param>
		public Vector2 GetVertRelativeEdgePosition (float edgeDistance) {
			edgeDistance = GetRepeatingVertexIndex(edgeDistance);
			if(edgeDistance.IsWhole()) 
				return GetVertex((int)edgeDistance);
			int leftIndex = Mathf.FloorToInt(edgeDistance);
			int rightIndex = leftIndex+1;
			return Vector2.Lerp(GetVertex(leftIndex), GetVertex(rightIndex), edgeDistance-leftIndex);
		}
		
		//Returns the angle of a side of a shape at a distance. 
		//If the shape is a triangle, the angle of the sides are 0, 120 and 240.
		public float GetVertRelativeEdgeDegreesFromCenter(float edgeDistance){
			return Vector2X.DegreesBetween(center, GetVertRelativeEdgePosition(edgeDistance));
		}
		
		//Returns the angle of a side of a shape at a distance. 
		//If the shape is a triangle, the angle of the sides are 0, 120 and 240.
		public float GetVertexDegreesInternal(int vertIndex){
			Vector2 leftDir = GetVertex(vertIndex) - GetVertex(vertIndex-1);
			Vector2 rightDir = GetVertex(vertIndex) - GetVertex(vertIndex+1);
			return Vector2X.DegreesBetween(leftDir, rightDir);
		}
		
		public float GetVertexDegreesInternal (int i = 0, int j = 1) {
			i = GetRepeatingVertexIndex(i);
			j = GetRepeatingVertexIndex(j);
			return Vector2X.DegreesBetween(vertices[i], vertices[j]);
		}
		
		/// <summary>
		/// Gets the edge at edge distance.
		/// </summary>
		/// <returns>The edge at the edge distance.</returns>
		/// <param name="edgeDistance">Edge distance.</param>
		public Line GetEdge (float edgeDistance) {
			int leftIndex = Mathf.FloorToInt(edgeDistance);
			int rightIndex = leftIndex+1;
			return GetEdge(leftIndex, rightIndex);
		}
		
		/// <summary>
		/// Gets the edge between two vertex indices.
		/// If the two verts are not adjacent, the edge is technically known as a diagonal.
		/// </summary>
		/// <returns>The edge.</returns>
		/// <param name="i">The index.</param>
		/// <param name="j">J.</param>
		public Line GetEdge (int i = 0, int j = 1) {
			return new Line(GetVertex(i), GetVertex(j));
		}
		
		/// <summary>
		/// Gets the edge length between two vert indices.
		/// If the two verts are not adjacent, the edge is technically known as a diagonal.
		/// </summary>
		/// <returns>The edge.</returns>
		/// <param name="i">The index.</param>
		/// <param name="j">J.</param>
		public float GetEdgeLength (int i = 0, int j = 1) {
			return Vector2.Distance(GetVertex(i), GetVertex(j));
		}
        
        public Vector2 GetEdgeTangentAtEdgeIndex (int edgeIndex) {
            return (GetVertex(edgeIndex+1) - GetVertex(edgeIndex)).normalized;
        }

        // Normals point outwards
        // Edge index 0 means points 0 and 1
        public Vector2 GetEdgeNormalAtEdgeIndex (int edgeIndex) {
            return GetEdgeNormalAtEdgeIndex(edgeIndex, GetIsClockwise());
        }

        Vector2 GetEdgeNormalAtEdgeIndex (int edgeIndex, bool isClockwise) {
            var tangent = GetEdgeTangentAtEdgeIndex(edgeIndex);
            var normal = new Vector2(-tangent.y, tangent.x);
            if(!isClockwise) normal *= -1;
            return normal;
        }
        
        public static Vector2 GetEdgeNormalAtEdgeIndex (Vector2 tangent, bool isClockwise) {
            var normal = new Vector2(-tangent.y, tangent.x);
            if(!isClockwise) normal *= -1;
            return normal;
        }

        public Vector2 GetVertexNormal (int index) {
            var isClockwise = GetIsClockwise();
            var lastEdgeNormal = GetEdgeNormalAtEdgeIndex(index-1, isClockwise);
            var nextEdgeNormal = GetEdgeNormalAtEdgeIndex(index, isClockwise);
            return Vector2.Lerp(lastEdgeNormal, nextEdgeNormal, 0.5f).normalized;
        }
		
		
		
		
		/// <summary>
		/// Gets the edge center.
		/// </summary>
		/// <returns>The edge center.</returns>
		/// <param name="edgeDistance">Edge distance.</param>
		public Vector2 GetEdgeCenter (float edgeDistance) {
			int leftIndex = Mathf.FloorToInt(edgeDistance);
			int rightIndex = leftIndex+1;
			return GetEdgeCenter(leftIndex, rightIndex);
		}
		
		/// <summary>
		/// Gets the edge between two vert indices.
		/// If the two verts are not adjacent, the edge is technically known as a diagonal.
		/// </summary>
		/// <returns>The edge.</returns>
		/// <param name="i">The index.</param>
		/// <param name="j">J.</param>
		public Vector2 GetEdgeCenter (int i = 0, int j = 1) {
			return Vector2.Lerp(GetVertex(i), GetVertex(j), 0.5f);
		}
		
		/// <summary>
		/// Gets the edge center.
		/// </summary>
		/// <returns>The edge center.</returns>
		/// <param name="edgeDistance">Edge distance.</param>
		public Vector2[] GetEdgeCenters () {
			Vector2[] centers = new Vector2[VertCount];
			for(int i = 0; i < centers.Length; i++) {
				centers[i] = GetEdgeCenter(i, i+1);
			}
			return centers;
		}
		
		


		

		public Vector2 GetInnermostVertex () {
			return Closest(Vector2.zero, new List<Vector2>(_vertices));
		}
		
		public Vector2 GetInnermostEdge () {
			return Closest(Vector2.zero, new List<Vector2>(GetEdgeCenters()));
		}
		
		public Vector2 GetInnermostPoint () {
			return Closest(Vector2.zero, new List<Vector2>(_vertices.Concat<Vector2>(GetEdgeCenters())));
		}
		
		
		public Vector2 GetOutermostVertex () {
			return Furthest(Vector2.zero, new List<Vector2>(_vertices));
		}
		
		public Vector2 GetOutermostEdge () {
			return Furthest(Vector2.zero, new List<Vector2>(GetEdgeCenters()));
		}
		
		public Vector2 GetOutermostPoint () {
			return Furthest(Vector2.zero, new List<Vector2>(_vertices.Concat<Vector2>(GetEdgeCenters())));
		}

		
		// The point on the polygon that best matches a direction
        public Vector2 FindPointInDirection (Vector2 direction) {
            return vertices.Best(x => {
                var y = Vector2.Dot(x, direction);
                return y;
            }, (other, currentBest) => other > currentBest, Mathf.NegativeInfinity, Vector2.zero);
        }
		

		public IEnumerable<Line> GetLines () {
			for(int i = 0; i < _vertices.Length-1; i++) yield return new Line(_vertices[i], _vertices[i+1]);
			yield return new Line(_vertices[_vertices.Length-1], _vertices[0]);
		}

		/// <summary>
		/// Gets the complete graph, a list of all the lines that can be formed by connecting the verts. 
		/// </summary>
		/// <returns>The complete graph.</returns>
		public Line[] GetCompleteGraph () {
			List<Line> _lines = new List<Line>();
			for(int i = 0; i < _vertices.Length; i++) {
				for(int j = 0; j < _vertices.Length; j++) {
					if(i == j) continue;
					bool found = false;
					Line tmpLine = new Line(_vertices[i], _vertices[j]);
					for(int k = 0; k < _lines.Count; k++) {
						if(Line.Equals(tmpLine, _lines[k])) {
							found = true;
							break;
						}
					}
					if(!found) {
						_lines.Add(new Line(_vertices[i], _vertices[j]));
					}
				}
			}
			return _lines.ToArray();
		}



		public struct PolygonRaycastHit {
			public float distance;
			public Vector2 point;
			// public Vector2 normal;
		}
		public bool RayPolygonIntersection(Vector2 rayOrigin, Vector2 rayDirection, out PolygonRaycastHit hit) {
			hit = new PolygonRaycastHit();
			hit.distance = float.MaxValue;
			hit.point = rayOrigin;
			// hit.normal = rayDirection;
			// Comparison variables.
			float distance;
			int crossings = 0;
		
			for (int j = _vertices.Length - 1, i = 0; i < _vertices.Length; j = i, i++) {
				if (new Line(_vertices[j], _vertices[i]).RayLineIntersect(rayOrigin, rayDirection, out distance)) {
					crossings++;
					// If new intersection is closer, update variables.
					if (distance < hit.distance) {
						hit.distance = distance;
						hit.point = rayOrigin + (rayDirection * hit.distance);
						// Vector2 edge = _vertices[j] - _vertices[i];
						// hit.normal = new Vector2(-edge.y, edge.x).normalized;
					}
				}
			}
		
			return crossings > 0 && crossings % 2 == 0;  
		}

		public List<PolygonRaycastHit> RayPolygonIntersections(Vector2 rayOrigin, Vector2 rayDirection) {
			var hits = new List<PolygonRaycastHit>();
			RayPolygonIntersectionsNonAlloc(rayOrigin, rayDirection, ref hits);
			return hits;
		}

		public void RayPolygonIntersectionsNonAlloc(Vector2 rayOrigin, Vector2 rayDirection, ref List<PolygonRaycastHit> hits) {
			if(hits == null) hits = new List<PolygonRaycastHit>();
			hits.Clear();
			float distance;
			for (int j = _vertices.Length - 1, i = 0; i < _vertices.Length; j = i, i++) {
				if (new Line(_vertices[j], _vertices[i]).RayLineIntersect(rayOrigin, rayDirection, out distance)) {
					var hit = new PolygonRaycastHit();
					hit.distance = distance;
					hit.point = rayOrigin + (rayDirection * hit.distance);
					// Vector2 edge = _vertices[j] - _vertices[i];
					// hit.normal = new Vector2(-edge.y, edge.x).normalized;
					hits.Add(hit);
				}
			}
		}
	
		public static Polygon LerpAuto (Polygon from, Polygon to, float lerp) {
			if(from.vertices.Length == to.vertices.Length) return LerpEqualVertCount (from, to, lerp);
			else return LerpDifferentVertCount (from, to, lerp);
		}

		public Vector2 EvaluateFromCenterUsingDegrees (float degrees) {
			PolygonRaycastHit intersection;
			RayPolygonIntersection(center, center + MathX.DegreesToVector2(degrees), out intersection);
			return intersection.point;
		}

		// Lerps by sampling at regular angular intervals. Loses fidelity, but results are smoother.
		public static Polygon LerpCustomVertCount (Polygon from, Polygon to, float lerp, int numSamples) {
			numSamples = Mathf.Max(numSamples, 3);
			Vector2[] newPolygonVerts = new Vector2[numSamples];
			float n = 1f/newPolygonVerts.Length;
			var centerA = from.center;
			var centerB = to.center;
			Vector2 sampleA, sampleB, dir;
			float degrees;
			PolygonRaycastHit intersection;
			for(int i = 0; i < newPolygonVerts.Length; i++) {
				degrees = n * i * 360;
				dir = MathX.DegreesToVector2(degrees);
				from.RayPolygonIntersection(centerA, dir, out intersection);
				sampleA = intersection.point;
				to.RayPolygonIntersection(centerB, dir, out intersection);
				sampleB = intersection.point;
				newPolygonVerts[i] = Vector2.Lerp(sampleA, sampleB, lerp);
			}
			return new Polygon(newPolygonVerts);
		}
	
	
		private static Polygon LerpEqualVertCount (Polygon from, Polygon to, float lerp) {
			Vector2[] newPolygonVerts = new Vector2[from.VertCount];
			for(int i = 0; i < newPolygonVerts.Length; i++) {
				newPolygonVerts[i] = Vector2.Lerp(from.vertices[i], to.vertices[i], lerp);
			}
			return new Polygon(newPolygonVerts);
		}
	
		private static Polygon LerpDifferentVertCount (Polygon from, Polygon to, float lerp) {
			// Vert count of the PolygonDefinition with the least verts
			int minVerts = Mathf.Min(from.vertices.Length, to.vertices.Length);
			// Vert count of the PolygonDefinition with the most verts
			int maxVerts = Mathf.Max(from.vertices.Length, to.vertices.Length);
			// Interpolated vert count between the min and max vert count, rounded up to the nearest whole number.
			Polygon largestPolygon = from.vertices.Length > to.vertices.Length ? from : to;
			
			//Polygon verts
			Vector2[] newPolygonVerts = new Vector2[maxVerts];
			
			for(int i = 0; i < minVerts; i++) {
				newPolygonVerts[i] = Vector2.Lerp(from[i], to[i], lerp);
			}
			
			for(int i = minVerts; i < maxVerts; i++) {
				if(from.vertices.Length > to.vertices.Length) {
					newPolygonVerts[i] = Vector2.Lerp(largestPolygon[i], newPolygonVerts[i-1], lerp);
				} else {
					newPolygonVerts[i] = Vector2.Lerp(newPolygonVerts[i-1], largestPolygon[i], lerp);
				}
			}
			return new Polygon(newPolygonVerts);
		}
		
		
		/// <summary>
		/// Array operator. 
		/// </summary>
		public Vector2 this[int key] {
			get {
				return _vertices[key];
			} set {
				_vertices[key] = value;
			}
		}
		
		
		/// <summary>
		/// Gets the index of the repeating vertex.
		/// </summary>
		/// <returns>The repeating vertex index.</returns>
		/// <param name="vertexIndex">Vertex index.</param>
		public int GetRepeatingVertexIndex (int vertexIndex) {
			return MathX.Mod(vertexIndex, vertices.Length);
		}
		public float GetRepeatingVertexIndex (float vertexIndex) {
			return Mathf.Repeat(vertexIndex, vertices.Length);
		}

		public Vector2 FindClosestVertex(Vector2 point) {
			return Closest(point, _vertices);
		}

		public int FindClosestVertexIndex(Vector2 point) {
			return ClosestIndex(point, _vertices);
		}

		public void FindClosestEdgeIndices(Vector2 point, ref int bestIndex1, ref int bestIndex2) {
			float bestDistance = Mathf.Infinity;
			Vector2 closestPointOnLine = Vector2.zero;
			bestIndex1 = 0;
			bestIndex2 = 0;
			for(int l = 0; l < _vertices.Length; l++) {
				Vector2 pointOnLine = Vector2.zero;
				if(l < _vertices.Length-1) pointOnLine = Line.GetClosestPointOnLine(_vertices[l], _vertices[l+1], point);
				else pointOnLine = Line.GetClosestPointOnLine(_vertices[l], _vertices[0], point);
				var distance = Vector2X.SqrDistance(point, pointOnLine);
				if(distance < bestDistance) {
					closestPointOnLine = pointOnLine;
					bestDistance = distance;
					bestIndex1 = l;
					bestIndex2 = l < (_vertices.Length-1) ? l+1 : 0;
				}
			}
		}

		public Vector2 FindClosestPointInPolygon(Vector2 point){
			if(ContainsPoint(point)) {
				return point;
			} else {
				return FindClosestPointOnPolygon(point);
			}
		}

		public Vector2 FindClosestPointOnPolygon(Vector2 point) {
			return FindClosestPointOnPolygon(_vertices, point);
		}

		public static Vector2 FindClosestPointOnPolygon(Vector2[] verts, Vector2 point) {
			float bestSqrDistance = Mathf.Infinity;
			Vector2 closestPoint = Vector2.zero;
			Vector2 currentPoint;
			for(int l = 0; l < verts.Length; l++) {
				if(l < verts.Length-1)
					currentPoint = Line.GetClosestPointOnLine(verts[l], verts[l+1], point);
				else
					currentPoint = Line.GetClosestPointOnLine(verts[l], verts[0], point);

				var sqrDistance = Vector2X.SqrDistance(point, currentPoint);
				if(sqrDistance < bestSqrDistance) {
					bestSqrDistance = sqrDistance;
					closestPoint = currentPoint;
				}
			}
			return closestPoint;
		}

		public bool ContainsPoint(Vector2 testPoint) {
			return ContainsPoint(_vertices, testPoint);
		}

		public static bool ContainsPoint(Vector2[] polyPoints, Vector2 testPoint) {
			bool result = false;
			int j = polyPoints.Length-1;
			for (int i = 0; i < polyPoints.Length; i++) {
				if (polyPoints[i].y < testPoint.y && polyPoints[j].y >= testPoint.y || polyPoints[j].y < testPoint.y && polyPoints[i].y >= testPoint.y) {
					if (polyPoints[i].x + (testPoint.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) * (polyPoints[j].x - polyPoints[i].x) < testPoint.x) {
						result = !result;
					}
				}
				j = i;
			}
			return result;
		}

		public static bool ContainsPoint(List<Vector2> polyPoints, Vector2 testPoint) {
			bool result = false;
			int j = polyPoints.Count-1;
			for (int i = 0; i < polyPoints.Count; i++) {
				if (polyPoints[i].y < testPoint.y && polyPoints[j].y >= testPoint.y || polyPoints[j].y < testPoint.y && polyPoints[i].y >= testPoint.y) {
					if (polyPoints[i].x + (testPoint.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) * (polyPoints[j].x - polyPoints[i].x) < testPoint.x) {
						result = !result;
					}
				}
				j = i;
			}
			return result;
		}

		// Returns negative when inside the poly
		public static float SignedDistanceFromPoint (Vector2 position, Vector2[] verts) {
			var pointOnEdge = FindClosestPointOnPolygon(verts, position);
			var distance = Vector2.Distance(position, pointOnEdge);
			if(ContainsPoint(verts, position)) distance = -distance;
			return distance;
		}

		public float SignedDistanceFromPoint (Vector2 position) {
			return SignedDistanceFromPoint(position, _vertices);
		}

		/// <summary>
		/// Gets a random point inside a polygon. Not optimized - the best way to do it would be to triangulate the polygon, then pick a triangle based on a weighted selection of area, and pick a random point inside it.
		/// </summary>
		/// <returns>The random point in polygon.</returns>
		public Vector2 GetRandomPointInPolygon () {
			Rect rect = RectX.CreateEncapsulating(vertices);
			Vector2? randomPoint = null;
			while(randomPoint == null) {
				Vector2 testPoint = new Vector2 (Random.Range(rect.min.x, rect.max.x), Random.Range(rect.min.y, rect.max.y));
				if(ContainsPoint(testPoint)) {
					randomPoint = testPoint;
				}
			}
			return (Vector2)randomPoint;
		}

		public float GetArea () {
			float area = 0;
			var t = new Triangulator(_vertices);
			var triangles = t.Triangulate();
			for(int i = 0; i < triangles.Length; i+=3) {
				var triangle = new Triangle(_vertices[triangles[i]], _vertices[triangles[i+1]], _vertices[triangles[i+2]]);
				area += triangle.area;
			}
			return area;
		}

		public void CopyFrom(Polygon src) {
			vertices = new Vector2[src.vertices.Length];
			for (int i = 0; i < _vertices.Length; ++i)
				_vertices[i] = src[i];		
		}
		
		public bool CompareTo(Polygon src)
		{
			if (_vertices.Length != src.vertices.Length) return false;
			for (int i = 0; i < _vertices.Length; ++i)
				if (_vertices[i] != src[i]) return false;
			return true;
		}


		
		
		
//		public override bool Equals(System.Object obj) {
//			if (obj == null) 
//				return false;
//			
//			Polygon p = obj as Polygon;
//			if ((System.Object)p == null) {
//				return false;
//			}
//			
//			// Return true if the fields match:
//			return (x == p.x) && (y == p.y);
//		}
//		
//		public bool Equals(Polygon p) {
//			// If parameter is null return false:
//			if ((object)p == null) {
//				return false;
//			}
//			
//			// Return true if the fields match:
//			return (x == p.x) && (y == p.y);
//		}
//		
//		public static bool operator == (Polygon left, Polygon right) {
//			if (System.Object.ReferenceEquals(left, right))
//			{
//				return true;
//			}
//			
//			// If one is null, but not both, return false.
//			if (((object)left == null) || ((object)right == null))
//			{
//				return false;
//			}
//			if(left.x == right.x && left.y == right.y)return true;
//			return false;
//		}
//		
//		public static bool operator != (Polygon left, Polygon right) {
//			return !(left == right);
//		}

		public override string ToString () {
			string s = "Polygon: VertCount="+VertCount+" Connected="+connected+" Valid="+IsValid();
			for (int i = 0; i < _vertices.Length; ++i)
				s += "\n"+i+": "+_vertices[i];
			return s;
		}

		/// <summary>
		/// Returns the index of the closest vector to v in the values list
		/// </summary>
		/// <returns>The index.</returns>
		/// <param name="v">V.</param>
		/// <param name="values">Values.</param>
		private static int ClosestIndex(Vector2 v, IList<Vector2> values){
			int index = 0;
			float closest = Vector2X.SqrDistance(v, values[index]);
			for(int i = 1; i < values.Count; i++){
				float distance = Vector2X.SqrDistance(v, values[i]);
				if (distance < closest) {
					closest = distance;
					index = i;
				}
			}
			return index;
		}

		/// <summary>
		/// Returns the closest vector to v in the values list
		/// </summary>
		/// <param name="v">V.</param>
		/// <param name="values">Values.</param>
		public static Vector2 Closest(Vector2 v, IList<Vector2> values){
			return values[ClosestIndex(v, values)];
		}

		/// <summary>
		/// Returns the index of the furthest vector to v in the values list
		/// </summary>
		/// <returns>The index.</returns>
		/// <param name="v">V.</param>
		/// <param name="values">Values.</param>
		public static int FurthestIndex(Vector2 v, IList<Vector2> values){
			if(values.Count == 0) {
				Debug.LogError("Values is empty!");
				return -1;
			}
			int index = 0;
			float furthest = Vector2X.SqrDistance(v, values[index]);
			for(int i = 1; i < values.Count; i++){
				float distance = Vector2X.SqrDistance(v, values[i]);
				if (distance > furthest) {
					furthest = distance;
					index = i;
				}
			}
			return index;
		}

		/// <summary>
		/// Returns the furthest vector to v in the values list
		/// </summary>
		/// <param name="v">V.</param>
		/// <param name="values">Values.</param>
		public static Vector2 Furthest(Vector2 v, IList<Vector2> values){
			return values[FurthestIndex(v, values)];
		}


        // Return the points that make up a polygon's convex hull.
        // This method leaves the points list unchanged.
		static List<Vector2> MakeConvexHullPoints(List<Vector2> points) {
			if(points.Count == 0) return points;
            // Cull. I was finding occasional minor errors with the result with this enabled.
            //points = HullCull(points);

            // Find the remaining point with the smallest Y value.
            // if (there's a tie, take the one with the smaller X value.
            Point best_pt = points[0];
            foreach (Point pt in points)
            {
                if ((pt.y < best_pt.y) ||
                   ((pt.y == best_pt.y) && (pt.x < best_pt.x)))
                {
                    best_pt = pt;
                }
            }

            // Move this point to the convex hull.
			List<Vector2> hull = new List<Vector2>();
            hull.Add(best_pt);
            points.Remove(best_pt);

            // Start wrapping up the other points.
            float sweep_angle = 0;
            for (;;)
            {
                // Find the point with smallest AngleValue
                // from the last point.
                float X = hull[hull.Count - 1].x;
				float Y = hull[hull.Count - 1].y;
                best_pt = points[0];
                float best_angle = 3600;

                // Search the rest of the points.
                foreach (Point pt in points)
                {
                    float test_angle = AngleValue(X, Y, pt.x, pt.y);
                    if ((test_angle >= sweep_angle) &&
                        (best_angle > test_angle))
                    {
                        best_angle = test_angle;
                        best_pt = pt;
                    }
                }

                // See if the first point is better.
                // If so, we are done.
                float first_angle = AngleValue(X, Y, hull[0].x, hull[0].y);
                if ((first_angle >= sweep_angle) &&
                    (best_angle >= first_angle))
                {
                    // The first point is better. We're done.
                    break;
                }

                // Add the best point to the convex hull.
                hull.Add(best_pt);
                points.Remove(best_pt);

                sweep_angle = best_angle;

                // If all of the points are on the hull, we're done.
                if (points.Count == 0) break;
            }

            return hull;
        }


        // Cull points out of the convex hull that lie inside the
        // trapezoid defined by the vertices with smallest and
        // largest X and Y coordinates.
        // Return the points that are not culled.
		static List<Vector2> HullCull(List<Vector2> points)
        {
            // Find a culling box.
			Rect culling_box = GetMinMaxBox(points);

            // Cull the points.
			List<Vector2> results = new List<Vector2>();
			foreach (Vector2 pt in points)
            {
                // See if (this point lies outside of the culling box.
                if (!culling_box.Contains(pt))
                {
                    // This point cannot be culled.
                    // Add it to the results.
                    results.Add(pt);
                }
            }
            return results;
        }

		// Find the points nearest the upper left, upper right,
        // lower left, and lower right corners.
		static void GetMinMaxCorners(List<Vector2> points, ref Vector2 ul, ref Vector2 ur, ref Vector2 ll, ref Vector2 lr)
        {
            // Start with the first point as the solution.
            ul = points[0];
            ur = ul;
            ll = ul;
            lr = ul;

            // Search the other points.
            foreach (Point pt in points) {
                if (-pt.x - pt.y > -ul.x - ul.y) ul = pt;
                if (pt.x - pt.y > ur.x - ur.y) ur = pt;
                if (-pt.x + pt.y > -ll.x + ll.y) ll = pt;
                if (pt.x + pt.y > lr.x + lr.y) lr = pt;
            }
        }

        // Find a box that fits inside the MinMax quadrilateral.
		static Rect GetMinMaxBox(List<Vector2> points)
        {
            // Find the MinMax quadrilateral.
			Vector2 ul = new Vector2(0, 0), ur = ul, ll = ul, lr = ul;
            GetMinMaxCorners(points, ref ul, ref ur, ref ll, ref lr);

            // Get the coordinates of a box that lies inside this quadrilateral.
            float xmin, xmax, ymin, ymax;
            xmin = ul.x;
            ymin = ul.y;

            xmax = ur.x;
            if (ymin < ur.y) ymin = ur.y;

            if (xmax > lr.x) xmax = lr.x;
            ymax = lr.y;

            if (xmin < ll.x) xmin = ll.x;
            if (ymax > ll.y) ymax = ll.y;

			Rect result = new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
            return result;
        }

        // Return a number that gives the ordering of angles
        // WRST horizontal from the point (x1, y1) to (x2, y2).
        // In other words, AngleValue(x1, y1, x2, y2) is not
        // the angle, but if:
        //   Angle(x1, y1, x2, y2) > Angle(x1, y1, x2, y2)
        // then
        //   AngleValue(x1, y1, x2, y2) > AngleValue(x1, y1, x2, y2)
        // this angle is greater than the angle for another set
        // of points,) this number for
        //
        // This function is dy / (dy + dx).
		static float AngleValue(float x1, float y1, float x2, float y2)
        {
            float dx, dy, ax, ay, t;

            dx = x2 - x1;
            ax = Mathf.Abs(dx);
            dy = y2 - y1;
            ay = Mathf.Abs(dy);
            if (ax + ay == 0)
            {
                // if (the two points are the same, return 360.
                t = 360f / 9f;
            }
            else
            {
                t = dy / (ax + ay);
            }
            if (dx < 0)
            {
                t = 2 - t;
            }
            else if (dy < 0)
            {
                t = 4 + t;
            }
            return t * 90;
        }

		// Removes points on a polygon that don't contribute to its shape (points lying on the line between neighbours)
		public static List<Vector2> GetSimplifiedVerts(IList<Vector2> verts) {
			List<Vector2> points = new List<Vector2>();
			var point = verts[0];
			Line line = default(Line);
			line.start = verts.GetRepeating(-1);
			for (int i = 0; i < verts.Count; i++) {
				line.end = verts.GetRepeating(i+1);
				var skipPoint = false;
				if(line.start == line.end || point == line.start || point == line.end || Vector2X.SqrDistance(point, line.GetClosestPointOnLine(point)) < 0.0001f) {
					skipPoint = true;
				}
				if(!skipPoint) {
					points.Add(point);
					line.start = point;
				}
				point = line.end;
			}
			return points;
		}
		public static List<Vector2> GetSimplifiedVerts(IList<Vector2> verts, float minDot = 0.1f) {
			List<Vector2> points = new List<Vector2>();
			var point = verts[0];
			Line line = default(Line);
			line.start = verts.GetRepeating(-1);
			for (int i = 0; i < verts.Count; i++) {
				line.end = verts.GetRepeating(i+1);
				var skipPoint = false;
				if(line.start == line.end || point == line.start || point == line.end || Vector2X.SqrDistance(point, line.GetClosestPointOnLine(point)) < 0.0001f) {
					skipPoint = true;
				} else {
					// var lineVector = line.end-line.start;
					// var startCenterVector = point-line.start;
					// if(Vector2.Dot(lineVector, startCenterVector)) {
					// 	skipPoint = true;
					// }
				}
				if(!skipPoint) {
					points.Add(point);
					line.start = point;
				}
				point = line.end;
			}
			return points;
		}


		public static List<Vector2> GetSmoothed(IList<Vector2> vertices, float radius, float degreesPerPoint) {
			List<Vector2> points = new List<Vector2>();
			if(vertices.Count == 0) return points;
			for(int i = 0; i < vertices.Count; i++) {
				var p1 = Vector2.Lerp(vertices.GetRepeating(i-1), vertices[i], 0.5f);
				var p2 = Vector2.Lerp(vertices[i], vertices.GetRepeating(i+1), 0.5f);
				if(p1 == p2 || vertices[i] == p1 || vertices[i] == p2) continue;
				var roundedCorner = DrawingUtils.DrawRoundedCorner(vertices[i], p1, p2, radius, degreesPerPoint);
				bool removeFirst = points.Count > 0 && points[points.Count-1] == roundedCorner[0];
				points.AddRange(roundedCorner);
				if(removeFirst) {
					// Remove the first point sinze it's the same as the first in the next corner
					points.RemoveAt(points.Count-roundedCorner.Length);
				}
			}
			if(points.Count > 0 && points.First() == points.Last()) points.RemoveAt(0);
			return points;
		}

		public override bool Equals(System.Object obj) {
			// If parameter is null return false.
			if (obj == null) {
				return false;
			}
	
			// If parameter cannot be cast to Point return false.
			Polygon p = (Polygon)obj;
			if ((System.Object)p == null) {
				return false;
			}
	
			// Return true if the fields match:
			return Equals(p);
		}
	
		public bool Equals(Polygon p) {
			// If parameter is null return false:
			if ((object)p == null) {
				return false;
			}
	
			// Return true if the fields match:
			return _vertices.Length == p._vertices.Length && _vertices.SequenceEqual(p._vertices);
		}
	
		public override int GetHashCode() {
			// Not 100% on this. Should I be using actual values, since arrays are classes?
			return _vertices.GetHashCode();
		}
	
		public static bool operator == (Polygon left, Polygon right) {
			if (System.Object.ReferenceEquals(left, right))
			{
				return true;
			}
	
			// If one is null, but not both, return false.
			if (((object)left == null) || ((object)right == null))
			{
				return false;
			}
			return left.Equals(right);
		}
	
		public static bool operator != (Polygon left, Polygon right) {
			return !(left == right);
		}


















        // JON'S INTERSECTION POINT FOR HV
        // IT WORKS, I THINK, BUT ITS WAY PAST WHAT THIS CLASS WAS SUPPOSED TO DO.
        // CONSIDER KILLING THIS


		static Vector2 PointInPolyFromIndex(Polygon poly, int index) {
			return poly.vertices [MathX.Mod(index, poly.vertices.Length)];
		}

		const float epsilon = 0.001f;

		static int GetIndexInPolyAtPoint(Polygon poly, Vector2 point) {
			return GetIndexInPolyLyingOnLineBetween (poly, point.AddX(-1 * epsilon), point.AddX(epsilon));
		}

		static int GetIndexInPolyLyingOnLineBetween(Polygon poly, Vector2 pointA, Vector2 pointB) {
			var lineSegment = new Line (pointA, pointB); 

			var index = -1;
			for (var i = 0 ; i < poly.vertices.Length; i++) {
				var vertex = poly.vertices [i];
				// don't allow vertex on top of pointA. Use a very, very tight epsilon for this
				if (Vector2.Distance(vertex, pointA) > epsilon * epsilon) {

					var dist = lineSegment.GetClosestDistanceFromLine (vertex);
					if (dist < epsilon) {
						// have we got a point closer to point A than the best so far (ie. first intersection?)
						if (index == -1 || (index > -1 && Vector2.Distance (vertex, pointA) < Vector2.Distance (poly.vertices [index], pointA))) {
							index = i; 
						}
					}
				}

			}
			return index; 
		}

		Polygon AddVert(int indexToAddAfter, Vector2 pointToAdd) {
			var verts = new List<Vector2> (vertices);
			if (indexToAddAfter == vertices.Length) {
				verts.Add (pointToAdd);
			} else {
				verts.Insert (indexToAddAfter + 1, pointToAdd);
			}
			return new Polygon (verts.ToArray ());
		}

        
		struct PolygonWithSubsetOfPoints {
			public Polygon polygon;
			public List<int> indexList;
		}

		private static bool PointsOverlap(Vector2 pointA, Vector2 pointB) {
			var dist = Vector2.Distance (pointA, pointB);
			return (dist < epsilon * epsilon);
		}

		private static void AddPointUniquelyToList (Vector2 thisPoint, List<Vector2> listOfPoints) {
			for (int i = 0; i < listOfPoints.Count; i++) {
				if (PointsOverlap (listOfPoints [i], thisPoint)) {
					// we've already got this point 
					return; 
				}
			}
			listOfPoints.Add(thisPoint);
		}

		/// <summary>
		///  Does this polygon intersect with another polygon?
		/// </summary>
		/// <returns><c>true</c>, if with polygon was intersectsed, <c>false</c> otherwise.</returns>
		/// <param name="otherPolygon">Other polygon.</param>
		public bool IntersectsWithPolygon(Polygon otherPolygon) {
			var intersections = AddIntersectionPointsOfPoly (this, otherPolygon); 
			return (intersections.indexList.Count > 0);
		}

		/// <summary>
		///  Does this polygon contain another polygon entirely?
		/// </summary>
		/// <returns><c>true</c>, if contains other polygon entirely, <c>false</c> otherwise.</returns>
		/// <param name="otherPolygon">Other polygon.</param>
		public bool whollyContainsOtherPolygon(Polygon otherPolygon) {
			if (!IntersectsWithPolygon(otherPolygon)) {
				return (ContainsPoint(otherPolygon.vertices[0]));
			}
			return false;
		}

		public static Polygon IntersectionPoly(Polygon addToPoly, Polygon intersectingPoly) {
            return AddIntersectionPointsOfPoly(addToPoly, intersectingPoly).polygon;
        }

		/// <summary>
		/// Creates a new polygone by adding the intersection points of the second polygon with the first polygon to the first polygon.
		/// Returns the results as a struct with the polygon, and a list of vertices indices which are intersections
		/// </summary>
		/// <returns>The intersection points of poly.</returns>
		/// <param name="addToPoly">Add to poly.</param>
		/// <param name="intersectingPoly">Intersecting poly.</param>
		static PolygonWithSubsetOfPoints AddIntersectionPointsOfPoly(Polygon addToPoly, Polygon intersectingPoly) {
			var polyWithPoints = new PolygonWithSubsetOfPoints ();
			polyWithPoints.polygon = addToPoly;

			var sidesOfIntersectingPoly = intersectingPoly.GetLines ().ToList();
			var intersectionPoints = new List<Vector2> ();

			for (int i = addToPoly.vertices.Length - 1; i >= 0; i--) {
				var linePoint = PointInPolyFromIndex (polyWithPoints.polygon, i);

				var side = new Line (linePoint, PointInPolyFromIndex(polyWithPoints.polygon, i + 1));

				var intersectionPointsWithCurrentSide = new List<Vector2>();

				for (int j = 0; j < sidesOfIntersectingPoly.Count; j++) {
					Vector2 thisPoint;
					if (Line.LineIntersectionPoint (side, sidesOfIntersectingPoly [j], out thisPoint)) {
						AddPointUniquelyToList (thisPoint, intersectionPointsWithCurrentSide);
					}
				}
				// sort so closest ones are nearest to the i-th point
				intersectionPointsWithCurrentSide.Sort (
					(v1,v2) => (v1-linePoint).sqrMagnitude.CompareTo((v2-linePoint).sqrMagnitude)
				);
				// then feed them in reverse order, pushing them back along the poly's edge (!)
				for (int k = intersectionPointsWithCurrentSide.Count - 1; k >= 0; k--) {
					var pointToAdd = intersectionPointsWithCurrentSide [k];

					// we've found an intersection on the side of subtract poly between i & i + 1 
					// add in this point above where we currently are (we're looping now, so this won't break the loop!)
					if (!polyWithPoints.polygon.vertices.Contains (pointToAdd)) {
						polyWithPoints.polygon = polyWithPoints.polygon.AddVert (i, pointToAdd);
					}
					AddPointUniquelyToList (pointToAdd, intersectionPoints);

				}
			}

			polyWithPoints.indexList = new List<int> ();
			for (int i = 0; i < intersectionPoints.Count; i++) {
				polyWithPoints.indexList.Add(GetIndexInPolyAtPoint(polyWithPoints.polygon, intersectionPoints[i]));
			}
			return polyWithPoints;
		}

		int IndexOfIndexInOtherPoly(int index, Polygon sourcePoly) {
			return GetIndexInPolyAtPoint(this, PointInPolyFromIndex (sourcePoly, index));
		}

		bool EncompassesPointOfIndexInOtherPoly(int index, Polygon sourcePoly) {
			return this.ContainsPoint (PointInPolyFromIndex (sourcePoly, index));
		}

		/// <summary>
		///  Subtracts the second polygon from the first polygon, and returns the result as a new polygon
		/// </summary>
		/// <param name="initialPoly">Initial poly.</param>
		/// <param name="subtractionPoly">Subtraction poly.</param>

		public static Polygon Subtract(Polygon initialPoly, Polygon subtractionPoly) {
			return CombinePolygons (initialPoly, subtractionPoly, false);
		}

		/// <summary>
		///  Adds the second polygon from the first polygon, and returns the result as a new polygon
		/// </summary>
		/// <param name="initialPoly">Initial poly.</param>
		/// <param name="additionPoly">Addition poly.</param>

		public static Polygon Add(Polygon initialPoly, Polygon additionPoly) {
			return CombinePolygons (initialPoly, additionPoly, true);
		}

		public static Polygon CombinePolygons(Polygon initialPoly, Polygon secondPoly, bool doingAddition) {	
			// Add intersection points as verts to the second poly *in the right places*
			// Also add to the indexesOfSubtractionPolyPointsToAdd array
			var subPolyAndPoints = AddIntersectionPointsOfPoly(secondPoly, initialPoly);
			secondPoly = subPolyAndPoints.polygon;

			var initialPolyAndPoints = AddIntersectionPointsOfPoly(initialPoly, secondPoly);
			initialPoly = initialPolyAndPoints.polygon;
			var intersectionIndiciesOnInitialPolygon = initialPolyAndPoints.indexList;



			if (intersectionIndiciesOnInitialPolygon.Count == 0) {
				if (doingAddition) {
					if (initialPoly.whollyContainsOtherPolygon (secondPoly)) {
						return initialPoly; // Set + subset = set 
					} else if (secondPoly.whollyContainsOtherPolygon (initialPoly)) {
						return secondPoly; // Subset + Set = Set
					} else {
						return BridgePolys (initialPoly, secondPoly);

					}
				} else {
					// Subtracting: can do A - B = A ; Subset - set = 0; all other cases result in holes
					if (!initialPoly.whollyContainsOtherPolygon (secondPoly)) {
						return initialPoly; 
					} else if (secondPoly.whollyContainsOtherPolygon (initialPoly)) {
						return new Polygon (); // the empty Polygon
					} else {
						Debug.LogError ("Tried to subtract a polygon and leave a hole. We can't support this.");
						return null; 
					}
				}

			}

			List<Vector2> outputPoints = new List<Vector2>();

			int windingDirection = 0;			// we work out the wind of the subtraction poly at the moment we start adding points
			var addingFromIndex = -1;

			// we start from a point that's definitely not going to be removed 
			int startPointIndex = -1;
			for (startPointIndex = 0; startPointIndex < initialPoly.vertices.Length; startPointIndex++) {
				if (!secondPoly.EncompassesPointOfIndexInOtherPoly (startPointIndex, initialPoly )
					&& !intersectionIndiciesOnInitialPolygon.Contains(startPointIndex))
					break;
			}

			AddPointUniquelyToList ( PointInPolyFromIndex (initialPoly, startPointIndex), outputPoints);
			bool outsideSubtractionPoly = true;

			// loop through the n-1 remaining points of the initial poly and decide what to do about them
			for (int indexInInitialPolyToAddNext = startPointIndex + 1; indexInInitialPolyToAddNext < startPointIndex + initialPoly.vertices.Length; indexInInitialPolyToAddNext++) 
			{
				if (intersectionIndiciesOnInitialPolygon.Contains ( MathX.Mod(indexInInitialPolyToAddNext, initialPoly.vertices.Length ))) {
					if (outsideSubtractionPoly) {
						
						// intersection from outside into subtract poly. 
						// We start outside. Record where on subtraction poly we've crossed.
						addingFromIndex = secondPoly.IndexOfIndexInOtherPoly(indexInInitialPolyToAddNext, initialPoly);
						outsideSubtractionPoly = false;

						// calcuate teh direction of wind based on where we started
						if (windingDirection == 0) {
							windingDirection = initialPoly.EncompassesPointOfIndexInOtherPoly (addingFromIndex + 1, secondPoly) ? -1 : 1;

							if (!doingAddition) {
								windingDirection *= -1;
							}
						}

					} else {

						// intersection from inside to outside. Find the coordinate on subtraction where we've emerged...
						outsideSubtractionPoly = true;
						var addingToIndex = secondPoly.IndexOfIndexInOtherPoly (indexInInitialPolyToAddNext, initialPoly);
								
						// add points in the direction of wind until we reach this intersection point
						for (; MathX.Mod (addingFromIndex - addingToIndex, secondPoly.vertices.Length) != 0; addingFromIndex += windingDirection) {
							AddPointUniquelyToList (PointInPolyFromIndex (secondPoly, addingFromIndex), outputPoints);
						}

					} 
					AddPointUniquelyToList (PointInPolyFromIndex (initialPoly, indexInInitialPolyToAddNext), outputPoints);

				} else if (outsideSubtractionPoly) {
					AddPointUniquelyToList (PointInPolyFromIndex (initialPoly, indexInInitialPolyToAddNext), outputPoints);
				}
			} 

			return new Polygon(outputPoints.ToArray());
		}

		public static Polygon BridgePolys (Polygon polyOne, Polygon polyTwo) {
			int closestPointOne = -1;
			int closestPointTwo = -1;
			float closestDistance = Vector2.Distance(polyOne.center , polyTwo.center);
			float thisDistance = 0;
			for (var i = 0; i < polyOne.vertices.Length; i++) {
				for (var j = 0; j < polyTwo.vertices.Length; j++) {
					thisDistance = Vector2.Distance (polyOne.vertices [i], polyTwo.vertices [j]);
					if (thisDistance < closestDistance) {
						closestDistance = thisDistance; 
						closestPointOne = i;
						closestPointTwo = j;
					}
				}
			}

			var createdPerimeterDelta = float.MaxValue;

			int closestOffsetPointOne = -1;
			int closestOffsetPointTwo = -1;

			for (int offsetOne = -1; offsetOne <= 1; offsetOne += 2) {
				var adjPointOne = polyOne.GetVertex (closestPointOne + offsetOne);
				for (int offsetTwo = -1; offsetTwo <= 1; offsetTwo += 2) {	
					var adjPointTwo = polyTwo.GetVertex (closestPointTwo + offsetTwo);

					bool firstHalfLineInsideFirstPoly = polyOne.ContainsPoint ((polyTwo.GetVertex (closestPointTwo) - adjPointOne) * epsilon + adjPointOne);
					bool secondHalfLineInsideSecondPoly = polyTwo.ContainsPoint ((polyOne.GetVertex (closestPointOne) - adjPointTwo) * epsilon + adjPointTwo);
					bool newLineInsideFirstPoly = polyOne.ContainsPoint ((adjPointTwo - adjPointOne) * epsilon + adjPointOne);
					bool newLineInsideSecondPoly = polyTwo.ContainsPoint ((adjPointOne - adjPointTwo) * epsilon + adjPointTwo);

					if (!newLineInsideFirstPoly && !newLineInsideSecondPoly 
						&& !firstHalfLineInsideFirstPoly && !secondHalfLineInsideSecondPoly) {

						var newPerimeterDelta = Vector2.Distance(adjPointOne, adjPointTwo) 
							- Vector2.Distance(polyTwo.GetVertex (closestPointTwo), adjPointTwo)
							- Vector2.Distance(polyOne.GetVertex (closestPointOne), adjPointOne);
						
						if (newPerimeterDelta < createdPerimeterDelta) {
							createdPerimeterDelta = newPerimeterDelta;
							closestOffsetPointOne = offsetOne;
							closestOffsetPointTwo = offsetTwo;
						}
					}
				}
			}

			var outputPoints = new List<Vector2> ();

			// walk around the first poly in the direction that puts the next closest point *last*
			for (var t = 0; t < polyOne.vertices.Length; t++) {
				outputPoints.Add(polyOne.GetVertex(closestPointOne + t * -1 * closestOffsetPointOne));
			}
			
			for (var k = 0; k < polyTwo.vertices.Length; k++) {
				outputPoints.Add(polyTwo.GetVertex(closestPointTwo + closestOffsetPointTwo + k * 1 * closestOffsetPointTwo));
			}
			
			return new Polygon(outputPoints.ToArray());

		}










		// Reduces the number of points
		public static void SimplifyRamerDouglasPeucker(List<Vector2> pointList, float epsilon, List<Vector2> output) {
			if (pointList.Count < 2) {
				return;
				// throw new ArgumentOutOfRangeException("Not enough points to simplify");
			}

			// Find the point with the maximum distance from line between the start and end
			float dmax = 0f;
			int index = 0;
			int end = pointList.Count - 1;
			for (int i = 1; i < end; ++i) {
				float d = PerpendicularDistance(pointList[i], pointList[0], pointList[end]);
				if (d > dmax) {
					index = i;
					dmax = d;
				}
			}

			// If max distance is greater than epsilon, recursively simplify
			if (dmax > epsilon) {
				List<Vector2> recResults1 = new List<Vector2>();
				List<Vector2> recResults2 = new List<Vector2>();
				List<Vector2> firstLine = pointList.Take(index + 1).ToList();
				List<Vector2> lastLine = pointList.Skip(index).ToList();
				SimplifyRamerDouglasPeucker(firstLine, epsilon, recResults1);
				SimplifyRamerDouglasPeucker(lastLine, epsilon, recResults2);

				// build the result list
				output.AddRange(recResults1.Take(recResults1.Count - 1));
				output.AddRange(recResults2);
				if (output.Count < 2) {
					return;
					// throw new Exception("Problem assembling output");
				}
			}
			else {
				// Just return start and end points
				output.Clear();
				output.Add(pointList[0]);
				output.Add(pointList[pointList.Count - 1]);
			}
		}

		static float PerpendicularDistance(Vector2 pt, Vector2 lineStart, Vector2 lineEnd) {
			float dx = lineEnd.x - lineStart.x;
			float dy = lineEnd.y - lineStart.y;

			// Normalize
			float mag = Mathf.Sqrt(dx * dx + dy * dy);
			if (mag > 0f) {
				dx /= mag;
				dy /= mag;
			}
			float pvx = pt.x - lineStart.x;
			float pvy = pt.y - lineStart.y;

			// Get dot product (project pv onto normalized direction)
			float pvdot = dx * pvx + dy * pvy;

			// Scale line direction vector and subtract it from pv
			float ax = pvx - pvdot * dx;
			float ay = pvy - pvdot * dy;

			return Mathf.Sqrt(ax * ax + ay * ay);
		}
	}
}