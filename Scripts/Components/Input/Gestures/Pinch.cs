using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Pinch : Gesture {
	public InputPoint inputPoint1 {
		get {
			return inputPoints[0];
		}
	}
	public InputPoint inputPoint2 {
		get {
			return inputPoints[1];
		}
	}

    // Per-finger movement projected onto the direction to the pinch center, summed (positive = fingers moving apart).
    // UNTESTED
	public Vector2 deltaPinch;

	public float startPinchDistance;
	public float currentPinchDistance;
	public float lastPinchDistance;
	public float deltaPinchDistance = 0;

	public float normalizedDeltaPinchDistance = 0;
	
	public Vector2 currentPinchCenter;
	public Vector2 lastPinchCenter;
	public Vector2 deltaPinchCenter;

	public bool hasChanged {
		get {
			return deltaPinchCenter != Vector2.zero || deltaPinchDistance != 0;
		}
	}
	public Pinch (InputPoint firstFinger, InputPoint secondFinger) {
		this.name = "Pinch "+((firstFinger is Finger) ? "Finger "+((Finger)firstFinger).fingerId : "Input Point") +" "+((secondFinger is Finger) ? "Finger "+((Finger)secondFinger).fingerId : "Input Point");
		this.inputPoints = new List<InputPoint>() {firstFinger, secondFinger};
		foreach(var inputPoint in inputPoints)
			inputPoint.OnEnd += OnFingerEnd;
		
		startPinchDistance = currentPinchDistance = GetPinchDistance();
		currentPinchCenter = GetPinchCenter();
	}

	void OnFingerEnd (InputPoint point) {
		CompleteGesture();
	}

	public override void UpdateGesture () {
		base.UpdateGesture();


		lastPinchCenter = currentPinchCenter;
		lastPinchDistance = currentPinchDistance;
		
		currentPinchDistance = GetPinchDistance();
		currentPinchCenter = GetPinchCenter();

		deltaPinchDistance = currentPinchDistance - lastPinchDistance;
		normalizedDeltaPinchDistance = deltaPinchDistance * ScreenX.diagonalReciprocal;

		deltaPinchCenter = currentPinchCenter - lastPinchCenter;
		
        // UNTESTED
        var deltaPinchFinger1Dot = Vector2.Dot((currentPinchCenter - inputPoint1.position).normalized, inputPoint1.deltaPosition.normalized);
        var deltaPinchFinger1 = deltaPinchFinger1Dot * inputPoint1.deltaPosition;
        var deltaPinchFinger2Dot = Vector2.Dot((currentPinchCenter - inputPoint2.position).normalized, inputPoint2.deltaPosition.normalized);
        var deltaPinchFinger2 = deltaPinchFinger2Dot * inputPoint2.deltaPosition;
        deltaPinch = deltaPinchFinger1 + deltaPinchFinger2;
	}

	public override void CompleteGesture () {
		foreach(var inputPoint in inputPoints)
			if(inputPoint != null) {
			    inputPoint.OnEnd -= OnFingerEnd;
				inputPoint.state = InputPointState.Started;
                inputPoint.UpdateState();
            } else {
                Debug.LogWarning("Pinch input point not found!");
            }
		base.CompleteGesture();
	}

	float GetPinchDistance(){
    	return Vector2.Distance(inputPoint1.position, inputPoint2.position);
    }

	Vector2 GetPinchCenter(){
    	return Vector2.Lerp(inputPoint1.position, inputPoint2.position, 0.5f);
    }
}
