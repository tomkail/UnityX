using UnityEngine;
using UnityEngine.Serialization;

// Minimal driver for DirectManipulationCamera — the essential pattern distilled out of a game's CameraController:
// hold a working CameraProperties, let the pinch camera mutate it each frame from the current input
// (pan / pinch / zoom, routed via InputPointManager), then apply it back to the Camera.
//
// Scene setup to test:
//   - This component + a Camera on the same GameObject.
//   - A DirectManipulationCamera (wire its `camera`, `floorPlaneTransform`, `settings`, and optional `region`).
//   - An InputPointManager in the scene.
//   - A touch source, e.g. TrackpadTouchProvider (feeds a synthetic Touchscreen -> EnhancedTouch).
//
// No game-specific dependencies — drop it on a camera and go.
[RequireComponent(typeof(Camera))]
public class DirectManipulationCameraTester : MonoBehaviour {
    [FormerlySerializedAs("pinchZoomCamera")]
    public DirectManipulationCamera directManipulationCamera;
    [Tooltip("When off, the camera holds still (input is ignored) — handy for A/B testing.")]
    public bool controlEnabled = true;
    [Tooltip("Fallback orbit distance used to seed the target when there's no floor plane to aim at.")]
    public float seedTargetDistance = CameraProperties.defaultDistance;
    public bool drawGizmos = true;

    Camera camera;
    // Serialized so you can watch it live in the inspector while testing (targetPoint / distance /
    // orthographicSize etc. should change as you pan/pinch). CameraProperties is [Serializable].
    [SerializeField] CameraProperties cameraProperties;

    void Awake () {
        camera = GetComponent<Camera>();
    }

    void Start () {
        // Convenience: if the pinch camera wasn't given a camera, use ours.
        if (directManipulationCamera != null && directManipulationCamera.camera == null)
            directManipulationCamera.camera = camera;

        // Aim the orbit target at the point where the camera's view meets the floor plane, so pan/pinch
        // pivot on the floor. Falls back to a fixed distance in front if there's no floor plane wired up.
        Vector3 targetPoint;
        var ray = new Ray(transform.position, transform.forward);
        if (directManipulationCamera != null && directManipulationCamera.floorPlaneTransform != null
            && directManipulationCamera.floorPlane.Raycast(ray, out float enter)) {
            targetPoint = ray.GetPoint(enter);
        } else {
            targetPoint = transform.position + transform.forward * seedTargetDistance;
        }

        cameraProperties = CameraProperties.FromTo(transform.position, targetPoint, transform.rotation);
        cameraProperties.orthographic = camera.orthographic;
        cameraProperties.orthographicSize = camera.orthographicSize;
        cameraProperties.fieldOfView = camera.fieldOfView;
        cameraProperties.ApplyTo(camera);
    }

    void Update () {
        if (!Application.isPlaying) return;
        if (controlEnabled && directManipulationCamera != null)
            directManipulationCamera.SetCameraProperties(ref cameraProperties);
        cameraProperties.ApplyTo(camera);
    }

    void OnDrawGizmosSelected () {
        if (!drawGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cameraProperties.targetPoint, 0.5f);
    }
}
