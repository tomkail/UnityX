using UnityEngine;

public class RectTween : TypeTween<Rect> {

	public RectTween () : base () {}
	public RectTween (Rect myStartValue) : base (myStartValue) {}
	public RectTween (Rect myStartValue, Rect myTargetValue, float myLength) : base (myStartValue, myTargetValue, myLength) {}
	public RectTween (Rect myStartValue, Rect myTargetValue, float myLength, AnimationCurve myLerpCurve) : base (myStartValue, myTargetValue, myLength, myLerpCurve) {}

	protected override void SetDefaultLerpFunction () {
		lerpFunction = (start, end, lerp) => {
			Vector4 newRect = Vector4.Lerp(new Vector4(start.x, start.y, start.width, start.height), new Vector4(end.x, end.y, end.width, end.height), easingCurve.Evaluate(lerp));
			return new Rect(newRect.x, newRect.y, newRect.z, newRect.w);
		};
	}

	protected override void SetDeltaValue (Rect myLastValue, Rect myCurrentValue) {
		deltaValue = new Rect(myCurrentValue.x-myLastValue.x, myCurrentValue.y-myLastValue.y, myCurrentValue.width-myLastValue.width, myCurrentValue.height-myLastValue.height);
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