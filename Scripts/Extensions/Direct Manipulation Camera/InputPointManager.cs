using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

// Tracks all pointers (mouse + touches) and assigns them to an action type (camera, ui, etc).
// Reads the Input System directly — touches via EnhancedTouch, mouse via a MouseInput driven each frame
// from legacy Input. Both are handled every frame with no touch-vs-mouse mode switch, so desktop (mouse)
// and mobile / trackpad (touch) both work. Mouse is real mouse input (a MouseInput point), never faked
// as a touch.
public class InputPointManager : MonoSingleton<InputPointManager> {
	public bool interactable;
	// Games set this to enable/disable routing input to the camera (pan/pinch).
	public bool cameraControlEnabled = true;
	public float doubleClickIntervalTime = 0.175f;
	public float timeUntilDisablingAsClick = 0.3f;

	public List<GameInputPoint> inputPoints = new List<GameInputPoint>();
	public List<Gesture> gestures = new List<Gesture>();

	[field: SerializeReference]
	public GameInputPoint cameraPan { get; private set; }
	[field: SerializeReference]
	public Pinch cameraPinch { get; private set; }

	public event System.Action<Pinch> OnCameraPinchStart;
	public event System.Action<GameInputPoint> OnStartInputPoint;
	float lastClickTime = Mathf.NegativeInfinity;

	// The desktop mouse pointer, driven every frame from legacy Input.
	MouseInput mouseInput;

	void OnEnable () {
		ETouch.EnhancedTouchSupport.Enable();
		Clear();
		mouseInput = new MouseInput(CurrentMousePosition());
		mouseInput.OnMouseLeftDown += OnMouseLeftDown;
		mouseInput.OnMouseLeftUp += OnMouseLeftUp;
	}
	void OnDisable () {
		Clear();
		if(mouseInput != null) {
			mouseInput.OnMouseLeftDown -= OnMouseLeftDown;
			mouseInput.OnMouseLeftUp -= OnMouseLeftUp;
			mouseInput = null;
		}
		ETouch.EnhancedTouchSupport.Disable();
	}

	void OnApplicationFocus (bool hasFocus) { Clear(); }
	void OnApplicationPause (bool pauseStatus) { Clear(); }

	void Update () {
		interactable = Application.isPlaying;
		if(!interactable) { Clear(); return; }

		UpdateTouches();
		UpdateMouse();

		CheckForPinchStart();
		foreach(var gesture in gestures)
			gesture.UpdateGesture();
		RefreshCameraPinchInput();
		RefreshCameraPanInput();
	}


	// --- Input sourcing (mouse = legacy Input, touch = EnhancedTouch) ---

	static Vector2 CurrentMousePosition () => (Vector2)Input.mousePosition;

	void UpdateTouches () {
		var activeTouches = ETouch.Touch.activeTouches;
		for(int i = 0; i < activeTouches.Count; i++) {
			var touch = activeTouches[i];
			var existing = FindTouchInputPoint(touch.touchId);
			if(touch.phase == UnityEngine.InputSystem.TouchPhase.Began) {
				if(existing == null)
					StartInputPoint(new GameInputPoint(new Finger(touch.touchId, touch.screenPosition)));
			} else if(touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled) {
				if(existing != null) {
					// End() first so any gesture (Pinch) using this finger completes via OnFingerEnd,
					// then remove the point. (This is what InputX.RemoveFinger used to do.)
					existing.inputPoint.End();
					var doubleClick = Time.time - lastClickTime < doubleClickIntervalTime;
					if(!ReleaseInputPoint(existing.inputPoint, doubleClick)) lastClickTime = Time.time;
				}
			} else if(existing != null) {
				existing.inputPoint.UpdatePosition(touch.screenPosition);
				existing.inputPoint.UpdateState();
			}
		}
	}

	GameInputPoint FindTouchInputPoint (int touchId) {
		foreach(var gameInputPoint in inputPoints)
			if(gameInputPoint.inputPoint is Finger finger && finger.fingerId == touchId) return gameInputPoint;
		return null;
	}

	void UpdateMouse () {
		if(mouseInput == null) return;
		mouseInput.UpdatePosition(CurrentMousePosition());
		mouseInput.UpdateState(); // drives leftButton -> OnMouseLeftDown / OnMouseLeftUp below
	}

	void OnMouseLeftDown (MouseInput mouse) {
		StartInputPoint(new GameInputPoint(mouse));
	}
	void OnMouseLeftUp (MouseInput mouse, float activeTime) {
		var doubleClick = Time.time - lastClickTime < doubleClickIntervalTime;
		if(!ReleaseInputPoint(mouse, doubleClick)) lastClickTime = Time.time;
	}


	// --- Routing to camera pan / pinch ---

	void StartInputPoint (GameInputPoint gameInputPoint) {
		if(InputUtils.HoveringOverUI(gameInputPoint.inputPoint.position))
			gameInputPoint.target = GameInputPoint.GameInputPointTarget.UI;
		inputPoints.Add(gameInputPoint);
		OnStartInputPoint?.Invoke(gameInputPoint);
	}

	void RefreshCameraPanInput () {
		if(cameraPan != null && !InputPointCanBeUsedAsCameraPan(cameraPan)) {
			cameraPan = null;
		}
		foreach(var inputPoint in inputPoints) {
			if(InputPointCanBeUsedAsCameraPan(inputPoint)) {
				cameraPan = inputPoint;
				cameraPan.target = GameInputPoint.GameInputPointTarget.Camera;
				break;
			}
		}
	}

	void RefreshCameraPinchInput () {
		if(cameraPinch != null && !gestures.Contains(cameraPinch)) {
			cameraPinch = null;
		}
		if(cameraPinch == null) {
			for(int i = gestures.Count-1; i >= 0; i--) {
				var gesture = gestures[i];
				if(gesture is Pinch) {
					var pinch = gesture as Pinch;
					cameraPinch = pinch;
					foreach(var gestureInputPoint in cameraPinch.inputPoints) {
						var inputPoint = inputPoints.First(x => x.inputPoint == gestureInputPoint);
						inputPoint.target = GameInputPoint.GameInputPointTarget.Camera;
					}
					if(OnCameraPinchStart != null) OnCameraPinchStart(pinch);
				}
			}
		}
	}

	bool InputPointCanBeUsedAsCameraPan (GameInputPoint gameInputPoint) {
		if(!cameraControlEnabled) return false;
		if(!(gameInputPoint.target == GameInputPoint.GameInputPointTarget.None || gameInputPoint.target == GameInputPoint.GameInputPointTarget.Camera)) return false;
		if(!inputPoints.Contains(gameInputPoint)) return false;
		if(gestures.SelectMany(x => x.inputPoints).Contains(gameInputPoint.inputPoint)) return false;
		return true;
	}

	bool InputPointCanBeUsedInCameraPinch (GameInputPoint gameInputPoint) {
		if(!cameraControlEnabled) return false;
		if(!(gameInputPoint.target == GameInputPoint.GameInputPointTarget.None || gameInputPoint.target == GameInputPoint.GameInputPointTarget.Camera)) return false;
		Finger finger = gameInputPoint.inputPoint as Finger;
		if(finger == null) return false;
		return true;
	}
	private void CheckForPinchStart() {
		var validFingers = inputPoints.Where(inputPoint => InputPointCanBeUsedInCameraPinch(inputPoint) && !gestures.SelectMany(x => x.inputPoints).Contains(inputPoint.inputPoint));
		if(validFingers.Count() < 2) return;
		List<Finger> pinchFingers = new List<Finger>();
		foreach(var inputPoint in validFingers.Reverse()) {
			pinchFingers.Add((Finger)inputPoint.inputPoint);
			if(pinchFingers.Count == 2) {
				AddGesture(new Pinch(pinchFingers[0], pinchFingers[1]));
				pinchFingers.Clear();
				break;
			}
		}
		CheckForPinchStart();
	}

	void AddGesture (Gesture gesture) {
		gesture.OnCompleteGesture += OnCompleteGesture;
		gestures.Add(gesture);
		RefreshCameraPinchInput();
		RefreshCameraPanInput();
	}

	void OnCompleteGesture (Gesture gesture) {
		gesture.OnCompleteGesture -= OnCompleteGesture;
		gestures.Remove(gesture);
		RefreshCameraPinchInput();
		RefreshCameraPanInput();
	}

	bool ReleaseInputPoint (InputPoint inputPoint, bool doubleClick) {
		var toRemove = inputPoints.IndexOf(x => x.inputPoint == inputPoint);
		if(toRemove == -1) {
			Debug.LogWarning("Input point for "+inputPoint+" not found on release!");
			Clear();
			return true;
		} else {
			bool didAction = false;
			var gameInputPoint = inputPoints[toRemove];

			if(gameInputPoint.target == GameInputPoint.GameInputPointTarget.None) {
				if(InputUtils.HoveringOverUI(gameInputPoint.inputPoint.position)) {
					gameInputPoint.target = GameInputPoint.GameInputPointTarget.UI;
					didAction = true;
				}
			}
			if(gameInputPoint.target == GameInputPoint.GameInputPointTarget.None && gameInputPoint.timeDown < timeUntilDisablingAsClick) {
				// Hook for click-to-interact / click-world here (game-specific).
			}

			inputPoints.RemoveAt(toRemove);
			RefreshCameraPinchInput();
			RefreshCameraPanInput();

			return didAction;
		}
	}

	void Clear () {
		for(int i = gestures.Count-1; i >= 0; i--)
			gestures[i].CompleteGesture();
		inputPoints.Clear();
		cameraPinch = null;
		cameraPan = null;
	}
}
