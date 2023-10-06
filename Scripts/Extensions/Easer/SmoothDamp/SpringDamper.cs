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
	/// Adds a force without deltaTime.
	/// </summary>
	/// <param name="force">Force.</param>
	public void AddImpulse (float force) {
		currentVelocity += force;
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


	// Ok, so this is not accurate and definitely breaks when deltaTime is > 1, and also maybe because both steps should happen at once.
	// The correct solution to this is http://www.ryanjuckett.com/programming/damped-springs/ which is a deterministic approach using a fixed wave period
	// But that's nuts so we just clamp delta time, which means it'll be wrong, but not especially noticably wrong.
	public static float DampedSpring(float current, float target, ref float velocity, float springConstant, float damping) {
		return DampedSpring(current, target, ref velocity, springConstant, damping, Time.deltaTime);
	}
	public static float DampedSpring(float current, float target, ref float velocity, float springConstant, float damping, float deltaTime) {
		// we fix deltatime because a varying rate can see the spring go out of equilibrium. This is always smooth!
		deltaTime = 1f/60f;

		var currentToTarget = target - current;
		var springForce = currentToTarget * springConstant;
		var dampingForce = velocity * -damping;

		float force = springForce + dampingForce;
		velocity += force * deltaTime;
		
		float displacement = velocity * deltaTime;
		return current + displacement;		
	}

	public static float CriticallyDampedSpring(float current, float target, ref float velocity, float springConstant) {
		// we fix deltatime because a varying rate can see the spring go out of equilibrium. This is always smooth! 
		var deltaTime = 1f/60f;

		float currentToTarget = target - current;
		float springForce = currentToTarget * springConstant;
		float dampingForce = -velocity * 2 * Mathf.Sqrt(springConstant);
		
		float force = springForce + dampingForce;		
		velocity += force * deltaTime;
		
		float displacement = velocity * deltaTime;
		return current + displacement;
	}
}