using UnityEngine;
using System.Collections;

[System.Serializable]
public class MouseInput : InputPoint {
	public MouseInputButton leftButton;
	public MouseInputButton rightButton;
	public MouseInputButton middleButton;
	public MouseInputWheel scrollWheel;
	
	public delegate void OnMouseLeftClickEvent(MouseInput inputPoint);
	public event OnMouseLeftClickEvent OnMouseLeftClick;
	public delegate void OnMouseLeftDownEvent(MouseInput inputPoint);
	public event OnMouseLeftDownEvent OnMouseLeftDown;
	public delegate void OnMouseLeftUpEvent(MouseInput inputPoint, float activeTime);
	public event OnMouseLeftUpEvent OnMouseLeftUp;

	public delegate void OnMouseRightClickEvent(MouseInput inputPoint);
	public event OnMouseRightClickEvent OnMouseRightClick;
	public delegate void OnMouseRightDownEvent(MouseInput inputPoint);
	public event OnMouseRightDownEvent OnMouseRightDown;
	public delegate void OnMouseRightUpEvent(MouseInput inputPoint, float activeTime);
	public event OnMouseRightUpEvent OnMouseRightUp;

	public delegate void OnMouseMiddleClickEvent(MouseInput inputPoint);
	public event OnMouseMiddleClickEvent OnMouseMiddleClick;
	public delegate void OnMouseMiddleDownEvent(MouseInput inputPoint);
	public event OnMouseMiddleDownEvent OnMouseMiddleDown;
	public delegate void OnMouseMiddleUpEvent(MouseInput inputPoint, float activeTime);
	public event OnMouseMiddleUpEvent OnMouseMiddleUp;

	public delegate void OnMouseScrollEvent(MouseInput inputPoint);
	public event OnMouseScrollEvent OnMouseScroll;

	public MouseInput(Vector2 _position) : base (_position) {
		name = "Mouse";
		leftButton = new MouseInputButton(0);
		rightButton = new MouseInputButton(1);
		middleButton = new MouseInputButton(2);
		scrollWheel = new MouseInputWheel();

		leftButton.OnMouseButtonDown += OnLeftMouseButtonDown;
		leftButton.OnMouseButtonUp += OnLeftMouseButtonUp;
		rightButton.OnMouseButtonDown += OnRightMouseButtonDown;
		rightButton.OnMouseButtonUp += OnRightMouseButtonUp;
		middleButton.OnMouseButtonDown += OnMiddleMouseButtonDown;
		middleButton.OnMouseButtonUp += OnMiddleMouseButtonUp;

		scrollWheel.OnMouseScroll += OnMouseWheelScroll;
	}

	public override void Start(){
		base.Start();
		state = InputPointState.Stationary;
	}

	public override void UpdateState(){
		leftButton.UpdateState();
		rightButton.UpdateState();
		middleButton.UpdateState();

		scrollWheel.UpdatePosition();

		base.UpdateState();		
	}

	public override void ResetInput(){
		leftButton.ResetInput();
		rightButton.ResetInput();
		middleButton.ResetInput();
		scrollWheel.ResetInput();

		base.ResetInput();
	}

	public override void End(){
		base.End();
	}

	private void OnLeftMouseButtonDown() {
        startPosition = position;
		if(OnMouseLeftDown != null){
			OnMouseLeftDown(this);
		}
	}

	private void OnLeftMouseButtonUp(float buttonDownTime){
		if(OnMouseLeftUp != null){
			OnMouseLeftUp(this, buttonDownTime);
		}

		if(deltaPosition.magnitude < 5 && buttonDownTime < tapTime) {
            leftButton.clicked = true;
            Tap();	
			if(OnMouseLeftClick != null){
				//Simulate touch events
				OnMouseLeftClick(this);
			}
		}

		//Simulate touch events
		// End();
	}

	private void OnRightMouseButtonDown () {
		if(OnMouseRightDown != null){
			OnMouseRightDown(this);
		}
	}

	private void OnRightMouseButtonUp(float buttonDownTime){
		if(OnMouseRightUp != null){
			OnMouseRightUp(this, buttonDownTime);
		}

		if(state == InputPointState.Stationary && buttonDownTime < tapTime){
			if(OnMouseRightClick != null){
				OnMouseRightClick(this);
			}
		}
	}

	private void OnMiddleMouseButtonDown(){
		if(OnMouseMiddleDown != null){
			OnMouseMiddleDown(this);
		}
	}

	private void OnMiddleMouseButtonUp(float buttonDownTime){
		if(OnMouseMiddleUp != null){
			OnMouseMiddleUp(this, buttonDownTime);
		}

		if(state == InputPointState.Stationary && buttonDownTime < tapTime){
			if(OnMouseMiddleClick != null){
				OnMouseMiddleClick(this);
			}
		}
	}

	private void OnMouseWheelScroll(Vector2 deltaScroll){
		if(OnMouseScroll != null){
			OnMouseScroll(this);
		}
	}
	
	public override string ToString () {
		return string.Format ("[MouseInput] State {0} Position {1} Left {2} Middle {3} Right {4} Scroll {5}", state, position, leftButton, middleButton, rightButton, scrollWheel);
	}
}