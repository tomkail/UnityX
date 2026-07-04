using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpringDamper {
	[SerializeField, Tooltip("The current value")]
	private float _current;
	public float current {
		get {
			return _current;
		} set {
			_current = value;
			if(OnChangeCurrent != null) OnChangeCurrent(current);
		}
	}
	public float target;

	[Tooltip("The current velocity")]
	public float currentVelocity;
	[Tooltip("The rigidity of the spring. A high value makes a more powerful spring.")]
	public float stiffness = 1;
	[Tooltip("The damping of the spring. It affects how quickly the spring comes to a stop.")]
	public float damping = 0.1f;
	public event System.Action<float> OnChangeCurrent;

	/// <summary>
	/// Applies an instantaneous change in velocity (an impulse).
	/// Unlike AddForce, this is NOT scaled by deltaTime — it's a one-off momentum kick (Δvelocity),
	/// mirroring Unity's ForceMode.Impulse vs ForceMode.Force distinction.
	/// </summary>
	/// <param name="impulse">The velocity change to apply.</param>
	public void AddImpulse (float impulse) {
		Debug.Assert(!float.IsNaN(impulse) && !float.IsInfinity(impulse), "Impulse is "+impulse);
		currentVelocity += impulse;
	}

	/// <summary>
	/// Adds the force using a default deltaTime
	/// </summary>
	/// <param name="force">Force.</param>
	public void AddForce (float force) {
		AddForce(force, Time.deltaTime);
	}

	/// <summary>
	/// Adds the force using a defined deltaTime.
	/// </summary>
	/// <param name="force">Force.</param>
	/// <param name="deltaTime">Delta time.</param>
	public void AddForce (float force, float deltaTime) {
		Debug.Assert(!float.IsNaN(force) && !float.IsInfinity(force), "Force is "+force);
		currentVelocity += force * deltaTime;
	}

	public virtual float Update () {
		return Update(Time.deltaTime);
	}

	public virtual float Update (float deltaTime) {
		return current = DampedSpring(current, target, ref currentVelocity, stiffness, damping, deltaTime);
	}

	public virtual void Reset (float newDefaultValue) {
		current = newDefaultValue;
		currentVelocity = default(float);
	}

	public override string ToString () {
		return string.Format ("[SpringDamper] Current={0}, Velocity={1}", current, currentVelocity);
	}


	// Explicit-Euler springs go unstable if a single integration step is too large (a low frame rate could
	// push the spring out of equilibrium). Rather than hard-code the step to 1/60 (which ignored the caller's
	// deltaTime and made the spring run at the wrong speed off 60fps), we sub-step the real deltaTime into
	// fixed <= 1/60s chunks: stable per-step AND framerate-independent. Total simulated time is capped at 1s
	// to guard against a huge hitch causing a catch-up spiral. (An analytic damped spring —
	// http://www.ryanjuckett.com/programming/damped-springs/ — would be exact; sub-stepping is simple + stable.)
	const float maxSpringStep = 1f/60f;

	public static float DampedSpring(float current, float target, ref float velocity, float springConstant, float damping) {
		return DampedSpring(current, target, ref velocity, springConstant, damping, Time.deltaTime);
	}
	public static float DampedSpring(float current, float target, ref float velocity, float springConstant, float damping, float deltaTime) {
		float remaining = Mathf.Min(deltaTime, 1f);
		while (remaining > 0f) {
			float dt = Mathf.Min(maxSpringStep, remaining);
			remaining -= dt;
			float force = (target - current) * springConstant + velocity * -damping;
			velocity += force * dt;
			current += velocity * dt;
		}
		return current;
	}

	public static float CriticallyDampedSpring(float current, float target, ref float velocity, float springConstant) {
		return CriticallyDampedSpring(current, target, ref velocity, springConstant, Time.deltaTime);
	}
	public static float CriticallyDampedSpring(float current, float target, ref float velocity, float springConstant, float deltaTime) {
		float remaining = Mathf.Min(deltaTime, 1f);
		while (remaining > 0f) {
			float dt = Mathf.Min(maxSpringStep, remaining);
			remaining -= dt;
			float force = (target - current) * springConstant + (-velocity * 2 * Mathf.Sqrt(springConstant));
			velocity += force * dt;
			current += velocity * dt;
		}
		return current;
	}
}