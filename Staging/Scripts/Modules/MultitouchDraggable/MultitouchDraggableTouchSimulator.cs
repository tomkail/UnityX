using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultitouchDraggableTouchSimulator : MonoBehaviour {
    public MultitouchDraggable draggable;

    public RectTransform target;
    public Transform pivotFingerTransform;
    public Vector2 pivotFingerScreenPos;
    public Vector2 lastFingerScreenPos;
    public Vector2 fingerPos;
    public Vector2 normalizedFingerPoint;
    
    void Start () {
        Application.targetFrameRate = 60;
    }
    
    void Update () {
        var camera = GetComponentInParent<Canvas>().rootCanvas.worldCamera;

        pivotFingerScreenPos = RectTransformUtility.WorldToScreenPoint(camera, pivotFingerTransform.position);

        lastFingerScreenPos = fingerPos;
        fingerPos = Input.mousePosition;

        if(Input.GetMouseButton(0)) {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(target, pivotFingerScreenPos, camera, out Vector3 worldPivotPos);

            MultitouchDraggableInternal.ScreenPointToNormalizedPointInRectangle(target, pivotFingerScreenPos, camera, out Vector2 normalizedPivotFingerScreenPos);
            var deltaAngle = Vector2.SignedAngle(Vector2.up, fingerPos-pivotFingerScreenPos) - Vector2.SignedAngle(Vector2.up, lastFingerScreenPos-pivotFingerScreenPos);
            target.RotateAround(worldPivotPos, new Vector3(0,0,1), deltaAngle);
            
            MultitouchDraggableInternal.ScreenPointToNormalizedPointInRectangle(target, lastFingerScreenPos, camera, out Vector2 normalizedLastFingerPoint);
            MultitouchDraggableInternal.ScreenPointToNormalizedPointInRectangle(target, fingerPos, camera, out Vector2 normalizedFingerPoint);
            var lastDistanceFromPivot = Vector2.Distance(normalizedLastFingerPoint, normalizedPivotFingerScreenPos);
            var delta = SignedDistanceInDirection(normalizedFingerPoint, normalizedLastFingerPoint, normalizedPivotFingerScreenPos-normalizedFingerPoint);
            float SignedDistanceInDirection (Vector2 fromVector, Vector2 toVector, Vector2 direction) {
                Vector2 normalizedDirection = direction.normalized;
                return Vector2.Dot(toVector - fromVector, normalizedDirection);
            }
            
            if(delta != 0 && lastDistanceFromPivot != 0) {
                float scaleMultiplier = 1+(delta/lastDistanceFromPivot);
                ScaleAroundRelative(target, worldPivotPos, scaleMultiplier * Vector3.one);
            }
        }
    }

    // Scale helpers live on MultitouchDraggable (same folder); forward to avoid a verbatim copy.
    /// <summary>
    /// Scales the target around an arbitrary point by scaleFactor (relative scaling).
    /// See <see cref="MultitouchDraggable.ScaleAroundRelative"/>.
    /// </summary>
    public static void ScaleAroundRelative(Transform target, Vector3 pivot, Vector3 scaleFactor)
        => MultitouchDraggable.ScaleAroundRelative(target, pivot, scaleFactor);

    /// <summary>
    /// Scales the target around an arbitrary pivot to an absolute new local scale.
    /// See <see cref="MultitouchDraggable.ScaleAround"/>.
    /// </summary>
    public static void ScaleAround(Transform target, Vector3 pivot, Vector3 newScale)
        => MultitouchDraggable.ScaleAround(target, pivot, newScale);

    void OnDrawGizmos () {
        // Camera.main.ScreenToWorldPoint(new Vector3(pivotFingerPos));
    }

    void OnGUI () {
        MultitouchDraggableInternal.DrawCircle(MultitouchDraggableInternal.ScreenToGUIPoint(pivotFingerScreenPos), 10, Color.white, 2);
        MultitouchDraggableInternal.DrawCircle(MultitouchDraggableInternal.ScreenToGUIPoint(fingerPos), 10, Color.white, 2);
    }
}
