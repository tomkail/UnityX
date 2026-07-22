using UnityEngine;

/// <summary>
/// A reusable driver that composes any number of camera behaviours onto one <see cref="CameraProperties"/>
/// and applies the result to a <see cref="Camera"/> every frame — the generalised, game-agnostic version of
/// the hand-rolled "CameraController" a game usually writes (and of <c>DirectManipulationCameraTester</c>).
///
/// It owns a persistent working <see cref="CameraProperties"/> and a <see cref="CameraPropertiesBuilderQueue"/>.
/// Each frame it runs the queue's per-frame updates, then runs every modifier in sort order over the working
/// properties, then applies them to the camera. Contributors register themselves via <see cref="Add"/> /
/// <see cref="Remove"/> (e.g. <c>DirectManipulationCamera</c> registers its pan/pinch/zoom in <c>OnEnable</c>),
/// so the rig stays decoupled from any specific behaviour — that's what makes it a composition point rather
/// than a one-controller wrapper.
///
/// Modifiers see and mutate the SAME running properties in order (a pipeline, not a blend): earlier modifiers'
/// output is the input to later ones, and — because the properties persist across frames — accumulating
/// behaviours like a 1:1 pan build up naturally. If you want a blend between distinct camera states, do it
/// inside a modifier using <c>CameraProperties.WeightedBlend</c> / <c>Lerp</c> / <c>SmoothDamp</c>.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraRig : MonoBehaviour {
    public new Camera camera;

    [Header("Seeding")]
    [Tooltip("Seed the working properties from this transform on Start. Off if you set them yourself first.")]
    public bool seedOnStart = true;
    [Tooltip("Optional ground plane to aim the initial target at (its up = plane normal). If unset, the target " +
             "is seeded a fixed distance straight ahead. Assign the same floor transform your pan/zoom behaviours use.")]
    public Transform groundTransform;
    [Tooltip("Fallback orbit distance used to seed the target when there's no ground plane to aim at.")]
    public float seedTargetDistance = CameraProperties.defaultDistance;

    [Header("Debug")]
    public bool drawGizmos = true;

    // Serialized so you can watch it change live in the inspector as behaviours pan / pinch / zoom.
    [SerializeField] CameraProperties cameraProperties;
    public CameraProperties properties { get => cameraProperties; set => cameraProperties = value; }

    [Header("Modifier stack")]
    [Tooltip("The composed camera stack. Author plain-class modifiers (e.g. CameraPropertiesModifier) here; " +
             "MonoBehaviour and code modifiers register themselves at runtime via Add().")]
    [SerializeField] CameraPropertiesBuilderQueue queue = new CameraPropertiesBuilderQueue();
    public CameraPropertiesBuilderQueue modifiers => queue;

    // --- Composition API (thin passthrough to the queue, so callers needn't reach through .modifiers) ---
    public CameraPropertiesBuilderQueue.Entry Add (ICameraPropertiesModifier modifier, int sortIndex = 0, string name = null) => queue.Add(modifier, sortIndex, name);
    public CameraPropertiesBuilderQueue.Entry Add (CameraPropertiesBuilderQueue.UpdateCameraPropertiesDelegate update, CameraPropertiesBuilderQueue.ModifyCameraPropertiesDelegate modify, int sortIndex = 0, string name = null) => queue.Add(update, modify, sortIndex, name);
    public bool Remove (ICameraPropertiesModifier modifier) => queue.Remove(modifier);

    void Awake () {
        if(camera == null) camera = GetComponent<Camera>();
    }

    void Start () {
        if(seedOnStart) Seed();
        cameraProperties.ApplyTo(camera);
    }

    // Aim the working properties at the camera's current pose, targeting the ground plane if one is wired up
    // (so pan/zoom pivot on the floor), else a fixed distance straight ahead. Projection is copied from the
    // camera so applying the seed doesn't stomp its configured ortho/perspective settings.
    public void Seed () {
        Vector3 targetPoint;
        var ray = new Ray(transform.position, transform.forward);
        if(groundTransform != null && new Plane(groundTransform.up, groundTransform.position).Raycast(ray, out float enter)) {
            targetPoint = ray.GetPoint(enter);
        } else {
            targetPoint = transform.position + transform.forward * seedTargetDistance;
        }
        cameraProperties = CameraProperties.FromTo(transform.position, targetPoint, transform.rotation);
        cameraProperties.orthographic = camera.orthographic;
        cameraProperties.orthographicSize = camera.orthographicSize;
        cameraProperties.fieldOfView = camera.fieldOfView;
    }

    // LateUpdate so all gameplay/input Updates (e.g. InputPointManager) have run for the frame first.
    void LateUpdate () {
        if(!Application.isPlaying) return;
        queue.Update(Time.deltaTime);
        queue.Generate(ref cameraProperties);
        cameraProperties.ApplyTo(camera);
    }

    void OnDrawGizmosSelected () {
        if(!drawGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cameraProperties.targetPoint, 0.5f);
    }
}
