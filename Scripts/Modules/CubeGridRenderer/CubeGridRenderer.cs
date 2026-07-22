using UnityEngine;
using System.Collections.Generic;

// A 3D world-space grid placed by its Transform, in the same spirit as SquareGridRenderer (2D).
// One chunk is one unit in local space, so the Transform's position/rotation/scale define where the
// grid sits and how large its cells are. Converts between chunk coordinates and world space, and
// draws the grid as gizmos.
[ExecuteAlways]
public class CubeGridRenderer : MonoBehaviour {

	public bool showGizmos;
	public int gizmoChunkExtent = 3;

	// Chunk space (one cell = one unit) -> world, straight off the Transform.
	public Matrix4x4 chunkToWorldMatrix => transform.localToWorldMatrix;

	public Vector3 ChunkToWorldSpace (Vector3 chunkPosition) {
		return chunkToWorldMatrix.MultiplyPoint3x4(chunkPosition);
	}

	public Vector3 ChunkToWorldSpace (Vector3Int chunkPoint) {
		return ChunkToWorldSpace((Vector3)chunkPoint);
	}

	public Vector3 WorldToChunkSpace (Vector3 worldPoint) {
		return chunkToWorldMatrix.inverse.MultiplyPoint3x4(worldPoint);
	}

	public Vector3Int WorldToChunkPoint (Vector3 worldPoint) {
		return ChunkSpaceToChunkPoint(WorldToChunkSpace(worldPoint));
	}

	public Vector3Int ChunkSpaceToChunkPoint (Vector3 chunkSpace) {
		return Vector3Int.RoundToInt(chunkSpace);
	}

	public HashSet<Vector3Int> GetPointsInRadius (Vector3 worldCenter, float radius) {
		var chunkSample = WorldToChunkSpace(worldCenter);
		HashSet<Vector3Int> points = new HashSet<Vector3Int>();

		Vector3 _start = WorldToChunkSpace(worldCenter - Vector3.one * radius);
		Vector3Int start = new Vector3Int(Mathf.FloorToInt(_start.x), Mathf.FloorToInt(_start.y), Mathf.FloorToInt(_start.z));
		Vector3 _end = WorldToChunkSpace(worldCenter + Vector3.one * radius);
		Vector3Int end = new Vector3Int(Mathf.CeilToInt(_end.x), Mathf.CeilToInt(_end.y), Mathf.CeilToInt(_end.z));

		float radiusSquared = radius * radius;
		for (int x = start.x; x <= end.x; x++)
			for (int y = start.y; y <= end.y; y++)
				for (int z = start.z; z <= end.z; z++) {
					var point = new Vector3Int(x, y, z);
					if (GetSqrDistanceToChunk(chunkSample, worldCenter, point) <= radiusSquared)
						points.Add(point);
				}
		return points;
	}

	float GetSqrDistanceToChunk (Vector3 chunkSpaceTarget, Vector3 worldSpaceTarget, Vector3Int chunk) {
		Vector3 testPoint = new Vector3(
			Mathf.Clamp(chunkSpaceTarget.x, chunk.x - 0.5f, chunk.x + 0.5f),
			Mathf.Clamp(chunkSpaceTarget.y, chunk.y - 0.5f, chunk.y + 0.5f),
			Mathf.Clamp(chunkSpaceTarget.z, chunk.z - 0.5f, chunk.z + 0.5f));
		Vector3 pointPosition = ChunkToWorldSpace(testPoint);
		return (worldSpaceTarget - pointPosition).sqrMagnitude;
	}

	void OnDrawGizmos () {
		if (!showGizmos) return;
		Gizmos.matrix = chunkToWorldMatrix;
		Gizmos.color = new Color(1f, 1f, 1f, 0.25f);
		int e = Mathf.Max(0, gizmoChunkExtent);
		for (int x = -e; x <= e; x++)
			for (int y = -e; y <= e; y++)
				for (int z = -e; z <= e; z++)
					Gizmos.DrawWireCube(new Vector3(x, y, z), Vector3.one);
		Gizmos.matrix = Matrix4x4.identity;
	}
}
