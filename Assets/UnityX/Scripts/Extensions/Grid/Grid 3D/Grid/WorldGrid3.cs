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

	public Vector3 ChunkToWorldSpace (Point3 chunkPoint) {
		return ChunkToWorldSpace((Vector3)chunkPoint);
	}

	public Vector3 WorldToChunkSpace (Vector3 worldPoint) {
		return chunkToWorldMatrix.inverse.MultiplyPoint3x4(worldPoint);
	}

	public Point3 WorldToChunkPoint (Vector3 worldPoint) {
		return ChunkSpaceToChunkPoint(WorldToChunkSpace(worldPoint));
	}

	public Point3 ChunkSpaceToChunkPoint (Vector3 chunkSpace) {
		return (Point3)chunkSpace;
	}

	public HashSet<Point3> GetPointsInRadius (Vector3 circleCenter, float radius) {
		var chunkSample = WorldToChunkSpace(circleCenter);

		HashSet<Point3> points = new HashSet<Point3>();

		Vector3 _start = WorldToChunkSpace(circleCenter - Vector3.one * radius);
		Point3 start = new Point3(Mathf.Floor(_start.x), Mathf.Floor(_start.y), Mathf.Floor(_start.z));
		Vector3 _end = WorldToChunkSpace(circleCenter + Vector3.one * radius);
		Point3 end = new Point3(Mathf.Ceil(_end.x), Mathf.Ceil(_end.y), Mathf.Ceil(_end.z));

		float radiusSquared = radius * radius;
		for (int x = start.x; x <= end.x; x++) {
			for (int y = start.y; y <= end.y; y++) {
				for (int z = start.z; z <= end.z; z++) {
					var point = new Point3(x,y,z);
					var distance = GetSqrDistanceToChunk(chunkSample, circleCenter, point);
					if (distance <= radiusSquared) {
						points.Add(point);
					}
				}
			}
		}
		return points;
	}

    float GetSqrDistanceToChunk (Vector3 chunkSpaceTarget, Vector3 worldSpaceTarget, Point3 chunk) {
        Vector3 testPoint = Vector3.zero;
        testPoint.x = Mathf.Clamp(chunkSpaceTarget.x, chunk.x-0.5f, chunk.x+0.5f);
        testPoint.y = Mathf.Clamp(chunkSpaceTarget.y, chunk.y-0.5f, chunk.y+0.5f);
		testPoint.z = Mathf.Clamp(chunkSpaceTarget.z, chunk.z-0.5f, chunk.z+0.5f);
        Vector3 pointPosition = ChunkToWorldSpace(testPoint);
        return Vector3X.SqrDistance(worldSpaceTarget, pointPosition);
    }
}
