

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

[System.Serializable]
public class AdvancedUILine {
    
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
    
    
    public AdvancedUILine (params Vector2[] _vertices) {
        this.vertices = _vertices;
    }
    
    public AdvancedUILine (AdvancedUILine _polygon) {
        vertices = (Vector2[]) _polygon.vertices.Clone();
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
    public static AdvancedUILine Scale (AdvancedUILine _polygonDefinition, AdvancedUILine _scaleModifier) {
        AdvancedUILine newAdvancedUILineDefinition = new AdvancedUILine(_polygonDefinition);
        if(newAdvancedUILineDefinition.vertices.Length > _scaleModifier.vertices.Length) 
            DebugX.Log(null, "Cannot Scale AdvancedUILineDefinition because the input modifier does not have enough vertices. It has "+_scaleModifier.vertices.Length+". It requires at least "+_polygonDefinition.vertices.Length+".");
        for(int i = 0; i < _polygonDefinition.vertices.Length; i++) {
            Vector2.Scale(newAdvancedUILineDefinition.vertices[i], _scaleModifier.vertices[i]);
        }

        return newAdvancedUILineDefinition;
    }
    
    /// <summary>
    /// Returns a scaled polygon.
    /// </summary>
    /// <param name="_polygonDefinition">_polygon definition.</param>
    /// <param name="_scaleModifier">_scale modifier.</param>
    public static AdvancedUILine Scale (AdvancedUILine _polygonDefinition, Vector2 _scaleModifier) {
        if(_scaleModifier == Vector2.one) return _polygonDefinition;
        
        AdvancedUILine newAdvancedUILineDefinition = new AdvancedUILine(_polygonDefinition);
        for(int i = 0; i < _polygonDefinition.vertices.Length; i++) {
            Vector2.Scale(newAdvancedUILineDefinition.vertices[i], _scaleModifier);
        }
        return newAdvancedUILineDefinition;
    }
    
    /// <summary>
    /// Returns a scaled polygon.
    /// </summary>
    /// <param name="_polygonDefinition">_polygon definition.</param>
    /// <param name="_scaleModifier">_scale modifier.</param>
    public static AdvancedUILine Scale (AdvancedUILine _polygonDefinition, float _scaleModifier) {
        if(_scaleModifier == 1) return _polygonDefinition;
        
        AdvancedUILine newAdvancedUILineDefinition = new AdvancedUILine(_polygonDefinition);
        for(int i = 0; i < _polygonDefinition.vertices.Length; i++) {
            newAdvancedUILineDefinition.vertices[i] = _polygonDefinition.vertices[i] * _scaleModifier;
        }
        return newAdvancedUILineDefinition;
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
    
    
    /// <summary>
    /// Gets the vertex.
    /// </summary>
    /// <returns>The vertex.</returns>
    /// <param name="edgeIndex">Edge index.</param>
    public Vector2 GetVertex (int edgeIndex) {
        return vertices[(int)GetRepeatingVertexIndex(edgeIndex)];
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
    public Vector2 GetEdgePosition (float edgeDistance) {
        edgeDistance = GetRepeatingVertexIndex(edgeDistance);
        if(edgeDistance.IsWhole()) 
            return GetVertex((int)edgeDistance);
        int leftIndex = Mathf.FloorToInt(edgeDistance);
        int rightIndex = leftIndex+1;
        return Vector2.Lerp(GetVertex(leftIndex), GetVertex(rightIndex), edgeDistance-leftIndex);
    }
    
    //Returns the angle of a side of a shape at a distance. 
    //If the shape is a triangle, the angle of the sides are 0, 120 and 240.
    public float GetEdgeDegreesFromCenter(float edgeDistance){
        return Vector2X.DegreesBetween(center, GetEdgePosition(edgeDistance));
    }
    
    //Returns the angle of a side of a shape at a distance. 
    //If the shape is a triangle, the angle of the sides are 0, 120 and 240.
    public float GetVertexDegreesInternal(int vertIndex){
        Vector2 leftDir = GetVertex(vertIndex) - GetVertex(vertIndex-1);
        Vector2 rightDir = GetVertex(vertIndex) - GetVertex(vertIndex+1);
        return Vector2X.DegreesBetween(leftDir, rightDir);
    }
    
    public float GetVertexDegreesInternal (int i = 0, int j = 1) {
        i = (int)GetRepeatingVertexIndex(i);
        j = (int)GetRepeatingVertexIndex(j);
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

    public Line[] GetLines () {
        Line[] _lines = new Line[_vertices.Length];
        for(int i = 0; i < _vertices.Length-1; i++) _lines[i] = new Line(_vertices[i], _vertices[i+1]);
        _lines[_vertices.Length-1] = new Line(_vertices[_vertices.Length-1], _vertices[0]);
        return _lines;
    }

    static Vector2 PointInPolyFromIndex(AdvancedUILine poly, int index) {
        return poly.vertices [MathX.Mod(index, poly.vertices.Length)];
    }

    const float epsilon = 0.001f;

    static int GetIndexInPolyAtPoint(AdvancedUILine poly, Vector2 point) {
        return GetIndexInPolyLyingOnLineBetween (poly, point.AddX(-1 * epsilon), point.AddX(epsilon));
    }

    static int GetIndexInPolyLyingOnLineBetween(AdvancedUILine poly, Vector2 pointA, Vector2 pointB) {
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

    AdvancedUILine AddVert(int indexToAddAfter, Vector2 pointToAdd) {
        var verts = new List<Vector2> (vertices);
        if (indexToAddAfter == vertices.Length) {
            verts.Add (pointToAdd);
        } else {
            verts.Insert (indexToAddAfter + 1, pointToAdd);
        }
        return new AdvancedUILine (verts.ToArray ());
    }

    public struct AdvancedUILineRaycastHit {
        public float distance;
        public Vector2 point;
        // public Vector2 normal;
    }
    public bool RayAdvancedUILineIntersection(Vector2 rayOrigin, Vector2 rayDirection, out AdvancedUILineRaycastHit hit) {
        hit = new AdvancedUILineRaycastHit();
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

    public List<AdvancedUILineRaycastHit> RayAdvancedUILineIntersections(Vector2 rayOrigin, Vector2 rayDirection) {
        var hits = new List<AdvancedUILineRaycastHit>();
        RayAdvancedUILineIntersectionsNonAlloc(rayOrigin, rayDirection, ref hits);
        return hits;
    }

    public void RayAdvancedUILineIntersectionsNonAlloc(Vector2 rayOrigin, Vector2 rayDirection, ref List<AdvancedUILineRaycastHit> hits) {
        if(hits == null) hits = new List<AdvancedUILineRaycastHit>();
        hits.Clear();
        float distance;
        for (int j = _vertices.Length - 1, i = 0; i < _vertices.Length; j = i, i++) {
            if (new Line(_vertices[j], _vertices[i]).RayLineIntersect(rayOrigin, rayDirection, out distance)) {
                var hit = new AdvancedUILineRaycastHit();
                hit.distance = distance;
                hit.point = rayOrigin + (rayDirection * hit.distance);
                // Vector2 edge = _vertices[j] - _vertices[i];
                // hit.normal = new Vector2(-edge.y, edge.x).normalized;
                hits.Add(hit);
            }
        }  
    }

    public static AdvancedUILine LerpAuto (AdvancedUILine from, AdvancedUILine to, float lerp) {
        if(from.vertices.Length == to.vertices.Length) return LerpEqualVertCount (from, to, lerp);
        else return LerpDifferentVertCount (from, to, lerp);
    }

    public Vector2 EvaluateFromCenterUsingDegrees (float degrees) {
        AdvancedUILineRaycastHit intersection;
        RayAdvancedUILineIntersection(center, center + MathX.DegreesToVector2(degrees), out intersection);
        return intersection.point;
    }

    // Lerps by sampling at regular angular intervals. Loses fidelity, but results are smoother.
    public static AdvancedUILine LerpCustomVertCount (AdvancedUILine from, AdvancedUILine to, float lerp, int numSamples) {
        numSamples = Mathf.Max(numSamples, 3);
        Vector2[] newAdvancedUILineVerts = new Vector2[numSamples];
        float n = 1f/newAdvancedUILineVerts.Length;
        var centerA = from.center;
        var centerB = to.center;
        Vector2 sampleA, sampleB, dir;
        float degrees;
        AdvancedUILineRaycastHit intersection;
        for(int i = 0; i < newAdvancedUILineVerts.Length; i++) {
            degrees = n * i * 360;
            dir = MathX.DegreesToVector2(degrees);
            from.RayAdvancedUILineIntersection(centerA, dir, out intersection);
            sampleA = intersection.point;
            to.RayAdvancedUILineIntersection(centerB, dir, out intersection);
            sampleB = intersection.point;
            newAdvancedUILineVerts[i] = Vector2.Lerp(sampleA, sampleB, lerp);
        }
        return new AdvancedUILine(newAdvancedUILineVerts);
    }


    private static AdvancedUILine LerpEqualVertCount (AdvancedUILine from, AdvancedUILine to, float lerp) {
        Vector2[] newAdvancedUILineVerts = new Vector2[from.VertCount];
        for(int i = 0; i < newAdvancedUILineVerts.Length; i++) {
            newAdvancedUILineVerts[i] = Vector2.Lerp(from.vertices[i], to.vertices[i], lerp);
        }
        return new AdvancedUILine(newAdvancedUILineVerts);
    }

    private static AdvancedUILine LerpDifferentVertCount (AdvancedUILine from, AdvancedUILine to, float lerp) {
        // Vert count of the AdvancedUILineDefinition with the least verts
        int minVerts = Mathf.Min(from.vertices.Length, to.vertices.Length);
        // Vert count of the AdvancedUILineDefinition with the most verts
        int maxVerts = Mathf.Max(from.vertices.Length, to.vertices.Length);
        // Interpolated vert count between the min and max vert count, rounded up to the nearest whole number.
        AdvancedUILine largestAdvancedUILine = from.vertices.Length > to.vertices.Length ? from : to;
        
        //AdvancedUILine verts
        Vector2[] newAdvancedUILineVerts = new Vector2[maxVerts];
        
        for(int i = 0; i < minVerts; i++) {
            newAdvancedUILineVerts[i] = Vector2.Lerp(from[i], to[i], lerp);
        }
        
        for(int i = minVerts; i < maxVerts; i++) {
            if(from.vertices.Length > to.vertices.Length) {
                newAdvancedUILineVerts[i] = Vector2.Lerp(largestAdvancedUILine[i], newAdvancedUILineVerts[i-1], lerp);
            } else {
                newAdvancedUILineVerts[i] = Vector2.Lerp(newAdvancedUILineVerts[i-1], largestAdvancedUILine[i], lerp);
            }
        }
        return new AdvancedUILine(newAdvancedUILineVerts);
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
    public float GetRepeatingVertexIndex (float vertexIndex) {
        return Mathf.Repeat(vertexIndex, VertCount);
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

    public Vector2 FindClosestPointOnAdvancedUILine(Vector2 point) {
        float bestSqrDistance = Mathf.Infinity;
        Vector2 closestPoint = Vector2.zero;
        Vector2 currentPoint;
        for(int l = 0; l < _vertices.Length; l++) {
            if(l < _vertices.Length-1)
                currentPoint = Line.GetClosestPointOnLine(_vertices[l], _vertices[l+1], point);
            else
                currentPoint = Line.GetClosestPointOnLine(_vertices[l], _vertices[0], point);

            var sqrDistance = Vector2X.SqrDistance(point, currentPoint);
            if(sqrDistance < bestSqrDistance) {
                bestSqrDistance = sqrDistance;
                closestPoint = point;
            }
        }
        return closestPoint;
    }

    public void CopyFrom(AdvancedUILine src) {
        vertices = new Vector2[src.vertices.Length];
        for (int i = 0; i < _vertices.Length; ++i)
            _vertices[i] = src[i];		
    }
    
    public bool CompareTo(AdvancedUILine src)
    {
        if (_vertices.Length != src.vertices.Length) return false;
        for (int i = 0; i < _vertices.Length; ++i)
            if (_vertices[i] != src[i]) return false;
        return true;
    }

    public override string ToString () {
        string s = "AdvancedUILine: VertCount="+VertCount+" Connected="+connected+" Valid="+IsValid();
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

    public override bool Equals(System.Object obj) {
        // If parameter is null return false.
        if (obj == null) {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        AdvancedUILine p = (AdvancedUILine)obj;
        if ((System.Object)p == null) {
            return false;
        }

        // Return true if the fields match:
        return Equals(p);
    }

    public bool Equals(AdvancedUILine p) {
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

    public static bool operator == (AdvancedUILine left, AdvancedUILine right) {
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

    public static bool operator != (AdvancedUILine left, AdvancedUILine right) {
        return !(left == right);
    }
}