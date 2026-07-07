using UnityEngine;
using System.Collections.Generic;
using UnityX.Geometry;

public class WorldGrid3 : ScriptableObject {
	[OnChange("SetAsDirty")]
	public Vector3 gridCenter = Vector3.zero;
	[OnChange("SetAsDirty")]
	public float gridStep = 1;
	[OnChange("SetAsDirty")]
	public Quaternion rotation = Quaternion.identity;
	bool _isDirty = true;
	Matrix4x4 _chunkToWorldMatrix;
    public Matrix4x4 chunkToWorldMatrix {
        get {
			if(_isDirty) {
				_chunkToWorldMatrix = Matrix4x4.TRS(gridCenter, rotation, Vector3.one * gridStep);
				_isDirty = false;
			}
			return _chunkToWorldMatrix; 
        }
    }

	void SetAsDirty () {
		_isDirty = true;
	}

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

	public HashSet<Vector3Int> GetPointsInRadius (Vector3 circleCenter, float radius) {
		var chunkSample = WorldToChunkSpace(circleCenter);

		HashSet<Vector3Int> points = new HashSet<Vector3Int>();

		Vector3 _start = WorldToChunkSpace(circleCenter - Vector3.one * radius);
		Vector3Int start = new Vector3Int(Mathf.FloorToInt(_start.x), Mathf.FloorToInt(_start.y), Mathf.FloorToInt(_start.z));
		Vector3 _end = WorldToChunkSpace(circleCenter + Vector3.one * radius);
		Vector3Int end = new Vector3Int(Mathf.CeilToInt(_end.x), Mathf.CeilToInt(_end.y), Mathf.CeilToInt(_end.z));

		float radiusSquared = radius * radius;
		for (int x = start.x; x <= end.x; x++) {
			for (int y = start.y; y <= end.y; y++) {
				for (int z = start.z; z <= end.z; z++) {
					var point = new Vector3Int(x,y,z);
					var distance = GetSqrDistanceToChunk(chunkSample, circleCenter, point);
					if (distance <= radiusSquared) {
						points.Add(point);
					}
				}
			}
		}
		return points;
	}

    float GetSqrDistanceToChunk (Vector3 chunkSpaceTarget, Vector3 worldSpaceTarget, Vector3Int chunk) {
        Vector3 testPoint = Vector3.zero;
        testPoint.x = Mathf.Clamp(chunkSpaceTarget.x, chunk.x-0.5f, chunk.x+0.5f);
        testPoint.y = Mathf.Clamp(chunkSpaceTarget.y, chunk.y-0.5f, chunk.y+0.5f);
		testPoint.z = Mathf.Clamp(chunkSpaceTarget.z, chunk.z-0.5f, chunk.z+0.5f);
        Vector3 pointPosition = ChunkToWorldSpace(testPoint);
        return Vector3X.SqrDistance(worldSpaceTarget, pointPosition);
    }
}
