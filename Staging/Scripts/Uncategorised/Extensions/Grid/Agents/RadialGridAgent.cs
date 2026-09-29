using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;

[ExecuteAlways]
public class RadialGridAgent : MonoBehaviour {
	public SquareGridRenderer worldGrid;
	public bool clampToGrid;
	public float radius = 50;
	public float worldRadius {
		get {
			return Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z) * radius;
		}
	}

	[System.NonSerialized]
	public List<Vector2Int> chunkPoints = new List<Vector2Int>();
	public System.Action<List<Vector2Int>> OnEnterPoints;
	public System.Action<List<Vector2Int>> OnExitPoints;
	public bool showGizmos;

	void OnEnable () {
		chunkPoints.Clear();
	}
	void Update () {
		var newChunkPoints = worldGrid.GetPointsInRadius(transform.position, worldRadius, clampToGrid);
		// The enter/exit diffing below is intentionally near-identical to SquareGridAgent's, not shared: both are
		// MonoBehaviours with no common base, so sharing it would need an invasive base-class/serialization change for little gain.
		var entered = newChunkPoints.Except(chunkPoints).ToList();
		var exited = chunkPoints.Except(newChunkPoints).ToList();
		chunkPoints.Clear();
		chunkPoints.AddRange(newChunkPoints);
		if(!entered.IsEmpty()) {
			if(OnEnterPoints != null) OnEnterPoints(entered);
		}
		if(!exited.IsEmpty()) {
			if(OnExitPoints != null) OnExitPoints(exited);
		}
	}

	void OnDrawGizmosSelected () {
		if(showGizmos) {
			GizmosX.BeginColor(Color.green.WithAlpha(0.3f));
			foreach(var chunkPoint in chunkPoints) {
				var center = worldGrid.cellCenter.GridToWorldPoint(chunkPoint);
				var size = worldGrid.cellCenter.GridToWorldVector(Vector2.one);
				// GizmosX.DrawWirePolygon(worldGrid.edge.GridRectToWorldRect(new Rect(chunkPoint.x, chunkPoint.y, 1, 1)));
				Gizmos.DrawWireCube(center, size);
				Gizmos.DrawCube(center, size);
			}
			GizmosX.EndColor();
		}
		GizmosX.BeginColor(Color.blue);
		Gizmos.DrawWireSphere(transform.position, worldRadius);
		GizmosX.EndColor();
	}
}
