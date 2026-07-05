using UnityEngine;

namespace UnityX.Tween {
	[System.Serializable]
	public class TweenProperties<T> {
		public bool setStartValue = false;
		public bool setEasingCurve = false;
		public T startValue; 
		public T targetValue;
		public float tweenTime;
		public AnimationCurve easingCurve;
		public TypeTween<T>.LerpFunction lerpFunction;

		// NOTE: no targetValue is supplied, so this tweens startValue -> default(T) (0 / Vector3.zero / …).
		// To tween *to* a value from the current one, use the (startValue, targetValue, tweenTime) ctor
		// or a Tween(target, time) overload.
		public TweenProperties (T startValue, float tweenTime) {
			this.setStartValue = true;
			this.startValue = startValue;
			this.tweenTime = tweenTime;
		}

		public TweenProperties (T startValue, T targetValue, float tweenTime) {
			this.setStartValue = true;
			this.startValue = startValue;
			this.targetValue = targetValue;
			this.tweenTime = tweenTime;
		}

		public TweenProperties (T startValue, T targetValue, float tweenTime, AnimationCurve easingCurve) {
			this.setStartValue = true;
			this.setEasingCurve = true;
			this.startValue = startValue;
			this.targetValue = targetValue;
			this.tweenTime = tweenTime;
			this.easingCurve = easingCurve;
		}
	}
}
