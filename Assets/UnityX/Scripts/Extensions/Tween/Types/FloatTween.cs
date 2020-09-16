using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FloatTween : TypeTween<float> {

	public FloatTween () : base () {}
	public FloatTween (float myCurrentValue) : base (myCurrentValue) {}
	public FloatTween (float myStartValue, float myTargetValue, float myLength) : base (myStartValue, myTargetValue, myLength) {}
	public FloatTween (float myStartValue, float myTargetValue, float myLength, AnimationCurve myLerpCurve) : base (myStartValue, myTargetValue, myLength, myLerpCurve) {}

	protected override void SetDefaultLerpFunction () {
		lerpFunction = (start, end, lerp) => {
			return Mathf.LerpUnclamped(start, end, easingCurve.Evaluate(lerp));
		};
	}

	protected override void SetDeltaValue (float myLastValue, float myCurrentValue) {
		deltaValue = myCurrentValue - myLastValue;	
	}
	
	//----------------- IOS GENERIC INHERITANCE EVENT CRASH BUG WORKAROUND ------------------
	//http://angrytroglodyte.net/cave/index.php/blog/11-unity-ios-doesn-t-like-generic-events
	public new event OnChangeEvent OnChange;
	public new event OnInterruptEvent OnInterrupt;
	public new event OnStartEvent OnStart;
	public new event OnCompleteEvent OnComplete;
	
	protected override void ChangedCurrentValue () {
		base.ChangedCurrentValue();
		if(OnChange != null) OnChange(currentValue);
	}

	protected override void TweenStart () {
		if(OnStart != null) OnStart();
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