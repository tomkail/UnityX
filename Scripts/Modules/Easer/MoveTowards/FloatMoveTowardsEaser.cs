using UnityEngine;

namespace UnityX.Easer {
	// Derives from MoveTowardsEaser<float> like every other concrete easer.
	[System.Serializable]
	public class FloatMoveTowardsEaser : MoveTowardsEaser<float> {
		protected FloatMoveTowardsEaser () : base () {}
		public FloatMoveTowardsEaser (float value) : this(value, value) {}
		public FloatMoveTowardsEaser (float target, float current) : base(target, current) {}
		public FloatMoveTowardsEaser (float target, float current, float maxDelta) : base(target, current, maxDelta) {}

		protected override float MoveTowards (float deltaTime) {
			return Mathf.MoveTowards(current, target, maxDelta * deltaTime);
		}

		protected override float GetDelta (float lastValue, float newValue) {
			return newValue - lastValue;
		}
	}
}
