﻿using System;
using UnityEngine;

public abstract class TypeTween<T> {
	[SerializeField]
	private T _currentValue;
	public T currentValue {
		get {
			return _currentValue;
		} set {
			_currentValue = value;
			ChangedCurrentValue();
		}
	}
	public T deltaValue;
	
	public bool tweening = false;
	public T targetValue;
	public T startValue;
	public Timer tweenTimer;

	[SerializeField]
	private AnimationCurve _easingCurve;
	public AnimationCurve easingCurve {
		get {
			return _easingCurve;
		} protected set {
			_easingCurve = value;
		}
	}
	
	public delegate void OnChangeEvent(T currentValue);
	public event OnChangeEvent OnChange;
	
	//Called when a new tween is started while another is in progress.
	public delegate void OnInterruptEvent();
	public event OnInterruptEvent OnInterrupt;

	public delegate void OnStopEvent();
	public event OnStopEvent OnStop;
	public delegate void OnStartEvent();
	public event OnStartEvent OnStart;

	public delegate void OnCompleteEvent();
	public event OnCompleteEvent OnComplete;

	public delegate T LerpFunction(T start, T end, float lerp);
	public LerpFunction lerpFunction;

	public TypeTween () {
		Init();
	}

	public TypeTween (T myCurrentValue) {
		Init();
		targetValue = currentValue = myCurrentValue;
	}
	
	public TypeTween (T myStartValue, T myTargetValue, float myTweenTime) {
		Init();
		Tween(myStartValue, myTargetValue, myTweenTime);
	}

	public TypeTween (T myStartValue, T myTargetValue, float myTweenTime, LerpFunction lerpFunction) {
		Init();
		Tween(myStartValue, myTargetValue, myTweenTime, lerpFunction);
	}
	
	public TypeTween (T myStartValue, T myTargetValue, float myTweenTime, AnimationCurve _easingCurve) {
		Init();
		Tween(myStartValue, myTargetValue, myTweenTime, _easingCurve);
	}

	public TypeTween (T myStartValue, T myTargetValue, float myTweenTime, AnimationCurve _easingCurve, LerpFunction lerpFunction) {
		Init();
		Tween(myStartValue, myTargetValue, myTweenTime, _easingCurve, lerpFunction);
	}
	
	public TypeTween (TweenProperties<T> tweenProperties) {
		Init();
		Tween(tweenProperties);
	}

	protected virtual void Init () {}

	/// <summary>
	/// Stops the tween and resets the current value to the default for this type.
	/// </summary>
	public void Reset () {
		Reset(default(T));
	}

	/// <summary>
	/// Stops the tween and resets the current value to the specified value
	/// </summary>
	/// <param name="myCurrentValue">My current value.</param>
	public void Reset (T myCurrentValue) {
		Stop();
		currentValue = myCurrentValue;
	}

	/// <summary>
	/// Stops the tween and resets values to the specified values
	/// </summary>
	/// <param name="myStartValue">My start value.</param>
	/// <param name="myTargetValue">My target value.</param>
	/// <param name="myTweenTime">My tween time.</param>
	public void Reset (T myStartValue, T myTargetValue, float myTweenTime) {
		Stop();
		Tween(myStartValue, myTargetValue, myTweenTime);
	}

	/// <summary>
	/// Stops the tween and resets values to the specified values
	/// </summary>
	/// <param name="myStartValue">My start value.</param>
	/// <param name="myTargetValue">My target value.</param>
	/// <param name="myTweenTime">My tween time.</param>
	/// <param name="_easingCurve">Easing curve.</param>
	public void Reset (T myStartValue, T myTargetValue, float myTweenTime, AnimationCurve _easingCurve) {
		Stop();
		Tween(myStartValue, myTargetValue, myTweenTime, _easingCurve);
	}
	
	/// <summary>
	/// Starts a new tween between the current value and the target value over a specified time
	/// </summary>
	/// <param name="myTargetValue">The value to tween towards.</param>
	/// <param name="myTargetTime">The time over which the tween will occur.</param>
	public virtual void Tween(T myTargetValue, float myTweenTime){
		Tween(currentValue, myTargetValue, myTweenTime);
	}
	
	/// <summary>
	/// Starts a new tween between the current value and the target value over a specified time, using an easing curve
	/// </summary>
	/// <param name="myTargetValue">The value to tween towards.</param>
	/// <param name="myTargetTime">The time over which the tween will occur.</param>
	/// <param name="myLerpCurve">The easing curve fro the tween</param>
	public virtual void Tween(T myTargetValue, float myTweenTime, AnimationCurve myLerpCurve){
		Tween(currentValue, myTargetValue, myTweenTime, myLerpCurve);
	}

	public virtual void Tween(T myTargetValue, float myTweenTime, LerpFunction lerpFunction){
		Tween(currentValue, myTargetValue, myTweenTime, lerpFunction);
	}

	public virtual void Tween(T myTargetValue, float myTweenTime, AnimationCurve myLerpCurve, LerpFunction lerpFunction){
		Tween(currentValue, myTargetValue, myTweenTime, myLerpCurve, lerpFunction);
	}

	/// <summary>
	/// Starts a new tween between the start value and the target value over a specified time
	/// </summary>
	/// <param name="myStartValue">The start value of the tween.</param>
	/// <param name="myTargetValue">The value to tween towards.</param>
	/// <param name="myTargetTime">The time over which the tween will occur.</param>
	public virtual void Tween(T myStartValue, T myTargetValue, float myTweenTime){
		Tween(myStartValue, myTargetValue, myTweenTime, AnimationCurve.Linear(0, 0, 1, 1));
	}

	public virtual void Tween(T myStartValue, T myTargetValue, float myTweenTime, AnimationCurve myLerpCurve){
		Tween(myStartValue, myTargetValue, myTweenTime, myLerpCurve, lerpFunction);
	}

	public virtual void Tween(T myStartValue, T myTargetValue, float myTweenTime, LerpFunction lerpFunction){
		Tween(myStartValue, myTargetValue, myTweenTime, AnimationCurve.Linear(0, 0, 1, 1), lerpFunction);
	}

	/// <summary>
	/// Starts a new tween with the TweenProperties object.
	/// </summary>
	/// <param name="tweenProperties">The tween properties.</param>
	public virtual void Tween(TweenProperties<T> tweenProperties){
		if(tweenProperties.setStartValue) {
			if(tweenProperties.setEasingCurve) {
				Tween(tweenProperties.startValue, tweenProperties.targetValue, tweenProperties.tweenTime, tweenProperties.easingCurve, tweenProperties.lerpFunction);
			} else {
				Tween(tweenProperties.startValue, tweenProperties.targetValue, tweenProperties.tweenTime, tweenProperties.lerpFunction);
			}
		} else {
			if(tweenProperties.setEasingCurve) {
				Tween(tweenProperties.targetValue, tweenProperties.tweenTime, tweenProperties.easingCurve, tweenProperties.lerpFunction);
			} else {
				Tween(tweenProperties.targetValue, tweenProperties.tweenTime, tweenProperties.lerpFunction);
			}
		}
	}
	
	/// <summary>
	/// Starts a new tween between the start value and the target value over a specified time, using an easing curve
	/// </summary>
	/// <param name="myStartValue">The start value of the tween.</param>
	/// <param name="myTargetValue">The value to tween towards.</param>
	/// <param name="myTargetTime">The time over which the tween will occur.</param>
	/// <param name="myLerpCurve">The easing curve fro the tween</param>
	public virtual void Tween(T myStartValue, T myTargetValue, float myTweenTime, AnimationCurve myLerpCurve, LerpFunction lerpFunction){
		if(tweening) {
			Interrupt();
		}
		
        SetEasingCurve(myLerpCurve);

        if(lerpFunction == null) {
            SetDefaultLerpFunction();
        } else {
            this.lerpFunction = lerpFunction;
        }
        
		if (myTweenTime > 0.0f) {
			tweenTimer = new Timer(myTweenTime);
			tweenTimer.OnComplete += TweenComplete;
			tweenTimer.Start();
			
			deltaValue = default(T);
			currentValue = myStartValue;
			startValue = myStartValue;
			targetValue = myTargetValue;

			tweening = true;
		} else {
			currentValue = myTargetValue;
			startValue = myStartValue;
			targetValue = myTargetValue;
		}
		TweenStart();
        if(!tweening) TweenComplete();
	}

	protected virtual void SetEasingCurve (AnimationCurve easingCurve) {
		this.easingCurve = easingCurve;
	}

	/// <summary>
	/// Updates the tween with deltaTime
	/// </summary>
	public virtual T Update () {
		return Update(Time.deltaTime);
	}
	
	/// <summary>
	/// Updates the tween with a custom deltaTime
	/// </summary>
	public virtual T Update (float myDeltaTime) {
		if(tweening){
			SetValue(GetValueAtNormalizedTime(tweenTimer.GetNormalizedTime()));
			tweenTimer.Update(myDeltaTime);
		}
		return currentValue;
	}
	

	protected virtual void TweenStart () {
		if(OnStart != null) OnStart();
	}

	/// <summary>
	/// Called when the tween is completed
	/// </summary>
	protected virtual void TweenComplete () {
		SetValue(GetValueAtNormalizedTime(1));
		deltaValue = default(T);
		Stop();
		if(OnComplete != null) OnComplete();
	}
	
	/// <summary>
	/// Stop tweening
	/// </summary>
	public virtual void Stop () {
		tweening = false;
		if(OnStop != null) OnStop();
	}
	
	public virtual void Interrupt () {
		Stop ();
		if(OnInterrupt != null) OnInterrupt();
	}
	
	/// <summary>
	/// Returns the current value of the tween at a specified time
	/// </summary>
	public T GetValueAtTime(float myTime){
		return GetValueAtNormalizedTime(myTime/tweenTimer.targetTime);
	}
	
	/// <summary>
	/// Returns the current value of the tween at a normalized specified time
	/// </summary>
	public T GetValueAtNormalizedTime(float myNormalizedTime) {
		return lerpFunction(startValue, targetValue, myNormalizedTime);
	}
	
	/// <summary>
	/// Sets the current value of the tween
	/// </summary>
	protected virtual void SetValue (T myValue) {
		T lastValue = currentValue;
		currentValue = myValue;
		SetDeltaValue(lastValue, currentValue);
	}
	
	protected abstract void SetDeltaValue (T myLastValue, T myCurrentValue);
	
	protected virtual void ChangedCurrentValue () {
		if(OnChange != null) OnChange(currentValue);
	}

	protected abstract void SetDefaultLerpFunction ();


	public override string ToString() {
		return "["+this.GetType().Name+"] tweening "+tweening+", currentValue "+currentValue+", targetValue "+targetValue+", timer norm time "+(tweenTimer == null ? "" : tweenTimer.GetNormalizedTime().ToString());
	}
}