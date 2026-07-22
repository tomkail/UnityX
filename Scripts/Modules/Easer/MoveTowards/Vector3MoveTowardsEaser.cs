using UnityEngine;

namespace UnityX.Easer {
	[System.Serializable]
	public class Vector3MoveTowardsEaser : MoveTowardsEaser<Vector3> {
		protected Vector3MoveTowardsEaser () : base () {}
		public Vector3MoveTowardsEaser (Vector3 value) : this(value, value) {}
		public Vector3MoveTowardsEaser (Vector3 target, Vector3 current) : base(target, current) {}
		public Vector3MoveTowardsEaser (Vector3 target, Vector3 current, float maxDelta) : base(target, current, maxDelta) {}

		protected override Vector3 MoveTowards (float deltaTime) {
			return Vector3.MoveTowards(current, target, maxDelta * deltaTime);
		}

		protected override Vector3 GetDelta (Vector3 lastValue, Vector3 newValue) {
			return newValue - lastValue;
		}
	}
}
