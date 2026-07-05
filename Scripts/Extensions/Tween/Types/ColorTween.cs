using UnityEngine;
using UnityX.Colors;

namespace UnityX.Tween {
	public class ColorTween : TypeTween<Color> {

		private BlendMode blendMode;

		public ColorTween () : base () {}

		public ColorTween (Color myStartValue, BlendMode myBlendMode = BlendMode.Normal) : base (myStartValue) {
			blendMode = myBlendMode;
		}

		public ColorTween (Color myStartValue, Color myTargetValue, float myLength, BlendMode myBlendMode = BlendMode.Normal) : base (myStartValue, myTargetValue, myLength) {
			blendMode = myBlendMode;
		}

		public ColorTween (Color myStartValue, Color myTargetValue, float myLength, AnimationCurve myLerpCurve, BlendMode myBlendMode = BlendMode.Normal) : base (myStartValue, myTargetValue, myLength, myLerpCurve) {
			blendMode = myBlendMode;
		}

		public virtual void Tween(Color myStartValue, Color myTargetValue, float myTweenTime, BlendMode myBlendMode = BlendMode.Normal){
			Tween(myStartValue, myTargetValue, myTweenTime, AnimationCurve.Linear(0, 0, 1, 1), myBlendMode);
		}

		public virtual void Tween(Color myStartValue, Color myTargetValue, float myTweenTime, AnimationCurve myLerpCurve, BlendMode myBlendMode = BlendMode.Normal){
			base.Tween(myStartValue, myTargetValue, myTweenTime, myLerpCurve);
			blendMode = myBlendMode;
		}

		protected override void SetDefaultLerpFunction () {
			lerpFunction = (start, end, lerp) => {
				return ColorBlend.Blend(start, end, easingCurve.Evaluate(lerp), blendMode);
			};
		}

		protected override void SetDeltaValue (Color myLastValue, Color myCurrentValue) {
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
			if(OnComplete != null) OnComplete();
		}

		public override void Interrupt () {
			base.Interrupt();
			if(OnInterrupt != null) OnInterrupt();
		}
	}
}
