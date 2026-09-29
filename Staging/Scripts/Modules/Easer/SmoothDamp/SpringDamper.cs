using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityX.Springs;

namespace UnityX.Easer {
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


		// Delegates to the analytic (closed-form) damped-spring solver in `Spring` — deterministic and
		// framerate-independent, so it needs no fixed-timestep hack or explicit-Euler sub-stepping.
		// `Spring.Update` is a Mathf.SmoothDamp-style step; SpringDamper models a unit-mass spring, so mass = 1.
		public static float DampedSpring(float current, float target, ref float velocity, float springConstant, float damping) {
			return DampedSpring(current, target, ref velocity, springConstant, damping, Time.deltaTime);
		}
		public static float DampedSpring(float current, float target, ref float velocity, float springConstant, float damping, float deltaTime) {
			return Spring.Update(current, target, ref velocity, 1f, springConstant, damping, deltaTime);
		}

		public static float CriticallyDampedSpring(float current, float target, ref float velocity, float springConstant) {
			return CriticallyDampedSpring(current, target, ref velocity, springConstant, Time.deltaTime);
		}
		public static float CriticallyDampedSpring(float current, float target, ref float velocity, float springConstant, float deltaTime) {
			// Critical damping for a unit-mass spring: damping = 2·√stiffness (dampingRatio == 1).
			return Spring.Update(current, target, ref velocity, 1f, springConstant, 2f * Mathf.Sqrt(springConstant), deltaTime);
		}
	}
}
