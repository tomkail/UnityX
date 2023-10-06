using UnityEngine;

[System.Serializable]
public class QuaternionRotateTowardsEaser : MoveTowardsEaser<Quaternion> {
	protected QuaternionRotateTowardsEaser () : base () {}
	public QuaternionRotateTowardsEaser (Quaternion value) : this(value, value) {}
	public QuaternionRotateTowardsEaser (Quaternion target, Quaternion current) : base(target, current) {}
	public QuaternionRotateTowardsEaser (Quaternion target, Quaternion current, float maxDelta) : base(target, current, maxDelta) {}

	protected override Quaternion MoveTowards (float deltaTime) {
		return Quaternion.RotateTowards(current, target, maxDelta * deltaTime);
	}

	protected override Quaternion GetDelta (Quaternion lastValue, Quaternion newValue) {
		return QuaternionX.Difference(newValue, lastValue);
	}
}