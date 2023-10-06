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

            RectTransformX.ScreenPointToNormalizedPointInRectangle(target, pivotFingerScreenPos, camera, out Vector2 normalizedPivotFingerScreenPos);
            var deltaAngle = Vector2.SignedAngle(Vector2.up, fingerPos-pivotFingerScreenPos) - Vector2.SignedAngle(Vector2.up, lastFingerScreenPos-pivotFingerScreenPos);
            target.RotateAround(worldPivotPos, new Vector3(0,0,1), deltaAngle);
            
            RectTransformX.ScreenPointToNormalizedPointInRectangle(target, lastFingerScreenPos, camera, out Vector2 normalizedLastFingerPoint);
            RectTransformX.ScreenPointToNormalizedPointInRectangle(target, fingerPos, camera, out Vector2 normalizedFingerPoint);
            var lastDistanceFromPivot = Vector2.Distance(normalizedLastFingerPoint, normalizedPivotFingerScreenPos);
            var delta = SignedDistanceInDirection(normalizedFingerPoint, normalizedLastFingerPoint, normalizedPivotFingerScreenPos-normalizedFingerPoint);
            float SignedDistanceInDirection (Vector2 fromVector, Vector2 toVector, Vector2 direction) {
                Vector2 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
                return Vector2.Dot(Vector2X.FromTo(fromVector, toVector), normalizedDirection);
            }
            
            if(delta != 0 && lastDistanceFromPivot != 0) {
                float scaleMultiplier = 1+(delta/lastDistanceFromPivot);
                ScaleAroundRelative(target, worldPivotPos, scaleMultiplier * Vector3.one);
            }
        }
    }

    /// <summary>
    /// Scales the target around an arbitrary point by scaleFactor.
    /// This is relative scaling, meaning using  scale Factor of Vector3.one
    /// will not change anything and new Vector3(0.5f,0.5f,0.5f) will reduce
    /// the object size by half.
    /// The pivot is in world space.
    /// Scaling is applied to localScale of target.
    /// </summary>
    /// <param name="target">The object to scale.</param>
    /// <param name="pivot">The point to scale around in space of target.</param>
    /// <param name="scaleFactor">The factor with which the current localScale of the target will be multiplied with.</param>
    public static void ScaleAroundRelative(Transform target, Vector3 pivot, Vector3 scaleFactor)
    {
        // pivot
        var pivotDelta = target.position - pivot;
        pivotDelta.Scale(scaleFactor);
        target.position = pivot + pivotDelta;
    
        // scale
        var finalScale = target.localScale;
        finalScale.Scale(scaleFactor);
        target.localScale = finalScale;
    }
    
    /// <summary>
    /// Scales the target around an arbitrary pivot.
    /// This is absolute scaling, meaning using for example a scale factor of
    /// Vector3.one will set the localScale of target to x=1, y=1 and z=1.
    /// The pivot is in world space.
    /// Scaling is applied to localScale of target.
    /// </summary>
    /// <param name="target">The object to scale.</param>
    /// <param name="pivot">The point to scale around in the space of target.</param>
    /// <param name="scaleFactor">The new localScale the target object will have after scaling.</param>
    public static void ScaleAround(Transform target, Vector3 pivot, Vector3 newScale)
    {
        // pivot
        Vector3 pivotDelta = target.position - pivot; // diff from object pivot to desired pivot/origin
        Vector3 scaleFactor = new Vector3(
            newScale.x / target.localScale.x,
            newScale.y / target.localScale.y,
            newScale.z / target.localScale.z );
        pivotDelta.Scale(scaleFactor);
        target.position = pivot + pivotDelta;
    
        //scale
        target.localScale = newScale;
    }

    void OnDrawGizmos () {
        // Camera.main.ScreenToWorldPoint(new Vector3(pivotFingerPos));
    }

    void OnGUI () {
        OnGUIX.DrawCircle(OnGUIX.ScreenToGUIPoint(pivotFingerScreenPos), 10, Color.white, 2);
        OnGUIX.DrawCircle(OnGUIX.ScreenToGUIPoint(fingerPos), 10, Color.white, 2);
    }
}
