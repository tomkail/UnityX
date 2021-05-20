using UnityEngine;

public class FPSDebugSettings : ScriptableObject {
    public float fpsGraphHistoryTime = 1.0f; // The number of seconds to keep the FPS history for the graph.
	public Rect fpsPos = new Rect (10, 10, 100, 400);
    public bool showInEditor = true;
    public bool showInDevBuilds = true;
    public bool showInReleaseBuilds = false;
}
