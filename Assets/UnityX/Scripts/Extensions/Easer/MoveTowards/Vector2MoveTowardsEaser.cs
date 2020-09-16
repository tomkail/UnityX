using UnityEngine;

[System.Serializable]
public class Vector2MoveTowardsEaser : MoveTowardsEaser<Vector2> {
	protected Vector2MoveTowardsEaser () : base () {}
	public Vector2MoveTowardsEaser (Vector2 value) : this(value, value) {}
	public Vector2MoveTowardsEaser (Vector2 target, Vector2 current) : base(target, current) {}
	public Vector2MoveTowardsEaser (Vector2 target, Vector2 current, float maxDelta) : base(target, current, maxDelta) {}

	protected override Vector2 MoveTowards (float deltaTime) {
		return Vector2.MoveTowards(current, target, maxDelta);
	}

	protected override Vector2 GetDelta (Vector2 lastValue, Vector2 newValue) {
		return newValue - lastValue;
	}
}