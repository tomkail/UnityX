using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Editor only component for probing the NavMesh to see what it does under given conditions.
/// Multi-tool with various modes:
///   - Raycast: calls NavMesh.Raycast from object pos to target pos, with Gizmo showing result
///   - SamplePosition: Draws a (very small) ball on the closest point on the NavMesh to the object using NavMesh.SamplePosition
///   - Path find: Tries to find a full path from object pos to target path and shows it using gizmo lines
///   - Clamp to sampled NavMesh: Similar to SamplePosition, except it auto clamps the current transform position to the mesh
///     rather than drawing a ball.
/// </summary>
[ExecuteInEditMode]
public class NavMeshTester : MonoBehaviour {

	public enum Tool {
		Raycast,
		SamplePosition,
		PathFind,
		ClampToSampledNavMesh
	}

	public Tool currentTool;

	[PositionHandle]
	public Vector3 target;

	void LateUpdate() {
		if( currentTool == Tool.ClampToSampledNavMesh ) {
			NavMeshHit hit;
			if( NavMesh.SamplePosition(transform.position, out hit, 1000.0f, -1) )
				transform.position = hit.position;
		}
	}

	void OnDrawGizmos() {

		// Raycast
		if( currentTool == Tool.Raycast ) {
			NavMeshHit hit;
			if( NavMesh.Raycast(transform.position, target, out hit, -1) ) {
				Gizmos.color = Color.green;
				Gizmos.DrawLine(transform.position, hit.position);

				Gizmos.color = Color.red;
				Gizmos.DrawLine(hit.position, target);
			} else {
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(transform.position, target);
			}
		}

		// Sample position
		else if( currentTool == Tool.SamplePosition ) {
			NavMeshHit hit;
			if( NavMesh.SamplePosition(transform.position, out hit, 1000.0f, -1) ) {
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(hit.position, 0.02f);
			} else {
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(transform.position, 0.02f);
			}
		}

		// Path find
		else if( currentTool == Tool.PathFind ) {
			NavMeshPath path = new NavMeshPath();
			bool found = NavMesh.CalculatePath(transform.position, target, -1, path);
			if( found ) {
				Gizmos.color = Color.white;
				for (int i = 0; i < path.corners.Length - 1; i++)
					Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
			}
		}

	}
}
