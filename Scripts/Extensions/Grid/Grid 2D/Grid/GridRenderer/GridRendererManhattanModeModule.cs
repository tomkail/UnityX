using UnityEngine;

public class GridRendererManhattanModeModule : GridRendererModeModule {
    
    public override Matrix4x4 GetGridToLocalMatrix (Vector3 cellScale, Vector2 gridSize) {
        var _gridToLocalMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, cellScale);
        Vector3 centerOffset = new Vector3(-gridSize.x * 0.5f, -gridSize.y * 0.5f, 0);
        _gridToLocalMatrix *= Matrix4x4.TRS(centerOffset, Quaternion.identity, Vector3.one);
        return _gridToLocalMatrix;
    }
}
