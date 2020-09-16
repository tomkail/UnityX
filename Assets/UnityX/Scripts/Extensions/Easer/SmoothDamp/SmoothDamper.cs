using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handy extendable wrapper for Unity's SmoothDamp functions, tucking away the several variables for each smooth damp you want to have running.
/// </summary>
[System.Serializable]
public abstract class SmoothDamper<T> : BaseEaser<T> {

	private const float defaultSmoothTime = 0.1f;
	private const float defaultMaxSpeed = Mathf.Infinity;

	[Tooltip("Higher is slower")]
	/// <summary>
	/// The smooth time. Higher is slower.
	/// </summary>
	public float smoothTime = defaultSmoothTime;
	[DisableAttribute, Tooltip("The current velocity of the damp")]
	public T currentVelocity;
	[Tooltip("The velocity clamp")]
	/// <summary>
	/// The max delta per second.
	/// </summary>
	public float maxSpeed = defaultMaxSpeed;

	// Used for correct Unity editor serializer initialization
	protected SmoothDamper () {
		lerpFunction = SmoothDamp;
		smoothTime = defaultSmoothTime;
		maxSpeed = defaultMaxSpeed;
	}

	public SmoothDamper (T current) {
		lerpFunction = SmoothDamp;
		this.current = this.target = current;
		this.smoothTime = defaultSmoothTime;
	}

	public SmoothDamper (T current, float smoothTime) {
		lerpFunction = SmoothDamp;
		this.current = this.target = current;
		this.smoothTime = smoothTime;
	}

	public SmoothDamper (T target, T current, float smoothTime) {
		lerpFunction = SmoothDamp;
		this.target = target;
		this.current = current;
		this.smoothTime = smoothTime;
	}

	public override void Reset (T newDefaultValue) {
		base.Reset(newDefaultValue);
		currentVelocity = default(T);
	}
	
	protected abstract T SmoothDamp (float deltaTime);

	public override string ToString () {
		return string.Format ("[SmoothDamper] Current={0}, Target={1}, Velocity={2}, SmoothTime={3}, MaxSpeed={4}", current, target, currentVelocity, smoothTime, maxSpeed);
	}
}
