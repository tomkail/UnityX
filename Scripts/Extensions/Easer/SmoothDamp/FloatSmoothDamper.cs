using UnityEngine;

namespace UnityX.Easer {
	// Derives from SmoothDamper<float> like every other concrete damper.
	[System.Serializable]
	public class FloatSmoothDamper : SmoothDamper<float> {
		protected FloatSmoothDamper () : base () {}
		public FloatSmoothDamper (float value) : base(value) {}
		public FloatSmoothDamper (float current, float smoothTime) : base(current, smoothTime) {}
		public FloatSmoothDamper (float target, float current, float smoothTime) : base(target, current, smoothTime) {}

		protected override float SmoothDamp (float deltaTime) {
			if(deltaTime == 0) return current;
			return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
		}

		protected override float GetDelta (float lastValue, float newValue) {
			return newValue - lastValue;
		}
	}
}
