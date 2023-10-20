using UnityEngine;

public class QuaternionTween : TypeTween<Quaternion> {

	public QuaternionTween () : base () {}
	public QuaternionTween (Quaternion myStartValue) : base (myStartValue) {}
	public QuaternionTween (Quaternion myStartValue, Quaternion myTargetValue, float myLength) : base (myStartValue, myTargetValue, myLength) {}
	public QuaternionTween (Quaternion myStartValue, Quaternion myTargetValue, float myLength, AnimationCurve myLerpCurve) : base (myStartValue, myTargetValue, myLength, myLerpCurve) {}

	protected override void SetDefaultLerpFunction () {
		lerpFunction = (start, end, lerp) => {
			return Quaternion.Slerp(start, end, easingCurve.Evaluate(lerp));
		};
	}

	protected override void SetDeltaValue (Quaternion myLastValue, Quaternion myCurrentValue) {
		deltaValue = myCurrentValue * myLastValue;
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