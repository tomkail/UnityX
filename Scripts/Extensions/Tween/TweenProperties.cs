using UnityEngine;
using System.Collections;

[System.Serializable]
public class TweenProperties<T> {
	public bool setStartValue = false;
	public bool setEasingCurve = false;
	public T startValue; 
	public T targetValue;
	public float tweenTime;
	public AnimationCurve easingCurve;
	public TypeTween<T>.LerpFunction lerpFunction;
	
	public TweenProperties (T startValue, float tweenTime) {
		this.startValue = startValue;
		this.tweenTime = tweenTime;
	}
	
	public TweenProperties (T startValue, T targetValue, float tweenTime) {
		this.startValue = startValue;
		this.targetValue = targetValue;
		this.tweenTime = tweenTime;
	}
	
	public TweenProperties (T startValue, T targetValue, float tweenTime, AnimationCurve easingCurve) {
		this.startValue = startValue;
		this.targetValue = targetValue;
		this.tweenTime = tweenTime;
		this.easingCurve = easingCurve;
	}
}