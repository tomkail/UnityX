using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Gesture {
	public string name;
	public List<InputPoint> inputPoints = new List<InputPoint>();

	public delegate void GestureEvent (Gesture gesture);
	public event GestureEvent OnCompleteGesture;
	
	public virtual void UpdateGesture () {}

	public virtual void CompleteGesture () {
		if(OnCompleteGesture != null) OnCompleteGesture(this);
		inputPoints = null;
	}
}
