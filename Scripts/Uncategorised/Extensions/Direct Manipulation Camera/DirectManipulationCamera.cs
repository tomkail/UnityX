using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityX.Inputs;

/// <summary>
/// Direct-manipulation camera controller for a single ground plane. Drives a <see cref="CameraProperties"/>
/// from touch (via InputPointManager / EnhancedTouch) and mouse input:
///   - one finger / left-drag : 1:1 pan (the grabbed point stays under the finger/cursor),
///   - two fingers            : pinch-zoom + rotate,
///   - mouse wheel            : zoom (auto-suppressed while a touchscreen is active).
/// Everything is anchored to <see cref="floorPlaneTransform"/> via screen-to-floor raycasts, so pan and
/// zoom work for both orthographic and perspective cameras.
///
/// Rotation caveat: two-finger rotation is a *straight-down* gesture. It rolls the view about the camera's
/// local Z (a "rotate the photo" feel), which only stays anchored — and only looks right — when the camera
/// is looking roughly straight at the plane. On a tilted camera it cants the horizon and can't keep both
/// points pinned, so turn <see cref="allowPinchRotation"/> off for tilted/angled cameras. Pan and zoom
/// degrade gracefully at a tilt; rotation is the part that assumes a top-down view.
/// </summary>
public class DirectManipulationCamera : MonoBehaviour, ICameraPropertiesModifier {
	public DirectManipulationCameraSettings settings;

	public new Camera camera;
	public Transform floorPlaneTransform;
	public Plane floorPlane {
		get {
			return new Plane(floorPlaneTransform.up, floorPlaneTransform.position);
		}
	}

	// [SerializeField]
	Vector2 momentumLastScreenPoint;
	Vector2 targetPointScreenSpaceMomentum;
	Vector2 targetPointScreenSpaceMomentumVelocity;
	// FloatSmoothDamper distanceMomentumDamper;

	// This should be used for perspective too.
	public float minFrustumHeight = 2;
	public float maxFrustumHeight = 20;

	[Tooltip("Mouse-wheel zoom. Auto-suppressed while a Touchscreen is active (e.g. TrackpadTouchProvider), " +
	         "since a trackpad two-finger scroll also emits touch contacts that drive the pinch.")]
	public bool allowMouseScrollZoom = true;

	[Tooltip("Allow two-finger rotation (rolls the view, like rotating a photo). Only makes sense when the " +
	         "camera looks roughly straight down at the plane — turn it off for tilted cameras, where it " +
	         "cants the horizon. Off = pinch does pan + zoom only.")]
	public bool allowPinchRotation = true;

	public Region region;

	[Tooltip("Optional. When set, this camera registers itself as a pan/pinch/zoom contributor in the rig's " +
	         "modifier queue (composition mode) instead of being driven directly by an external caller. Leave " +
	         "unset if something else calls SetCameraProperties() itself (e.g. DirectManipulationCameraTester).")]
	public CameraRig rig;
	[Tooltip("Sort order within the rig's queue (lower runs first). Put input-driven pan/zoom early so later " +
	         "modifiers — clamps, framing, shake — layer on top of it.")]
	public int rigSortIndex = 0;

	public delegate void OnInputEvent();
	public event OnInputEvent OnDoInput;

	void OnEnable () {
		if(rig != null) {
			// Adopt the rig's camera / ground plane if we weren't given our own, so a rig setup needs no double-wiring.
			if(camera == null) camera = rig.camera;
			if(rig.groundTransform == null && floorPlaneTransform != null) rig.groundTransform = floorPlaneTransform;
			rig.Add(this, rigSortIndex, "Direct Manipulation");
		}
		var inputManager = InputPointManager.Instance;
		if(inputManager == null) return;
		inputManager.OnCameraPinchStart += OnCameraPinchStart;
		inputManager.OnStartInputPoint += OnStartInputPoint;
	}

	void OnDisable () {
		if(rig != null) rig.Remove(this);
		var inputManager = InputPointManager.Instance;
		if(inputManager != null) {
			inputManager.OnCameraPinchStart -= OnCameraPinchStart;
			inputManager.OnStartInputPoint -= OnStartInputPoint;
		}
		SetScreenSpaceMomentum(Vector3.zero);
	}

	// void LateUpdate () {
		// Tests();
		// SetCameraProperties(ref cameraProperties);
		// cameraProperties.ApplyTo(camera);
	// }

	void OnStartInputPoint (GameInputPoint gameInputPoint) {
		ClearMomentum();
	}

	public void ClearMomentum () {
		SetScreenSpaceMomentum(Vector3.zero);
	}
	
	// ICameraPropertiesModifier — all the per-frame work happens in Modify (which reads the current input),
	// so there's nothing to do in the separate update tick.
	public void UpdateModifier (float deltaTime) {}
	public void Modify (ref CameraProperties properties) => SetCameraProperties(ref properties);

	public void SetCameraProperties (ref CameraProperties cameraProperties) {
		SetCameraProperties(ref cameraProperties, Time.deltaTime);
	}
	public void SetCameraProperties (ref CameraProperties cameraProperties, float deltaTime) {
		if(!Application.isFocused) return;
		var inputManager = InputPointManager.Instance;
		if(inputManager == null || !inputManager.interactable) return;
		cameraProperties.targetPoint = Clamp(cameraProperties.targetPoint);

		// Vector2 keyboardMoveInput = KeyboardInput.GetCombinedDirectionFromArrowKeys();
		Vector2 keyboardMoveInput = Vector2.zero;

		bool mouseIsOnScreen = Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width && Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height;

		var pinch = inputManager.cameraPinch;
		var pan = inputManager.cameraPan;

		if(pinch != null) {
			if(pinch.hasChanged) {
				UpdatePinch(ref cameraProperties, pinch);
				if(OnDoInput != null) OnDoInput();
			}
			SetScreenSpaceMomentum(Vector3.zero);
		}
		else if(keyboardMoveInput != Vector2.zero) {
			Vector2 screenVector = keyboardMoveInput * settings.keyboardPanSpeed * deltaTime;
			// Vector3 translation = keyboardMoveInput * settings.keyboardPanSpeed * deltaTime;
			// translation *= cameraProperties.orthographic ? cameraProperties.orthographicSize : camera.GetFrustumHeightAtDistance(cameraProperties.distance);
			// translation = camera.transform.rotation * translation;
			
			Vector3 translation;
			ScreenToWorldVectorOnPlane(camera, new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), new Vector2(Screen.width * 0.5f, Screen.height * 0.5f) + screenVector, floorPlane, out translation);
			Translate(ref cameraProperties, translation);

			var maxScreenSpaceMomentum = settings.maxViewportSpaceMomentum * Mathf.Lerp(Screen.width, Screen.height, 0.5f);
			screenVector = Vector2.ClampMagnitude(screenVector, maxScreenSpaceMomentum);
			SetScreenSpaceMomentum(screenVector);
			if(OnDoInput != null) OnDoInput();
		}
		// A touch pan is always valid; the mouse-cursor-on-screen check only applies to mouse panning
		// (so dragging off the window edge with the mouse doesn't keep panning).
		else if(pan != null && (pan.inputPoint is Finger || mouseIsOnScreen)) {
			var translation = GetTranslationFromCameraPan(cameraProperties, pan.inputPoint.lastPosition, pan.inputPoint.position);
			if(translation != Vector3.zero) {
				var lastTargetPoint = cameraProperties.targetPoint;
				Translate(ref cameraProperties, translation);
				
				var screenVector = pan.inputPoint.deltaPosition;
				var maxScreenSpaceMomentum = settings.maxViewportSpaceMomentum * Mathf.Lerp(Screen.width, Screen.height, 0.5f);
				screenVector = Vector2.ClampMagnitude(screenVector, maxScreenSpaceMomentum);
				SetScreenSpaceMomentum(screenVector);
				if(OnDoInput != null) OnDoInput();
			} else {
				// When we're holding in place, simulate momentum for a bit. This means if you drag, hold, release, momentum should be zero - but means if you drag, hold for a single frame, release, there's still some momentum.
				targetPointScreenSpaceMomentum = Vector2.SmoothDamp(targetPointScreenSpaceMomentum, Vector2.zero, ref targetPointScreenSpaceMomentumVelocity, 0.05f, Mathf.Infinity, deltaTime);
			}
			momentumLastScreenPoint = pan.inputPoint.position;
		}
		else if(targetPointScreenSpaceMomentum != Vector2.zero) {
			var translation = GetTranslationFromCameraPan(cameraProperties, momentumLastScreenPoint, momentumLastScreenPoint+targetPointScreenSpaceMomentum);
			Translate(ref cameraProperties, translation);
		}
		// Suppress mouse-wheel zoom while a Touchscreen is active (e.g. TrackpadTouchProvider registers one):
		// a trackpad two-finger scroll emits wheel deltas AND touch contacts, which would fight the pinch.
		if(allowMouseScrollZoom && UnityEngine.InputSystem.Touchscreen.current == null && mouseIsOnScreen && Input.mouseScrollDelta.y != 0) {
			var deltaZoom = settings.mouseWheelSpeed * -Input.mouseScrollDelta.y * deltaTime;
			// if(SystemInfoX.IsMacOS) zoomSpeed *= settings.OSXDefaultScrollSpeedMultipler;
			if(cameraProperties.orthographic) deltaZoom *= cameraProperties.orthographicSize;
			
			if(cameraProperties.orthographic) {
				float deltaSize = OrthographicZoom(ref cameraProperties, deltaZoom);

				Vector2 normalizedMousePos = Rect.PointToNormalized(camera.pixelRect, Input.mousePosition);
				normalizedMousePos = 2 * (normalizedMousePos - new Vector2(0.5f, 0.5f));
				var translation = -new Vector2(deltaSize * camera.aspect * normalizedMousePos.x, deltaSize * normalizedMousePos.y);
				cameraProperties.TranslateUsingDistance(floorPlane, cameraProperties.rotation * translation);
				cameraProperties.targetPoint = Clamp(cameraProperties.targetPoint);
				SetScreenSpaceMomentum(Vector3.zero);
			} else {
				var translation = GetTranslationFromScaledZoomInAtScreenPoint(Input.mousePosition, deltaZoom);
				cameraProperties.TranslateUsingDistance(floorPlane, translation);
				cameraProperties.targetPoint = Clamp(cameraProperties.targetPoint);
			}
			SetScreenSpaceMomentum(Vector3.zero);
			if(OnDoInput != null) OnDoInput();
		}
		
		if(Time.timeScale == 0) SetScreenSpaceMomentum(Vector3.zero);
		else targetPointScreenSpaceMomentum = Vector2.SmoothDamp(targetPointScreenSpaceMomentum, Vector2.zero, ref targetPointScreenSpaceMomentumVelocity, settings.momentumSmoothTime, Mathf.Infinity, deltaTime);
		// if(clampMax) ClampMax(ref rect);
	}


	// True 1:1 drag, for both orthographic and perspective cameras: find where the previous and current
	// screen positions hit the floor, and move the camera by the opposite of that world delta so the
	// world point under the cursor stays put. If a ray misses the floor (e.g. dragging above the horizon
	// on a tilted perspective camera) there's nothing to anchor to, so we don't pan.
	Vector3 GetTranslationFromCameraPan (CameraProperties cameraProperties, Vector2 lastScreenPosition, Vector2 screenPosition) {
		if(lastScreenPosition == screenPosition) return Vector3.zero;
		if(ScreenToWorldPointOnPlane(camera, lastScreenPosition, floorPlane, out Vector3 lastFloorPoint) &&
		   ScreenToWorldPointOnPlane(camera, screenPosition, floorPlane, out Vector3 currentFloorPoint)) {
			// Both points lie on the floor, so the delta is already in-plane.
			return lastFloorPoint - currentFloorPoint;
		}
		return Vector3.zero;
	}



	// Zooms in with the zoom amount scaled by the current distance. This is what you want to use for scroll wheels, for example.
	Vector3 GetTranslationFromScaledZoomInAtScreenPoint (Vector2 screenPoint, float deltaZoom) {
		ScreenToWorldPointOnPlane(camera, screenPoint, floorPlane, out Vector3 floorPoint);
		var directionToCamera = (camera.transform.position - floorPoint);
		// Just in case the camera ever gets under the plane, reverse the direction
		directionToCamera *= Mathf.Sign(Vector3.Dot(floorPlane.normal, directionToCamera));
		
		var delta = directionToCamera * deltaZoom;
		return delta;
	}

	// Zooms in by the specified amount
	// Vector3 ZoomInAtScreenPoint (Vector2 screenPoint, float deltaZoom) {
	//     Debug.Log(cameraProperties.distance +" "+deltaZoom);
	// 	return ZoomToDistanceAtScreenPoint(screenPoint, cameraProperties.distance + deltaZoom);
	// }

	// Sets the camera a target distance from the floor in the direction of the floor normal
	Vector3 ZoomToDistanceAtScreenPoint (Vector2 screenPoint, float targetDistanceFromFloor) {
		// Clamp distance
		// targetDistanceFromFloor = Mathf.Clamp(targetDistanceFromFloor, minZoom, calculatedMaxZoom);

		ScreenToWorldPointOnPlane(camera, screenPoint, floorPlane, out Vector3 floorPoint);
		var directionToCamera = (camera.transform.position - floorPoint).normalized;
		// This enforces direction (above the floor) and scales so  the distance acts from the closest point on the floor rather than the floor point under the cursor
		var directionFloorDot = Vector3.Dot(floorPlane.normal, directionToCamera);
		directionToCamera /= directionFloorDot;
		var targetPoint = floorPoint + directionToCamera * targetDistanceFromFloor;
		return targetPoint - camera.transform.position;
	}











	// Applies a movement vector, clamping according to settings. Returns the movement vector that was actually travelled.
	private Vector3 Translate (ref CameraProperties cameraProperties, Vector3 delta) {
		Vector3 lastPosition = cameraProperties.targetPoint;
		cameraProperties.targetPoint = Clamp(cameraProperties.targetPoint + delta);
		return (Vector3)cameraProperties.targetPoint - lastPosition;
	}
	
	Vector3 Clamp (Vector3 targetPoint) {
		if (region != null) {
			// Clamp to bounding region
			targetPoint = region.ClosestPointInRegion(targetPoint);
		}
		// Clamp to floor
		targetPoint = floorPlane.ClosestPointOnPlane(targetPoint);
		return targetPoint;
	}


	private void OnCameraPinchStart (Pinch pinch) {
		// No per-pinch state needed — UpdatePinch works from each frame's finger deltas.
	}

	// Two-finger similarity solve on the floor. Find where each finger sits on the floor this frame and
	// last frame, then move the camera by the inverse of the fingers' rigid+scale motion so both grabbed
	// points stay exactly under both fingers: pan + rotate-about-the-plane-up + zoom, like manipulating a
	// photo. Works for orthographic and perspective. (Rotation is about floorPlane.normal, which for a
	// standard floor is the camera's yaw axis.)
	private void UpdatePinch (ref CameraProperties cameraProperties, Pinch pinch) {
		if(pinch == null || !pinch.hasChanged || pinch.inputPoints.Count < 2) return;

		if(!ScreenToWorldPointOnPlane(camera, pinch.inputPoints[0].position, floorPlane, out Vector3 cur0)) return;
		if(!ScreenToWorldPointOnPlane(camera, pinch.inputPoints[1].position, floorPlane, out Vector3 cur1)) return;
		if(!ScreenToWorldPointOnPlane(camera, pinch.inputPoints[0].lastPosition, floorPlane, out Vector3 last0)) return;
		if(!ScreenToWorldPointOnPlane(camera, pinch.inputPoints[1].lastPosition, floorPlane, out Vector3 last1)) return;

		Vector3 up = floorPlane.normal;
		Vector3 lastVec = last1 - last0;
		Vector3 curVec = cur1 - cur0;
		float lastLen = lastVec.magnitude;
		float curLen = curVec.magnitude;
		if(lastLen < 0.0001f || curLen < 0.0001f) return;

		float fingerScale = curLen / lastLen;                                          // >1 = fingers spreading = zoom in
		float angle = allowPinchRotation ? Vector3.SignedAngle(lastVec, curVec, up) : 0f;

		// Zoom first, capturing the scale actually applied (clamping can limit it) so the pivot stays anchored.
		float appliedScale = fingerScale;
		if(cameraProperties.orthographic) {
			float newSize = Mathf.Clamp(cameraProperties.orthographicSize / fingerScale, minFrustumHeight, maxFrustumHeight);
			if(newSize > 0f) appliedScale = cameraProperties.orthographicSize / newSize;
			cameraProperties.orthographicSize = newSize;
		} else {
			float newDistance = Mathf.Max(0.01f, cameraProperties.distance / fingerScale);
			if(newDistance > 0f) appliedScale = cameraProperties.distance / newDistance;
			cameraProperties.distance = newDistance;
		}

		// Move the target by the inverse similarity about the current finger point (G^-1): rotate by -angle
		// about the plane up, scale by 1/appliedScale, anchored so last0 maps back under finger 0.
		Vector3 rel = Quaternion.AngleAxis(-angle, up) * (cameraProperties.targetPoint - cur0) / appliedScale;
		cameraProperties.targetPoint = Clamp(last0 + rel);

		// Roll the view about the camera's local Z — rotates the rendered image about the screen centre,
		// i.e. a true "rotate the photo". (For a straight-down camera this matches a rotation about the
		// plane up; for a tilted camera it cants the horizon — see note.)
		if(angle != 0f) cameraProperties.localEulerAngles.z += angle;
	}


	private void SetScreenSpaceMomentum (Vector3 delta) {
		targetPointScreenSpaceMomentumVelocity = Vector3.zero;
		targetPointScreenSpaceMomentum = delta;
	}

	private float OrthographicZoom (ref CameraProperties cameraProperties, float delta) {
		float lastSize = cameraProperties.orthographicSize;
		cameraProperties.orthographicSize += delta;
		cameraProperties.orthographicSize = Mathf.Clamp(cameraProperties.orthographicSize, minFrustumHeight, maxFrustumHeight);
		return cameraProperties.orthographicSize - lastSize;
	}





	#region Utils
	// Gets the world position on a target plane using a screen space camera ray
	public static bool ScreenToWorldPointOnPlane (Camera camera, Vector2 screenPoint, Plane plane, out Vector3 floorPoint) {
		return plane.TryGetHitPoint(camera.ScreenPointToRay(screenPoint), out floorPoint);
	}

	// Gets the world vector on a target plane between two screen space camera rays
	public static bool ScreenToWorldVectorOnPlane (Camera camera, Vector2 startScreenPosition, Vector2 endScreenPosition, Plane plane, out Vector3 worldVector) {
		worldVector = Vector3.zero;
		if(!ScreenToWorldPointOnPlane(camera, startScreenPosition, plane, out Vector3 startWorldPoint)) return false;
		if(!ScreenToWorldPointOnPlane(camera, endScreenPosition, plane, out Vector3 endWorldPoint)) return false;
		worldVector = endWorldPoint - startWorldPoint;
		return true;
	}

	public static bool ScreenToWorldVectorOnPlane (Camera camera, Vector2 screenVector, float distanceFromPlane, Plane plane, out Vector3 worldVector) {
		worldVector = camera.ScreenToWorldVector(screenVector, distanceFromPlane);
		worldVector = Vector3.ProjectOnPlane(worldVector, plane.normal).normalized * worldVector.magnitude;
		return true;
	}
	#endregion




	// void Tests () {
		// if(Input.GetMouseButtonDown(0)) {
		//     cameraProperties.TranslateUsingDistance(floorPlane, ZoomToDistanceAtScreenPoint(Input.mousePosition, 4));
		// }
		// if(Input.GetMouseButtonDown(1)) {
		//     cameraProperties.TranslateUsingDistance(floorPlane, ZoomToDistanceAtScreenPoint(Input.mousePosition, 8));
		// }
	// }

	private void OnDrawGizmos () {
		// GizmosX.BeginColor(Color.red);
		// GizmosX.DrawWireRect(maxArea);
		// GizmosX.EndColor();

		// ScreenToPointOnPlane(Input.mousePosition, floorPlane, out Vector3 p);
		// Gizmos.DrawSphere(p, 0.2f);
		
		// ScreenToPointOnPlane(GameInputController.Instance.cameraPan.inputPoint.position, floorPlane, out p);
		// Gizmos.DrawSphere(p, 0.2f);
		
		// ScreenToPointOnPlane(GameInputController.Instance.cameraPan.inputPoint.lastPosition, floorPlane, out p);
		// Gizmos.DrawSphere(p, 0.2f);
	}
}