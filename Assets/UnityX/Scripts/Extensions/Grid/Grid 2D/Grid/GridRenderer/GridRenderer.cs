using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class GridRenderer : MonoBehaviour {
    public event System.Action<GridRenderer> OnRefresh;
	public Plane floorPlane => new(-transform.forward, transform.position);
	public GridRendererModeModule modeModule;
	public bool scaleWithGridSize = true;

	[SerializeField]
    Point _gridSize;
	public Point gridSize {
		get => _gridSize;
		set {
			if(_gridSize == value) return;
            _gridSize = value;
            Refresh();
		}
	}
	public Vector3 cellSize => scaleWithGridSize ? Vector3.one : new Vector3(1f/gridSize.x, 1f/gridSize.y, 1f/gridSize.x);
	
	public bool showGizmos;
	
	GridCenterConversion _cellCenter;
	public GridCenterConversion cellCenter {
        get {
            if(_cellCenter == null) Refresh();
            return _cellCenter;
        } private set => _cellCenter = value;
	}
	GridEdgeConversion _edge;

	public GridEdgeConversion edge {
        get {
            if(_edge == null) Refresh();
            return _edge;
        } private set => _edge = value;
	}

	void OnEnable () {
		Refresh();
	}
	void Update () {
		if (transform.hasChanged) {
            transform.hasChanged = false;
            Refresh();
        }
	}
    
	public void Refresh () {
		cellCenter = new GridCenterConversion(this);
		edge = new GridEdgeConversion(this);
        if(OnRefresh != null) OnRefresh(this);
	}
	
	public Vector2 CenterPositionToEdgePosition (Vector2 centerPoint) {
		return centerPoint+Vector2X.half;
	}

	public Vector2 EdgePositionToCenterPosition (Vector2 edgePoint) {
		return edgePoint-Vector2X.half;
	}

    [System.Serializable]
	public class GridCenterConversion : GridConversion {
		public override Point gridSize => gridRenderer.gridSize;

		public override Matrix4x4 gridToLocalMatrix {
			get {
                if(!_gridToLocalMatrixSet) {
	                _gridToLocalMatrix = gridRenderer.modeModule.GetGridToLocalMatrix(gridRenderer.cellSize, gridRenderer.gridSize);
                    Vector3 halfCellOffset = new Vector3(0.5f, 0.5f, 0);
                    _gridToLocalMatrix *= Matrix4x4.TRS(halfCellOffset, Quaternion.identity, Vector3.one);
                    _gridToLocalMatrixSet = true;
                }
                return _gridToLocalMatrix;
			}
		}
		public GridCenterConversion (GridRenderer gridRenderer) : base (gridRenderer) {}
	}

    [System.Serializable]
	public class GridEdgeConversion : GridConversion {
		public override Point gridSize => gridRenderer.gridSize+Point.one;

		public override Matrix4x4 gridToLocalMatrix {
			get {
                if(!_gridToLocalMatrixSet) {
	                _gridToLocalMatrix = gridRenderer.modeModule.GetGridToLocalMatrix(gridRenderer.cellSize, gridRenderer.gridSize);
                    _gridToLocalMatrixSet = true;
                }
                return _gridToLocalMatrix;
			}
		}
		public GridEdgeConversion (GridRenderer gridRenderer) : base (gridRenderer) {}
	}
        
	[System.Serializable]
	public abstract class GridConversion {
		protected GridRenderer gridRenderer;
		public abstract Point gridSize {get;}
		protected Transform transform => gridRenderer.transform;

        protected bool _gridToLocalMatrixSet = false;
        protected Matrix4x4 _gridToLocalMatrix = Matrix4x4.identity;
		public abstract Matrix4x4 gridToLocalMatrix {get;}
	
        protected bool _normalizedToGridMatrixSet = false;
    	Matrix4x4 _normalizedToGridMatrix = Matrix4x4.identity;
		public Matrix4x4 normalizedToGridMatrix {
            get {
                Debug.Assert(gridRenderer != null);
                if(!_normalizedToGridMatrixSet) {
                    var scale = new Vector3(gridSize.x-1, gridSize.y-1, 1);
                    _normalizedToGridMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
                    _normalizedToGridMatrixSet = true;
                }
                return _normalizedToGridMatrix;
            }
        }
        public Matrix4x4 gridToNormalizedMatrix => normalizedToGridMatrix.inverse;

        protected bool _normalizedToLocalMatrixSet = false;
        Matrix4x4 _normalizedToLocalMatrix = Matrix4x4.identity;
		public Matrix4x4 normalizedToLocalMatrix {
			get {
                if(!_normalizedToLocalMatrixSet) {
				    _normalizedToLocalMatrix = gridToLocalMatrix * normalizedToGridMatrix;
                    _normalizedToLocalMatrixSet = true;
                }
                return _normalizedToLocalMatrix;
            }
		}


        protected bool _gridToWorldMatrixSet = false;
        Matrix4x4 _gridToWorldMatrix = Matrix4x4.identity;
        public Matrix4x4 gridToWorldMatrix {
			get {
                if(!_gridToWorldMatrixSet) {
                    _gridToWorldMatrix = transform.localToWorldMatrix * gridToLocalMatrix;
                    _gridToWorldMatrixSet = true;
                }
				return _gridToWorldMatrix;
			}
		}

        protected bool _normalizedToWorldMatrixSet = false;
        Matrix4x4 _normalizedToWorldMatrix = Matrix4x4.identity;
		public Matrix4x4 normalizedToWorldMatrix {
			get {
                if(!_normalizedToWorldMatrixSet) {
                    _normalizedToWorldMatrix = transform.localToWorldMatrix * normalizedToLocalMatrix;
                    _normalizedToWorldMatrixSet = true;
                }
				return _normalizedToWorldMatrix;
			}
		}

		public GridConversion (GridRenderer gridRenderer) {
			this.gridRenderer = gridRenderer;
		}

		public Vector2 WorldToGridPosition (Vector3 worldPosition) {
			return gridToWorldMatrix.inverse.MultiplyPoint3x4(worldPosition);
		}
		
		public Vector2 LocalToNormalizedPosition (Vector3 worldPosition) {
			return normalizedToLocalMatrix.inverse.MultiplyPoint3x4(worldPosition);
		}

		public Vector2 WorldToNormalizedPosition (Vector3 worldPosition) {
			return normalizedToWorldMatrix.inverse.MultiplyPoint3x4(worldPosition);
		}


		public Vector3 GridToLocalPoint (Vector2 gridPosition) {
			return gridToLocalMatrix.MultiplyPoint3x4(gridPosition);
		}
		
		public Vector3 GridToWorldPoint (Vector2 gridPosition) {
			return gridToWorldMatrix.MultiplyPoint3x4(gridPosition);
		}

		public Vector3 NormalizedToLocalPoint (Vector2 normalizedPosition) {
			return normalizedToLocalMatrix.MultiplyPoint3x4(normalizedPosition);
		}
		
		public Vector3 NormalizedToWorldPoint (Vector2 normalizedPosition) {
			return normalizedToWorldMatrix.MultiplyPoint3x4(normalizedPosition);
		}

		public virtual Vector2 NormalizedToGridPosition (Vector2 normalizedPosition){
			return Grid.NormalizedToGridPosition(normalizedPosition, gridSize);
		}

		public virtual Vector2 GridToNormalizedPosition (Vector2 normalizedPosition){
			return Grid.GridToNormalizedPosition(normalizedPosition, gridSize);
		}
		

		public Vector3 WorldToGridVector (Vector3 worldVector) {
			return gridToWorldMatrix.inverse.MultiplyVector(worldVector);
		}
		public Vector3 LocalVectorToGridVector (Vector3 worldVector) {
			return gridToLocalMatrix.inverse.MultiplyVector(worldVector);
		}
		public Vector3 GridToWorldVector (Vector2 gridVector) {
			return gridToWorldMatrix.MultiplyVector(gridVector);
		}
		public Vector3 GridToLocalVector (Vector2 gridVector) {
			return gridToLocalMatrix.MultiplyVector(gridVector);
		}

		public Vector3 NormalizedToWorldVector (Vector2 normalizedVector) {
			var gridVector = NormalizedToGridPosition(normalizedVector);
			return GridToWorldVector(gridVector);
		}
		public Vector2 WorldToNormalizedVector (Vector3 worldVector) {
			var gridVector = WorldToGridVector(worldVector);
			return GridToNormalizedPosition(gridVector);
		}


		public Vector3[] GridToWorldRect (Rect gridRect) {
			Vector3[] worldPoints = new Vector3[4];
			worldPoints[0] = GridToWorldPoint(gridRect.position);
			worldPoints[1] = GridToWorldPoint(gridRect.position + new Vector2(gridRect.size.x, 0));
			worldPoints[2] = GridToWorldPoint(gridRect.position + gridRect.size);
			worldPoints[3] = GridToWorldPoint(gridRect.position + new Vector2(0, gridRect.size.y));
			return worldPoints;
		}
		public void GridToWorldRectNonAlloc (Rect gridRect, ref Vector3[] worldPoints) {
			if(worldPoints == null || worldPoints.Length != 4)
				worldPoints = new Vector3[4];
			worldPoints[0] = GridToWorldPoint(gridRect.position);
			worldPoints[1] = GridToWorldPoint(gridRect.position + new Vector2(gridRect.size.x, 0));
			worldPoints[2] = GridToWorldPoint(gridRect.position + gridRect.size);
			worldPoints[3] = GridToWorldPoint(gridRect.position + new Vector2(0, gridRect.size.y));
		}

		public Vector2[] GridToLocalRect (Rect gridRect) {
			Vector2[] worldPoints = new Vector2[4];
			worldPoints[0] = GridToLocalPoint(gridRect.position);
			worldPoints[1] = GridToLocalPoint(gridRect.position + new Vector2(gridRect.size.x, 0));
			worldPoints[2] = GridToLocalPoint(gridRect.position + gridRect.size);
			worldPoints[3] = GridToLocalPoint(gridRect.position + new Vector2(0, gridRect.size.y));
			return worldPoints;
		}
		
		public Vector2[] NormalizedToLocalRect (Rect normalizedRect) {
			Vector2[] worldPoints = new Vector2[4];
			worldPoints[0] = NormalizedToLocalPoint(normalizedRect.position);
			worldPoints[1] = NormalizedToLocalPoint(normalizedRect.position + new Vector2(normalizedRect.size.x, 0));
			worldPoints[2] = NormalizedToLocalPoint(normalizedRect.position + normalizedRect.size);
			worldPoints[3] = NormalizedToLocalPoint(normalizedRect.position + new Vector2(0, normalizedRect.size.y));
			return worldPoints;
		}

		public Vector3[] NormalizedToWorldRect (Rect normalizedRect) {
			Vector3[] worldPoints = new Vector3[4];
			worldPoints[0] = NormalizedToWorldPoint(normalizedRect.position);
			worldPoints[1] = NormalizedToWorldPoint(normalizedRect.position + new Vector2(normalizedRect.size.x, 0));
			worldPoints[2] = NormalizedToWorldPoint(normalizedRect.position + normalizedRect.size);
			worldPoints[3] = NormalizedToWorldPoint(normalizedRect.position + new Vector2(0, normalizedRect.size.y));
			return worldPoints;
		}

		public void NormalizedToWorldRectNonAlloc (Rect normalizedRect, Vector3[] worldPoints) {
			worldPoints[0] = NormalizedToWorldPoint(normalizedRect.position);
			worldPoints[1] = NormalizedToWorldPoint(normalizedRect.position + new Vector2(normalizedRect.size.x, 0));
			worldPoints[2] = NormalizedToWorldPoint(normalizedRect.position + normalizedRect.size);
			worldPoints[3] = NormalizedToWorldPoint(normalizedRect.position + new Vector2(0, normalizedRect.size.y));
		}
	}

	public IEnumerable<Point> GetPointsInWorldBounds (Bounds bounds, bool clamped = true) {
		Vector2 _min = cellCenter.WorldToGridPosition(bounds.min);
		Point min = new Point(Mathf.Floor(_min.x), Mathf.Floor(_min.y));
		Vector2 _max = cellCenter.WorldToGridPosition(bounds.max);
		Point max = new Point(Mathf.Ceil(_max.x), Mathf.Ceil(_max.y));

		foreach(var vert in bounds.GetVertices()) {
			var gridVert = cellCenter.WorldToGridPosition(vert);
			if(gridVert.x < min.x) min.x = Mathf.FloorToInt(gridVert.x);
			if(gridVert.y < min.y) min.y = Mathf.FloorToInt(gridVert.y);
			if(gridVert.x > max.x) max.x = Mathf.CeilToInt(gridVert.x);
			if(gridVert.y > max.y) max.y = Mathf.CeilToInt(gridVert.y);
		}
		if (clamped) {
			min.x = Mathf.Max(min.x, 0);
			min.y = Mathf.Max(min.y, 0);
			max.x = Mathf.Min(max.x, gridSize.x);
			max.y = Mathf.Min(max.y, gridSize.y);
		}
		var pointRect = PointRect.MinMaxRect(min, max);

		foreach(var point in pointRect.GetPoints()) {
			// if(bounds.Contains(cellCenter.GridToWorldPoint(point)))
				yield return point;
		}
	}

	public IEnumerable<Point> GetPointsInRadius (Vector3 circleCenter, float radius, bool clampToGrid) {
		var chunkSample = cellCenter.WorldToGridPosition(circleCenter);

		Vector2 _start = edge.WorldToGridPosition(circleCenter - Vector3.one * radius);
		Point start = new Point(Mathf.Floor(_start.x), Mathf.Floor(_start.y));
		Vector2 _end = edge.WorldToGridPosition(circleCenter + Vector3.one * radius);
		Point end = new Point(Mathf.Ceil(_end.x)+1, Mathf.Ceil(_end.y)+1);
		
		if(clampToGrid) {
			start.x = Mathf.Clamp(start.x, 0, gridSize.x);
			start.y = Mathf.Clamp(start.y, 0, gridSize.y);
			end.x = Mathf.Clamp(end.x, 0, gridSize.x);
			end.y = Mathf.Clamp(end.y, 0, gridSize.y);
		}
		if(start.x == end.x || start.y == end.y) yield break;
		// var gridStep = cellCenter.WorldVectorToGridVector(Vector2.one);
		// var gridStep = new Vector2(0,cellCenter.gridSize.x);
		// new Vector2(1f/(cellCenter.gridSize.x-1), 1f/(cellCenter.gridSize.y-1));
		float radiusSquared = radius * radius;
		//  - (gridStep * 0.5f).sqrMagnitude;
		// radiusSquared = (radius - (gridStep.magnitude * 0.5f));
		// radiusSquared *= radiusSquared;
		for (int x = start.x; x < end.x; x++) {
			for (int y = start.y; y < end.y; y++) {
				var point = new Point(x,y);
				var distance = GetSqrDistanceToChunk(chunkSample, circleCenter, point);
				if (distance <= radiusSquared) {
					yield return new Point(x,y);
				}
			}
		}
	}

	float GetSqrDistanceToChunk (Vector2 chunkSpaceTarget, Vector3 worldSpaceTarget, Point chunk) {
		Vector2 testPoint = Vector2.zero;
		testPoint.x = Mathf.Clamp(chunkSpaceTarget.x, chunk.x-0.5f, chunk.x+0.5f);
		testPoint.y = Mathf.Clamp(chunkSpaceTarget.y, chunk.y-0.5f, chunk.y+0.5f);
		Vector3 pointPosition = cellCenter.GridToWorldPoint(testPoint);
		return Vector3X.SqrDistanceAgainstDirection(worldSpaceTarget, pointPosition, transform.rotation * Vector3.forward);
	}

	public List<Point> OrderPointsByDistance (List<Point> points, Vector3 position) {
		var chunkSample = cellCenter.WorldToGridPosition(position);
		return points.OrderBy(x => GetSqrDistanceToChunk(chunkSample, position, x)).ToList();
	}

	public Vector3 ScreenToFloorPoint (Ray ray) {
		float distance;
		floorPlane.Raycast(ray, out distance);
		return ray.GetPoint(distance);
	}

	void OnDrawGizmos () {
		if(!showGizmos) return;
        
		GizmosX.BeginColor(Color.white.WithAlpha(1f));
        var bounds = edge.NormalizedToWorldRect(new Rect(0,0,1,1));
		GizmosX.DrawWirePolygon(bounds);
        // bounds = cellCenter.NormalizedRectToWorldRect(new Rect(0,0,1,1));
		// GizmosX.DrawWirePolygon(bounds);
		GizmosX.EndColor();

		GizmosX.BeginColor(Color.white.WithAlpha(0.25f));
		for(int y = 1; y < gridSize.y; y++)
            Gizmos.DrawLine(edge.GridToWorldPoint(new Vector2(0,y)), edge.GridToWorldPoint(new Vector2(gridSize.x,y)));
        for(int x = 1; x < gridSize.x; x++)
            Gizmos.DrawLine(edge.GridToWorldPoint(new Vector2(x,0)), edge.GridToWorldPoint(new Vector2(x,gridSize.y)));
        
		GizmosX.EndColor();
	}

	#if UNITY_EDITOR
	public void DrawHandles () {        
		HandlesX.BeginColor(Color.white.WithAlpha(1f));
        var bounds = edge.NormalizedToWorldRect(new Rect(0,0,1,1));
		HandlesX.DrawWirePolygon(bounds);
        // bounds = cellCenter.NormalizedRectToWorldRect(new Rect(0,0,1,1));
		// HandlesX.DrawWirePolygon(bounds);
		HandlesX.EndColor();

		HandlesX.BeginColor(Color.white.WithAlpha(0.25f));
		for(int y = 1; y < gridSize.y; y++)
            UnityEditor.Handles.DrawLine(edge.GridToWorldPoint(new Vector2(0,y)), edge.GridToWorldPoint(new Vector2(gridSize.x,y)));
        for(int x = 1; x < gridSize.x; x++)
            UnityEditor.Handles.DrawLine(edge.GridToWorldPoint(new Vector2(x,0)), edge.GridToWorldPoint(new Vector2(x,gridSize.y)));
        
		HandlesX.EndColor();
	}
	#endif
}