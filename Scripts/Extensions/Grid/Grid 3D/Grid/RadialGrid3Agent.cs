using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;
using System.Collections.ObjectModel;

[ExecuteAlways]
public class RadialGrid3Agent : MonoBehaviour {
	public WorldGrid3 worldGrid;
	public float spawnRadius = 50;	
	[System.NonSerialized] public HashSet<Vector3Int> chunkPoints = new HashSet<Vector3Int>();
	public System.Action<List<Vector3Int>> OnEnterPoints;
	public System.Action<List<Vector3Int>> OnExitPoints;

	void OnEnable () {
		chunkPoints.Clear();
	}
	
	static List<Vector3Int> entered;
	static List<Vector3Int> exited;
	public void Update () {
		var newChunkPoints = worldGrid.GetPointsInRadius(transform.position, spawnRadius);
		
		if(!chunkPoints.SequenceEqual(newChunkPoints)) {
			IEnumerableX.GetChanges(chunkPoints, newChunkPoints, out exited, out entered);
			chunkPoints.Clear();
			chunkPoints.AddRange(newChunkPoints);
			if(!entered.IsEmpty()) if(OnEnterPoints != null) OnEnterPoints(entered);
			if(!exited.IsEmpty()) if(OnExitPoints != null) OnExitPoints(exited);
		}
	}

	#if UNITY_EDITOR
	[SerializeField]
	bool drawRadius = true;
	[SerializeField]
	bool drawIntersectionPoints = true;
	void OnDrawGizmosSelected () {
		GizmosX.BeginColor(Color.green.WithAlpha(0.3f));
		if(drawIntersectionPoints) {
			foreach(var chunkPoint in chunkPoints)
				Gizmos.DrawCube(worldGrid.ChunkToWorldSpace(chunkPoint), worldGrid.chunkToWorldMatrix.MultiplyVector(Vector3.one));
			Gizmos.color = Color.blue;
		}
		if(drawRadius) {
			Gizmos.DrawWireSphere(transform.position, spawnRadius);
		}
		GizmosX.EndColor();
	}
	#endif
}
