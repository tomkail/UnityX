using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;

[ExecuteAlways]
public class SquareGridAgent : MonoBehaviour {
	public GridRenderer worldGrid;
	public List<Vector2Int> chunkPoints = new List<Vector2Int>();
	public System.Action<List<Vector2Int>> OnEnterPoints;
	public System.Action<List<Vector2Int>> OnExitPoints;

	void OnEnable () {
		chunkPoints.Clear();
	}
	void Update () {
		var newChunkPoints = worldGrid.GetPointsInWorldBounds(transform.GetBounds());
		// The enter/exit diffing below is intentionally near-identical to RadialGridAgent's, not shared: both are
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
		GizmosX.BeginColor(Color.green.WithAlpha(0.3f));
		foreach(var chunkPoint in chunkPoints) {
			// GizmosX.DrawWirePolygon(worldGrid.edge.GridRectToWorldRect(new Rect(chunkPoint.x, chunkPoint.y, 1, 1)));
			Gizmos.DrawWireCube(worldGrid.cellCenter.GridToWorldPoint(chunkPoint), worldGrid.cellCenter.GridToWorldVector(Vector2.one));
			Gizmos.DrawCube(worldGrid.cellCenter.GridToWorldPoint(chunkPoint), worldGrid.cellCenter.GridToWorldVector(Vector2.one));
		}
		Gizmos.color = Color.blue;
		GizmosX.DrawWireCube(transform.GetBounds());
		GizmosX.EndColor();
	}
}