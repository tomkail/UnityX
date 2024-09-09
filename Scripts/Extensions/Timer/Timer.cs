using UnityEngine;
using System;

/// <summary>
/// Timer class.
/// </summary>
[Serializable]
public class Timer {

	public enum State {
		Stopped,
		Playing
	}

	public State state = State.Stopped;

	public float currentTime = 0f;
	public bool useTargetTime = true;
	public bool stopOnReachingTarget = true;

	[SerializeField, Disable]
	private float _targetTime = 0f;
	public float targetTime {
		get => _targetTime;
		set {
			_targetTime = value;
			_targetTimeReciprocal = null;

            if(OnSetTargetTime != null) OnSetTargetTime(); 
		}
	}
	public float remainingTime => targetTime - currentTime;

	public bool isComplete => remainingTime <= 0.0f;

	public int currentRepeats = 0;
	public int targetRepeats = 1;
	public bool repeatForever = false;

	// Used for quickly obtaining normalized time
	private float? _targetTimeReciprocal;

	public event Action OnStart;
	public event Action OnStop;
	public event Action OnReset;
	public event Action OnRepeat;
	public event Action OnComplete;
	public event Action OnSetTargetTime;

	public static Timer StartNew(float targetTime, Action onComplete = null)
	{
		var t = new Timer(targetTime);
		if( onComplete != null )
			t.OnComplete += onComplete;
		t.Start();
		return t;
	}

	public Timer () {
		useTargetTime = false;
	}

	public Timer (float myTargetTime) {
		Set(myTargetTime);
	}

	public Timer (float myTargetTime, int myTargetRepeats) {
		Set(myTargetTime, myTargetRepeats);
	}

	public Timer (float myTargetTime, bool myRepeatForever) {
		Set(myTargetTime, myRepeatForever);
	}

	public virtual void Set (float myTargetTime, int myTargetRepeats = 1) {
		targetTime = myTargetTime;
		targetRepeats = myTargetRepeats;
		useTargetTime = (targetTime >= 0 && targetRepeats > 0);
	}

	public virtual void Set (float myTargetTime, bool myRepeatForever) {
		targetTime = myTargetTime;
		repeatForever = myRepeatForever;
		useTargetTime = (targetTime > 0);
	}

	/// <summary>
	/// Starts the timer
	/// </summary>
	public virtual void Start () {
		if(state == State.Playing) return;
		state = State.Playing;
		if(OnStart != null) OnStart();
	}

	// Shorthand for a common set of commands
	public virtual void ResetAndStart () {
		Reset();
		Start();
	}

	// Shorthand for a common set of commands
	public virtual void ResetAndStart (float myTargetTime) {
		Reset();
		Set(myTargetTime);
		Start();
	}

	/// <summary>
	/// Pauses the updating of the timer
	/// </summary>
	public virtual void Stop () {
		if(state == State.Stopped) return;
		state = State.Stopped;
		if(OnStop != null) OnStop();
	}

	// Shorthand for two common commands
	public virtual void StopAndReset () {
		Stop();
		Reset();
	}

	/// <summary>
	/// Resets the time and repeat count. Does not change the play state.
	/// </summary>
	public virtual void Reset () {
		currentTime = 0;
		currentRepeats = 0;
		if(OnReset != null) OnReset();
	}

	public void CopyFrom(Timer otherTimer) {
		state       = otherTimer.state;
		currentTime = otherTimer.currentTime;
		useTargetTime = otherTimer.useTargetTime;
		stopOnReachingTarget = otherTimer.stopOnReachingTarget;
		targetTime = otherTimer.targetTime;
		currentRepeats = otherTimer.currentRepeats;
		targetRepeats = otherTimer.targetRepeats;
		repeatForever = otherTimer.repeatForever;
	}

	/// <summary>
	/// Update the timer using the delta time.
	/// </summary>
	public virtual void Update () {
		Update(Time.deltaTime);
	}

	/// <summary>
	/// Update the timer using a given delta time.
	/// </summary>
	public virtual void Update (float _deltaTime) {
		if(state == State.Playing) {
			UpdateTimer(_deltaTime);
		}
	}

	/// <summary>
	/// Returns the normalized time, between the range 0,1. Does not take repeats into account.
	/// </summary>
	public virtual float GetNormalizedTime () {
		if(targetTime == 0) return 1;
		if(_targetTimeReciprocal == null) {
			_targetTimeReciprocal = 1f/targetTime;
		}
		return Mathf.Clamp01((float)_targetTimeReciprocal * currentTime);
	}

	/// <summary>
	/// Update the timer using a given delta time.
	/// </summary>
	protected virtual void UpdateTimer (float _deltaTime) {
		currentTime += _deltaTime;
		if(useTargetTime && currentTime > targetTime) {
			ReachTargetTime();
		}
	}

	/// <summary>
	/// Called when the current time reaches the target time.
	/// </summary>
	protected virtual void ReachTargetTime () {
		currentRepeats++;
		if(currentRepeats < targetRepeats || repeatForever) {
			currentTime = 0;
			if(OnRepeat != null) OnRepeat();
		} else {
			if(stopOnReachingTarget) Stop();
			if(OnComplete != null) OnComplete();
		}
	}
	
	public override string ToString () {
		return string.Format("{0}: State: {1}, Time: {2}, Repeats: {3}", GetType(), state, currentTime, currentRepeats);
	}
}
