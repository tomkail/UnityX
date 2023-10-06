using UnityEngine;
using System.Collections;


[System.Serializable]
public class InputPoint {
	public string name;
	internal float tapTime = 0.2f;

	public InputPointState state;

	public float startTime;
	public float activeTime {
        get {
            return Time.time - startTime;
        }
    }
	public int startFrame;
    // Note that this is 0 on the first frame this is active
	public int activeFrames {
        get {
            return Time.frameCount - startFrame;
        }
    }

	public Vector2 startPosition;
	public Vector2 position;
	public Vector2 lastPosition;
	public Vector2 deltaPosition {
        get {
            return position-lastPosition;
        }
    }
	public Vector2 deltaPositionSinceStart {
        get {
            return position - startPosition;
        }
    }
	
	public float lerpedMovement;
	
	public bool isOnScreen = true;

	public delegate void OnInputPointEvent(InputPoint inputPoint);
	public event OnInputPointEvent OnStart;
	public event OnInputPointEvent OnEnd;
	public event OnInputPointEvent OnTap;

    
    // If faked
    public bool updatedManually;

	public InputPoint(Vector2 _position){
		name = "Input Point";
		position = lastPosition = _position;
		Start();
	}

	public virtual void Start(){
		startTime = Time.time;
		startFrame = Time.frameCount;
		state = InputPointState.Started;
        startPosition = position;
		if(OnStart != null){
			OnStart(this);
		}
	}

	public virtual void UpdatePosition(Vector2 newPosition){
		isOnScreen = new Rect(0,0,Screen.width,Screen.height).Contains(newPosition);
		
		SetPosition(newPosition);
		UpdateDeltaMovement();
	}
	
	public virtual void UpdateState () {
		// if(state != InputPointState.Pinch1 && state != InputPointState.Pinch2){
			if(state == InputPointState.Started && activeTime > tapTime){
				state = InputPointState.Stationary;
			}

			if(lerpedMovement > 5f) {
				state = InputPointState.Moving;
			} else {
				if(state != InputPointState.Started) {
					state = InputPointState.Stationary;
				}
			}
		// }
	}

	public virtual void ResetInput () {}

	public virtual void End () {
		if(state == InputPointState.Started && activeTime < tapTime){
			Tap();
		} else {
			state = InputPointState.Released;
		}
		if(OnEnd != null){
			OnEnd(this);
		}
	}

	public virtual void Tap() {
		state = InputPointState.Tap;
		if(OnTap != null){
			OnTap(this);
		}
	}
	
	private void SetPosition (Vector2 newPosition) {
		lastPosition = position;
		position = newPosition;
	}
	
	private void UpdateDeltaMovement () {
		lerpedMovement = Mathf.Lerp(lerpedMovement, deltaPosition.magnitude, Time.unscaledDeltaTime*40);
		lerpedMovement += deltaPosition.magnitude;
	}

	public override string ToString () {
		return string.Format ("[InputPoint] State {0} Position {1} Delta Position {2} Active Time {3}", state, position, deltaPosition, activeTime);
	}
}