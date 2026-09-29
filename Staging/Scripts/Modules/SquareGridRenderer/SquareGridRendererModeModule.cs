using UnityEngine;

public abstract class SquareGridRendererModeModule : ScriptableObject {
    public abstract Matrix4x4 GetGridToLocalMatrix (Vector3 cellScale, Vector2 gridSize);
}