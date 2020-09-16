using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RectTween : TypeTween<Rect> {

	public RectTween () : base () {}
	public RectTween (Rect myStartValue) : base (myStartValue) {}
	public RectTween (Rect myStartValue, Rect myTargetValue, float myLength) : base (myStartValue, myTargetValue, myLength) {}
	public RectTween (Rect myStartValue, Rect myTargetValue, float myLength, AnimationCurve myLerpCurve) : base (myStartValue, myTargetValue, myLength, myLerpCurve) {}

	protected override void SetDefaultLerpFunction () {
		lerpFunction = (start, end, lerp) => {
			return RectX.Lerp(start, end, easingCurve.Evaluate(lerp));
		};
	}

	protected override void SetDeltaValue (Rect myLastValue, Rect myCurrentValue) {
		deltaValue = myCurrentValue.Subtract(myLastValue);
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