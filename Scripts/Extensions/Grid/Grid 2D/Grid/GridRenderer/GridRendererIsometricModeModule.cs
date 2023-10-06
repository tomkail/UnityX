using UnityEngine;

public class GridRendererIsometricModeModule : GridRendererModeModule {
    public float isometricHeight = 0.6f;
    public Vector3 isometricScale {
        get {
            return new Vector3(1,isometricHeight,1);
        }
    }
    public float isometricAngle = -45f;
    public Quaternion isometricRotation {
        get {
            return Quaternion.Euler(0,0,isometricAngle);
        }
    }
    public override Matrix4x4 GetGridToLocalMatrix (Vector3 cellScale, Vector2 gridSize) {
        var _gridToLocalMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, isometricScale);
        _gridToLocalMatrix *= Matrix4x4.TRS(Vector3.zero, isometricRotation, cellScale);
        Vector3 centerOffset = new Vector3(-gridSize.x * 0.5f, -gridSize.y * 0.5f, 0);
        _gridToLocalMatrix *= Matrix4x4.TRS(centerOffset, Quaternion.identity, Vector3.one);
        return _gridToLocalMatrix;
    }
}
