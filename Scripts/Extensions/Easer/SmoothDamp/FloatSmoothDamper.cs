using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloatSmoothDamper {
	private const float defaultSmoothTime = 0.1f;
	private const float defaultMaxSpeed = Mathf.Infinity;



	[SerializeField, Tooltip("The target value")]
	private float _target;
	public float target {
		get {
			return _target;
		} set {
			if(_target.Equals(value)) return;
			_target = value;
			if(OnChangeTarget != null) OnChangeTarget(target);
		}
	}
	[SerializeField, Tooltip("The current value")]
	private float _current;
	public float current {
		get {
			return _current;
		} set {
			if(_current.Equals(value)) return;
			_current = value;
			if(OnChangeCurrent != null) OnChangeCurrent(current);
		}
	}
	
	private float _delta;
	public float delta {
		get {
			return _delta;
		} set {
			_delta = value;
		}
	}

	

	[Tooltip("Higher is slower")]
	/// <summary>
	/// The smooth time. Higher is slower.
	/// </summary>
	public float smoothTime = defaultSmoothTime;
	[DisableAttribute, Tooltip("The current velocity of the damp")]
	public float currentVelocity;
	[Tooltip("The velocity clamp")]
	/// <summary>
	/// The max delta per second.
	/// </summary>
	public float maxSpeed = defaultMaxSpeed;


	public float remaining {
		get {
			return GetDelta(target, current);
		}
	}

	public System.Func<float, float> lerpFunction;

	public event System.Action<float> OnChangeTarget;
	public event System.Action<float> OnChangeCurrent;




	// Used for correct Unity editor serializer initialization
	FloatSmoothDamper () {
		lerpFunction = SmoothDamp;
		smoothTime = defaultSmoothTime;
		maxSpeed = defaultMaxSpeed;
	}

	public FloatSmoothDamper (float current) {
		lerpFunction = SmoothDamp;
		this.current = this.target = current;
		this.smoothTime = defaultSmoothTime;
	}

	public FloatSmoothDamper (float current, float smoothTime) {
		lerpFunction = SmoothDamp;
		this.current = this.target = current;
		this.smoothTime = smoothTime;
	}

	public FloatSmoothDamper (float target, float current, float smoothTime) {
		lerpFunction = SmoothDamp;
		this.target = target;
		this.current = current;
		this.smoothTime = smoothTime;
	}

	


	float SmoothDamp (float deltaTime) {
		if(deltaTime == 0) return current;
		else return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
	}

	float GetDelta (float lastValue, float newValue) {
		return newValue - lastValue;
	}

	public float Update () {
		return Update(Time.deltaTime);
	}

	public float Update (float deltaTime) {
		var last = current;
		current = lerpFunction(deltaTime);
		delta = GetDelta(last, current);
		return current;
	}

	// Forces OnChangeCurrent event.
	public void ForceOnChangeEvent () {
		if(OnChangeCurrent != null) OnChangeCurrent(current);
	}
	
	public void Reset (float newDefaultValue) {
		target = current = newDefaultValue;
		currentVelocity = 0;
	}


	public override string ToString () {
		return string.Format ("[SmoothDamper] Current={0}, Target={1}, Velocity={2}, SmoothTime={3}, MaxSpeed={4}", current, target, currentVelocity, smoothTime, maxSpeed);
	}
}