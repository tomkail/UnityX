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

		/// <summary>
		/// Gets the vert count.
		/// </summary>
		/// <value>The vert count.</value>
		public int VertCount {
			get {
				if(_vertices == null) return 0;
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
				return (VertCount > 0 && _vertices[0] == _vertices[_vertices.Length-1]);
			}
		}
		
		/// <summary>
		/// Determines whether this instance is valid.
		/// </summary>
		/// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
		public bool IsValid () {
			if(VertCount == 0) 
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
		
		public Polygon (params Vector2[] vertices) {
			SetVertices(vertices);
		}
		public Polygon (IEnumerable<Vector2> vertices) {
			SetVertices(vertices);
		}
		
		public Polygon (Polygon _polygon) {
			SetVertices(_polygon.vertices);
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

		public void SetVertices (IList<Vector2> verts) {
			if(_vertices == null) _vertices = new Vector2[verts.Count];
			else System.Array.Resize(ref _vertices, verts.Count);
			for(int i = 0; i < verts.Count; i++) _vertices[i] = verts[i];
		}
		public void SetVertices (IEnumerable<Vector2> verts) {
			System.Array.Resize(ref _vertices, verts.Count());
			int i = 0;
			foreach(var vert in verts) {
				_vertices[i] = vert;
				i++;
			}
		}


		public Rect GetRect () {
			Rect rect = new Rect();
			if(!IsValid()) 
				return rect;

			float xMin = _vertices[0].x;
			float xMax = _vertices[0].x;
			float yMin = _vertices[0].y;
			float yMax = _vertices[0].y;
			for(int i = 1; i < _vertices.Length; i++) {
				var vector = _vertices[i];
				xMin = Mathf.Min (xMin, vector.x);
				xMax = Mathf.Max (xMax, vector.x);
				yMin = Mathf.Min (yMin, vector.y);
				yMax = Mathf.Max (yMax, vector.y);
			}
			return Rect.MinMaxRect (xMin, yMin, xMax, yMax);
		}

		public float GetTotalLength () {
			var length = 0f;
			foreach(var line in GetLines()) length += line.length;
			return length;
		}
		
		public Vector2 GetPositionAtArcLength (float edgeLength) {
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
		public Vector2 GetPositionAtNormalizedArcLength (float normalizedEdgeLength) {
			normalizedEdgeLength %= 1; 
			var edgeLength = GetTotalLength() * normalizedEdgeLength;
			// var edgeIndex = Mathf.FloorToInt(edgeLength);
			// var edgeArcLength = edgeIndex-edgeLength;
			return GetPositionAtArcLength(edgeLength);
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
			// If it's a whole number
			if(edgeDistance % 1 == 0) 
				return GetVertex((int)edgeDistance);
			int leftIndex = Mathf.FloorToInt(edgeDistance);
			int rightIndex = leftIndex+1;
			return Vector2.Lerp(GetVertex(leftIndex), GetVertex(rightIndex), edgeDistance-leftIndex);
		}
		
		//Returns the angle of a side of a shape at a distance. 
		//If the shape is a triangle, the angle of the sides are 0, 120 and 240.
		public float GetVertRelativeEdgeDegreesFromCenter(float edgeDistance){
			return Vector2.SignedAngle(GetVertRelativeEdgePosition(edgeDistance)-center, Vector2.up);
		}
		
		//Returns the angle of a side of a shape at a distance. 
		//If the shape is a triangle, the angle of the sides are 0, 120 and 240.
		public float GetVertexDegreesInternal(int vertIndex){
			Vector2 leftDir = GetVertex(vertIndex) - GetVertex(vertIndex-1);
			Vector2 rightDir = GetVertex(vertIndex) - GetVertex(vertIndex+1);
			return Vector2.SignedAngle(rightDir-leftDir, Vector2.up);
		}
		
		public float GetVertexDegreesInternal (int i = 0, int j = 1) {
			i = GetRepeatingVertexIndex(i);
			j = GetRepeatingVertexIndex(j);
			return Vector2.SignedAngle(vertices[j]-vertices[i], Vector2.up);
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
			var bestScore = Mathf.NegativeInfinity;
			var best = Vector2.zero;
			foreach(var vert in vertices) {
                var score = Vector2.Dot(vert, direction);
				if(score > bestScore) {
					best = vert;
					score = bestScore;
				}
			}
			return best;
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


		public static Polygon MakeConvexHull(List<Vector2> points) {
			return new Polygon(MakeConvexHullPoints(points));
		}


		
		

		/// 
		/// Transformation. 
		/// 
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
				Debug.Log("Cannot Scale PolygonDefinition because the input modifier does not have enough vertices. It has "+_scaleModifier.vertices.Length+". It requires at least "+_polygonDefinition.vertices.Length+".");
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


		///
		/// WINDING DIRECTION
		///
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

		public void FlipWindingOrder () {
			System.Array.Reverse(_vertices);
		}



		
		//
		// RAYCASTING
		//
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


		
		//
		// BLENDING
		//
		public static Polygon LerpAuto (Polygon from, Polygon to, float lerp) {
			if(from.vertices.Length == to.vertices.Length) return LerpEqualVertCount (from, to, lerp);
			else return LerpDifferentVertCount (from, to, lerp);
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
				var radians = degrees * Mathf.Deg2Rad;
				dir = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
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
		/// Gets the index of the repeating vertex.
		/// </summary>
		/// <returns>The repeating vertex index.</returns>
		/// <param name="vertexIndex">Vertex index.</param>
		public int GetRepeatingVertexIndex (int vertexIndex) {
			return Mod(vertexIndex, vertices.Length);
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
				var distance = SqrDistance(point, pointOnLine);
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

				var sqrDistance = SqrDistance(point, currentPoint);
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
			Rect rect = CreateEncapsulating(vertices);
			Vector2? randomPoint = null;
			while(randomPoint == null) {
				Vector2 testPoint = new Vector2 (Random.Range(rect.min.x, rect.max.x), Random.Range(rect.min.y, rect.max.y));
				if(ContainsPoint(testPoint)) {
					randomPoint = testPoint;
				}
			}
			return (Vector2)randomPoint;

			Rect CreateEncapsulating (params Vector2[] vectors) {
				float xMin = vectors[0].x;
				float xMax = vectors[0].x;
				float yMin = vectors[0].y;
				float yMax = vectors[0].y;
				for(int i = 1; i < vectors.Length; i++) {
					var vector = vectors[i];
					xMin = Mathf.Min (xMin, vector.x);
					xMax = Mathf.Max (xMax, vector.x);
					yMin = Mathf.Min (yMin, vector.y);
					yMax = Mathf.Max (yMax, vector.y);
				}
				return Rect.MinMaxRect (xMin, yMin, xMax, yMax);
			}
		}

		public float GetArea () {
			float area = 0;
			var triangles = new List<int>();
			Triangulator.GenerateIndices(_vertices, triangles);
			for(int i = 0; i < triangles.Count; i+=3) {
				var triangle = new Triangle(_vertices[triangles[i]], _vertices[triangles[i+1]], _vertices[triangles[i+2]]);
				area += triangle.area;
			}
			return area;
		}



		struct QuadCell {
			public Vector2 centre; // cell center
			public float halfSize; // half the cell size
			public float distanceFromPoly; 
			public float maxDistanceFromPoly; 
		}

		/// <summary>
		/// The point within the poly which is the hardest to reach from all of its boundaries.
		/// See https://github.com/mapbox/polylabel
		/// </summary>
		public Vector2 PoleOfInaccesibility() {
			QuadCell MakeQuadCell(Vector2 centre, float halfHeight ) {
				var qc = new QuadCell();
				qc.centre = centre;
				qc.halfSize = halfHeight;

				qc.distanceFromPoly = SignedDistanceFromPoint(qc.centre); // distance from cell center to polygon
				qc.maxDistanceFromPoly = qc.distanceFromPoly + qc.halfSize * Mathf.Sqrt(2); // max distance to polygon within a cell

				return qc;
			}
			
			if (_vertices.Length == 0) {
				return new Vector2(0,0);
			}

			// find the bounding box of the outer ring
			float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MaxValue, maxY = float.MaxValue;
			for (var i = 0; i < _vertices.Length; i++) {
			var p = _vertices[i];
				if (i == 0 || p[0] < minX) minX = p[0];
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
					cellQueue.Add(MakeQuadCell(new Vector2 (x + h, y + h), h));
				}
			}

			// take zero-size centroid cell as the first best guess. Might be outside region.
			var bestCell = MakeQuadCell(centroid, 0);

			// special case for rectangular polygons
			var bboxCell = MakeQuadCell(new Vector2 (minX + width / 2, minY + height / 2), 0);
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
				cellQueue.Add(MakeQuadCell(new Vector2 (topCell.centre.x - h, topCell.centre.y - h), h));
				cellQueue.Add(MakeQuadCell(new Vector2 (topCell.centre.x + h, topCell.centre.y - h), h));
				cellQueue.Add(MakeQuadCell(new Vector2 (topCell.centre.x - h, topCell.centre.y + h), h));
				cellQueue.Add(MakeQuadCell(new Vector2 (topCell.centre.x + h, topCell.centre.y + h), h));
				numProbes += 4;
			}

			return bestCell.centre;
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
		public bool CompareTo(Vector2[] verts)
		{
			if (_vertices.Length != verts.Length) return false;
			for (int i = 0; i < _vertices.Length; ++i)
				if (_vertices[i] != verts[i]) return false;
			return true;
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
			float AngleValue(float x1, float y1, float x2, float y2) {
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
			/*
			// Cull points out of the convex hull that lie inside the
			// trapezoid defined by the vertices with smallest and
			// largest X and Y coordinates.
			// Return the points that are not culled.
			List<Vector2> HullCull(List<Vector2> _points)
			{
				// Find a culling box.
				Rect culling_box = GetMinMaxBox(_points);

				// Cull the _points.
				List<Vector2> results = new List<Vector2>();
				foreach (Vector2 pt in _points)
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

			// Find a box that fits inside the MinMax quadrilateral.
			Rect GetMinMaxBox(List<Vector2> _points) {
				// Find the MinMax quadrilateral.
				Vector2 ul = new Vector2(0, 0), ur = ul, ll = ul, lr = ul;
				GetMinMaxCorners(_points, ref ul, ref ur, ref ll, ref lr);

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

			// Find the points nearest the upper left, upper right,
			// lower left, and lower right corners.
			void GetMinMaxCorners(List<Vector2> _points, ref Vector2 ul, ref Vector2 ur, ref Vector2 ll, ref Vector2 lr) {
				// Start with the first point as the solution.
				ul = _points[0];
				ur = ul;
				ll = ul;
				lr = ul;

				// Search the other _points.
				foreach (Point pt in _points) {
					if (-pt.x - pt.y > -ul.x - ul.y) ul = pt;
					if (pt.x - pt.y > ur.x - ur.y) ur = pt;
					if (-pt.x + pt.y > -ll.x + ll.y) ll = pt;
					if (pt.x + pt.y > lr.x + lr.y) lr = pt;
				}
			}
			*/
        }




        

		
		///
		/// SMOOTHING
		///
		public static List<Vector2> GetSmoothed(IList<Vector2> vertices, float radius, float degreesPerPoint) {
			List<Vector2> points = new List<Vector2>();
			if(vertices.Count == 0) return points;
			for(int i = 0; i < vertices.Count; i++) {
				var p1 = Vector2.Lerp(vertices[(i == 0 ? vertices.Count-1 : i-1)], vertices[i], 0.5f);
				var p2 = Vector2.Lerp(vertices[i], vertices[(i == vertices.Count-1 ? 0 : i+1)], 0.5f);
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


		public static Vector2[] GetExtruded (Polygon polygon, float extrusion) {
			var clockwise = polygon.GetIsClockwise();
        	Vector2[] extrudedPoints = new Vector2[polygon._vertices.Length];
			Vector2 endCorner = Vector2.zero;
			Vector2 endCornerOuter = Vector2.zero;
			for(int i = 0; i < polygon._vertices.Length; i++) {
				GetVertPoints(i+1, clockwise, out endCorner, out endCornerOuter, extrusion);
				extrudedPoints[i] = endCornerOuter;
			}

			void GetVertPoints (int i, bool _clockwise, out Vector2 point, out Vector2 outerPoint, float _extrusion) {
				point = polygon.GetVertex(i);
				
				var edgeATangent = polygon.GetEdgeTangentAtEdgeIndex(i-1);
				var edgeANormal = Polygon.GetEdgeNormalAtEdgeIndex(edgeATangent, _clockwise);

				var edgeBTangent = polygon.GetEdgeTangentAtEdgeIndex(i);
				var edgeBNormal = Polygon.GetEdgeNormalAtEdgeIndex(edgeBTangent, _clockwise);
				
				var edgeAOffsetOuterLine = new Line(point + edgeANormal * _extrusion, point + edgeANormal * _extrusion + edgeATangent);
				var edgeBOffsetOuterLine = new Line(point + edgeBNormal * _extrusion, point + edgeBNormal * _extrusion + edgeBTangent);
				if(!Line.LineIntersectionPoint(edgeAOffsetOuterLine, edgeBOffsetOuterLine, out outerPoint, false)) {
					outerPoint = point + edgeANormal * _extrusion;
				}
			}

			return extrudedPoints;
		}
		///
		/// SIMPLIFICATION
		///

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
				float d = Line.GetClosestDistanceFromLine(pointList[0], pointList[end], pointList[i], false);
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

		/*
		// Removes points on a polygon that don't contribute to its shape (points lying on the line between neighbours)
		public static List<Vector2> GetSimplifiedVerts(IList<Vector2> verts) {
			List<Vector2> points = new List<Vector2>();
			var point = verts[0];
			Line line = default(Line);
			line.start = verts[verts.Count-1];
			for (int i = 0; i < verts.Count; i++) {
				line.end = verts[i == verts.Count-1 ? 0 : (i+1)];
				var skipPoint = false;
				if(line.start == line.end || point == line.start || point == line.end || SqrDistance(point, line.GetClosestPointOnLine(point)) < 0.0001f) {
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
				if(line.start == line.end || point == line.start || point == line.end) skipPoint = true;
				if(!skipPoint) {
					var closestPointOnLine = line.GetClosestPointOnLine(point);
					var sqrDistance = (point.x-closestPointOnLine.x) * (point.x-closestPointOnLine.x) + (point.y-closestPointOnLine.y) * (point.y-closestPointOnLine.y);
					if(sqrDistance < 0.0001f) skipPoint = true;
				}
				// if(!skipPoint) {
					// var lineVector = line.end-line.start;
					// var startCenterVector = point-line.start;
					// if(Vector2.Dot(lineVector, startCenterVector)) {
					// 	skipPoint = true;
					// }
				// }
				if(!skipPoint) {
					points.Add(point);
					line.start = point;
				}
				point = line.end;
			}
			return points;
		}
		*/


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

		public override string ToString () {
			string s = "Polygon: VertCount="+VertCount+" Connected="+connected+" Valid="+IsValid();
			for (int i = 0; i < _vertices.Length; ++i)
				s += "\n"+i+": "+_vertices[i];
			return s;
		}
		




		///
		/// UTILS
		///

		/// <summary>
		/// Returns the index of the closest vector to v in the values list
		/// </summary>
		/// <returns>The index.</returns>
		/// <param name="v">V.</param>
		/// <param name="values">Values.</param>
		private static int ClosestIndex(Vector2 v, IList<Vector2> values){
			int index = 0;
			float closest = SqrDistance(v, values[index]);
			for(int i = 1; i < values.Count; i++){
				float distance = SqrDistance(v, values[i]);
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
			float furthest = SqrDistance(v, values[index]);
			for(int i = 1; i < values.Count; i++){
				float distance = SqrDistance(v, values[i]);
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

		static float SqrDistance (Vector2 a, Vector2 b) {
			return (a.x-b.x) * (a.x-b.x) + (a.y-b.y) * (a.y-b.y);
		}
		
		public static int Mod(int a, int n) {
			int remainder = a%n;
			if ((n > 0 && remainder < 0) || (n < 0 && remainder > 0)) return remainder + n;
			return remainder;
		}
	}
}