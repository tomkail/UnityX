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

    // Sum of the delta of both fingers from the center.
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
		
		// inputPoint1.state = InputPointState.Pinch1;
    	// inputPoint2.state = InputPointState.Pinch2;
    	
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

    
    // private void CheckForPinchEnd(){

    // 	if(fingers.Count < 2){
	// 		PinchEnd();
    //     } else {
    //     	if(pinchFinger1 != null){
	//     		for(int i = 0; i < fingers.Count; i++){
	// 				if(fingers[i].state != InputPointState.Started && fingers[i].state != InputPointState.Pinch2){
	// 					fingers[i].state = InputPointState.Pinch1;
	// 					pinchFinger1 = fingers[i];
	// 				}
	// 			}
	//     	}
	//     	if(pinchFinger2 != null){
	//     		for(int i = 0; i < fingers.Count; i++){
	// 				if(fingers[i].state != InputPointState.Started && fingers[i].state != InputPointState.Pinch1){
	// 					fingers[i].state = InputPointState.Pinch2;
	// 					pinchFinger2 = fingers[i];
	// 				}
	// 			}
	//     	}
    //     }
    // }


	float GetPinchDistance(){
    	return Vector2.Distance(inputPoint1.position, inputPoint2.position);
    }

	Vector2 GetPinchCenter(){
    	return Vector2.Lerp(inputPoint1.position, inputPoint2.position, 0.5f);
    }
}
