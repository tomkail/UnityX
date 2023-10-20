using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InputX : MonoSingleton<InputX> {
    public bool useFakeMouseFingerInput;

	[SerializeField]
	private bool _acceptInput = true;
	public bool acceptInput {
		get {
			return _acceptInput;
		} set {
			if(_acceptInput == value) return;
			_acceptInput = value;
			if(acceptInput){
				ResetInput();
			} else {
				ResetInput();
			}
		}
	}

	public bool resetOnLoseFocus = true;
	public bool resetOnPause = true;
	
	public KeyboardInput keyboardInput;
	public IEnumerable<InputPoint> inputPoints {
        get {
            yield return mouseInput;
            foreach(var finger in fingers)  
                yield return finger;
        }
    }
	public MouseInput mouseInput;
	public List<Finger> fingers = new List<Finger>();

	public List<Gesture> gestures = new List<Gesture>();
	public IEnumerable<Pinch> pinches {
		get {
			return gestures.Where(x => x is Pinch).Select(x => x as Pinch);
		}
	}

	//public delegate void OnDragEvent(InputPoint inputPoint);
	//public event OnDragEvent OnDrag;

	public delegate void OnTouchStartEvent(Finger finger);
	public event OnTouchStartEvent OnTouchStart;
	public delegate void OnTouchEndEvent(Finger finger);
	public event OnTouchEndEvent OnTouchEnd;
	public delegate void OnTapEvent(InputPoint inputPoint);
	public event OnTapEvent OnTap;

	public delegate void OnPinchStartEvent(Pinch pinch);
	public event OnPinchStartEvent OnPinchStart;
	public delegate void OnPinchEndEvent();
	public event OnPinchEndEvent OnPinchEnd;

	public delegate void OnMouseStartEvent(MouseInput mouse);
	public event OnMouseStartEvent OnMouseStart;
	public delegate void OnMouseEndEvent(MouseInput mouse);
	public event OnMouseEndEvent OnMouseEnd;

	public delegate void OnMouseLeftDownEvent(MouseInput inputPoint);
	public event OnMouseLeftDownEvent OnMouseLeftDown;
	public delegate void OnMouseLeftUpEvent(MouseInput inputPoint, float activeTime);
	public event OnMouseLeftUpEvent OnMouseLeftUp;
	public delegate void OnMouseLeftClickEvent(MouseInput inputPoint);
	public event OnMouseLeftClickEvent OnMouseLeftClick;

	public delegate void OnMouseRightDownEvent(MouseInput inputPoint);
	public event OnMouseRightDownEvent OnMouseRightDown;
	public delegate void OnMouseRightUpEvent(MouseInput inputPoint, float activeTime);
	public event OnMouseRightUpEvent OnMouseRightUp;
	public delegate void OnMouseRightClickEvent(MouseInput inputPoint);
	public event OnMouseRightClickEvent OnMouseRightClick;

	public delegate void OnMouseMiddleDownEvent(MouseInput inputPoint);
	public event OnMouseMiddleDownEvent OnMouseMiddleDown;
	public delegate void OnMouseMiddleUpEvent(MouseInput inputPoint, float activeTime);
	public event OnMouseMiddleUpEvent OnMouseMiddleUp;
	public delegate void OnMouseMiddleClickEvent(MouseInput inputPoint);
	public event OnMouseMiddleClickEvent OnMouseMiddleClick;
	
	public delegate void OnMouseScrollEvent(MouseInput inputPoint);
	public event OnMouseScrollEvent OnMouseScroll;

	protected override void Awake () {
        base.Awake();
	    InitMouse();
        InitKeyboard();
	}

	private void Update () {
		if(acceptInput){
			UpdateInput();
		}
	}
    
    void OnApplicationFocus (bool hasFocus) {
		if(resetOnLoseFocus)
        	ResetInput();
    }
    void OnApplicationPause (bool pauseStatus) {
		if(resetOnPause)
        	ResetInput();
    }

	//Resets all input values.
	public void ResetInput(){
		if(fingers != null) {
			for (int i = fingers.Count-1; i >= 0; i--) {
				fingers[i].ResetInput();
                RemoveFinger(i);
			}
		}

		if(mouseInput != null) {
			mouseInput.ResetInput();
		}

		if(keyboardInput != null) {
			keyboardInput.ResetInput();
		}
	}

	private void UpdateInput(){
		if((TouchInputSimulator.Instance != null && TouchInputSimulator.Instance.enabled) || Input.multiTouchEnabled){
			UpdateTouch();
			// This allows deltaPosition to work when using left mouse via touch.
			if (Input.GetMouseButtonDown (0)) {
				mouseInput.position = Input.mousePosition;
			}
		}
        UpdateMouse();
        UpdateKeyboard();
	}

	private void UpdateTouch(){
		Finger tmpFinger;

		for (int i = Input.touchCount-1; i >= 0; i--) {
            var touch = Input.touches[i];
            Debug.Assert(touch.fingerId >= 0);
            
            tmpFinger = GetFingerByID(touch.fingerId);
        	if(tmpFinger != null){
        		Debug.Assert(tmpFinger.fingerId == touch.fingerId);
        		
				if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
        	        int? fingerIndex = GetFingerIndexByID(touch.fingerId);        	
					if(fingerIndex != null){
                        // var log = "Touch with fingerId "+touch.fingerId+" ended! Finger index is "+fingerIndex+". Index of this touch is "+i+". Listing touches:";
                        // for(int j = 0; j < Input.touchCount; j++) log += "\n Touch "+j+", fingerId "+Input.touches[j].fingerId;
                        // log += "\nListing Fingers";
                        // for(int j = 0; j < fingers.Count; j++) log += "\n Finger "+j+", fingerId "+fingers[j].fingerId;
                        // Debug.Log(log);
						RemoveFinger((int)fingerIndex);
						continue;
					} else {
                        Debug.LogError("Touch "+touch.phase+" but no finger found with fingerId "+touch.fingerId);
                    }
				}
        	} else {
				if(touch.phase == TouchPhase.Began){
					TouchStart(touch);
				} else {
                    var errorLog = "Couldn't find finger with touch's fingerId " +touch.fingerId+"! This touch's phase is "+touch.phase+". There are "+fingers.Count+" fingers. Listing IDs:";
                    foreach(var finger in fingers) errorLog += "\n"+finger.fingerId;
					Debug.LogError (errorLog);
				}
        	}
        }

		for (int i = 0; i < fingers.Count; i++) {
			fingers[i].fingerArrayIndex = i;
            if(fingers[i].updatedManually) continue;
            if(fingers[i].isFakeMouseFinger) {
                fingers[i].UpdatePosition(mouseInput.position);
            } else {
                Touch _touch;
                if(TryGetTouchByID(fingers[i].fingerId, out _touch)) {
                    fingers[i].UpdatePosition(_touch.position);
                } else {
                    var errorLog = "Can't find touch with fingerId "+fingers[i].fingerId+". There are "+Input.touchCount+" touches. Listing IDs:";
                    foreach(var touch in Input.touches) errorLog += "\n"+touch.fingerId;
					Debug.LogError (errorLog);
                }
            }
            fingers[i].UpdateState();
		}

		CheckForPinchStart();
		foreach(var gesture in gestures) {
			gesture.UpdateGesture();
		}
	}

	private void UpdateMouse(){
		if(mouseInput != null && !mouseInput.updatedManually) {
			mouseInput.UpdatePosition(Input.mousePosition);
			mouseInput.UpdateState();
		}		
	}

	private void UpdateKeyboard(){
		if(keyboardInput != null) {
			//mouseInput.UpdateLifeTime(Time.deltaTime);
			//mouseInput.UpdatePosition(Input.mousePosition);
			keyboardInput.UpdateState();
		}
	}

	private void InitMouse(){
		mouseInput = new MouseInput(Input.mousePosition);

		//mouseInput.OnStart += InputPointStart;
		mouseInput.OnTap += Tap;
		mouseInput.OnMouseLeftClick += MouseLeftClick;
		mouseInput.OnMouseLeftDown += MouseLeftDown;
		mouseInput.OnMouseLeftUp += MouseLeftUp;
		mouseInput.OnMouseRightClick += MouseRightClick;
		mouseInput.OnMouseRightDown += MouseRightDown;
		mouseInput.OnMouseRightUp += MouseRightUp;
		mouseInput.OnMouseMiddleClick += MouseMiddleClick;
		mouseInput.OnMouseMiddleDown += MouseMiddleDown;
		mouseInput.OnMouseMiddleUp += MouseMiddleUp;
		mouseInput.OnMouseScroll += MouseScroll;
		//mouseInput.OnEnd += InputPointEnd;

		if(OnMouseStart != null){
			OnMouseStart(mouseInput);
		}
	}

	private void DestroyMouse(){
		mouseInput.End();

		//mouseInput.OnStart -= InputPointStart;
		mouseInput.OnTap -= Tap;
		mouseInput.OnMouseLeftClick -= MouseLeftClick;
		mouseInput.OnMouseLeftDown -= MouseLeftDown;
		mouseInput.OnMouseLeftUp -= MouseLeftUp;
		mouseInput.OnMouseRightClick -= MouseRightClick;
		mouseInput.OnMouseRightDown -= MouseRightDown;
		mouseInput.OnMouseRightUp -= MouseRightUp;
		mouseInput.OnMouseMiddleClick -= MouseMiddleClick;
		mouseInput.OnMouseMiddleDown -= MouseMiddleDown;
		mouseInput.OnMouseMiddleUp -= MouseMiddleUp;
		mouseInput.OnMouseScroll -= MouseScroll;
		//mouseInput.OnEnd -= InputPointEnd;

		if(OnMouseEnd != null){
			OnMouseEnd(mouseInput);
		}

		mouseInput = null;
	}

	private void InitKeyboard(){
		keyboardInput = new KeyboardInput();

	}

	private void DestroyKeyboard(){
		keyboardInput.End();
		keyboardInput = null;
	}

	private void TouchStart(Touch _touch){
		AddFinger(new Finger(_touch));
	}

	public void AddFinger (Finger finger) {
		fingers.Add(finger);

        // var log = "Start touch with fingerId "+_touch.fingerId+". Listing touches:";
        // for(int j = 0; j < Input.touchCount; j++) log += "\n Touch "+j+", fingerId "+Input.touches[j].fingerId;
        // log += "\nListing Fingers";
        // for(int j = 0; j < fingers.Count; j++) log += "\n Finger "+j+", fingerId "+fingers[j].fingerId;
        // Debug.Log(log);

		if(OnTouchStart != null) OnTouchStart(finger);
	}
	public void RemoveFinger (Finger finger) {
		var index = fingers.IndexOf(finger);
		if(index != -1) RemoveFinger(index);
	}

	private void RemoveFinger(int _index) {
		var finger = fingers[_index];
        // Debug.Log("End touch with fingerId "+finger.fingerId);
		finger.End();

		if(OnTouchEnd != null) OnTouchEnd(finger);

		fingers.RemoveAt(_index);
	}

	private void CheckForPinchStart() {
		if(fingers.Where(finger => !finger.isFakeMouseFinger).Cast<InputPoint>().Except(gestures.SelectMany(x => x.inputPoints)).Count() < 2) return;
		
		List<Finger> pinchFingers = new List<Finger>();
		foreach(var finger in fingers.AsEnumerable().Reverse()) {
            if(finger.isFakeMouseFinger) continue;
			if(gestures.SelectMany(x => x.inputPoints).Contains(finger)) continue;
			// if(finger.state != InputPointState.Started){
				pinchFingers.Add(finger);
				if(pinchFingers.Count == 2) {
					AddGesture(new Pinch(pinchFingers[0], pinchFingers[1]));
					pinchFingers.Clear();
					break;
				}
			// }
		}
		// We might be able to do another!
		CheckForPinchStart();
	}

	void AddGesture (Gesture gesture) {
		gesture.OnCompleteGesture += OnCompleteGesture;
		gestures.Add(gesture);
		if(gesture is Pinch) {
			if(OnPinchStart != null) OnPinchStart((Pinch)gesture);
		}
	}

	void OnCompleteGesture (Gesture gesture) {
		gesture.OnCompleteGesture -= OnCompleteGesture;
		gestures.Remove(gesture);
		if(gesture is Pinch) {
			if(OnPinchEnd != null) OnPinchEnd();
		}
	}
    
    public bool TryGetTouchByID(int _id, out Touch touch){
		for (int i = 0; i < Input.touches.Length; i++) {
			if(Input.touches[i].fingerId == _id){
				touch = Input.touches[i];
                return true;
            }
		}
        touch = default(Touch);
        return false;
	}

	public int? GetTouchIndexByID(int _id){
		for (int i = 0; i < Input.touches.Length; i++) {
			if(Input.touches[i].fingerId == _id){
				return i;
			}
		}
		return null;
	}

	public Finger GetFingerByID(int _id){
		for (int i = 0; i < fingers.Count; i++) {
			if(fingers[i].fingerId == _id){
				return fingers[i];
			}
		}
		return null;
	}

	public int? GetFingerIndexByID(int _id){
		for (int i = 0; i < fingers.Count; i++) {
			if(fingers[i].fingerId == _id){
				return i;
			}
		}
		return null;
	}

	private void Tap (InputPoint inputPoint) {
		if(OnTap != null){
			OnTap(inputPoint);
		}
	}
    Finger fakeMouseFinger;
	private void MouseLeftDown (MouseInput inputPoint) {
        if(useFakeMouseFingerInput) {
            if(fingers.Remove(fakeMouseFinger)) {
                if(OnTouchEnd != null) OnTouchEnd(fakeMouseFinger);
                fakeMouseFinger = null;
            }
            fakeMouseFinger = new Finger(mouseInput);
            fingers.Add(fakeMouseFinger);
            if(OnTouchStart != null) OnTouchStart(fakeMouseFinger);
        }

        if(OnMouseLeftDown != null) OnMouseLeftDown(inputPoint);
	}
	private void MouseLeftUp (MouseInput inputPoint, float activeTime) {
        if(useFakeMouseFingerInput) {
            if(fingers.Remove(fakeMouseFinger)) {
                if(OnTouchEnd != null) OnTouchEnd(fakeMouseFinger);
                fakeMouseFinger = null;
            }
        }
        
		if(OnMouseLeftUp != null) OnMouseLeftUp(inputPoint, activeTime);
	}
	private void MouseLeftClick (MouseInput inputPoint) {
        // if(useFakeMouseFingerInput) {
        //     fakeMouseFinger.Tap();
        // }
		if(OnMouseLeftClick != null){
			OnMouseLeftClick(inputPoint);
		}
		Tap(inputPoint);
	}
	
	private void MouseRightDown (MouseInput inputPoint) {
		if(OnMouseRightDown != null){
			OnMouseRightDown(inputPoint);
		}
	}
	private void MouseRightUp (MouseInput inputPoint, float activeTime) {
		if(OnMouseRightUp != null){
			OnMouseRightUp(inputPoint, activeTime);
		}
	}
	private void MouseRightClick (MouseInput inputPoint) {
		if(OnMouseRightClick != null){
			OnMouseRightClick(inputPoint);
		}
	}
	
	private void MouseMiddleDown (MouseInput inputPoint) {
		if(OnMouseMiddleDown != null){
			OnMouseMiddleDown(inputPoint);
		}
	}
	private void MouseMiddleUp (MouseInput inputPoint, float activeTime) {
		if(OnMouseMiddleUp != null){
			OnMouseMiddleUp(inputPoint, activeTime);
		}
	}
	private void MouseMiddleClick (MouseInput inputPoint) {
		if(OnMouseMiddleClick != null){
			OnMouseMiddleClick(inputPoint);
		}
	}

	private void MouseScroll (MouseInput inputPoint) {
		if(OnMouseScroll != null){
			OnMouseScroll(inputPoint);
		}
	}

    [SerializeField]
    bool drawGUI = false;
    void OnGUI () {
        if(!drawGUI) return;
        GUI.Box(ScreenToGUIRect(RectX.CreateFromCenter(mouseInput.position, Vector2.one * 50)), "Mouse");

        foreach(var finger in fingers) {
            GUI.Box(ScreenToGUIRect(RectX.CreateFromCenter(finger.position, Vector2.one * 50)), "Finger "+finger.fingerArrayIndex.ToString());
        }

        foreach(var pinch in pinches) {
            var start = ScreenToGUIPoint(pinch.inputPoint1.position);
            var end = ScreenToGUIPoint(pinch.inputPoint2.position);
            // DrawLine(start, end);
            GUI.Box(RectX.CreateFromCenter(Vector2.Lerp(start, end, 0.5f), new Vector2(60, 40)), pinch.currentPinchDistance.ToString());
        }

		Rect ScreenToGUIRect (Rect rect) {
			rect.center = ScreenToGUIPoint(rect.center);
			return rect;
		}

		Vector2 ScreenToGUIPoint (Vector2 point) {
			return new Vector2(point.x, Screen.height-point.y);
		}
    }
}