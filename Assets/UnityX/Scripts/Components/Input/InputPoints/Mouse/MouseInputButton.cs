using UnityEngine;
using System.Collections;

[System.Serializable]
public class MouseInputButton {
	public int buttonIndex = 0;
	public bool down = false;
	public bool held = false;
	public bool up = false;
	public bool clicked = false;
	public float startTime = -1;
    public float activeTime {
        get {
            if(startTime == -1) return 0;
            return Mathf.Max(Time.time - startTime, 0);
        }
    }
    public int startFrame = -1;
    // Note that this is 0 on the first frame this is active
	public int activeFrames {
        get {
            if(startFrame == -1) return 0;
            return Mathf.Max(Time.frameCount-startFrame, 0);
        }
    }

	public delegate void OnMouseButtonDownEvent();
	public event OnMouseButtonDownEvent OnMouseButtonDown;
	public delegate void OnMouseButtonUpEvent(float activeTime);
	public event OnMouseButtonUpEvent OnMouseButtonUp;

	public MouseInputButton(int _buttonIndex){
		buttonIndex = _buttonIndex;
	}
    
	public void UpdateState(){
		down = false;
		up = false;
		clicked = false;
		if(Input.GetMouseButtonDown(buttonIndex)){
			ButtonDown();
		}

		if(Input.GetMouseButtonUp(buttonIndex)){
			ButtonUp();
		}
	}

	public void ResetInput(){
        down = false;
        if(held) {
            held = false;
            ButtonUp();
        }
	}

	private void ButtonDown(bool sendEvent = true){
		down = true;
		held = true;
		startTime = Time.time;
        startFrame = Time.frameCount;

		if(sendEvent && OnMouseButtonDown != null){
			OnMouseButtonDown();
		}
	}

	private void ButtonUp(bool sendEvent = true) {
        var _activeTime = activeTime;
		held = false;
		up = true;
		if(sendEvent && OnMouseButtonUp != null) {
			OnMouseButtonUp(_activeTime);
		}
        startTime = -1;
        startFrame = -1;
	}
	
	public override string ToString () {
		return string.Format ("[MouseInputButton] Button Index {0} Down {1} Up {2}", buttonIndex, held, up);
	}
}