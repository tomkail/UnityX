using UnityEngine;

public abstract class GridRendererModeModule : ScriptableObject {
    public abstract Matrix4x4 GetGridToLocalMatrix (Vector3 cellScale, Vector2 gridSize);
}