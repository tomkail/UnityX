using UnityEngine;

public class Vector3Tween : TypeTween<Vector3> {

	public Vector3Tween () : base () {}
	public Vector3Tween (Vector3 myStartValue) : base (myStartValue) {}
	public Vector3Tween (Vector3 myStartValue, Vector3 myTargetValue, float myLength) : base (myStartValue, myTargetValue, myLength) {}
	public Vector3Tween (Vector3 myStartValue, Vector3 myTargetValue, float myLength, AnimationCurve myLerpCurve) : base (myStartValue, myTargetValue, myLength, myLerpCurve) {}

	protected override void SetDefaultLerpFunction () {
		lerpFunction = (start, end, lerp) => {
			return Vector3.Lerp(start, end, easingCurve.Evaluate(lerp));
		};
	}

	protected override void SetDeltaValue (Vector3 myLastValue, Vector3 myCurrentValue) {
		deltaValue = myCurrentValue - myLastValue;
	}
	
	//----------------- IOS GENERIC INHERITANCE EVENT CRASH BUG WORKAROUND ------------------
	//http://angrytroglodyte.net/cave/index.php/blog/11-unity-ios-doesn-t-like-generic-events
	public new event OnChangeEvent OnChange;
	public new event OnInterruptEvent OnInterrupt;
	public new event OnCompleteEvent OnComplete;
	
	protected override void ChangedCurrentValue () {
		base.ChangedCurrentValue();
		if(OnChange != null) OnChange(currentValue);
	}
	
	protected override void TweenComplete () {
		base.TweenComplete();
		if(OnComplete != null)OnComplete();
	}
	
	public override void Interrupt () {
		base.Interrupt();
		if(OnInterrupt != null) OnInterrupt();
	}
}